
using RPGFramework.Enums;
using RPGFramework.Geography;
using Spectre.Console;
using System.Diagnostics;
using System.Formats.Asn1;

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
                new AreaBuilderCommand(),
                new RoomCopyCommand(),
            };
        }
    }
    internal class RoomCopyCommand : ICommand
    {
        public string Name => "copy";
        public IEnumerable<string> Aliases => Array.Empty<string>();
        public string Description => "Clone a room without exits";

        // This matches your existing ICommands
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;

            // Usage check: /room copy <roomId> <newName>
            if (parameters.Count < 3)
            {
                player.WriteLine("");
                player.WriteLine("Usage: /room copy <roomId> <newName>");
                return false;
            }

            string roomIdText = parameters[1];
            string newName = string.Join(" ", parameters.Skip(2));

            if (!int.TryParse(roomIdText, out int roomId))
            {
                player.WriteLine("Invalid room ID.");
                return false;
            }

            // Find the original room in all areas
            Room originalRoom = GameState.Instance.Areas.Values
                                        .SelectMany(a => a.Rooms.Values)
                                        .FirstOrDefault(r => r.Id == roomId);

            if (originalRoom == null)
            {
                player.WriteLine($"Room '{roomId}' not found.");
                return false;
            }

            // Clone the room
            Room clonedRoom = originalRoom.CloneWithoutExits(newName);
            clonedRoom.Id = Room.GetNextId(clonedRoom.AreaId);

            // Add the cloned room to the area
            GameState.Instance.Areas[clonedRoom.AreaId].Rooms.Add(clonedRoom.Id, clonedRoom);

            player.WriteLine($"Room '{originalRoom.Name}' copied successfully as '{newName}'. (No exits copied)");

            return true;
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
                case "desc":
                case "description":
                    RoomSetDescription(player, parameters);
                    break;
                case "name":
                    RoomSetName(player, parameters);
                    break;
                case "color":
                    RoomSetColor(player, parameters);
                    break;
                case "create":
                    RoomCreate(player, parameters);
                    break;
                case "show":
                    RoomShow(player, parameters);
                    break;
                case "icon":
                    RoomSetIcon(player, parameters);
                    break;
                default:
                    WriteUsage(player);
                    break;
            }

            return true;
        }

        private static void WriteUsage(Player player)
        {
            player.WriteLine("");
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
                player.WriteLine("");
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

        /*private static void RoomSetDescription(Player player, List<string> parameters)
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
        }*/

        private static void RoomSetName(Player player, List<string> parameters)
        {
            if (parameters.Count < 3)
            {
                player.WriteLine("");
                player.WriteLine(player.GetRoom().Name);
                /*Try something new*/
            }
            else
            {
                /*player.GetRoom().Name = parameters[2];*/
                player.GetRoom().Name = string.Join(" ", parameters.Skip(2));
                player.WriteLine("");
                player.WriteLine("Room name set.");
            }
        }
        
        private static void RoomShow(Player player, List<string> parameters)
        {
            Room r = player.GetRoom();
            player.WriteLine("");
            /*player.WriteLine($"Room name: {r.Name}  Room description: {r.Description}  Room Id: {r.Id} Tags:");*/
            player.WriteLine(
            $"Room name: {r.Name}\n" +
            $"Room description: {r.Description}\n" +
            $"Room Id: {r.Id}\n" +
            "Tags:");
            /*broke up the room show line, because it was crowded*/
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


        private static void RoomSetColor(Player player, List<string> parameters)
        {
            if (parameters.Count < 3)
            {
                player.WriteLine("");
                player.WriteLine(player.GetRoom().MapColor.Replace("[","").Replace("]",""));
            }
            else
            {
                player.GetRoom().MapColor = parameters[2];
                player.WriteLine("");
                player.WriteLine("Room color set.");                
            }
        }

        private static void RoomSetIcon(Player player, List<string> parameters)
        {
            // Show current icon
            if (parameters.Count < 3)
            {
                player.WriteLine("");
                player.WriteLine(player.GetRoom().MapIcon);
                return;
            }

            // Set icon
            player.GetRoom().MapIcon = parameters[2];
            player.WriteLine("");
            player.WriteLine("Room icon set.");
        }

        private static string EscapeMarkup(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("[", "[[").Replace("]", "]]"); // Escape square brackets
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
                player.WriteLine("");
                player.WriteLine(player.GetRoom().Description);
            }
            else
            {
                string desc = string.Join(" ", parameters.Skip(2));
                player.GetRoom().Description = desc;
                player.WriteLine("");
                player.WriteLine("Room description set.");
            }
        }

    }

    /// <summary>
    /// /area command for building and editing areas.
    /// </summary>
    internal class AreaBuilderCommand : ICommand
    {
        public string Name => "/area";
        public IEnumerable<string> Aliases => Array.Empty<string>();

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player) return false;
            if (parameters.Count < 2) { WriteUsage(player); return false; }

            switch (parameters[1].ToLower())
            {
                case "name":
                    AreaSetName(player, parameters);
                    break;

                case "desc":
                case "description":
                    AreaSetDescription(player, parameters);
                    break;

                default:
                    WriteUsage(player);
                    break;
            }

            return true;
        }

        /*private static void WriteUsage(Player player)
        {
            player.WriteLine("");
            player.WriteLine("Usage:");
            player.WriteLine("/area name <new area name>");
        }*/
        private static void WriteUsage(Player player)
        {
            player.WriteLine("");
            player.WriteLine("Usage:");
            player.WriteLine("/area name <new area name>");
            player.WriteLine("/area desc <new area description>");
            player.WriteLine("");
        }

        private static void AreaSetName(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            // Get the area the player is currently in
            Area area = GameState.Instance.Areas[player.AreaId];

            if (parameters.Count < 3)
            {
                player.WriteLine("");
                player.WriteLine(area.Name);
            }
            else
            {
                // Multi-word support
                area.Name = string.Join(" ", parameters.Skip(2));
                player.WriteLine("");
                player.WriteLine("Area name set.");
            }
        }
        private static void AreaSetDescription(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            // Get the area the player is currently in
            Area area = GameState.Instance.Areas[player.AreaId];

            if (parameters.Count < 3)
            {
                player.WriteLine("");
                player.WriteLine(area.Description);
            }
            else
            {
                /*Multi-word support*/
                area.Description = string.Join(" ", parameters.Skip(2));
                player.WriteLine("");
                player.WriteLine("Area description set.");
            }
        }

    }
}

