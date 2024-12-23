using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGFramework.MapObjects
{
    /// <summary>
    /// Primarily respoonsible for handling move commands (n, e, s, w, u, d)
    /// </summary>
    public class Navigation
    {
        /// <summary>
        /// We'll use Character instead of Player so that we can use the same commands for NPCs.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="command"></param>
        /// <returns>Returns true if a command was matched.</returns>
        public static bool ProcessCommand(Character character, List<string> parameters)
        {            
            switch (parameters[0].ToLower())
            {
                case "n":
                case "north":
                    Move(character, Direction.North);
                    return true;
                case "e":
                case "east":
                    Move(character, Direction.East);
                    return true;
                case "s":
                case "south":
                    Move(character, Direction.South);
                    return true;
                case "w":
                case "west":
                    Move(character, Direction.West);
                    return true;
                case "u":
                case "up":
                    Move(character, Direction.Up);
                    return true;
                case "d":
                case "down":
                    Move(character, Direction.Down);
                    return true;
            }

            return false;
        }

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
