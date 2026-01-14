
using RPGFramework.Display;

namespace RPGFramework.Commands
{
    internal class AdminCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                new AnnounceCommand(),
                new ShutdownCommand(),
                // Add more builder commands here as needed
            };
        }
    }

    internal class AnnounceCommand : ICommand
    {
        public string Name => "announce";
        public IEnumerable<string> Aliases => new List<string>() { "ann" };
        public bool Execute(Character character, List<string> parameters)
        {
            Comm.Broadcast($"{DisplaySettings.AnnouncementColor}[[Announcement]]: [/][white]" + 
                $"{string.Join(' ', parameters.Skip(1))}[/]");
            return true;
        }
    }

    internal class ShutdownCommand : ICommand
    {
        public string Name => "shutdown";
        public IEnumerable<string> Aliases => new List<string>() { };
        public bool Execute(Character character, List<string> parameters)
        {
            Comm.Broadcast($"{DisplaySettings.AnnouncementColor}[[WARNING]]: [/][white]" +
                $"Server is shutting down. All data will be saved.[/]");

            using var _ = GameState.Instance.Stop();
            return true;
        }
    }
}
