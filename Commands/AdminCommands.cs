
using RPGFramework.Display;
using RPGFramework.Enums;
using RPGFramework.Workflows;

namespace RPGFramework.Commands
{
    internal class AdminCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return
            [
                new AnnounceCommand(),
                new ReloadSeedDataCommand(),
                new ShutdownCommand(),
                new GoToCommand(),
                new FindRoomCommand(),
                new FindAreaCommand(),
                // Add more builder commands here as needed
            ];
        }
    }

    internal class AnnounceCommand : ICommand
    {
        public string Name => "announce";
        public IEnumerable<string> Aliases => ["ann"];
        public string Help => "";
        public bool Execute(Character character, List<string> parameters)
        {
            Comm.Broadcast($"{DisplaySettings.AnnouncementColor}[[Announcement]]: [/][white]" +
                $"{string.Join(' ', parameters.Skip(1))}[/]");
            return true;
        }
    }

    #region ReloadSeedDataCommand Class
    internal class ReloadSeedDataCommand : ICommand
    {
        public string Name => "/reloadseeddata";
        public IEnumerable<string> Aliases => [];
        public string Help => "";
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;

            if (Utility.CheckPermission(player, PlayerRole.Admin) == false)
            {
                player.WriteLine("You do not have permission to use this command.");
                return false;
            }

            player.CurrentWorkflow = new WorkflowReloadSeedData();
            player.WriteLine("Watch out, you're about to overwrite your data with the default seed files. If that's what you want, type YES!");
            return true;
        }
    }
    #endregion

    internal class ShutdownCommand : ICommand
    {
        public string Name => "shutdown";
        public IEnumerable<string> Aliases => [];
        public string Help => "";
        public bool Execute(Character character, List<string> parameters)
        {
            Comm.Broadcast($"{DisplaySettings.AnnouncementColor}[[WARNING]]: [/][white]" +
                $"Server is shutting down. All data will be saved.[/]");

            using var _ = GameState.Instance.Stop();
            return true;
        }
    }

    internal class GoToCommand : ICommand
    {
        public string Name => "GoTo";
        public IEnumerable<string> Aliases => new List<string>() { };
        public string Help => "Teleports an admin to a specific room using areaId and roomId.";
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;
            // permissions check
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return true;
            }

            //usage
            if (parameters.Count < 3)
            {
                player.WriteLine("Usage:");
                player.WriteLine("goto <areaId> <roomId>");
                return true;
            }

            // input for <Id> was not a number
            if (!int.TryParse(parameters[1], out int areaID) ||
                !int.TryParse(parameters[2], out int roomId))
            {
                player.WriteLine("AreaID and RoomID must be numbers");
                return true;
            }

            // specified area doesnt exist
            if (!GameState.Instance.Areas.TryGetValue(areaID, out var area))
            {
                player.WriteLine($"Area{areaID} does not exist");
                return true;
            }

            // if the specified room Id doesnt exist in the specified area
            if (!area.Rooms.TryGetValue(roomId, out var room))
            {
                player.WriteLine($"Room{roomId} does not exist in Area {areaID}");
                return true;
            }

            player.AreaId = areaID;
            player.LocationId = roomId;
            //success :)
            player.WriteLine($"you teleport to room {roomId} in {area.Name}");

            return true;

        }
    }

    internal class FindRoomCommand : ICommand
    {
        public string Name => "/findroom";
        public IEnumerable<string> Aliases => new[] { "/findrm" };
        public string Help => "Shows information about the specified room.";

        public bool Execute(Character character, List<string> parameters)
        {
            var player = character as Player;
            if (player == null)
                return false;

            // Get area
            if (!GameState.Instance.Areas.TryGetValue(player.AreaId, out var area))
            {
                player.WriteLine("Area not found.");
                return false;
            }

            // Get room
            if (!area.Rooms.TryGetValue(player.RoomId, out var room))
            {
                player.WriteLine("Room not found.");
                return false;
            }
            player.WriteLine($"Area Id: {area.Id}");
            player.WriteLine($"Room Id: {room.Id}");
            player.WriteLine($"Room name: {room.Name}");
            player.WriteLine($"Room description: {room.Description}");


            // Exits (CORRECT MODEL ACCESS)
            var exits = room.GetExits();

            if (exits != null && exits.Any())
            {
                player.WriteLine($"Exits ({exits.Count()}):");

                foreach (var exit in exits)
                {
                    player.WriteLine(
                        $" - {exit.ExitDirection} -> Room {exit.DestinationRoomId}"
                    );
                }
            }
            else
            {
                player.WriteLine("Exits: None");
            }

            return true;
        }
    }

    internal class FindAreaCommand : ICommand
    {
        public string Name => "findarea";
        public IEnumerable<string> Aliases => new[] { "findar" };
        public string Help => "Shows information about the specified area.";

        public bool Execute(Character character, List<string> parameters)
        {
            var player = character as Player;
            if (player == null)
                return false;

            if (!GameState.Instance.Areas.TryGetValue(player.AreaId, out var area))
            {
                player.WriteLine("Area not found.");
                return false;
            }
            player.WriteLine($"Area Id: {area.Id}");
            player.WriteLine($"Area name: {area.Id}");
            player.WriteLine($"Area description: {area.Description}");
            player.WriteLine($"Rooms ({area.Rooms.Count})");

            foreach (var room in area.Rooms.Values.OrderBy(r => r.Id))
            {
                player.WriteLine($"Room {room.Id}: {room.Name}");
            }

            return true;
        }
    }
}
