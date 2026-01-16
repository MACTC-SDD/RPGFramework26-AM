using RPGFramework.Display;
using RPGFramework.Enums;

namespace RPGFramework.Geography
{
    internal class Room
    {
        #region --- Properties ---
        // Unique identifier for the room
        public int Id { get; set; } = 0;

        // What area this belongs to 
        public int AreaId { get; set; } = 0;

        // Description of the room
        public string Description { get; set; } = "";

        // Icon to display on map
        public string MapIcon { get; set; } = DisplaySettings.RoomMapIcon;
        public string MapColor { get; set; } = DisplaySettings.RoomMapIconColor;

        // Name of the room
        public string Name { get; set; } = "";
       
        public List<string> Tags { get; set; } = new List<string>(); // (for scripting or special behavior)

        // List of exits from the room
        public List<int> ExitIds { get; set; } = new List<int>();
        #endregion --- Properties ---

        #region --- Methods ---
        /// <summary>
        /// This is for creating a new exit (and return exit), not linking existing exit items.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="exitDescription"></param>
        /// <param name="destinationRoom"></param>
        /// <param name="returnExit"></param>
        public void AddExits(Player player, Direction direction, string exitDescription, Room destinationRoom, bool returnExit = true)
        {
            // Make sure there isn't already an exit in the specified direction from this room

            if (GetExits().Any(e => e.ExitDirection == direction))
            {
                player.WriteLine("There is already an exit going that direction.");
                return;
            }

            // Make sure the destination room doesn't already have an exit in the opposite direction
            if (returnExit 
                && destinationRoom.GetExits().Any(e => e.ExitDirection == Navigation.GetOppositeDirection(direction)))
            {
                player.WriteLine("The destination room already has an exit coming from the opposite direction.");
                return;
            }

            // Create a new Exit object from this room
            Exit exit = new Exit();
            exit.Id = Exit.GetNextId(AreaId);
            exit.SourceRoomId = Id;
            exit.DestinationRoomId = destinationRoom.Id;
            exit.ExitDirection = direction;
            exit.Description = exitDescription;
            ExitIds.Add(exit.Id);
            GameState.Instance.Areas[AreaId].Exits.Add(exit.Id, exit);

            // Create a new exit from the destination room back to this room
            if (returnExit)
            {
                Exit exit1 = new Exit();
                exit1.Id = Exit.GetNextId(destinationRoom.AreaId);
                exit1.SourceRoomId = destinationRoom.Id;
                exit1.DestinationRoomId = Id;
                exit1.ExitDirection = Navigation.GetOppositeDirection(direction);
                exit1.Description = exitDescription.Replace(direction.ToString(), exit1.ExitDirection.ToString());
                destinationRoom.ExitIds.Add(exit1.Id);
                GameState.Instance.Areas[destinationRoom.AreaId].Exits.Add(exit1.Id, exit1);
            }
        }

        /// <summary>
        /// Create a new room object in specified area and add it to GameState Area
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Room CreateRoom(int areaId, string name, string description)
        {
            Room room = new Room();
            room.Id = GetNextId(areaId);
            room.Name = name;
            room.Description = description;
            GameState.Instance.Areas[areaId].Rooms.Add(room.Id, room);

            return room;
        }

        public static Room CreateRoom(Area area, string name, string description)
        {
            return CreateRoom(area.Id, name, description);
        }

        /// <summary>
        /// Delete a room (and its linked exits) from the specified area
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="roomId"></param>
        public static void DeleteRoom(int areaId, int roomId)
        {
            // Remove the room from the area
            GameState.Instance.Areas[areaId].Rooms.Remove(roomId);

            // Remove all exits from the room
            List<Exit> exits = GameState.Instance.Areas[areaId].Exits.Values
                .Where(e => e.SourceRoomId == roomId || e.DestinationRoomId == roomId).ToList();

            foreach (Exit e in exits)
            {
                GameState.Instance.Areas[areaId].Exits.Remove(e.Id);
            }
        }

        public static void DeleteRoom(Room room)
        {
            DeleteRoom(room.AreaId, room.Id);
        }

        /// <summary>
        /// Return a list of Exit objects that are in this room.
        /// </summary>
        /// <returns></returns>
        public List<Exit> GetExits()
        {
            // This works just like the loop in GetPlaysersInRoom, but is shorter
            // This style of list maniuplation is called "LINQ"
            return GameState.Instance.Areas[AreaId].Exits.Values
                .Where(e => e.SourceRoomId == Id).ToList();
        }

        /// <summary>
        /// Returns the first exit in this room that matches the specified name.
        /// </summary>
        /// <returns></returns>
        public Exit? GetExitByName(string name)
        {
            return GetExits().Find(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns the first exit in this room that matches the specified id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Exit? GetExitById(int id)
        {
            return GetExits().Find(e => e.Id == id);
        }

        /// <summary>
        /// Get the next available room ID for the specified area.
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public static int GetNextId(int areaId)
        {
            if (GameState.Instance.Areas[areaId].Rooms.Count == 0)
            {
                return 0;
            }

            return GameState.Instance.Areas[areaId].Rooms.Keys.Max() + 1;
        }

        /// <summary>
        /// Return a list of Player objects that are in this room.
        /// </summary>
        /// <note>
        /// We have both an instance method (GetPlayers) and a static method (GetPlayersInRoom) that do the same thing.
        /// </note>
        /// <returns></returns>
        public List<Player> GetPlayers()
        {
            return GetPlayersInRoom(this);
        }

        /// <summary>
        /// Return a list of player objects that are in the specified room
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<Player> GetPlayersInRoom(Room room)
        {
            // Loop through GameState.ConnectedPlayers and return a list of players in the room
            List<Player> playersInRoom = new List<Player>();
            foreach (Player p in GameState.Instance.Players.Values)
            {
                if (p.IsOnline 
                    && p.AreaId == room.AreaId 
                    && p.LocationId == room.Id)
                {
                    playersInRoom.Add(p);
                }
            }

            return playersInRoom;
        }
        #endregion --- Methods ---

        #region --- Methods (Events) ---
        
        /// <summary>
        /// When a character enters a room, do this.
        /// </summary>
        /// <param name="character"></param>
        public void EnterRoom(Character character, Room fromRoom)
        {
            // Send a message to the player
            if (character is Player) ((Player)character).WriteLine(Description);

            // Send a message to all players in the room
            Comm.SendToRoomExcept(this, $"{character.Name} enters the room.", character);
        }

        /// <summary>
        /// When a character leaves a room, do this.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="toRoom"></param>
        public void LeaveRoom(Character character, Room toRoom)
        {
           // Send a message to all players in the room
            Comm.SendToRoomExcept(this, $"{character.Name} leaves the room.", character);
        }
        #endregion
    }
}
