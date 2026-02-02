
using System.Text.RegularExpressions;

namespace RPGFramework.Commands
{
    internal static class CommandManager
    {
        private static readonly List<ICommand> _commands = new List<ICommand>();

        static CommandManager()
        {
            // Register all commands from various command sets
            // Add new command sets here as needed
            AdminCommands.GetAllCommands().ForEach(o => Register(o));
            BuilderCommands.GetAllCommands().ForEach(o => Register(o));
            CharacterCommands.GetAllCommands().ForEach(o => Register(o));
            CommunicationCommands.GetAllCommands().ForEach(o => Register(o));
            CoreCommands.GetAllCommands().ForEach(o => Register(o));
            HelpCommands.GetAllCommands().ForEach(o => Register(o));
            InventoryCommands.GetAllCommands().ForEach(o => Register(o));
            NavigationCommands.GetAllCommands().ForEach(o => Register(o));
            TestCommands.GetAllCommands().ForEach(o => Register(o));
            UXCommands.GetAllCommands().ForEach(o => Register(o));
            ItemCommands.GetAllCommands().ForEach(o => Register(o));
        }

        #region Execute Methods (Static)
        /// <summary>
        /// Look through registered commands and execute the first one that matches the verb.
        /// The verb will always be the first parameter.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="parameters"></param>
        /// <param name="commands">The specific commands we should search through.</param>
        /// <param name="ignoreWorkflow">If true, will skip checking for a workflow on the player.
        /// This is useful if we want to process commands while in a workflow.</param>
        /// <returns>Returns true if a command or workflow was found.</returns>
        public static bool Execute(Character character, List<string> parameters, List<ICommand> commands,
            bool ignoreWorkflow = false)
        {
            // If Player.Workflow is not null, send command to specific workflow handler
            // unless ignoreWorkflow is true. This takes precedence over normal commands.
            // This allows for multi-step commands (like onboarding, building, trading, banking, etc)
            if (!ignoreWorkflow && character is Player p && p.CurrentWorkflow != null)
            {
                p.CurrentWorkflow.Execute(p, parameters);
                return true;
            }

            // If we have no parameters, we can't execute anything
            if (parameters == null || parameters.Count == 0)
            {
                return false;
            }

            // Search through specified commands for a match, if found execute it
            string verb = parameters[0].ToLowerInvariant();
            foreach (ICommand command in commands)
            {
                if (IsMatch(command, verb))
                {
                    command.Execute(character, parameters);
                    return true; // We found a matching command
                }
            }

            return false;
        }

        // This is what we normally use, it processes all commands.
        public static bool Execute(Character character, List<string> parameters, bool ignoreWorkflow = false)
        {
            return Execute(character, parameters, _commands, ignoreWorkflow);
        }

        #endregion

        #region ParseCommand Method (Static)
        /// <summary>
        /// Parse a command into a list of parameters. These are separated by spaces.
        /// Parameters with multiple words should be inside single quotes.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>A list of strings representing each parameter in the command.</returns>
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
                    output.Add(Regex.Unescape(quotedValue[1..^1]));
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
        #endregion

        #region Process Methods (Static)
        /// <summary>
        /// Take a command string from a character and process it.
        /// </summary>
        /// <param name="character">The NPC or player issuing a command</param>
        /// <param name="command">The raw command (verb + parameters)</param>
        public static bool Process(Character character, string command, bool ignoreWorkflow = false)
        {
            List<string> parameters = ParseCommand(command);
            if (!Execute(character, parameters, ignoreWorkflow))
            {
                ((Player)character).WriteLine($"I don't know what you mean by '{parameters[0]}'");
                return false;
            }

            return true;
        }
        #endregion

        #region ProcessSpecificCommands Methods (Static)
        /// <summary>
        /// There are three main differences between this and Process():
        /// 1. This allows specifying a specific list of commands to search through
        /// 2. This ignores workflow processing by default
        /// 3. This does not provide feedback if no command is found
        /// </summary>
        /// <param name="character"></param>
        /// <param name="command"></param>
        /// <param name="commands"></param>
        /// <param name="ignoreWorkflow"></param>
        /// <returns>True if the command was found.</returns>
        public static bool ProcessSpecificCommands(Character character, string command,
            List<ICommand> commands, bool ignoreWorkflow = true)
        {
            return Execute(character, ParseCommand(command), commands, ignoreWorkflow: true);
        }

        public static bool ProcessSpecificCommands(Character character, List<string> parameters,
            List<ICommand> commands, bool ignoreWorklow = true)
        {
            return Execute(character, parameters, commands, ignoreWorklow);
        }
        #endregion

        #region IsMatch Method (Static)
        /// <summary>
        /// Determines whether the specified verb matches the command's name or any of its aliases, using a
        /// case-insensitive comparison.
        /// </summary>
        /// <param name="command">The command to check for a matching name or alias. Cannot be <c>null</c>.</param>
        /// <param name="verb">The verb to compare against the command's name and aliases. Cannot be <c>null</c>.</param>
        /// <returns><see langword="true"/> if <paramref name="verb"/> matches the command's name or any alias; otherwise, <see
        /// langword="false"/>.</returns>
        private static bool IsMatch(ICommand command, string verb)
        {
            // Check command name
            if (string.Equals(command.Name, verb, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // If no aliases, can't match
            if (command.Aliases == null)
            {
                return false;
            }

            // Check aliases
            foreach (string alias in command.Aliases)
            {
                if (string.Equals(alias, verb, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Register Method (Static)
        /// <summary>
        /// Add a command to the list of available commands.
        /// </summary>
        /// <param name="command"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Register(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            _commands.Add(command);
        }
        #endregion
    }
}

