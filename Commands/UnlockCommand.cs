using System;
using System.Collections.Generic;
using System.Text;
using RPGFramework.Enums;
using RPGFramework.Geography;
namespace RPGFramework.Commands
{
    internal class UnlockCommand : ICommand
    {
        public string Name => "unlock";
        public IEnumerable<string> Aliases => Array.Empty<string>();

        string ICommand.Name => throw new NotImplementedException();

        IEnumerable<string> ICommand.Aliases => throw new NotImplementedException();

        internal static object GetAllCommands()
        {
            throw new NotImplementedException();
        }

        public bool Execute(Character character, string[] args)
        {
            if (character is not Player player)
            {
                return false; // Only players can use this command
            }
            if (args.Length < 1)
            {
                player.WriteLine("Unlock what?");
                return false;
            }
            string targetExitName = args[0];
            Room currentRoom = player.GetRoom();
            Exit? targetExit = null;
            foreach (var exit in currentRoom.GetExits())
            {
                if (string.Equals(exit.Name, targetExitName, StringComparison.OrdinalIgnoreCase))
                {
                    targetExit = exit;
                    break;
                }
            }
            if (targetExit == null)
            {
                player.WriteLine("There is no exit by that name here.");
                return false;
            }
            if (targetExit.ExitType != ExitType.LockedDoor)
            {
                player.WriteLine("That exit is not locked.");
                return false;
            }
            if (targetExit.RequiredKeyId != null && player.HasItem(targetExit.RequiredKeyId))
            {
                targetExit.ExitType = ExitType.Door; // Unlock the door
                player.WriteLine("You unlocked the door!");
                return true;
            }
            else
            {
                player.WriteLine("You do not have the key to unlock this door.");
                return false;
            }
        }

        bool ICommand.Execute(Character character, List<string> parameters)
        {
            throw new NotImplementedException();
        }
    }
}

