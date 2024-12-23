using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RPGFramework
{
    public class Player : Character
    {
        [JsonIgnore]
        public PlayerNetwork Network { get; set; }
        [JsonIgnore]
        public bool IsOnline { get; set; } 
        public DateTime LastLogin { get; set; }
        public TimeSpan PlayTime { get; set; } = new TimeSpan();
        public PlayerRole Role { get; set; } = PlayerRole.Player;

        // Constructor (creates a new player)
        // Review how this is handled in TelnetServer, might not need this anymore
        // Or maybe a different variant that has more initial setup values
        public Player(TcpClient client, string name)
        {
            Network = new PlayerNetwork(client);
            LocationId = GameState.Instance.Areas[GameState.Instance.StartAreaId].Rooms[GameState.Instance.StartRoomId].Id;
            Name = name;
        }

        public Player()
        { }

        // This is just for convenience since it's kind of buried in the Network object
        public string GetIPAddress()
        {
            return ((System.Net.IPEndPoint)Network.Client.Client.RemoteEndPoint).Address.ToString();
        }

        /// <summary>
        /// Things that should happen when a player logs in.
        /// </summary>
        public void Login()
        {

        }

        /// <summary>
        /// Things that should happen when a player logs out. 
        /// </summary>
        public void Logout()
        {
            TimeSpan duration = DateTime.Now - LastLogin;
            PlayTime += duration;
            IsOnline = false;            
            Save();

            WriteLine("Bye!");
            Network.Client.Close();
        }

        /// <summary>
        /// Save the player to the database.
        /// </summary>
        private void Save()
        {
            GameState.SavePlayer(this);
        }

        // This is just for convenience, we could access the Network.Writer directly
        // Should this be done throught the Comm class instead so it's all in one place?
        public void WriteLine(string message)
        {
            Network.Writer.WriteLine(message);
        }
    }


}
