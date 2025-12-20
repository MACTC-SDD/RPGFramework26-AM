using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGFramework.Display;

namespace RPGFramework.Commands
{
    public class AdminCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                new AnnounceCommand(),
                // Add more builder commands here as needed
            };
        }
    }

    public class AnnounceCommand : ICommand
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
}
