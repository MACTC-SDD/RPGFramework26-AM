
using System.Text.Json.Serialization;
using RPGFramework.Enums;
using RPGFramework.Geography;
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
        // Static Fields and Properties
        private static readonly Lazy<GameState> _instance = new(() => new GameState());

        public static GameState Instance => _instance.Value;

        // The persistence mechanism to use. Default is JSON-based persistence.
        public static IGamePersistence Persistence { get; set; } = new JsonGamePersistence();

        public bool IsRunning { get; private set; } = false;

        // Fields
        //private bool IsRunning = false;
        private Thread? _saveThread;
        private Thread? _timeOfDayThread;

        #region --- Properties ---

        /// <summary>
        /// All Areas are loaded into this dictionary
        /// </summary>
        [JsonIgnore] public Dictionary<int, Area> Areas { get; set; } =
            new Dictionary<int, Area>();

        // TODO: Move this to configuration settings class
        public DebugLevel DebugLevel { get; set; } = DebugLevel.Verbose;

        /// <summary>
        /// The date of the game world. This is used for time of day, etc.
        /// </summary>
        public DateTime GameDate { get; set; } = new DateTime(2021, 1, 1);

        /// <summary>
        /// All Players are loaded into this dictionary, with the player's name as the key 
        /// </summary>
        [JsonIgnore] public Dictionary<string, Player> Players { get; set; } = new Dictionary<string, Player>();

        // Move starting area/room to configuration settings
        public int StartAreaId { get; set; } = 0;
        public int StartRoomId { get; set; } = 0;

        public TelnetServer? TelnetServer { get; private set; }

        #endregion --- Properties ---

        #region --- Methods ---
        private GameState()
        {

        }

        public void AddPlayer(Player player)
        {
            Players.Add(player.Name, player);
        }

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
                if (Areas.ContainsKey(area.Id))
                    Areas[area.Id] = area;
                else
                    Areas.Add(area.Id, area);

                GameState.Log(DebugLevel.Alert, $"Area '{area.Name}' loaded successfully.");
            }

            return Task.CompletedTask;
        }

        // Load all Area files from /data/areas. Each Area file will contain some
        // basic info and lists of rooms and exits.
        private async Task LoadAllAreas()
        {
            Areas.Clear();

            var loaded = await Persistence.LoadAreasAsync();
            foreach (var kvp in loaded)
            {
                Areas.Add(kvp.Key, kvp.Value);
                GameState.Log(DebugLevel.Alert, $"Area '{kvp.Value.Name}' loaded successfully.");
            }
        }

        /// <summary>
        /// Loads all player data from persistent storage and adds each player 
        /// to the <see cref="Players"/> collection.
        /// </summary>
        /// <remarks>This method loads all player objects from the data source and 
        /// populates the <see cref="Players"/> dictionary using each player's name 
        /// as the key. Existing entries in <see cref="Players"/>
        /// are not cleared before loading; newly loaded players are added or 
        /// overwrite existing entries with the same name.</remarks>
        private async Task LoadAllPlayers()
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

        /// <summary>
        /// Saves all area entities asynchronously to the persistent storage.
        /// </summary>
        /// <remarks>This method initiates an asynchronous operation to persist 
        /// the current set of areas. The save operation is performed for all 
        /// areas contained in the collection at the time of invocation.</remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous save operation.</returns>
        private Task SaveAllAreas()
        {
            return Persistence.SaveAreasAsync(Areas.Values);
        }

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

        /// <summary>
        /// Saves the specified player to persistent storage asynchronously.
        /// </summary>
        /// <param name="p">The <see cref="Player"/> instance to be saved. Cannot be <c>null</c>.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public Task SavePlayer(Player p)
        {
            return Persistence.SavePlayerAsync(p);
        }

        /// <summary>
        /// Initializes and starts the game server 
        ///   loading all areas
        ///   loading all players
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
            await Persistence.EnsureInitializedAsync(new GamePersistenceInitializationOptions());

            await LoadAllAreas();
            await LoadAllPlayers();

            // Load Item (Weapon/Armor/Consumable/General) catalogs
            // Load NPC (Mobs/Shop/Guild/Quest) catalogs

            this.TelnetServer = new TelnetServer(5555);
            await this.TelnetServer.StartAsync();


            // TODO: Consider moving thread methods to their own class

            // Start threads that run periodically
            _saveThread = new Thread(() => SaveTask(10000));
            _saveThread.IsBackground = true;
            _saveThread.Start();

            _timeOfDayThread = new Thread(() => TimeOfDayTask(15000));
            _timeOfDayThread.IsBackground = true;
            _timeOfDayThread.Start();

            // Other threads will go here
            // Weather?
            // Area threads?
            // NPC threads?
            // Room threads?


        }

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
            _saveThread?.Join();
            _timeOfDayThread?.Join();

            // Exit program
            Environment.Exit(0);
        }

        #endregion --- Methods ---

        #region --- Static Methods ---
        internal static void Log(DebugLevel level, string message)
        {
            if (level <= GameState.Instance.DebugLevel)
            {
                Console.WriteLine($"[{level}] {message}");
            }
        }

        #endregion

        #region --- Thread Methods ---
        /// <summary>
        /// Things that need to be saved periodically
        /// </summary>
        /// <param name="interval"></param>
        private async void SaveTask(int interval)
        {
            while (IsRunning)
            {
                await SaveAllPlayers();
                await SaveAllAreas();

                Thread.Sleep(interval);
                GameState.Log(DebugLevel.Alert, "Autosave complete.");
            }
        }

        /// <summary>
        /// Update the time periodically.
        /// We might want to create game variables that indicate how often this should run
        /// and how much time should pass each time. For now it adds 1 hour / minute.
        /// </summary>
        /// <param name="interval"></param>
        private void TimeOfDayTask(int interval)
        {
            while (IsRunning)
            {
                GameState.Log(DebugLevel.Debug, "Updating time...");
                double hours = (double)interval / 60000;
                GameState.Instance.GameDate = GameState.Instance.GameDate.AddHours(hours);
                Thread.Sleep(interval);
            }
        }
        #endregion --- Thread Methods ---

    }
}
