using RPGFramework.Enums;

namespace RPGFramework.Geography
{
    /// <summary>
    /// Primarily respoonsible for handling move commands (n, e, s, w, u, d)
    /// </summary>
    internal class Navigation
    {

        /// <summary>
        /// Move the character in the specified direction if possible, otherwise, send error.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="direction"></param>
        public static void Move(Character character, Direction direction)
        {
            Room currentRoom = character.GetRoom();
            Exit? exit = currentRoom.GetExits().FirstOrDefault(e => e.ExitDirection == direction);

            // If invalid exit, send error message (if player)
            if (exit == null)
            {
                Comm.SendToIfPlayer(character, "You can't go that way.");
                return;
            }

            Room destinationRoom = GameState.Instance.Areas[character.AreaId].Rooms[exit.DestinationRoomId];

            currentRoom.LeaveRoom(character, destinationRoom);
            destinationRoom.EnterRoom(character, currentRoom);
            
            character.AreaId = destinationRoom.AreaId;
            character.LocationId = exit.DestinationRoomId;
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
}
