using RPGFramework.Enums;
using RPGFramework.Geography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using RPGFramework.Items;
using System.Net;
using System.Text.Json.Serialization;


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

        enum CharacterState
        {
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
        public List<NPCTag> Tags { get; set; } = []; // (for scripting or special behavior)
        [JsonIgnore] public Character? Target { get; set; } = null; // (for combat or interaction)
        public int XP { get; protected set; } = 0;
        public CharacterClass Class { get; set; } = new CharacterClass();
        public List<Armor> EquippedArmor { get; set; } = [];
        public Weapon PrimaryWeapon { get; set; }
        public static double CritChance { get; set; } = CritChance = Math.Clamp(CritChance, 0.0, 0.38);
        public static double CritDamage { get; set; } = 1;

        public Inventory PlayerInventory { get; set; } = new Inventory(); 
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
        /// 
        #region --- Skill Attribute Methods ---
        protected void SetStrength(int value)
        {
            Strength = Math.Clamp(value, 0, 20);
        }

        protected void SetDexterity(int value)
        {
            Dexterity = Math.Clamp(value, 0, 20);
        }

        protected void SetConstitution(int value)
        {
            Constitution = Math.Clamp(value, 0, 20);
        }

        protected void SetIntelligence(int value)
        {
            Intelligence = Math.Clamp(value, 0, 20);
        }

        protected void SetWisdom(int value)
        {
            Wisdom = Math.Clamp(value, 0, 20);
        }

        protected void SetCharisma(int value)
        {
            Charisma = Math.Clamp(value, 0, 20);
        }

        public int GetStrength()
        {
            return Strength;
        }
        public int GetDexterity()
        {
            return Dexterity;
        }
        public int GetConstitution()
        {
            return Constitution;
        }
        public int GetIntelligence()
        {
            return Intelligence;
        }
        public int GetWisdom()
        {
            return Wisdom;
        }
        public int GetCharisma()
        {
            return Charisma;
        }

        public void IncrimentStrength(int value)
        {
            SetStrength(Strength + value);
        }

        public void IncrimentDexterity(int value)
        {
            SetDexterity(Dexterity + value);
        }

        public void IncrimentConstitution(int value)
        {
            SetConstitution(Constitution + value);
        }

        public void IncrimentIntelligence(int value)
        {
            SetIntelligence(Intelligence + value);
        }

        public void IncrimentWisdom(int value)
        {
            SetWisdom(Wisdom + value);
        }

        public void IncrimentCharisma(int value)
        {
            SetCharisma(Charisma + value);
        }
        #endregion

        public void LevelUp(int amount)
        {
            Level += amount;
            Random random = new Random();
            for (int i = 0; i < amount; i++)
            {
                int healthIncrease = (int)(MaxHealth * 0.1);
                SetMaxHealth(MaxHealth + healthIncrease);
                int randomSkill = random.Next(0, 5);
                switch (randomSkill)
                {
                    case 0:
                        IncrimentStrength(1); break;
                    case 1:
                        IncrimentDexterity(1); break;
                    case 2:
                        IncrimentCharisma(1); break;
                    case 3:
                        IncrimentConstitution(1); break;
                    case 4:
                        IncrimentWisdom(1); break;
                    case 5:
                        IncrimentIntelligence(1); break;

                }
            }
            // Restore health to full on level up
            SetHealth(MaxHealth);
        }
        public Room GetRoom()
        {
            return GameState.Instance.Areas[AreaId].Rooms[LocationId];
        }

        public int GetXPtoNextLevel()
        {
            return Level * 100; // Example: 100 XP per level
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
            // Accept enum names (case-insensitive) and avoid duplicates
            NPCTag item;
            Enum.TryParse<NPCTag>(tag, true, out item);
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (item != null && !Tags.Contains(item))
            {
                Tags.Add(item);
                return true;
            }
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            return false;
        }
        //removes tags from character
        public bool RemoveTag(NPCTag tag)
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
        //Attack Resolution
        public bool WillHit()
        {



            int HitChance = 50 + (Dexterity) * 5; /* Add - Enemy.Dexterity*/
            HitChance = Math.Clamp(HitChance, 5, 95);
            int roll = Random.Shared.Next(1, 101);
            bool hit = roll <= HitChance;
            return hit;

        }

    
        public void WeaponStrengthDamage(int strength)
        {
            RPGFramework.Weapon test = new RPGFramework.Weapon();
     
            Double Damage = Strength + test.Damage;
        }

        public bool WillDodge()
        {
            int DodgeChance = Dexterity;



            int Dodge = Random.Shared.Next(1, 101);
            bool dodgedroll = Dodge <= DodgeChance;
            return dodgedroll;
        }
        //End Attack Resolution
        //Stored Actions 

        /* public void Task StoredActions()
         {
            if //(Enemy attacking)
               {
                 (Actions) await (EnemyAttack);
             }
         }*/

        //End Stored Actions

        //Critical hit based on level
        private void CritOnLevel()
        {
            CritChance = (Level / 50.0) * 0.10;
            CritChance = Math.Clamp(CritChance, 0.0, 0.10);
        }
        //Mythril critical hit 
        private void MythrilCrit()
        {
            Armor equiped = new Armor();


            if (Armor.WearingMythril(equiped))
                CritChance += 0.13;
                CritDamage = 2;
            string ResultP = Player.CritChance.ToString("P");

        }

        //Mythril end critical hit
        //armor type crits
        private void ArmorTypeCrit()
        {
            Armor type = new Armor();

            if (Armor.WearingLight(type))
            {
                CritChance += 0.05;
            }
            if (Armor.WearingMedium(type))
            {
                CritChance += 0.1;
            }
            if (Armor.WearingHeavy(type))
            {
                CritChance += 0.2;
            }
        }
        //end armor type crits
        //End critical hit

        // CODE REVIEW: Shelton PR #60 - See notes on CharacterCommands / ShowNPCTags for reasoning behind this method.
        public List<string> GetTags()
        {
            //return Tags;
            // Look at the tags list, sort by the the string representatation and return those strings
            return [.. this.Tags.Select(t => t.ToString()).OrderBy(t => t.ToString())];

        }
    }
}
        

      
