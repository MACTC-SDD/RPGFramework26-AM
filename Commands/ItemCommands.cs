using RPGFramework.Display;
using RPGFramework.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPGFramework.Commands
{
    internal class ItemCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                new ListInventoryCommand(),
                new ListRoomItemCommand(),
                // Add more builder commands here as needed
            };
        }
    }

    internal class ListInventoryCommand : ICommand
    {
        public string Name => "inventory";
        public IEnumerable<string> Aliases => new List<string>() { "inv" };
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
            {
                return false;
            }
            player.WriteLine("");
                return true;
        }
    }
    internal class ListRoomItemCommand : ICommand
    {
        public string Name => "roomitems";
        public IEnumerable<string> Aliases => new List<string>() { "ri" };
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
            {
                return false;
            }
            player.WriteLine("");
            return true;
        }
    }
}

