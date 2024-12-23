using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using RPGFramework.MapObjects;

namespace RPGFramework
{
    // TODO: Consider moving all load/save methods to ObjectStorage
    public sealed class GameState
    {
        // Static Fields and Properties
        private static readonly Lazy<GameState> _instance = new Lazy<GameState>(() => new GameState());

        public static GameState Instance => _instance.Value;

        // Fields
        private bool _isRunning = false;
        private Thread _saveThread;
        private Thread _timeOfDayThread;

        #region --- Properties ---

        /// <summary>
        /// All Areas are loaded into this dictionary
        /// </summary>
        [JsonIgnore] public Dictionary<int, Area> Areas { get; set; } = new Dictionary<int, Area>();

        /// <summary>
        /// The date of the game world. This is used for time of day, etc.
        /// </summary>
        public DateTime GameDate { get; set; } = new DateTime(2021, 1, 1);

        /// <summary>
        /// All Players are loaded into this dictionary, with the player's name as the key 
        /// </summary>
        [JsonIgnore] public Dictionary<string, Player> Players { get; set; } = new Dictionary<string, Player>();

        public int StartAreaId {  get; set; } = 0;
        public int StartRoomId {  get; set; } = 0;
        

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
        /// TODO: It would be handy to be able to load a single area. 
        /// </summary>
        /// <param name="areaName"></param>
        private static void LoadArea(string areaName)
        {

        }

        // Load all Area files from /data/areas. Each Area file will contain some basic info and lists of rooms and exits
        private static void LoadAllAlreas()
        {
            GameState.Instance.Areas.Clear();
            List<Area> areas = ObjectStorage.LoadAllObjects<Area>("data/areas/");
            foreach (Area area in areas)
            {
                GameState.Instance.Areas.Add(area.Id, area);
                Console.WriteLine($"Loaded area: {area.Name}");
            }
        }
        private static void LoadAllPlayers()
        {
            // Should we clear all first?

            // Load all players
            List<Player> players = ObjectStorage.LoadAllObjects<Player>("data/players/");
            foreach (Player player in players)
            {
                GameState.Instance.Players.Add(player.Name, player);
                Console.WriteLine($"Loaded player: {player.Name}");
            }
        }

        private static void SaveAllAreas()
        {
            foreach(Area a in GameState.Instance.Areas.Values)
            {
                ObjectStorage.SaveObject(a, "data/areas/", $"{a.Name}.xml");
                Console.WriteLine($"Saved area: {a.Name}");
            }
        }

        private static void SaveAllPlayers(bool includeOffline = false)
        {
            foreach (Player player in GameState.Instance.Players.Values)
            {
                if (player.IsOnline || includeOffline)
                {
                    ObjectStorage.SaveObject(player, "data/players/", $"{player.Name}.xml");
                    Console.WriteLine($"Saved player: {player.Name}");
                }
            }
        }

        public static void SavePlayer(Player p)
        {
            ObjectStorage.SaveObject(p, "data/players/", $"{p.Name}.xml");
        }

        public void Start()
        {
            _isRunning = true;
            LoadAllAlreas();
            LoadAllPlayers();

            /* We may want to do this to bootstrap a starting area/room if none are available to be loaded.
            //Area startArea = new Area() { Id = 0, Name = "Void Area", Description = "Start Area" };
            //new Room()
            //{ Id = 0, Name = "The Void", Description = "You are in a void. There is nothing here." };

            //startArea.Rooms.Add(StartingRoom.Id, StartingRoom);
            //GameState.Instance.Areas.Add(startArea.Id, startArea);
            */

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

        #endregion --- Methods ---

        #region --- Thread Methods ---
        /// <summary>
        /// Things that need to be saved periodically
        /// </summary>
        /// <param name="interval"></param>
        private void SaveTask(int interval)
        {
            while (true)
            {
                SaveAllPlayers();

                SaveAllAreas();
                
                /*

                // Save all NPCs
                foreach (NPC npc in NPCManager.Instance.NPCs)
                {
                    // Save NPC
                }

                // Save all items
                foreach (Item item in ItemManager.Instance.Items)
                {
                    // Save item
                }
                */

                // Sleep for interval
                Thread.Sleep(interval);
                Console.WriteLine("Autosave complete.");
                
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
            while (true)
            {
                Console.WriteLine("Updated time.");
                double hours = (double)interval / 60000;
                GameState.Instance.GameDate = GameState.Instance.GameDate.AddHours(hours);
                Thread.Sleep(interval);
            }
        }
        #endregion --- Thread Methods ---

    }
}
