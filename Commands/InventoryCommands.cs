
namespace RPGFramework.Commands
{
    internal class InventoryCommands
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
        public string Name => "equip";
        public IEnumerable<string> Aliases => new List<string> { };

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
}
