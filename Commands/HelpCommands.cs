using RPGFramework.Interfaces;
using RPGFramework.Enums;

namespace RPGFramework.Commands
{
    internal class HelpCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return
            [
                new HelpBuilderCommand(),
            ];
        }
    }

    internal class HelpBuilderCommand : ICommand
    {
        public string Name => "/help";

        public IEnumerable<string> Aliases => [];
        public string Help => "Create and modify help entries.\n" +
            "/help create <topic> <category> 'content'\n" +
            "/help set <topic> content|category 'updated content or category'\n" +
            "/help delete <topic>\n";

        private readonly GameState _instance = GameState.Instance;
 
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;

            if (Utility.CheckPermission(player, PlayerRole.Admin) == false)
            {
                player.WriteLine("You do not have permission to use this command.");
                return false;
            }

            if (parameters.Count < 2)
                return ShowHelp(player);

            // In case you wonder, this is like a switch statement but it's called a 
            // switch expression. 
            return parameters[1].ToLower() switch
            {
                "create" => HelpCreate(player, parameters),
                "delete" => HelpDelete(player, parameters),
                "set" => HelpSet(player, parameters),
                _ => ShowHelp(player),
            };
        }

        #region HelpCreate Method
        private bool HelpCreate(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
                return ShowHelp(player);

            string topic = parameters[2];
            string category = parameters[3];
            string content = parameters[4];

            if (_instance.HelpCatalog.TryGetValue(topic, out HelpEntry? _))
            {
                player.WriteLine($"Help topic ({topic}) already exists!");
                return false;
            }

            HelpEntry help = new()
            { Topic = topic, Category = category, Content = content };

            _instance.HelpCatalog.Add(help.Topic, help);

            player.WriteLine($"Help entry created:\nTopic: {topic}\nCategory: {category}\nContent: {content}");
            return true;
        }
        #endregion

        #region HelpDelete Method
        private bool HelpDelete(Player player, List<string> parameters)
        {
            if (parameters.Count < 3)
                return ShowHelp(player);

            string topic = parameters[2];
            if (!_instance.HelpCatalog.TryGetValue(topic, out HelpEntry? help) || help == null)
            {
                player.WriteLine("Help topic ({topic}) could not be found!");
                return false;
            }

            _instance.HelpCatalog.Remove(help.Topic);
            player.WriteLine($"Help topic ({topic}) deleted!");
            return true;
        }
        #endregion

        #region HelpSet Method
        private bool HelpSet(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
                return ShowHelp(player);

            string topic = parameters[2];
            string toChange = parameters[3].ToLower();
            string content = parameters[4];

            if (!_instance.HelpCatalog.TryGetValue(topic, out HelpEntry? help) || help == null)
            {
                player.WriteLine($"Help topic ({topic}) could not be found!");
                return false;
            }

            switch (toChange)
            {
                case "content":
                    help.Content = content;
                    break;
                case "category":
                    help.Category = content;
                    break;
                default:
                    player.WriteLine("I'm not sure what you want to change (you should use 'topic or 'category')");
                    return ShowHelp(player);
            }

            player.WriteLine($"Help entry ({topic}/{toChange}) updated successfully.");
            return true;
        }
        #endregion

        private bool ShowHelp(Player player)
        {
            player.WriteLine(Help);
            return false;
        }
    }
}
