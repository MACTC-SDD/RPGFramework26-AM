using RPGFramework.Enums;
using RPGFramework.Geography;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPGFramework.Commands
{
    internal class SetExitTypeCommand : ICommand
    {
        public string Name => "/set";
        public IEnumerable<string> Aliases => Array.Empty<string>();

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;

            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to build.");
                return true;
            }

            // Expect: /set exit type <direction> <type>
            if (parameters.Count < 5 ||
                parameters[1].ToLower() != "exit" ||
                parameters[2].ToLower() != "type")
            {
                player.WriteLine("Usage: /set exit type <direction> <open|door|lockeddoor|impassable>");
                return true;
            }

            // Parse direction
            if (!Enum.TryParse(parameters[3], true, out Direction direction))
            {
                player.WriteLine("Invalid direction.");
                return true;
            }

            Room room = player.GetRoom();

            Exit? exit = room.GetExits()
                .FirstOrDefault(e => e.ExitDirection == direction);

            if (exit == null)
            {
                player.WriteLine("There are no exits that way.");
                return true;
            }

            // Parse exit type
            if (!Enum.TryParse(parameters[4], true, out ExitType type))
            {
                player.WriteLine("Invalid exit type.");
                return true;
            }

            exit.ExitType = type;

            // Clear key unless locked
            if (type != ExitType.LockedDoor)
                exit.RequiredKeyId = null;

            player.WriteLine($"Exit {direction} is now {type}.");

            return true;
        }
    }
}

internal class Room
    {
    public object Name { get; internal set; }
    public object Description { get; internal set; }
    public object Id { get; internal set; }
    public object Tags { get; internal set; }
    public int AreaId { get; internal set; }
    public object MapColor { get; internal set; }

    internal static Room CreateRoom(int areaId, string v1, string v2)
    {
        throw new NotImplementedException();
    }

    internal static void DeleteRoom(Room roomToDelete)
    {
        throw new NotImplementedException();
    }

    internal static IEnumerable<RPGFramework.Player> GetPlayersInRoom(Room roomToDelete)
    {
        throw new NotImplementedException();
    }

    internal void AddExits(RPGFramework.Player player, Direction exitDirection, string v, Room room)
    {
        throw new NotImplementedException();
    }

    internal object GetExits()
        {
            throw new NotImplementedException();
        }
    }
}
