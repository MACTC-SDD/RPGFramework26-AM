using RPGFramework.Items;

namespace RPGFramework.Commands
{
    internal class SpawnCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            TestItemSizeCommand testItemSizeCommand = new();
            return new List<ICommand>
            {
                new SpawnCommand(),
                // Add more test commands here as needed
            };
        }
    }


    internal class SpawnCommand : ICommand
    {
        // This is the command a player would type to execute this command
        public string Name => "/spawn";

        // These are the aliases that can also be used to execute this command. This can be empty.
        public IEnumerable<string> Aliases => [];
        public string Help => "Spawn some kind of item into your current room.\nUsage: /spawn <mob|item|armor|weapon> <name>";

        // What will happen when the command is executed
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;



            return true;
        }
    }
}
