
using RPGFramework.Enums;
using RPGFramework.Geography;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Reflection.Metadata.Ecma335;

namespace RPGFramework.Commands
{
    internal class BuilderCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                new RoomBuilderCommand(),
                new AreaBuilderCommand(),
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
                case "show":
                    RoomShow(player, parameters);
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

        private static void WriteAreaUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine("/area create '<name>' '<description>'");
            player.WriteLine("/area show");
        }

        private static void WriteAreaDeleteUsage(Player player)
        {
            player.WriteLine("Usage:");
            player.WriteLine("/area delete <areaId>");
            player.WriteLine("/area delete <areaId> confirm");
        }


        private static void RoomCreate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.PlayerRole.ToString());
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
        
        private static void RoomShow(Player player, List<string> parameters)
        {
            Room r = player.GetRoom();
            player.Write($"Room name: {r.Name}  Room description: {r.Description}  Room Id: {r.Id} Tags:");
            
            if (r.Tags.Count == 0)
            {
                player.WriteLine("  None");
            }
            else
            {
                foreach (var tag in r.Tags)
                {
                    player.WriteLine($"  {tag}");
                }
            }

            // until end is commented, the following code is generated, but understood in how it does what it does.
            Room room = player.GetRoom();
            Area area = GameState.Instance.Areas[player.AreaId];

            var exits = area.Exits.Values
       .Where(e => e.SourceRoomId == room.Id)
       .ToList();

            if (exits.Count == 0)
            {
                player.WriteLine("There are no exits, forces beyond this realm are at play...");
                return;
            }

            foreach (var exit in exits)
            {
                player.WriteLine(
                    $" Room Exit(s): {exit.ExitDirection} -> Room {exit.DestinationRoomId} ({exit.Description})"
                );
            }
            //end
        }

        private static void DeleteRoom(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
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

    internal class AreaBuilderCommand : ICommand
    {
        public string Name => "/area";

        public IEnumerable<string> Aliases => Array.Empty<string>();

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;

            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return true;
            }

            if (parameters.Count < 2)
            {
                WriteAreaUsage(player);
                return true;
            }

            switch (parameters[1].ToLower())
            {
                case "create":
                    CreateArea(player, parameters);
                    break;

                case "show":
                    AreaShow(player);
                    break;

                case "delete":
                    AreaDelete(player, parameters);
                    break;

                default:
                    WriteAreaUsage(player);
                    break;
            }
            return true;
        }

        private static void WriteAreaUsage(Player player)
        {
            player.WriteLine("Usage:");
            player.WriteLine("/area create '<name>' '<description>'");
        }

        private static void CreateArea(Player player, List<string> parameters)
        {
            // 0: /area
            // 1: create
            // 2: name
            // 3: description
            if (parameters.Count < 4)
            {
                WriteAreaUsage(player);
                return;
            }

            int newAreaId = GameState.Instance.Areas.Count == 0
                ? 0
                : GameState.Instance.Areas.Keys.Max() + 1;

            Area area = new Area
            {
                Id = newAreaId,
                Name = parameters[2],
                Description = parameters[3],
                Rooms = new Dictionary<int, Room>(),
                Exits = new Dictionary<int, Exit>()
            };

            // Create starting room
            GameState.Instance.Areas.Add(area.Id, area);
            
            Room startRoom = Room.CreateRoom(area.Id, "Start Room", "You are in a newly created area.");
            area.Rooms.Add(startRoom.Id, startRoom);

            // Move builder into new area
            player.AreaId = area.Id;
            player.LocationId = startRoom.Id;

            player.WriteLine($"Area '{area.Name}' created (ID {area.Id}).");
        }

        private static void AreaShow(Player player)
        {
            Area area = GameState.Instance.Areas[player.AreaId];

            player.WriteLine($"Area name: {area.Id}");
            player.WriteLine($"Area description: {area.Description}");
            player.WriteLine($"Area Id: {area.Id}");
            player.WriteLine($"Rooms ({area.Rooms.Count})");

            foreach (var room in area.Rooms.Values.OrderBy(r => r.Id))
            {
                player.WriteLine($"Room {room.Id}: {room.Name}");
            }
        }

        private static void AreaDelete(Player player, List<string> parameters)
        {
            if (parameters.Count < 3)
            {
                WriteAreaUsage(player);
                return;
            }

            if (!int.TryParse(parameters[2], out int areaId))
            {
                WriteAreaUsage(player);
                return;
            }

            if (!GameState.Instance.Areas.TryGetValue(areaId, out Area area))
            {
                player.WriteLine("Area not found.");
                return;
            }

            // Prevent deleting the start area
            if (areaId == GameState.Instance.StartAreaId)
            {
                player.WriteLine("You cannot delete the starting area.");
                return;
            }

            bool confirmed = parameters.Count >= 4 &&
                             parameters[3].Equals("confirm", StringComparison.OrdinalIgnoreCase);

            if (!confirmed)
            {
                player.WriteLine("[red]WARNING:[/]");
                player.WriteLine($"You are about to permanently delete:");
                player.WriteLine($"Area {area.Id}: {area.Name}");
                player.WriteLine("This will delete ALL rooms and exits in this area.");
                player.WriteLine("Type:");
                player.WriteLine($"/area delete {area.Id} confirm");
                return;
            }

            // Move players out of the area
            int fallbackAreaId = GameState.Instance.Areas.Keys
                .First(id => id != areaId);

            Area fallbackArea = GameState.Instance.Areas[fallbackAreaId];
            int fallbackRoomId = fallbackArea.Rooms.Keys.First();

            foreach (Player p in GameState.Instance.Players.Values)
            {
                if (p.IsOnline && p.AreaId == areaId)
                {
                    p.WriteLine("The area collapses and reality shifts!");
                    p.AreaId = fallbackAreaId;
                    p.LocationId = fallbackRoomId;
                }
            }

            // Delete the area
            GameState.Instance.Areas.Remove(areaId);

            player.WriteLine($"Area {areaId} deleted.");
        }
    }

}


