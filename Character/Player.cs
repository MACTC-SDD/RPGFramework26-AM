using System.Text.Json.Serialization;
using Spectre.Console;
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
        public DateTime LastLogin { get; set; }
        public int MapRadius { get; set; } = 2; // How far the player can see on the map
        public string Password { get; private set; } = "SomeGarbage";
        public TimeSpan PlayTime { get; set; } = new TimeSpan();
        public PlayerRole Role { get; set; }
        #endregion

        public string DisplayName()
        {
            // We could add colors and other things later, for now, just afk
            return Name + (IsAFK ? " (AFK)" : "");

        }
        /// <summary>
        /// Checks if a player with the specified name exists in the provided dictionary. This is case-insensitive!
        /// That is why we don't just use players.ContainsKey.
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="players"></param>
        /// <returns></returns>
        public static bool Exists(string playerName, Dictionary<string, Player> players)
        {
            // Check dictionary keys in a case-insensitive manner
            return players.Keys.Any(name => string.Equals(name, playerName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Searches for a player by name in the specified collection and returns the corresponding player if found.
        /// This search is case-insensitive, which is why we should use this method instead of directly accessing the dictionary.
        /// </summary>
        /// <param name="playerName">The name of the player to locate. The comparison is case-insensitive.</param>
        /// <param name="players">A dictionary containing player names as keys and their corresponding Player objects as values. Cannot be
        /// null. This will usually be GameState.Instance.Players in our case.</param>
        /// <returns>The Player object associated with the specified name if found; otherwise, null.</returns>
        public static Player? FindPlayer(string playerName, Dictionary<string, Player> players)
        {
            foreach (var kvp in players)
            {
                if (string.Equals(kvp.Key, playerName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Attempts to find a player with the specified name in the provided collection.
        /// </summary>
        /// <param name="playerName">The name of the player to locate. Cannot be null.</param>
        /// <param name="players">A dictionary containing player names as keys and corresponding <see cref="Player"/> objects as values.
        /// Cannot be null.</param>
        /// <param name="player">When this method returns, contains the <see cref="Player"/> object associated with the specified name, if
        /// found;</param>
        /// <returns><see langword="true"/> if a player with the specified name is found; otherwise, <see langword="false"/>.</returns>
        public static bool TryFindPlayer(string playerName, Dictionary<string, Player> players, out Player? player)
        {
            player = FindPlayer(playerName, players);
            return player != null;
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
            WriteNewLineIfNeeded();
            Console?.Write(message);
            var line = Network?.TelnetConnection?.CurrentLineText;
            Console?.Write(line ?? String.Empty); // Re-write current input line
        }

        public void Write(IRenderable renderable)
        {
            WriteNewLineIfNeeded();
            Console?.Write(renderable);
            var line = Network?.TelnetConnection?.CurrentLineText;
            Console?.Write(line ?? String.Empty); // Re-write current input line
        }

        
        /// <summary>
        /// Writes the specified message to the output, followed by a line terminator.
        /// </summary>
        /// <param name="message">The message to write. This value can include marku
        /// p formatting supported by the output system.</param>
        public void WriteLine(string message)
        {
            WriteNewLineIfNeeded();
            Console?.MarkupLine(message);
            var line = Network?.TelnetConnection?.CurrentLineText;
            Console?.Write(line ?? String.Empty); // Re-write current input line
        }
        private void WriteNewLineIfNeeded()
        {
            if (Network == null)
                return;
            if (Network.TelnetConnection == null)
                return;
            if (Network.NeedsOutputNewline)
            {
                Console?.Write("\r\n");
            }
        }


    }


}
