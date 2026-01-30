
using RPGFramework.Geography;
using System.Security.Cryptography.X509Certificates;

namespace RPGFramework
{
    /// <summary>
    /// Represents a base character in the game, providing common properties and functionality for players, non-player
    /// characters (NPCs), and other entities.
    /// </summary>
    /// <remarks>This abstract class defines shared attributes such as health, level, skills, equipment, and
    /// location for all character types. Derived classes should implement specific behaviors and additional properties
    /// as needed. The class enforces valid ranges for skill attributes and manages health and alive status. Instances
    /// of this class are not created directly; instead, use a concrete subclass representing a specific character
    /// type.</remarks>
    internal abstract class Character : IDescribable
    {
        enum CharacterState { 
            Idle, 
            Moving, 
            Attacking, 
            Dead 
        }

        #region --- Properties ---
        public static Random random = new Random();
        public bool Alive { get; set; } = true;
        public int AreaId { get; set; } = 0;
        public string Description { get; set; } = "";
        public int Gold { get; set; } = 0;
        public int Health { get; protected set; } = 0;
        public int Level { get; protected set; } = 1;
        public int LocationId { get; set; } = 0;
        public int MaxHealth { get; protected set; } = 0;
        public string Name { get; set; } = "";
        protected List<string> Tags { get; set; } = []; // (for scripting or special behavior)
        public List<string> ValidTags { get; set; } = ["Wanderer", "Shopkeep", "Mob", "Hostile", "Greedy", "Healer", "Wimpy", "Talkative"];
        //Might need to move later, but for now I need a place to keep them -Shelton
        public Character? Target { get; set; } = null; // (for combat or interaction)
        public int XP { get; protected set; } = 0;
        public CharacterClass Class { get; set; } = new CharacterClass();
        public List<Armor> EquippedArmor { get; set; } = [];
        public Weapon PrimaryWeapon { get; set; }
        //public Inventory PlayerInventory { get; set; } = new Inventory(); 
        #endregion

        #region --- Skill Attributes --- (0-20)
        public int Strength { get; private set { field = Math.Clamp(value, 0, 20); } } = 0;
        public int Dexterity { get; private set { field = Math.Clamp(value, 0, 20); } } = 0;
        public int Constitution { get; private set { field = Math.Clamp(value, 0, 20); } } = 0;
        public int Intelligence { get; private set { field = Math.Clamp(value, 0, 20); } } = 0;
        public int Wisdom { get; private set { field = Math.Clamp(value, 0, 20); } } = 0;
        public int Charisma { get; private set { field = Math.Clamp(value, 0, 20); } } = 0;
        #endregion


        public Character()
        {
            Health = MaxHealth;
            Weapon w = new Weapon() 
              { Damage = 2, Description = "A fist", Name = "Fist", Value = 0, Weight = 0 };
            PrimaryWeapon = w;
        }

        /// <summary>
        /// Get Room object of current location.
        /// </summary>
        /// <returns></returns>
        public Room GetRoom()
        {
            return GameState.Instance.Areas[AreaId].Rooms[LocationId];
        }

        public Area GetArea()
        {
            return GameState.Instance.Areas[AreaId];
        }

        // get exits in current room
        public List<Exit> GetExits()
        {
            Room currentRoom = GetRoom();
            List<Exit> exits = new List<Exit>();
            foreach (int exitId in currentRoom.ExitIds)
            {
                exits.Add(GameState.Instance.Areas[AreaId].Exits[exitId]);
            }
            return exits;
        }

        public void SetRoom(int id)
        {
            LocationId = id;
        }

        public void SetArea(int id)
        {
            AreaId = id;
        }

        // Set Health to a specific value
        public void SetHealth(int health)
        {
            // Doesn't make sense if player is dead
            if (Alive == false)
                return;


            // Can't have health < 0
            if (health < 0)
                health = 0;

            // Can't have health > MaxHealth
            if (health > MaxHealth)
                health = MaxHealth;

            Health = health;

            // If Health == 0, Make Unalive
            if (Health == 0)
            {
                Alive = false;
            }
        }

        // Set Max Health to a specific value, use sparingly, mostly for creating characters
        public void SetMaxHealth(int maxHealth)
        {
            if (maxHealth < 1)
                maxHealth = 1;
            MaxHealth = maxHealth;
            // Ensure current health is not greater than new max health

            Health = MaxHealth;
        }


        // Remove some amount from health
        public void TakeDamage(int damage)
        {
            SetHealth(Health - damage);
        }

        // Add some amount to health
        public void Heal(int heal)
        {
            SetHealth(Health + heal);

        }

        internal void ApplyBleed(double bleedDamagePerSecond, int bleedDuration)
        {
            throw new NotImplementedException();
        }

        //Add tags to character
        public bool AddTag(string tag)
        {
           if(ValidTags.Contains(tag) && !Tags.Contains(tag))
           {
                Tags.Add(tag);
                return true;
           }
            else
            {
                return false;
            }
        }
        //removes tags from character
        public bool RemoveTag(string tag)
        {
            if (Tags.Contains(tag))
            {
                Tags.Remove(tag);
                return true;
            }
            else
            {
                return false;
            }
        }
        public List<string> GetTags()
        {
            return Tags;
        }
    }
}
        
