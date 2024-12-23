using RPGFramework.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RPGFramework
{
    // A class for processing commands
    // We'll start simple and expand on this later
    internal class Command
    {

        // Process a command from a player or character (NPC)
        // Although the other methods could be called directly, it's better to have a single
        // entry point for all commands. It's probably a good idea for us to make those methods private for clarity.
        public static void Process(Character character, string command)
        {
            // This is broken into multiple parts
            //   So far we have this.ProcessCommand, Navigation.ProcessCommand and Builder.ProcesCommand
            //   We should probably make an interface for command processors.

            List<string> parameters = ParseCommand(command);

            bool cmdExecuted = ProcessCommand(character, parameters);
            cmdExecuted = cmdExecuted || Navigation.ProcessCommand(character, parameters);
            cmdExecuted = cmdExecuted || Builder.ProcessCommand(character, parameters);

            // Unknown command
            if (!cmdExecuted)
            {
                ((Player)character).WriteLine($"I don't know what you mean by '{parameters[0]}'");                            
            }

        }

        private static bool ProcessCommand(Character character, List<string> parameters)
        {
            switch (parameters[0].ToLower())
            {
                case "exit" when character is Player:
                case "quit":
                    ((Player)character).Logout();
                    return true;
                case "ip" when character is Player:
                    Ip((Player)character, parameters);
                    return true;
                case "look":
                case "l" when character is Player:
                    Look((Player)character, parameters);
                    return true;

                case "say":
                case "\"":
                    Say(character, parameters);
                    return true;
                case "time" when character is Player:
                    ((Player)character).WriteLine($"The time is {GameState.Instance.GameDate.ToShortTimeString()}");
                    break;
            }

            return false;
        }

        /// <summary>
        /// Look at something (or room).
        /// Where is the most appropriate place to put this?
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        private static void Look(Player player, List<string> parameters)
        {
            // For now, we'll ignore the command and just show the room description
            player.WriteLine($"{player.GetRoom().Description}");
            player.WriteLine("Exits:");
            foreach (var exit in player.GetRoom().GetExits())
            {
                player.WriteLine($"{exit.Description} to the {exit.ExitDirection}");
            }
        }

        private static void Ip(Player player, List<string> parameters)
        {
            player.WriteLine($"Your IP address is {player.GetIPAddress()}");
        }

        // Send message from player to all players in room.
        // TODO: Make this smarter so the speaker either doesn't hear their message
        // or sees it in the format "You say 'message'"
        private static void Say(Character character, List<string> parameters) 
        {
            // If no message and it's a player, tell them to say something
            if (parameters.Count < 2 && character is Player)
            {
                ((Player)character).WriteLine("Say what?");
                return;
            }

            Comm.RoomSay(character.GetRoom(), parameters[1], character);
        }

        /// <summary>
        /// Parse a command into a list of parameters. These are separated by spaces.
        /// Parameters with multiple words should be inside single quotes.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static List<string> ParseCommand(string command)
        {
            var output = new List<string>();

            // Match words by spaces, multiple words by single quotes
            // Words within single quotes can contain escaped single quotes
            // Single words CANNOT escape single quotes
            string pattern = @"(?<quoted>'(?:\\'|[^'])*')|(?<word>\S+)";
            var matches = Regex.Matches(command, pattern);

            foreach (Match match in matches)
            { 
                if (match.Groups["quoted"].Success)
                {
                    // Remove the outer single quotes and unescape single quotes inside
                    string quotedValue = match.Groups["quoted"].Value;
                    output.Add(Regex.Unescape(quotedValue.Substring(1, quotedValue.Length - 2)));
                }
                else if (match.Groups["word"].Success)
                {
                    output.Add(match.Groups["word"].Value);
                }                
            }

            // Since we don't want to always have to check length of output, we'll add an empty string if no parameters
            if (output.Count == 0)
                output.Add("");
            
            return output;
        }

    }
}
