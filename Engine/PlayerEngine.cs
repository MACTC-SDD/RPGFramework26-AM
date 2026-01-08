using RPGFramework.Workflows;
using Spectre.Console;
using System.Net.Sockets;
using System.Text.Json.Serialization;

namespace RPGFramework
{
    internal partial class Player
    {
        // Things to not save (don't serialize)
        [JsonIgnore]
        public IWorkflow? CurrentWorkflow { get; set; } = null;
        [JsonIgnore]
        public PlayerNetwork? Network { get; set; }

        [JsonIgnore]
        public IAnsiConsole Console { get; set; }

        #region --- Constructors ---
        // Constructor (creates a new player)
        // Review how this is handled in TelnetServer, might not need this anymore
        // Or maybe a different variant that has more initial setup values
        public Player(TcpClient client, string name)
        {
            Network = new PlayerNetwork(client);
            Console = CreateAnsiConsole();
            LocationId = GameState.Instance
                .Areas[GameState.Instance.StartAreaId]
                .Rooms[GameState.Instance.StartRoomId]
                .Id;
            Name = name;
        }

        public Player()
        { }
        #endregion

        #region CreateAnsiConsole Method
        /// <summary>
        /// Create an individual AnsiConsole for this player. We will use this as our
        /// Spectre Console.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private IAnsiConsole CreateAnsiConsole()
        {
            if (Network == null)
                throw new InvalidOperationException("No network connection.");

            var output = new AnsiConsoleOutput(Network.Writer);

            var settings = new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.Yes,
                Out = output,
                Interactive = InteractionSupport.No
            };

            return AnsiConsole.Create(settings);
        }
        #endregion

        /// <summary>
        /// Returns the IP Address of Player. 
        /// This is just for convenience since it's kind of buried in the Network object
        /// </summary>
        /// <returns></returns>
        public string GetIPAddress()
        {
            if (Network == null || Network.Client.Client.RemoteEndPoint == null)
                return "Disconnected";

            return ((System.Net.IPEndPoint)Network.Client.Client.RemoteEndPoint).Address.ToString();
        }
    }
}
