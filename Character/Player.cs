using System.Text.Json.Serialization;
using Spectre.Console;
using Spectre.Console.Rendering;

using RPGFramework.Enums;
using RPGFramework.Engine;

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
        public DateTime LastLogin { get; set; }
        public int MapRadius { get; set; } = 2; // How far the player can see on the map
        public string Password { get; private set; } = "SomeGarbage";
        public TimeSpan PlayTime { get; set; } = new TimeSpan();
        public PlayerRole PlayerRole { get; set; } = PlayerRole.Player;
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

        /// <summary>
        /// Sets the password to the specified value.
        /// </summary>
        /// <param name="newPassword">The new password to assign. Cannot be null.</param>
        /// <returns>true if the password was set successfully; otherwise, false.</returns>
        public bool SetPassword(string newPassword)
        {
            // TODO: Consider adding password complexity checking
            Password = newPassword;
            return true;
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
