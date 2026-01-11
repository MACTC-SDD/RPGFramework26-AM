
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
            CoreCommands.GetAllCommands().ForEach(o => Register(o));
            NavigationCommands.GetAllCommands().ForEach(o => Register(o));
            TestCommands.GetAllCommands().ForEach(o => Register(o));
            UXCommands.GetAllCommands().ForEach(o => Register(o));
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

        /// <summary>
        /// Take a command string from a character and process it.
        /// </summary>
        /// <param name="character">The NPC or player issuing a command</param>
        /// <param name="command">The raw command (verb + parameters)</param>
        public static void Process(Character character, string command)
        {
            List<string> parameters = ParseCommand(command);
            if (!Execute(character, parameters))
                ((Player)character).WriteLine($"I don't know what you mean by '{parameters[0]}'");
        }

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

        /// <summary>
        /// Look through registered commands and execute the first one that matches the verb.
        /// The verb will always be the first parameter.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static bool Execute(Character character, List<string> parameters)
        {
            // If Player.Workflow is not null, send command to specific workflow handler
            // This allows for multi-step commands (like onboarding, building, trading, banking, etc)
            if (character is Player p && p.CurrentWorkflow != null)
            {
                p.CurrentWorkflow.Execute(p, parameters);
                return true;
            }

            if (parameters == null || parameters.Count == 0)
            {
                return false;
            }

            string verb = parameters[0].ToLowerInvariant();
            foreach (ICommand command in _commands)
            {
                if (IsMatch(command, verb))
                {
                    bool success = command.Execute(character, parameters);
                    return true;
                }
            }

            return false;
        }

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
    }
}

