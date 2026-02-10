
using static RPGFramework.Commands.TimeCommand;
using RPGFramework.Geography;
using RPGFramework.Display;
using Spectre.Console;

namespace RPGFramework.Commands
{
    /// <summary>
    /// Provides access to the set of built-in core command implementations.
    /// </summary>
    /// <remarks>The <c>CoreCommands</c> class exposes static methods for retrieving all available core
    /// commands. These commands represent fundamental operations supported by the system </remarks>
    internal class CoreCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                new AFKCommand(),
                new AreaShowCommand(),
                new IpCommand(),
                new LookCommand(),
                new QuitCommand(),
                new SayCommand(),
                new TimeCommand(),
                new AreaShowCommand(),
                new RoomShowCommand(),
                new WhoCommand(),
                // Add other core commands here as they are implemented
            };
        }


    }

    internal class AFKCommand : ICommand
    {
        public string Name => "afk";
        public IEnumerable<string> Aliases => new List<string> { };
        public string Help => "";
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is Player player)
            {
                player.IsAFK = !player.IsAFK;
                player.WriteLine($"You are now {(player.IsAFK ? "AFK" : "no longer AFK")}.");
                return true;
            }
            return false;
        }
    }

    internal class IpCommand : ICommand
    {
        public string Name => "ip";
        public IEnumerable<string> Aliases => new List<string> { };
        public string Help => "";
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is Player player)
            {
                player.WriteLine($"Your IP address is {player.GetIPAddress()}");
                return true;
            }
            return false;
        }
    }

    internal class LookCommand : ICommand
    {
        public string Name => "look";
        public IEnumerable<string> Aliases => new List<string> { "l" };
        public string Help => "";
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player) return false;
            Room room = player.GetRoom();

            player.WriteLine($"[bold white]{room.Name}[/]");
            player.WriteLine(room.Description);

            if (room.Items.Count > 0)
            {
                foreach (var item in room.Items)
                {
                    if (!string.IsNullOrWhiteSpace(item.Name))
                    {
                        string showText = item.DisplayText == "" ? item.Name : item.DisplayText;
                        player.WriteLine($"[yellow]{showText}[/]");
                    }
                }
            }
            // For now, we'll ignore the command and just show the room description

            string content = "[red]Exits[/]\n";
            string title = " ";

            foreach (var exit in player.GetRoom().GetExits())
            {
                content += $"[Salmon1]{exit.Description} to the {exit.ExitDirection}[/]\n";
            }

            Panel panel = RPGPanel.GetPanel(content, title);

            panel.Border = BoxBorder.Ascii;
            panel.BorderColor(Color.Maroon);
            player.Write(panel);
            return true;
        }
    }

    internal class QuitCommand : ICommand
    {
        public string Name => "quit";
        public IEnumerable<string> Aliases => new List<string> { "exit" };
        public string Help => "";

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is Player player)
            {
                player.Logout();
                return true;
            }
            return false;
        }
    }

    internal class SayCommand : ICommand
    {
        public string Name => "say";
        public IEnumerable<string> Aliases => new List<string> { "\"", "'" };
        public string Help => "";
        public bool Execute(Character character, List<string> parameters)
        {
            // If no message and it's a player, tell them to say something
            if (parameters.Count < 2 && character is Player player)
            {
                player.WriteLine("Say what?");
                return true;
            }
            Comm.RoomSay(character.GetRoom(), parameters[1], character);
            return true;
        }
    }

    internal class TimeCommand : ICommand
    {
        public string Name => "time";
        public IEnumerable<string> Aliases => new List<string> { };
        public string Help => "";
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is Player player)
            {
                player.WriteLine($"The time is {GameState.Instance.GameDate.ToShortTimeString()}");
                return true;
            }
            return false;
        }
    }

    // CODE REVIEW: Jibril PR #48 - You needed that extra using at the top because this was nested under
    // the TimeCommand. It just needed to be moved outside of it.
    internal class AreaShowCommand : ICommand
    {
        public string Name => "areashow";
        public IEnumerable<string> Aliases => new[] { "arshow", "areainfo", "arinfo" };
        public string Help => "Shows information about the current area.";

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

            player.WriteLine($"Area name: {area.Name}");
            player.WriteLine($"Area description: {area.Description}");
            player.WriteLine($"Area Id: {area.Id}");

            return true;

        }
    }


    internal class RoomShowCommand : ICommand
    {
        public string Name => "roomshow";
        public IEnumerable<string> Aliases => new[] { "rmshow", "roominfo", "rminfo" };
        public string Help => "Shows info for the current room and its available exits.";

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

            if (!area.Rooms.TryGetValue(player.LocationId, out var room))
            {
                player.WriteLine("Room not found.");
                return false;
            }

            player.WriteLine($"Room name: {room.Name}");
            player.WriteLine($"Room description: {room.Description}");
            player.WriteLine($"Room Id: {room.Id}");
            player.WriteLine("Exits:");

            foreach (var exit in room.GetExits())
            {
                player.WriteLine($"{exit.ExitDirection}: {exit.Description}");
            }


            foreach (Room r in area.Rooms.Values.OrderBy(r => r.Id))
            {
                player.WriteLine($"Room {r.Id}: {r.Name}");
            }

            return true;
        }
    }

    internal class WhoCommand : ICommand
    {
        public string Name => "who";
        public IEnumerable<string> Aliases => [];
        public string Help => "See who is online.";
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;

            var onlinePlayers = Player.GetOnlinePlayers(GameState.Instance.Players);

            Table table = new Spectre.Console.Table();
            table.AddColumn(new TableColumn("Name"));
            table.AddColumn(new TableColumn("Last Login"));

            foreach (Player p in onlinePlayers)
            {
                table.AddRow(p.Name, p.LastLogin.ToString("g"));
            }

            player.Write(table);
            return true;
        }
    }
}


