
using RPGFramework.Enums;
using RPGFramework.Geography;

namespace RPGFramework.Commands
{
    internal class NavigationCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                new MapCommand(),
                new MoveCommand(),
                // Add more navigation commands here as needed
            };
        }
    }

    /// <summary>
    /// Represents a command that displays the local map to a player character.
    /// </summary>
    /// <remarks>The <see cref="MapCommand"/> is typically invoked by player characters to view their
    /// immediate surroundings. This command is not applicable to non-player characters.</remarks>
    internal class MapCommand : ICommand
    {
        public string Name => "map";

        public IEnumerable<string> Aliases => new List<string> { };

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is Player p)
            {
                MapRenderer.RenderLocalMap(p, p.MapRadius);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Move a character (player or NPC) in a direction if possible.
    /// </summary>
    internal class MoveCommand : ICommand
    {
        public string Name => "move";
        public IEnumerable<string> Aliases => new List<string> 
        {
            "n", "north",
            "e", "east",
            "s", "south",
            "w", "west",
            "u", "up",
            "d", "down"      
        };

        public bool Execute(Character character, List<string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                if (character is Player player)
                {
                    player.WriteLine("Usage: move <direction>");
                }
                return false;
            }

            // Get direction from last parameter (in case they used 'move north' or just 'north')
            string directionStr = parameters[parameters.Count-1].ToLower();
            Direction? direction = directionStr switch
            {
                "n" or "north" => Direction.North,
                "e" or "east" => Direction.East,
                "s" or "south" => Direction.South,
                "w" or "west" => Direction.West,
                "u" or "up" => Direction.Up,
                "d" or "down" => Direction.Down,
                _ => null
            };
            if (direction.HasValue)
            {
                Navigation.Move(character, direction.Value);
                return true;
            }
            else if (character is Player player)
            {
                player.WriteLine("Invalid direction. Valid directions are: north, east, south, west, up, down.");
            }
            return false;
        }
    }
}
