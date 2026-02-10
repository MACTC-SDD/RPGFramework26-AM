using RPGFramework.Display;
using RPGFramework.Enums;
using System.Security.Cryptography.X509Certificates;

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
        //Items in the room
        public List<Item> Items { get; set; } = [];

        // Icon to display on map
        public string MapIcon { get; set; } = DisplaySettings.RoomMapIcon;
        public string MapColor { get; set; } = DisplaySettings.RoomMapIconColor;

        // Name of the room
        public string Name { get; set; } = "";

        public Dictionary<string, int> SpawnableMobs { get; set; } = new Dictionary<string, int>();
        public List<Mob> Mobs { get; set; } = [];
        public Dictionary<string, int> SpawnableNpcs { get; set; } = new Dictionary<string, int>();
        public List<NonPlayer> Npcs{ get; set; } = [];
        public int MaxSpawnedAllowed { get; set; } = 3;
        public List<string> Tags { get; set; } = []; // (for scripting or special behavior)
        // List of exits from the room
        public List<int> ExitIds { get; set; } = [];
        public object Exits { get; internal set; }

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
            Exit exit = new()
            {
                Id = Exit.GetNextId(AreaId),
                SourceRoomId = Id,
                DestinationRoomId = destinationRoom.Id,
                ExitDirection = direction,
                Description = exitDescription
            };
            ExitIds.Add(exit.Id);
            GameState.Instance.Areas[AreaId].Exits.Add(exit.Id, exit);

            // Create a new exit from the destination room back to this room
            if (returnExit)
            {
                Exit exit1 = new()
                {
                    Id = Exit.GetNextId(destinationRoom.AreaId),
                    SourceRoomId = destinationRoom.Id,
                    DestinationRoomId = Id,
                    ExitDirection = Navigation.GetOppositeDirection(direction)
                };
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
            Room room = new()
            {
                Id = GetNextId(areaId),
                Name = name,
                Description = description
            };
            GameState.Instance.Areas[areaId].Rooms.Add(room.Id, room);

            return room;
        }

        public static Room CreateRoom(Area area, string name, string description)
        {
            return CreateRoom(area.Id, name, description);
        }

        /// <summary>
        /// Create a copy of this room without copying exits.
        /// </summary>
        public Room CloneWithoutExits(string newName)
        {
            Room newRoom = new Room
            {
                Name = newName,
                Description = this.Description,
                MapColor = this.MapColor,
                MapIcon = this.MapIcon,
                Tags = new List<string>(this.Tags),
                AreaId = this.AreaId // Important: assign the same area
            };

            return newRoom;
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
            List<Exit> exits = [.. GameState.Instance.Areas[areaId]
                .Exits
                .Values.Where(e => e.SourceRoomId == roomId || e.DestinationRoomId == roomId)];

            foreach (Exit e in exits)
            {
                GameState.Instance.Areas[areaId].Exits.Remove(e.Id);
            }
        }

        public static void DeleteRoom(Room room)
        {
            DeleteRoom(room.AreaId, room.Id);
        }
         public Item? FindItem(string itemName)
        {
            return Items.Find(x => x.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }

        public static Mob? FindMob(string name, Room room)
        {
            return room.Mobs.Find(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Return a list of Exit objects that are in this room.
        /// </summary>
        /// <returns></returns>
        public List<Exit> GetExits()
        {
            // This works just like the loop in GetPlaysersInRoom, but is shorter
            // This style of list manipulation is called "LINQ"
            return [.. GameState.Instance.Areas[AreaId].Exits.Values.Where(e => e.SourceRoomId == Id)];
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
            List<Player> playersInRoom = [];
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
            Comm.SendToIfPlayer(character, Description);

            // Send a message to all players in the room
            Comm.SendToRoomExcept(this, $"{character.Name} enters the room.", character);
            if (character is NonPlayer npc) {
                Npcs.Add(npc);
            }
            else if (character is Mob mob)
            {
                Mobs.Add(mob);
            }
        }

        //EngagementRules
        public void SafeZone()
        {
            
        }
        public void AgroRoom() 
        { 
        
        }
        public void SameRoom() 
        { 
        
        }
        public void TrapRoom() 
        { 
        
        }
        //end EngagementRules

        /// <summary>
        /// When a character leaves a room, do this.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="toRoom"></param>
        public void LeaveRoom(Character character, Room toRoom)
        {
           // Send a message to all players in the room
            Comm.SendToRoomExcept(this, $"{character.Name} leaves the room.", character);
            if (character is NonPlayer npc)
            {
                Npcs.Remove(npc);
            }
            else if (character is Mob mob)
            {
                Mobs.Remove(mob);
            }
        }
        #endregion

        #region --- Methods (NPC Handling) ---
        /// <summary>
        ///  Currently just working on spawning mobs in rooms based on SpawnableMobs dictionary. 
        ///  (commands not working yet)
        /// </summary>
        public void SpawnMobsInRoom()
        {
            if(this.GetPlayers().Count <= 0)
            {
                // Don't spawn mobs if players aren't present
                return;
            }

            Area area = GameState.Instance.Areas[AreaId];
            Room room = this;
            // Count current mobs in the room
            int currentMobCount = Mobs.Count;
            Random rand = new Random();
            foreach(var kvp in SpawnableMobs)
            {
                string npcName = kvp.Key;
                int maxToSpawn = kvp.Value;
                // Spawn mobs until we reach the max allowed or the room's max spawn limit
                if (currentMobCount >= MaxSpawnedAllowed)
                {
                    break;
                }
                int numberRolled = rand.Next(1, 20);
                if(numberRolled >= SpawnableMobs[npcName])
                {
                    SpawnMob(npcName);
                }
            }
            return;
        }

        public void AddToSpawnable(string npcName, int spawnChance, Player player, string type)
        {
            if (type.ToLower().Equals("mob"))
            {
                if (!SpawnableMobs.ContainsKey(npcName))
                {
                    SpawnableMobs.Add(npcName, spawnChance);
                    player.WriteLine($"{npcName} added to spawnable mobs with a spawn chance of {spawnChance}.");
                }
                else
                {
                    player.WriteLine($"{npcName} is already in the spawnable mobs list.");
                }
            }
            else if (type.ToLower().Equals("npc"))
            {
                if (!SpawnableNpcs.ContainsKey(npcName))
                {
                    SpawnableNpcs.Add(npcName, spawnChance);
                    player.WriteLine($"{npcName} added to spawnable npcs with a spawn chance of {spawnChance}.");
                }
                else
                {
                    player.WriteLine($"{npcName} is already in the spawnable npcs list.");
                }
            }
            return;
        }

        public void RemoveFromSpawnable(string npcName, Player player, string type)
        {
            if (type.ToLower().Equals("mob"))
            {
                if (SpawnableMobs.ContainsKey(npcName))
                {
                    SpawnableMobs.Remove(npcName);
                    player.WriteLine($"{npcName} removed from spawnable mobs.");
                }
                else
                {
                    player.WriteLine($"{npcName} is not in the spawnable mobs list.");
                }
            }
            else if (type.ToLower().Equals("npc"))
            {
                if (SpawnableNpcs.ContainsKey(npcName))
                {
                    SpawnableNpcs.Remove(npcName);
                    player.WriteLine($"{npcName} removed from spawnable npcs.");
                }
                else
                {
                    player.WriteLine($"{npcName} is not in the spawnable npcs list.");
                }
            }
            return;
        }
        public void ModifyChance(string npcName, Player player, string type, int chance)
        {
            if (type.ToLower().Equals("mob"))
            {
                if (SpawnableMobs.ContainsKey(npcName))
                {
                    SpawnableMobs[npcName] = chance;
                    player.WriteLine($"{npcName} spawn chance modified to {chance}.");
                }
                else
                {
                    player.WriteLine($"{npcName} is not in the spawnable mobs list.");
                }
            }
            else if( type.ToLower().Equals("npc"))
            {
                if (SpawnableNpcs.ContainsKey(npcName))
                {
                    SpawnableNpcs[npcName] = chance;
                    player.WriteLine($"{npcName} spawn chance modified to {chance}.");
                }
                else
                {
                    player.WriteLine($"{npcName} is not in the spawnable npcs list.");
                }
            }
        }

        public void ListSpawnables(Player player, string type)
        {
            if (type.ToLower().Equals("mob"))
            {
                player.WriteLine("Spawnable Mobs:");
                foreach (var kvp in SpawnableMobs)
                {
                    player.WriteLine($"- {kvp.Key}: Spawn Chance {kvp.Value}");
                }
            }
            else if (type.ToLower().Equals("npc"))
            {
                player.WriteLine("Spawnable NPCs:");
                foreach (var kvp in SpawnableNpcs)
                {
                    player.WriteLine($"- {kvp.Key}: Spawn Chance {kvp.Value}");
                }
            }
            return;
        }

        public void SpawnMob(string npcName)
        {
            Mob mob = GameState.Instance.MobCatalog[npcName];
            Comm.SendToRoom(this, $"{npcName} has appeared in the room.");

            Mobs.Add(mob);
            return;
        }

        public void SpawnNpcsInRoom()
        {
            if (this.GetPlayers().Count <= 0)
            {
                // Don't spawn npcs if players aren't present
                return;
            }
            Area area = GameState.Instance.Areas[AreaId];
            Room room = this;
            // Count current npcs in the room
            int currentNpcCount = Npcs.Count;
            Random rand = new Random();
            foreach (var kvp in SpawnableNpcs)
            {
                string npcName = kvp.Key;
                int maxToSpawn = kvp.Value;
                // Spawn npcs until we reach the max allowed or the room's max spawn limit
                if (currentNpcCount >= MaxSpawnedAllowed)
                {
                    break;
                }
                int numberRolled = rand.Next(1, 20);
                if (numberRolled >= SpawnableNpcs[npcName])
                {
                    SpawnNpc(npcName);
                }
            }
            return;
        }

        public void SpawnNpc(string npcName)
        {
            Comm.SendToRoom(this, $"{npcName} has appeared in the room.");
            NonPlayer npc = GameState.Instance.NPCCatalog[npcName];
            npc.Spawned = true;
            Npcs.Add(npc);
            return;
        }

        public void SpawnEntitiesInRoom()
        {
            SpawnMobsInRoom();
            SpawnNpcsInRoom();
            return;
        }

        public void DespawnEntitiesInRoom()
        {
            foreach (var mob in Mobs)
            {
                mob.Spawned = false;
            }
            Mobs.Clear();
            foreach(var npc in Npcs)
            {
                npc.Spawned = false;
            }
            Npcs.Clear();
            return;
        }
        public void DespawnEntity(string mobName, string type)
        {
            if (type.ToLower().Equals("mob"))
            {
                Mob? mobToRemove = Mobs.Find(m => m.Name.Equals(mobName, StringComparison.OrdinalIgnoreCase));
                if (mobToRemove != null)
                {
                    mobToRemove.Spawned = false;
                    Mobs.Remove(mobToRemove);
                    Comm.SendToRoom(this, $"{mobName} has been despawned from the room.");
                }
            }
            else if(type.ToLower().Equals("npc"))
            {
                NonPlayer? npcToRemove = Npcs.Find(m => m.Name.Equals(mobName, StringComparison.OrdinalIgnoreCase));
                if (npcToRemove != null)
                {
                    npcToRemove.Spawned = false;
                    Npcs.Remove(npcToRemove);
                    Comm.SendToRoom(this, $"{mobName} has been despawned from the room.");
                }
            }
                return;
        }

        public NonPlayer GetNpcByName(string name) {
            return Npcs.Find(npc => npc.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        #endregion
    }
}
