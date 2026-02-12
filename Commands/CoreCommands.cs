
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
            if (character is Player player)
            {
                // For now, we'll ignore the command and just show the room description
                player.WriteLine($"{player.GetRoom().Description}");
                player.WriteLine("Exits:");
                foreach (var exit in player.GetRoom().GetExits())
                {
                    player.WriteLine($"{exit.Description} to the {exit.ExitDirection}");
                }

                player.WriteLine("\n\nMobs:");
                foreach (Mob m in player.GetRoom().Mobs)
                {
                    player.WriteLine($"{m.Name} ");
                }
                return true;
            }
            return false;
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


}
