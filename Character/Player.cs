using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

using Spectre.Console.Rendering;
using RPGFramework.Enums;

namespace RPGFramework
{
    internal partial class Player : Character
    {
        #region --- Properties --- 
        // Properties to NOT save (don't serialize)
        [JsonIgnore]
        public bool IsAFK { get; set; } = false;

        [JsonIgnore]
        public bool IsOnline { get; set; }
        
        // Properties
        public DateTime LastLogin { get; private set; }
        public TimeSpan PlayTime { get; private set; } = new TimeSpan();
        public PlayerRole PlayerRole { get; private set; } = PlayerRole.Player;
        #endregion

        public string DisplayName()
        {
            // We could add colors and other things later, for now, just afk
            return Name + (IsAFK ? " (AFK)" : "");

        }
        
        /// <summary>
        /// Things that should happen when a player logs in.
        /// </summary>
        public void Login()
        {
            IsOnline = true;
            LastLogin = DateTime.Now; 
            Console = CreateAnsiConsole();
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
            Network?.Client.Close();
        }

        /// <summary>
        /// Save the player to the database.
        /// </summary>
        public void Save()
        {
            GameState.Instance.SavePlayer(this);
        }


        public void Write(string message)
        {
            Console.Write(message);
        }

        public void Write(IRenderable renderable)
        {
            Console.Write(renderable);
        }

        
        /// <summary>
        /// Writes the specified message to the output, followed by a line terminator.
        /// </summary>
        /// <param name="message">The message to write. This value can include marku
        /// p formatting supported by the output system.</param>
        public void WriteLine(string message)
        {
            Console.MarkupLine(message);
            //Network?.Writer.WriteLine(message);
        }

    }


}
