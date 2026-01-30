
namespace RPGFramework.Commands
{
    internal class CommunicationCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                // Add other communication commands here as they are implemented
            };
        }


    }


    internal class SocialCommand : ICommand
    {
        public string Name => "/soc";
        public IEnumerable<string> Aliases => new List<string> { };
        public string Help => "";
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
