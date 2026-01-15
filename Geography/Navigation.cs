using RPGFramework.Enums;

namespace RPGFramework.Geography
{
    /// <summary>
    /// Primarily respoonsible for handling move commands (n, e, s, w, u, d)
    /// </summary>
    internal class Navigation
    {
        private object player;

        public object RequiredKeyId { get; private set; }
        public ExitType ExitType { get; private set; }

        /// <summary>
        /// Move the character in the specified direction if possible, otherwise, send error.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="direction"></param>
        public static void Move(Character character, Direction direction)
        {
            Room currentRoom = character.GetRoom();
            Exit exit = currentRoom.GetExits().FirstOrDefault(e => e.ExitDirection == direction);

            // If invalid exit, send error message (if player)
            if (exit == null)
            {
                if (character is Player)
                {
                    Player p = (Player)character;
                    p.WriteLine("You can't go that way.");
                }
                return;
            }
        }

        public ExitResult TryUse(player)
        {
            switch (ExitType)
            {
                case ExitType.Open:
                case ExitType.Door:
                    return ExitResult.Success;

                case ExitType.LockedDoor:
                    if (RequiredKeyId != null && player.HasItem(RequiredKeyId))
                    {
                        ExitType = ExitType.Door; // permanently unlock
                        return ExitResult.Success;
                    }
                    return ExitResult.Locked;

                case ExitType.Impassable:
                    return ExitResult.Blocked;

                default:
                    return ExitResult.Blocked;
            }
        }
        

        public static Direction GetOppositeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Direction.South;
                case Direction.South:
                    return Direction.North;
                case Direction.East:
                    return Direction.West;
                case Direction.West:
                    return Direction.East;
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                default:
                    return Direction.None;
            }
        }
    }

    public class player
    {
    }

    internal enum ExitResult
    {
        Blocked,
        Locked,
        Success,
        MissingKey
    }
}
