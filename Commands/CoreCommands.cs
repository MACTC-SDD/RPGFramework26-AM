
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
                new IpCommand(),
                new LookCommand(),
                new QuitCommand(),
                new SayCommand(),
                new TimeCommand(),
                new AreaShowCommand(),
                new StatsCommand(),
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
                        player.WriteLine($"[yellow]{item.DisplayText}[/]");
                    }
                }
                // For now, we'll ignore the command and just show the room description

                player.WriteLine($"{player.GetRoom().Description}");


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

            player.WriteLine($"Area name: {area.Id}");
            player.WriteLine($"Area description: {area.Description}");
            player.WriteLine($"Area Id: {area.Id}");
            player.WriteLine($"Rooms ({area.Rooms.Count})");

            foreach (var room in area.Rooms.Values.OrderBy(r => r.Id))
            {
                player.WriteLine($"Room {room.Id}: {room.Name}");
            }

            return true;
        }
    }

    internal class StatsCommand : ICommand
    {
        public string Name => "stats";
        public IEnumerable<string> Aliases => [];
        public string Help => "Shows information about the current player stats.";

        public bool Execute(Character character, List<string> parameters)
        {
            var player = character as Player;
            if (player == null)
                return false;
            if(parameters.Count > 3 || parameters.Count < 2)
            {
                WriteUsage(player);
                return false;
            }
            switch (parameters[1].ToLower())
            {
                case "atributes":
                    ShowStats(player);
                    break;
                case "level":
                    ShowLevelInformation(player);
                    break;
                case "equipment":
                    ShowEquipment(player);
                    break;
                case "desc":
                    ShowBasicInfo(player);
                    break;
            }

            return true;
        }

        public static void WriteUsage(Player player)
        {
            player.WriteLine("stats desc");
            player.WriteLine("stats atributes");
            player.WriteLine("stats level");
            player.WriteLine("stats equipment");
        }

        public static void ShowBasicInfo(Player player)
        {
            player.WriteLine($"Name: {player.DisplayName()}");
            player.WriteLine($"Description: {player.Description}");
            player.WriteLine($"Role: {player.Role}");
        }
        public static void ShowStats(Player player)
        {
            player.WriteLine($"Health: {player.Health}/{player.MaxHealth}");
            player.WriteLine($"Strength: {player.GetStrength()}");
            player.WriteLine($"Agility: {player.GetStrength()}");
            player.WriteLine($"Intellect: {player.GetIntelligence()}");
            player.WriteLine($"Wisdom: {player.GetWisdom()}");
            player.WriteLine($"Charisma: {player.GetCharisma()}");
            player.WriteLine($"Constitution: {player.GetConstitution()}");
        }

        public static void ShowLevelInformation(Player player)
        {
            player.WriteLine($"Level: {player.Level}");
            player.WriteLine($"XP: {player.XP}");
            player.WriteLine($"XP to next level: {player.GetXPtoNextLevel()}");
        }

        public static void ShowEquipment(Player player)
        {
            player.WriteLine($"Primary Weapon: {player.PrimaryWeapon.Name}");
            player.WriteLine("Equipped Armor:");
            foreach (var armor in player.EquippedArmor)
            {
                player.WriteLine($"- {armor.Name} ({armor.Slot})");
            }
        }
    }
}
