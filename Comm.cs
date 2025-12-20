using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGFramework.Geography;

namespace RPGFramework
{
    // A centralized placed to put common communication methods
    internal class Comm
    {
        /// <summary>
        /// Send a message to all connected players.
        /// TODO: This should check that player has sufficient permissions.
        /// </summary>
        /// <param name="message"></param>
        public static void Broadcast(string message)
        {
            // Loop through all online players
            foreach (Player p in GameState.Instance.Players.Values.Where(p => p.IsOnline))
            {
                p.WriteLine(message);
            }
        }
    
        /// <summary>
        /// Send a message to all players in a room.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="message"></param>
        public static void SendToRoom(Room room, string message)
        {
            foreach (Player player in Room.GetPlayersInRoom(room))
            {
                player.WriteLine(message);
            }
        }

        /// <summary>
        /// Send a message to all players in a room except one.
        /// To simplify, we'll just use the Character class so that when NPCs are added, we can use the same method.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="message"></param>
        /// <param name="except"></param>
        public static void SendToRoomExcept(Room room, string message, Character except)
        {
            foreach (Player player in Room.GetPlayersInRoom(room))
            {
                if (except is Player && player != except)
                {
                    player.WriteLine(message);
                }
            }
        }

        /// <summary>
        /// Say something in a room.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="message"></param>
        /// <param name="speaker"></param>
        public static void RoomSay(Room room, string message, Character speaker)
        {
            SendToRoomExcept(room, message, speaker);

            if (speaker is Player) ((Player)speaker).WriteLine($"You say, '{message}'");                           
        }
    }
}
