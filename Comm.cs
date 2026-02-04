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
            SendToRoomExcept(room, message, null);
        }

        /// <summary>
        /// Send a message to all players in a room except one.
        /// To simplify, we'll just use the Character class so that when NPCs are added, we can use the same method.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="message"></param>
        /// <param name="except"></param>
        public static void SendToRoomExcept(Room room, string message, Character? except)
        {
            foreach (Player player in Room.GetPlayersInRoom(room))
            {
                if (except == null || player != except)
                {
                    player.WriteLine(message);
                }
            }
            foreach (NonPlayer npc in room.Npcs)
            {
                if (except == null || npc != except)
                {
                    npc.CheckPlayerDialogue(message);
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
            SendToRoomExcept(room, $"{speaker.Name} says '{message}'", speaker);

            Comm.SendToIfPlayer(speaker, $"You say, '{message}'");                         
        }

        /// <summary>
        /// Sends the specified message to the character if it is a player.
        /// </summary>
        /// <param name="character">The character to which the message may be sent. 
        /// If the character is a player, the message will be delivered;
        /// otherwise, no action is taken.</param>
        /// <param name="message">The message to send to the player character. Cannot be null.</param>
        /// <returns>true if the message was sent to a player; otherwise, false.</returns>
        public static bool SendToIfPlayer(Character character, string message)
        {
            if (character is Player player)
            {
                player.WriteLine(message);
                return true;
            }
            return false;
        }
    }
}
