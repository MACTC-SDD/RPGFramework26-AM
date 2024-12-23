using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGFramework.MapObjects
{
    public class Builder
    {
        public static bool ProcessCommand(Character character, List<string> parameters)
        {
            // It only makes sense for players to use builder commands (for now).
            if (!(character is Player))
                return false;

            Player player = (Player)character;
            
            switch (parameters[0].ToLower())
            {
                case "/room":
                    RoomCommand(player, parameters);
                    return true;
            }

            return false;
        }

        private static void RoomCommand(Player player, List<string> parameters)
        {
            if (parameters.Count < 2)
            {
                // Write help message.
                player.WriteLine("Usage: ");
                player.WriteLine("/room description '<set room desc to this>'");
                player.WriteLine("/room name '<set room name to this>'");
                player.WriteLine("/room create '<name>' '<description>' <exit direction> '<exit description>'");
                return;
            }            

            switch (parameters[1].ToLower())
            {
                // Probably this should be a separate method.
                case "description":
                    RoomSetDescription(player, parameters);
                    break;
                case "name":
                    RoomSetName(player, parameters);
                    break;
                case "create":
                    RoomCreate(player, parameters);
                    break;
            }            
        }

        private static void RoomCreate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            if (parameters.Count < 5)
            {
                player.WriteLine("Usage: /room create '<name>' '<description>' <exit direction> '<exit description>'");
                return;
            }

            Direction exitDirection = Direction.None;
            if (!Enum.TryParse(parameters[4], true, out exitDirection))
            {
                player.WriteLine("Invalid exit direction.");
                return;
            }

            try
            {
                // Create a new room.
                Room room = Room.CreateRoom(player.AreaId, parameters[2], parameters[3]);

                // Create exits to/from the new room.
                player.GetRoom().AddExits(player, exitDirection, parameters[5], room);                
                player.WriteLine("Room created.");
            }
            catch (Exception ex)
            {
                player.WriteLine($"Error creating room: {ex.Message}");
                player.WriteLine(ex.StackTrace);
            }
        }
        /// <summary>
        /// Set description of a room, or just display it if no new description provided.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="parameters"></param>
        private static void RoomSetDescription(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            if (parameters.Count < 3)
            {
                player.WriteLine($"{player.GetRoom().Description}");
            }
            else
            {
                player.GetRoom().Description = parameters[2];
                player.WriteLine("Room description set.");
            }
        }

        private static void RoomSetName(Player player, List<string> parameters)
        {
            if (parameters.Count < 3)
            {
                player.WriteLine($"{player.GetRoom().Name}");
            }
            else
            {
                player.GetRoom().Name = parameters[2];
                player.WriteLine("Room name set.");
            }
        }
    }
}
