
using RPGFramework.Enums;
using RPGFramework.Geography;
using Spectre.Console;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace RPGFramework.Commands
{

    internal class BuilderCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return
            [
                new RoomBuilderCommand(),
                new AreaBuilderCommand(),
                new RoomCopyCommand(),
                new SetExitTypeCommand(),
            ];
        }
    }
    internal class RoomCopyCommand : ICommand
    {
        public string Name => "copy";
        public IEnumerable<string> Aliases => Array.Empty<string>();
        public string Help => "Clone a room without exits";

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
            Room? originalRoom = GameState.Instance.Areas.Values
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
    // CODE REVIEW: Jibril PR #26
    // I added regions to separate the two classes for better readability.
    // You can delete this comment after you've read it.

    #region RoomBuilderCommand
    /// <summary>
    /// /room command for building and editing rooms.
    /// </summary>
        internal class RoomBuilderCommand : ICommand
    {
        public string Name => "/room";

        public string Name2 => "/exit";

        public IEnumerable<string> Aliases2 => Array.Empty<string>();
        public string Help => "";

        public bool Execute2(Character character, List<string> parameters)
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
                    ExitDescription(player, parameters);
                    break;
                default:
                    WriteUsage(player);
                    break;
            }

            return true;
        }

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
                case "tag":
                    RoomTag(player, parameters);
                    break;
                case "spawnable":
                    RoomSpawnable(player, parameters);
                    break;
                default:
                    WriteUsage(player);
                    break;
                case "validate":
                    RoomValidate(player, parameters);
                    break;
                case "exitdescription":
                    ExitDescription(player, parameters);
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
            player.WriteLine("/room show 'Details about the room you are in'");
            player.WriteLine("/room Tag '<add or remove room tags>'");
            player.WriteLine("/room validate <roomId>");
            player.WriteLine("/room spawnable add '<areaid>' '<roomid>' '<mob/npc>' '<name>' '<chance>'");
            player.WriteLine("/room spawnable remove '<areaid>' '<roomid>' '<mob/npc>' '<name>'");
            player.WriteLine("/room spawnable chance '<areaid>' '<roomid>' '<mob/npc>' '<name>' '<chance>'");
            player.WriteLine("/room spawnable list '<areaid>' '<roomid>'");
            player.WriteLine("/room spawn '<areaid>' '<roomid>' '<mob/npc> '<name>''");
            player.WriteLine("/room triggerspawn '<areaid>' '<roomid>'");
            player.WriteLine("/room triggerdeletion '<areaid>' '<roomid>'");
            player.WriteLine("/room kill '<areaid>' '<roomid>' '<mob/npc> ' <name>'");
        }

        private static void RoomValidate(Player player, List<string> parameters)  /*Made changes here*/
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            if (parameters.Count < 3)
            {
                WriteUsage(player);
                return;
            }

            if (!int.TryParse(parameters[2], out int roomId))
            {
                player.WriteLine("Invalid room id.");
                return;
            }

            ValidateRoom(player, roomId);  /*Made changes here*/
        }

        private static void ValidateRoom(Player player, int roomId)
        /*Okay so using ChatGPT, I added HashSet<Direction> directionsUsed to detect duplicate directions because It was recommended and to avoid rooms connected to the same room and creating erriors,
         * I also found a check for one-way exits could be helpful when we begin to world build-Landon*/
        {
            Room? room = null;
            Area? area = null;

            // Find the room in all areas
            foreach (var a in GameState.Instance.Areas.Values)
            {
                if (a.Rooms.TryGetValue(roomId, out room))
                {
                    area = a;
                    break;
                }
            }

            if (room == null || area == null)
            {
                player.WriteLine($"Room {roomId} does not exist.");
                return;
            }

            player.WriteLine($"Validating room {roomId}...");
            bool hasErrors = false;

            /*Track used exit directions to detect duplicates, I don't quite know everything it does I just know using it allows us to track*/
            HashSet<Direction> directionsUsed = new();

            foreach (int exitId in room.ExitIds)
            {
                if (!area.Exits.TryGetValue(exitId, out Exit? exit))
                {
                    player.WriteLine($"Exit ID {exitId} does not exist in this area.");
                    hasErrors = true;
                    continue;
                }

                if (exit.DestinationRoomId <= 0)
                {
                    player.WriteLine($"Exit {exit.Id} has no destination.");
                    hasErrors = true;
                    continue;
                }

                if (!area.Rooms.ContainsKey(exit.DestinationRoomId))
                {
                    player.WriteLine($"Exit {exit.Id} points to invalid room id {exit.DestinationRoomId}.");
                    hasErrors = true;
                }

                // Check for duplicate directions
                if (directionsUsed.Contains(exit.ExitDirection))
                {
                    player.WriteLine($"Duplicate exit direction detected: {exit.ExitDirection}.");
                    hasErrors = true;
                }
                else
                {
                    directionsUsed.Add(exit.ExitDirection);
                }

                // Check for one-way exits
                if (area.Rooms.TryGetValue(exit.DestinationRoomId, out Room? destRoom))
                {
                    bool hasReturn = destRoom.GetExits()
                        .Any(e => e.DestinationRoomId == room.Id);
                    if (!hasReturn)
                    {
                        player.WriteLine($"One-way exit detected: Room {room.Id} -> Room {destRoom.Id} ({exit.ExitDirection})");
                        hasErrors = true;
                    }
                }
            }

            if (!hasErrors)
            {
                player.WriteLine("Room validation passed. No issues found.");
            }
            else
            {
                player.WriteLine("Room validation completed with errors.");
            }
        }
        private static void WriteDeleteUsage(Player player)
        {
            player.WriteLine("Usage:");
            player.WriteLine("/room delete here");
            player.WriteLine("/room delete here confirm");
            player.WriteLine("/room delete <roomId>");
            player.WriteLine("/room delete <roomId> confirm");
        }
        #region RoomCommandMethods

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

                player.WriteLine(message: ex.StackTrace ?? "");
            }
        }


        private bool ExitDescription(Player player, List<string> parameters)
        {

            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /exit setdesc <direction> <description>");
                return false;
            }

            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return false;
            }

            Room room = player.GetRoom();

            string direction = parameters[2].ToLower();

            if (!Enum.TryParse(direction, out Direction targetD))
            {
                player.WriteLine("what direction?");
                return false;
            }
            
            Exit? exit = room.GetExits()
              .Find(e => e.ExitDirection == targetD);

            if (exit == null)
            {
                player.WriteLine($"There is no exit to the {direction}.");
                return false;
            }

            // Everything after the direction is the description, at least I think, I can't directly test it due to permission restrictions that I can't simply comment out
            string description = parameters[3];
            
            exit.Description = description;

            player.WriteLine($"Exit '{direction}' description updated.");
            return true;
        }

        private static void RoomSetColor(Player player, List<string> parameters)
        {
            if (parameters.Count < 3)
            {
                player.WriteLine(player.GetRoom().MapColor.Replace("[", "").Replace("]", ""));
            }
            else
            {
                player.GetRoom().MapColor = parameters[2];
                player.WriteLine("Room color set.");
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
                string desc = string.Join(" ", parameters.Skip(2));
                player.GetRoom().Description = desc;
                player.WriteLine("Room description set.");
            }
        }

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

            Room room = player.GetRoom();

            var exits = room.GetExits();

            if (exits.Count == 0)
            {
                player.WriteLine("There are no exits, forces beyond this realm are at play...");
                return;
            }

            foreach (var exit in exits)
            {
                player.WriteLine($" Room Exit(s): Id: {exit.Id} {exit.ExitDirection}  -> Room {exit.DestinationRoomId} ({exit.Description})");
            }
        }

        private static void RoomTag(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            Room room = player.GetRoom();

            if (parameters.Count < 3)
            {
                player.WriteLine("Options:");
                player.WriteLine("/room tag add (tag you want to add)");
                player.WriteLine("/room tag remove (tag you want to remove");
                return;
            }

            string action = parameters[2].ToLower();
            switch (action)
            {
                case "add":
                    if (parameters.Count < 4)
                    {
                        player.WriteLine("Use: /room tag add <tag>");
                        return;
                    }

                    string tagToAdd = parameters[3].ToLower();



                    if (room.Tags.Contains(tagToAdd))
                    {
                        player.WriteLine($"Room already has tag '{tagToAdd}'.");
                        return;
                    }

                    room.Tags.Add(tagToAdd);
                    player.WriteLine($"Tag '{tagToAdd}' added to room.");
                    break;

                case "remove":
                    if (parameters.Count < 4)
                    {
                        player.WriteLine("Use: /room tag remove <tag>");
                        return;
                    }

                    string tagToRemove = parameters[3].ToLower();

                    if (!room.Tags.Remove(tagToRemove))
                    {
                        player.WriteLine($"Room does not have tag '{tagToRemove}'.");
                        return;
                    }

                    player.WriteLine($"Tag '{tagToRemove}' removed from room.");
                    break;

                case "list":
                    if (room.Tags.Count == 0)
                    {
                        player.WriteLine("This room has no tags.");
                    }
                    else
                    {
                        player.WriteLine("Room tags:");
                        foreach (var tag in room.Tags)
                        {
                            player.WriteLine($" - {tag}");
                        }
                    }
                    break;

                default:
                    player.WriteLine("Invalid tag");
                    break;
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

            Room? roomToDelete = null;

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
        //For npc and mob spawning in rooms - Shelton
        private static void RoomSpawnable(Player player, List<string> parameters)
        {
            int areaId = int.Parse(parameters[3]);
            int roomId = int.Parse(parameters[4]);
            Room room = GameState.Instance.Areas[areaId].Rooms[roomId];
            string type = parameters[5].ToLower();
            if (room == null)
            {
                player.WriteLine("Room not found.");
                return;
            }
            switch (parameters[2].ToLower())
            {
                case "add":
                    int spawnChance = int.Parse(parameters[7]);
                    if (type == "npc")
                    {
                        room.AddToSpawnable(parameters[6], spawnChance, player, type);
                    }
                    else if (type == "mob")
                    {
                        room.AddToSpawnable(parameters[6], spawnChance, player, type);
                    }
                    break;
                case "remove":
                    if (parameters[5].ToLower() == "npc")
                    {
                        room.RemoveFromSpawnable(parameters[6], player, type);
                    }
                    else if (parameters[5].ToLower() == "mob")
                    {
                        room.RemoveFromSpawnable(parameters[6], player, type);
                    }
                    break;
                case "chance":
                    room.ModifyChance(parameters[6], player, type, int.Parse(parameters[7]));
                    break;
                case "list":
                    room.ListSpawnables(player, type);
                    break;
                case "spawn":
                    areaId = int.Parse(parameters[2]);
                    roomId = int.Parse(parameters[3]);
                    room = GameState.Instance.Areas[areaId].Rooms[roomId];
                    if (parameters[4].ToLower() == "mob")
                    {
                        if (GameState.Instance.MobCatalog.ContainsKey(parameters[5]))
                        {
                            room.SpawnMob(parameters[5]);
                        }
                        else
                        {
                            player.WriteLine("Mob does not exist!");
                        }
                    }
                    else if (parameters[4].ToLower() == "npc")
                    {
                        if (GameState.Instance.NPCCatalog.ContainsKey(parameters[5]))
                        {
                            room.SpawnNpc(parameters[5]);
                        }
                        else
                        {
                            player.WriteLine("Npc does not exist!");
                        }
                    }
                        break;
                case "triggerspawn":
                    areaId = int.Parse(parameters[2]);
                    roomId = int.Parse(parameters[3]);
                    room = GameState.Instance.Areas[areaId].Rooms[roomId];
                    room.SpawnNpcsInRoom();
                    break;
                case "triggerdeletion":
                    areaId = int.Parse(parameters[2]);
                    roomId = int.Parse(parameters[3]);
                    room = GameState.Instance.Areas[areaId].Rooms[roomId];
                    room.DespawnEntitiesInRoom();
                    break;
                case "kill":
                    areaId = int.Parse(parameters[2]);
                    roomId = int.Parse(parameters[3]);
                    room = GameState.Instance.Areas[areaId].Rooms[roomId];
                    room.DespawnEntity(parameters[5], parameters[4]);
                    break;
                default:
                    player.WriteLine("Invalid spawnable command.");
                    return;
            }
        }
    }
    #endregion
    #endregion
        #region AreaBuilderCommand
    internal class AreaBuilderCommand : ICommand
    {
        public string Name => "/area";

        public IEnumerable<string> Aliases => [];
        public string Help => "";

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

                case "delete":
                    AreaDelete(player, parameters);
                    break;

                case "validate":
                    AreaValidate(player, parameters);
                    break;

                default:
                    WriteAreaUsage(player);
                    break;

                case "name":
                    AreaSetName(player, parameters);
                    break;

                case "desc":
                case "description":
                    AreaSetDescription(player, parameters);
                    break;
            }
            return true;
        }

        private static void WriteAreaUsage(Player player)
        {
            player.WriteLine("Usage:");
            player.WriteLine("/area create '<name>' '<description>'");
            player.WriteLine("/area show");
            player.WriteLine("/area delete <areaId> confirm");
            player.WriteLine("");
            player.WriteLine("Usage:");
            player.WriteLine("/area name <new area name>");
            player.WriteLine("/area desc <new area description>");
            player.WriteLine("");
        }

        private static void AreaValidate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            if (parameters.Count < 3)
            {
                WriteAreaUsage(player);
                return;
            }

            if (!int.TryParse(parameters[2], out int areaId))
            {
                player.WriteLine("Invalid area id.");
                return;
            }

            if (!GameState.Instance.Areas.TryGetValue(areaId, out Area? area))
            {
                player.WriteLine($"Area {areaId} does not exist.");
                return;
            }

            ValidateArea(player, area);
        }

        private static void ValidateArea(Player player, Area area)
        {
            player.WriteLine($"Validating area {area.Id}...");
            bool hasErrors = false;

            foreach (Room room in area.Rooms.Values)
            {
                player.WriteLine($" Checking room {room.Id} ({room.Name})");

                foreach (int exitId in room.ExitIds)
                {
                    if (!area.Exits.TryGetValue(exitId, out Exit? exit))
                    {
                        player.WriteLine($"   Exit ID {exitId} does not exist in this area.");
                        hasErrors = true;
                        continue;
                    }

                    if (exit.DestinationRoomId <= 0)
                    {
                        player.WriteLine($"   Exit {exit.Id} has no destination.");
                        hasErrors = true;
                        continue;
                    }

                    if (!area.Rooms.ContainsKey(exit.DestinationRoomId))
                    {
                        player.WriteLine(
                            $"   Exit {exit.Id} points to invalid room id {exit.DestinationRoomId}."
                        );
                        hasErrors = true;
                    }
                }
            }

            if (!hasErrors)
            {
                player.WriteLine(" Area validation passed. No issues found.");
            }
            else
            {
                player.WriteLine(" Area validation completed with errors.");
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
                player.WriteLine("");
                player.WriteLine(player.GetRoom().Description);
                WriteAreaUsage(player);
                return;
            }

            if (!GameState.Instance.Areas.TryGetValue(areaId, out var area))
            {
                string desc = string.Join(" ", parameters.Skip(2));
                player.GetRoom().Description = desc;
                player.WriteLine("");
                player.WriteLine("Room description set.");
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

//      ---------------------------------------------------------------------------------
        
// ---------------------------------------------------------------------------------------

        private static void CreateArea(Player player, List<string> parameters)
        {
            // 0: /area
            // 1: create
            // 2: name
            // 3: description
            if (parameters.Count < 4)
            {
                player.WriteLine("");
                player.WriteLine(player.GetRoom().MapColor.Replace("[", "").Replace("]", ""));
                WriteAreaUsage(player);
                return;
            }

            int newAreaId = GameState.Instance.Areas.Count == 0
                ? 0
                : GameState.Instance.Areas.Keys.Max() + 1;

            Area area = new()
            {
                Id = newAreaId,
                Name = parameters[2],
                Description = parameters[3],
                Rooms = [],
                Exits = []
            };

            // Create starting room
            GameState.Instance.Areas.Add(area.Id, area);

            Room startRoom = Room.CreateRoom(area.Id, "Start Room", "You are in a newly created area.");

            // Move builder into new area
            player.AreaId = area.Id;
            player.LocationId = startRoom.Id;

            player.WriteLine($"Area '{area.Name}' created (ID {area.Id}).");
        }



        private static string EscapeMarkup(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("[", "[[").Replace("]", "]]"); // Escape square brackets
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

    // CODE REVIEW: BUILD TEAM (Landon, Jibril)
    // There appear to be two AreaBuilderCommand classes in this file.
    // I'm not sure which one is intended to be used, or maybe pieces of both.
    // These merged really strangely and I tried to correct it, but please review and adjust as needed.
    // I renamed the second one to AreaBuilderCommand2 to avoid conflicts.
    /// <summary>
    /// /area command for building and editing areas.
    /// </summary>
 //   internal class AreaBuilderCommand2 : ICommand
  //  {
//        public string Name => "/area";
//        public IEnumerable<string> Aliases => Array.Empty<string>();
 //       public string Help => "";

   //     public bool Execute(Character character, List<string> parameters)
    //    {
  //          if (character is not Player player) return false;
   //         if (parameters.Count < 2) { WriteUsage(player); return false; }

  //          switch (parameters[1].ToLower())
  //          {
  //              case "name":
  //                  AreaSetName(player, parameters);
  //                  break;

  //              case "desc":
  //            case "description":
  //                  AreaSetDescription(player, parameters);
  //                  break;

  //              default:
  //                  WriteUsage(player);
  //                  break;
  //          }

  //          return true;
  //      }

  //      private static void WriteUsage(Player player)
   //     {
  //          player.WriteLine("");
  //          player.WriteLine("Usage:");
   //         player.WriteLine("/area name <new area name>");
  //          player.WriteLine("/area desc <new area description>");
   //         player.WriteLine("");
  //      }

   //     private static void AreaSetName(Player player, List<string> parameters)
     //   {
   //         if (!Utility.CheckPermission(player, PlayerRole.Admin))
        //    {
   //             player.WriteLine("You do not have permission to do that.");
      //          return;
       //     }

            // Get the area the player is currently in
   //         Area area = GameState.Instance.Areas[player.AreaId];

     //       if (parameters.Count < 3)
       //     {
     //           player.WriteLine("");
       //         player.WriteLine(area.Name);
    //        }
      //      else
        //    {
        //        // Multi-word support
          //      area.Name = string.Join(" ", parameters.Skip(2));
          //      player.WriteLine("");
 //               player.WriteLine("Area name set.");
         //   }
   //     }

      //  private static void AreaSetDescription(Player player, List<string> parameters)
      //  {
     //       if (!Utility.CheckPermission(player, PlayerRole.Admin))
         //   {
      //          player.WriteLine("You do not have permission to do that.");
        //        return;
         //   }

            // Get the area the player is currently in
         //   Area area = GameState.Instance.Areas[player.AreaId];

      //      if (parameters.Count < 3)
         //   {
         //       player.WriteLine("");
         //       player.WriteLine(area.Description);
         //   }
        //    else
       //     {
                /*Multi-word support*/
           //     area.Description = string.Join(" ", parameters.Skip(2));
          //      player.WriteLine("");
           //     player.WriteLine("Area description set.");
         //   }
       // }

   // }
    #endregion
}

