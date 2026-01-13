
namespace RPGFramework.Commands
{
    internal class CommunicationCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                new InventoryCommand(),
                // Add other communication commands here as they are implemented
            };
        }


    }

    internal class InventoryCommand : ICommand
    {
        public string Inventory => "equip";
        public IEnumerable<string> Aliases => new List<string> { };

        public string Name => throw new NotImplementedException();

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is Player player)
            {
                player.WriteLine($"Your Inventory is....");
                return true;
            }
            return false;
        }
    }

    internal class SocialCommand : ICommand
    {
        public string Name => "ip";
        public IEnumerable<string> Aliases => new List<string> { };
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is Player player)
            {
                player.WriteLine($"Your IP address is {player.GetIPAddress()}");
                return true;
            }
            return false;
        }
    }
}
