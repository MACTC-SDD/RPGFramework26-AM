using RPGFramework.Enums;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Numerics;
using System.Text.Json.Serialization;

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
        /*Made a small change?, undid it as it was for just testing*/
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
        
        public static List<Player> GetOnlinePlayers(Dictionary<string, Player> players)
        {
            return [.. players.Values.Where(p => p.IsOnline).OrderBy(p => p.Name)];
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
            if (!IsOnline) 
                return; // If they're already logged out, we don't need to do anything

            TimeSpan duration = DateTime.Now - LastLogin;
            PlayTime += duration;
            IsOnline = false;            
            Save();

            // If we were logging out becuase of a lost connection, we likely won't be able to send this message, and that's fine, so we'll just comment it out for now.
            // We could consider trying to send it and catching any exceptions that occur when trying to write to a lost connection, but for now, we'll just leave it out.
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
            try
            {
                WriteNewLineIfNeeded();
                Console?.Write(message);
                var line = Network?.TelnetConnection?.CurrentLineText;
                Console?.Write(line ?? String.Empty); // Re-write current input line
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                GameState.Log(Enums.DebugLevel.Error, $"Error sending message to player {Name}: {ex.Message}");
                Logout(); // Log the player out if we can't send messages to them, as this likely means their connection is lost
            }
        }

        public void Write(IRenderable renderable)
        {
            try
            {
                WriteNewLineIfNeeded();
                Console?.Write(renderable);
                var line = Network?.TelnetConnection?.CurrentLineText;
                Console?.Write(line ?? String.Empty); // Re-write current input line
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                GameState.Log(Enums.DebugLevel.Error, $"Error sending message to player {Name}: {ex.Message}");
                Logout(); // Log the player out if we can't send messages to them, as this likely means their connection is lost
            }
        }


        /// <summary>
        /// Writes the specified message to the output, followed by a line terminator.
        /// </summary>
        /// <param name="message">The message to write. This value can include marku
        /// p formatting supported by the output system.</param>
        public void WriteLine(string message)
        {
            try
            {
                WriteNewLineIfNeeded();
                Console?.MarkupLine(message);
                var line = Network?.TelnetConnection?.CurrentLineText;
                Console?.Write(line ?? String.Empty); // Re-write current input line
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                GameState.Log(Enums.DebugLevel.Error, $"Error sending message to player {Name}: {ex.Message}");
                Logout(); // Log the player out if we can't send messages to them, as this likely means their connection is lost
            }
        }
        private void WriteNewLineIfNeeded()
        {
            try
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
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                GameState.Log(Enums.DebugLevel.Error, $"Error sending message to player {Name}: {ex.Message}");
                Logout(); // Log the player out if we can't send messages to them, as this likely means their connection is lost
            }
        }

        #region Leveling Methods

        public void AddXP(int xp)
        {
            XP += xp;
            // Check for level up
            int xpForNextLevel = Level * 100; // Example: 100 XP per level
            while (XP >= xpForNextLevel)
            {
                XP -= xpForNextLevel;
                LevelUp();
                xpForNextLevel = Level * 100;
            }
        }
        protected void LevelUp()
        {
            Level += 1;
            // Increase Max Health by 10% per level
            int healthIncrease = (int)(MaxHealth * 0.1);
            SetMaxHealth(MaxHealth + healthIncrease);
            // Restore health to full on level up
            SetHealth(MaxHealth);
            this.WriteLine($"[green]Congratulations! You've reached level {Level}![/]");
            this.WriteLine($"[green]Your Max Health has increased by {healthIncrease} to {MaxHealth}.[/]");
            this.WriteLine($"Please select a stat to level up!: ");
            /*string input = Network!.TelnetConnection!.ReadLine();*/ //I commented it out because it had a warning, I used ChatGPT to make a version that used ?, so it could be null?(Just delete the line below and make this one work if needed/wanted)-Landon
            string? input = Network?.TelnetConnection?.ReadLine();

            if (input != null)
            {
                switch (input.ToLower())
                {
                    case "strength":
                        // Use the provided increment method on Character instead of assigning to the private setter
                        IncrimentStrength(1);
                        this.WriteLine($"[green]Your Strength has increased to {Strength}.[/]");
                        break;
                }
            }
        }
        
        #endregion

        public void RemoveArmor(ArmorSlot slot)
        {
            // 1. Find the armor currently equipped in this slot (e.g., Head)
            Armor? armorToRemove = EquippedArmor.FirstOrDefault(a => a.Slot == slot);

            // 2. If we found something, remove it
            if (armorToRemove != null)
            {
                EquippedArmor.Remove(armorToRemove);       // Remove from body
                PlayerInventory.Items.Add(armorToRemove);  // Add back to backpack

                // Optional: specific message
                this.WriteLine($"You unequipped the [cyan]{armorToRemove.Name}[/].");
            }
        }
        public void RemoveWeapon()
        {
            if (PrimaryWeapon.Name == "Fists")
            {
                WriteLine("You aren't holding anything to unequip.");
                return;
            }

            PlayerInventory.Items.Add(PrimaryWeapon);
            WriteLine($"You put away the {PrimaryWeapon.Name}.");

            AddFists();
        }
    }


}
