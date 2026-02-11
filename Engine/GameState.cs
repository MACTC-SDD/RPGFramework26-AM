
using System.Text.Json.Serialization;
using RPGFramework.Combat;
using RPGFramework.Enums;
using RPGFramework.Geography;
using RPGFramework.Interfaces;
using RPGFramework.Persistence;

namespace RPGFramework
{
    /// <summary>
    /// Represents the global state and core management logic for the game server, 
    /// including loaded areas, players, game time, and server lifecycle.
    /// </summary>
    /// <remarks><para> <b>Singleton Access:</b> Use the <see cref="Instance"/> property 
    /// to access the single <see cref="GameState"/> instance throughout the application. </para>
    /// <para> <b>Persistence:</b> The <see cref="Persistence"/> property determines how 
    /// game data is loaded and saved. By default, a JSON-based persistence
    /// mechanism is used, but this can be replaced with a custom implementation. </para> 
    internal sealed class GameState
    {
        // IF YOU SET THIS TO TRUE, IT WILL OVERWRITE ALL DATA FILES DURING INITIALIZATION
        // This is a good thing if you want to reset everything, like after world files
        // have been updated in data_seed, but be careful as it will wipe out
        // any existing area, room, and catalog (mob, item, etc.) data.
        private readonly bool _OVERWRITE_DATA = false;

        // Static Fields and Properties
        private static readonly Lazy<GameState> _instance = new(() => new GameState());

        public static GameState Instance => _instance.Value;

        // The persistence mechanism to use. Default is JSON-based persistence.
        public static IGamePersistence Persistence { get; set; } = new JsonGamePersistence();


        #region --- Fields ---
        private CancellationTokenSource? _saveCts;
        private Task? _saveTask;

        private CancellationTokenSource? _timeOfDayCts;
        private Task? _timeOfDayTask;

        private CancellationTokenSource? _battleCts;
        private Task? _battleTask;

        private CancellationTokenSource? _spawnMobsCts;
        private Task? _spawnMobsTask;

        private int _logSuppressionSeconds = 30;
        #endregion

        #region --- Properties ---

        #region --- Unserialized Properties ---

        [JsonIgnore] public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// All Areas are loaded into this dictionary
        /// </summary>
        [JsonIgnore] public Dictionary<int, Area> Areas { get; set; } = [];

        [JsonIgnore] public List<Battle> Battles { get; set; } = [];

        /// <summary>
        /// All Players are loaded into this dictionary, with the player's name as the key 
        /// </summary>
        [JsonIgnore] public Dictionary<string, Player> Players { get; set; } = [];

        [JsonIgnore] public List<ICatalog> Catalogs { get; private set; } = [];
        [JsonIgnore] public Catalog<string, Mob> MobCatalog { get; set; } = [];
        [JsonIgnore] public Catalog<string, NonPlayer> NPCCatalog { get; set; } = [];
        [JsonIgnore] public Catalog<string, Item> ItemCatalog { get; set; } = [];
        [JsonIgnore] public Catalog<string, Weapon> WeaponCatalog { get; set; } = [];
        [JsonIgnore] public Catalog<string, Armor> ArmorCatalog { get; set; } = [];
        [JsonIgnore] public Catalog<string, Shopkeep> ShopkeepCatalog { get; set; } = [];
        [JsonIgnore] public Catalog<string, HelpEntry> HelpCatalog { get; set; } = [];

        [JsonIgnore] public TelnetServer? TelnetServer { get; private set; }
        #endregion

        // TODO: Move this to configuration settings class
        public DebugLevel DebugLevel { get; set; } = DebugLevel.Debug;

        /// <summary>
        /// The date of the game world. This is used for time of day, etc.
        /// </summary>
        public DateTime GameDate { get; set; } = new DateTime(2021, 1, 1);

        // Move starting area/room to configuration settings
        public int StartAreaId { get; set; } = 0;
        public int StartRoomId { get; set; } = 0;

        #endregion --- Properties ---

        private GameState()
        {
            Catalogs.Add(ArmorCatalog);
            Catalogs.Add(ItemCatalog);
            Catalogs.Add(HelpCatalog);
            Catalogs.Add(MobCatalog);
            Catalogs.Add(NPCCatalog);
            Catalogs.Add(WeaponCatalog);
            Catalogs.Add(ShopkeepCatalog);
        }

        public void AddPlayer(Player player)
        {
            Players.Add(player.Name, player);
        }

        #region --- Methods ---
        #region LoadArea Method
        /// <summary>
        /// This would be used by an admin command to load an area on demand. 
        /// For now useful primarily for reloading externally crearted changes
        /// </summary>
        /// <param name="areaName"></param>
        private Task LoadArea(string areaName)
        {
            Area? area = GameState.Persistence.LoadAreaAsync(areaName).Result;
            if (area != null)
            {
                if (!Areas.TryAdd(area.Id, area))
                    Areas[area.Id] = area;
                GameState.Log(DebugLevel.Alert, $"Area '{area.Name}' loaded successfully.");
            }

            return Task.CompletedTask;
        }
        #endregion

        #region LoadAllAreas Method
        // Load all Area files from /data/areas. Each Area file will contain some
        // basic info and lists of rooms and exits.
        public async Task LoadAllAreas()
        {
            Areas.Clear();

            var loaded = await Persistence.LoadAreasAsync();
            foreach (var kvp in loaded)
            {
                Areas.Add(kvp.Key, kvp.Value);
                GameState.Log(DebugLevel.Alert, $"Area '{kvp.Value.Name}' loaded successfully.");
            }

            // Ensure start area/room are valid
            if (!Areas.TryGetValue(StartAreaId, out Area? value) || value.Rooms.Count == 0)
            {
                var firstArea = Areas.Values.First();
                StartAreaId = firstArea.Id;
                StartRoomId = firstArea.Rooms.Keys.First();
            }
        }
        #endregion

        #region LoadAllPlayers Method
        /// <summary>
        /// Loads all player data from persistent storage and adds each player 
        /// to the <see cref="Players"/> collection.
        /// </summary>
        /// <remarks>This method loads all player objects from the data source and 
        /// populates the <see cref="Players"/> dictionary using each player's name 
        /// as the key. Existing entries in <see cref="Players"/>
        /// are not cleared before loading; newly loaded players are added or 
        /// overwrite existing entries with the same name.</remarks>
        public async Task LoadAllPlayers()
        {
            Players.Clear();

            var loaded = await Persistence.LoadPlayersAsync();
            foreach (var kvp in loaded)
            {
                Players.Add(kvp.Key, kvp.Value);
                GameState.Log(DebugLevel.Debug, $"Player '{kvp.Value.Name}' loaded successfully.");
            }

            GameState.Log(DebugLevel.Alert, $"{Players.Count} players loaded.");
        }
        #endregion

        #region LoadAllCatalogs Method
        public async Task LoadCatalogs()
        {
            foreach (ICatalog catalog in Catalogs)
            {
                await catalog.LoadCatalogAsync();
            }
        }
        #endregion

        #region SaveAllAreas Method
        /// <summary>
        /// Saves all area entities asynchronously to the persistent storage.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous save operation.</returns>
        private Task SaveAllAreas()
        {
            return Persistence.SaveAreasAsync(Areas.Values);
        }
        #endregion

        #region SaveAllPlayers Method
        /// <summary>
        /// Saves all player data asynchronously.
        /// </summary>
        /// <param name="includeOffline"><see langword="true"/> to include offline 
        /// players in the save operation; otherwise, only online players are saved.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        private Task SaveAllPlayers(bool includeOffline = false)
        {
            var toSave = includeOffline
                ? Players.Values
                : Players.Values.Where(p => p.IsOnline);

            return Persistence.SavePlayersAsync(toSave);
        }
        #endregion

        #region SavePlayer Method
        /// <summary>
        /// Saves the specified player to persistent storage asynchronously.
        /// </summary>
        /// <param name="p">The <see cref="Player"/> instance to be saved. Cannot be <c>null</c>.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public Task SavePlayer(Player p)
        {
            return Persistence.SavePlayerAsync(p);
        }
        #endregion

        #region SaveAllCatalogs Method
        public async Task SaveAllCatalogs()
        {
            foreach (ICatalog catalog in Catalogs)
            {
                await catalog.SaveCatalogAsync();
            }
        }
        #endregion

        #region Start Method (Async)
        /// <summary>
        /// Initializes and starts the game server 
        ///   loading all areas
        ///   loading all players
        ///   loading all catalogs
        ///   starting the Telnet server
        ///   launching background threads for periodic tasks.
        /// </summary>
        /// <remarks>This method must be called before the server can accept 
        /// Telnet connections or process game logic. It loads all required game 
        /// data and starts background threads for saving state and updating the
        /// time of day.</remarks>
        /// <returns>A task that represents the asynchronous start operation.</returns>
        public async Task Start()
        {
            // Prevent multiple starts (TODO: Add restart / stop functionality)
            if (IsRunning)
                throw new InvalidOperationException("Game server is already running.");

            IsRunning = true;

            // Initialize game data if it doesn't exist            
            await Persistence.EnsureInitializedAsync(new GamePersistenceInitializationOptions()
            {
                OverwriteFromDataSeedToRuntimeData = _OVERWRITE_DATA
            });

            await LoadAllAreas();
            await LoadAllPlayers();
            await LoadCatalogs();

            // TODO: Consider moving thread methods to their own class

            // Start threads that run periodically
            _saveCts = new CancellationTokenSource();
            _saveTask = RunAutosaveLoopAsync(TimeSpan.FromMilliseconds(10000), _saveCts.Token);

            _timeOfDayCts = new CancellationTokenSource();
            _timeOfDayTask = RunTimeOfDayLoopAsync(TimeSpan.FromMilliseconds(15000), _timeOfDayCts.Token);

            _battleCts = new CancellationTokenSource();
            _battleTask = RunBattleManagerLoopAsync(TimeSpan.FromSeconds(5), _battleCts.Token);

            _spawnMobsCts = new CancellationTokenSource();
            _spawnMobsTask = RunSpawnMobLoopAsync(TimeSpan.FromSeconds(30), _spawnMobsCts.Token);
            // Other threads will go here
            // Weather?
            // Area threads?
            // NPC threads?
            // Room threads?

            // This needs to be last
            this.TelnetServer = new TelnetServer(5555);
            await this.TelnetServer.StartAsync();
        }
        #endregion

        #region Stop Method (Async)
        /// <summary>
        /// Stops the server, saving all player and area data, disconnecting online players, 
        /// and terminating the application.
        /// </summary>
        /// <remarks>This method performs a graceful shutdown by persisting all relevant data, 
        /// logging out online users, stopping background threads, and closing network 
        /// connections before exiting the process.</remarks>
        /// <returns>A task that represents the asynchronous stop operation. The application 
        /// will terminate upon completion.</returns>
        /// TODO: Allow user to supply a duration to avoid immediate shutdown
        public async Task Stop()
        {
            await SaveAllPlayers(includeOffline: true);
            await SaveAllAreas();

            foreach (var player in Players.Values.Where(p => p.IsOnline))
            {
                player.Logout();
            }

            this.TelnetServer!.Stop();

            // Signal threads to stop
            IsRunning = false;

            // Wait for threads to finish
            _saveCts?.Cancel();
            _timeOfDayCts?.Cancel();
            _battleCts?.Cancel();
            _spawnMobsCts?.Cancel();
            // Exit program
            Environment.Exit(0);
        }
        #endregion

        #endregion --- Methods ---

        #region --- Static Methods ---
        internal static bool Log(DebugLevel level, string message)
        {
            if (level <= GameState.Instance.DebugLevel)
            {
                Console.WriteLine($"[{level}] {message}");
                return true;
            }
            return false;
        }

        internal static bool Log(DebugLevel level, string message, DateTime lastLog, int suppressionSeconds)
        {
            if ((DateTime.Now - lastLog).TotalSeconds >= suppressionSeconds)
            {
                return Log(level, message);
            }

            return false;
        }
        #endregion

        #region --- Thread Methods ---
        /// <summary>
        /// Things that need to be saved periodically
        /// </summary>
        /// <param name="interval"></param>
        private async Task RunAutosaveLoopAsync(TimeSpan interval, CancellationToken ct)
        {
            GameState.Log(DebugLevel.Alert, "Autosave thread started.");
            while (!ct.IsCancellationRequested && IsRunning)
            {
                try
                {
                    await SaveAllPlayers();
                    await SaveAllAreas();
                    await SaveAllCatalogs();

                    GameState.Log(DebugLevel.Info, "Autosave complete.");
                }
                catch (Exception ex)
                {
                    GameState.Log(DebugLevel.Error, $"Error during autosave: {ex.Message}");
                }

                await Task.Delay(interval, ct);
            }

            GameState.Log(DebugLevel.Alert, "Autosave thread stopping.");
        }

        /// <summary>
        /// Update the time periodically.
        /// We might want to create game variables that indicate how often this should run
        /// and how much time should pass each time. For now it adds 1 hour / minute.
        /// </summary>
        /// <param name="interval"></param>
        private async Task RunTimeOfDayLoopAsync(TimeSpan interval, CancellationToken ct)
        {
            GameState.Log(DebugLevel.Alert, "Time of Day thread started.");
            while (!ct.IsCancellationRequested && IsRunning)
            {
                try
                {
                    GameState.Log(DebugLevel.Debug, "Updating time...");
                    double hours = interval.TotalMinutes * 60;
                    GameState.Instance.GameDate = GameState.Instance.GameDate.AddHours(hours);
                }
                catch (Exception ex)
                {
                    GameState.Log(DebugLevel.Error, $"Error during time update: {ex.Message}");
                }

                await Task.Delay(interval, ct);
            }
            GameState.Log(DebugLevel.Alert, "Time of Day thread stopping.");
        }

        private async Task RunBattleManagerLoopAsync(TimeSpan interval, CancellationToken ct)
        {
            GameState.Log(DebugLevel.Alert, "Battle Manager thread started.");
            while (!ct.IsCancellationRequested && IsRunning)
            {
                try
                {
                    GameState.Log(DebugLevel.Debug, "Updating battles...");
                    foreach (Battle b in Battles)
                    {
                        b.ProcessTurn();
                    }
                }
                catch (Exception ex)
                {
                    GameState.Log(DebugLevel.Error, $"Error during battle update: {ex.Message}");
                }
                await Task.Delay(interval, ct);
            }
        }
        #endregion --- Thread Methods ---
        #region RunSpawnMobLoopAsync Method
        private async Task RunSpawnMobLoopAsync(TimeSpan interval, CancellationToken ct)
        {
            GameState.Log(DebugLevel.Alert, "Spawn mobs started.");
            while (!ct.IsCancellationRequested && IsRunning)
            {
                try
                {
                    GameState.Log(DebugLevel.Debug, "Spawning Mobs...");
                    foreach (Area area in GameState.Instance.Areas.Values)
                    {
                        foreach (Room room in area.Rooms.Values)
                        {
                            room.SpawnMobsInRoom();
                        }
                    }
                }
                catch (Exception ex)
                {
                    GameState.Log(DebugLevel.Error, $"Error during Mobs spawning: {ex.Message}");
                }

                await Task.Delay(interval, ct);
            }
            GameState.Log(DebugLevel.Alert, "Mobs spawn thread stopping.");
        }
        #endregion

    }
}
