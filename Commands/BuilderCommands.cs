
using RPGFramework.Enums;
using RPGFramework.Geography;

namespace RPGFramework.Commands
{
    internal class BuilderCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                new RoomBuilderCommand(),
                // Add more builder commands here as needed
            };
        }
    }

    /// <summary>
    /// /room command for building and editing rooms.
    /// </summary>
    internal class RoomBuilderCommand : ICommand
    {
        public string Name => "/room";

        public IEnumerable<string> Aliases => Array.Empty<string>();

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
            {
                return false;
            }

            if (parameters.Count < 2)
            {
                WriteUsage(player);
                return false;
            }

            switch (parameters[1].ToLower())
            {
                case "description":
                    RoomSetDescription(player, parameters);
                    break;
                case "name":
                    RoomSetName(player, parameters);
                    break;
                case "create":
                    RoomCreate(player, parameters);
                    break;
                case "delete":
                    DeleteRoom(player, parameters);
                    break;
                default:
                    WriteUsage(player);
                    break;
            }

            return true;
        }

        private static void WriteUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine("/room description '<set room desc to this>'");
            player.WriteLine("/room name '<set room name to this>'");
            player.WriteLine("/room create '<name>' '<description>' <exit direction> '<exit description>'");
        }

        private static void WriteDeleteUsage(Player player)
        {
            player.WriteLine("Usage:");
            player.WriteLine("/room delete here");
            player.WriteLine("/room delete here confirm");
            player.WriteLine("/room delete <roomId>");
            player.WriteLine("/room delete <roomId> confirm");
        }


        private static void RoomCreate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }

            // 0: /room
            // 1: create
            // 2: name
            // 3: description
            // 4: exit direction
            // 5: exit description
            if (parameters.Count < 6)
            {
                player.WriteLine("Usage: /room create '<name>' '<description>' <exit direction> '<exit description>'");
                return;
            }

            if (!Enum.TryParse(parameters[4], true, out Direction exitDirection))
            {
                player.WriteLine("Invalid exit direction.");
                return;
            }

            try
            {
                Room room = Room.CreateRoom(player.AreaId, parameters[2], parameters[3]);

                player.GetRoom().AddExits(player, exitDirection, parameters[5], room);
                player.WriteLine("Room created.");
            }
            catch (Exception ex)
            {
                player.WriteLine($"Error creating room: {ex.Message}");
                player.WriteLine(ex.StackTrace);
            }
        }

        private static void RoomSetDescription(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            if (parameters.Count < 3)
            {
                player.WriteLine(player.GetRoom().Description);
            }
            else
            {
                player.GetRoom().Description = parameters[2];
                player.WriteLine("Room description set.");
            }
        }

        private static void RoomSetName(Player player, List<string> parameters)
        {
            if (parameters.Count < 3)
            {
                player.WriteLine(player.GetRoom().Name);
            }
            else
            {
                player.GetRoom().Name = parameters[2];
                player.WriteLine("Room name set.");
            }
        }

        private static void DeleteRoom(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Player))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            if (parameters.Count < 3)
            {
                WriteDeleteUsage(player);
                return;
            }

            // Determine room to delete

            Room roomToDelete = null;

            if (parameters[2].Equals("here", StringComparison.OrdinalIgnoreCase))
            {
                roomToDelete = player.GetRoom();
            }
            else if (int.TryParse(parameters[2], out int roomId))
            {
                if (!GameState.Instance.Areas[player.AreaId].Rooms.TryGetValue(roomId, out roomToDelete))
                {
                    player.WriteLine("Room not found in this area.");
                    return;
                }
            }
            else
            {
                WriteDeleteUsage(player);
                return;
            }

            //  confirm
            
            bool confirmed = parameters.Count >= 4 &&
                             parameters[3].Equals("confirm", StringComparison.OrdinalIgnoreCase);

            if (!confirmed)
            {
                player.WriteLine($"[red]WARNING:[/]");
                player.WriteLine($"You are about to permanently delete:");
                player.WriteLine($"Room {roomToDelete.Id}: {roomToDelete.Name}");
                player.WriteLine($"Type:");
                player.WriteLine($"/room delete {(parameters[2])} confirm");
                return;
            }

            int areaId = roomToDelete.AreaId;

            // Safety checks

            if (GameState.Instance.Areas[areaId].Rooms.Count <= 1)
            {
                player.WriteLine("You cannot delete the last room in an area.");
                return;
            }


            // Move players out of the room
 
            int fallbackRoomId = GameState.Instance.Areas[areaId].Rooms.Keys
                .First(id => id != roomToDelete.Id);

            foreach (Player p in Room.GetPlayersInRoom(roomToDelete))
            {
                p.WriteLine("The room dissolves around you!");
                p.LocationId = fallbackRoomId;
            }

            // Delete room + exits

            Room.DeleteRoom(roomToDelete);

            player.WriteLine($"Room {roomToDelete.Id} deleted.");
        }

    }
}


