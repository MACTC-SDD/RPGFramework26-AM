using RPGFramework.Combat;
using RPGFramework.Commands;
using RPGFramework.Geography;

namespace RPGFramework.Combat_Stuff
{
    internal class CombatCommands
    {
        // variables not set to actual stats yet
        //changed the lists to dictionaries so it is easier to use and cleaner
        // will make it add the enemies stats to the dictionaries later
        public static double playerHealth { get; set; } = 100.0;
        public Dictionary<int, string> enemyNames = new Dictionary<int, string>();
        public Dictionary<string, double> enemyHealth = new Dictionary<string, double>();
        public static double damageDealt { get; set; } = 0.0;
        public Dictionary<string, double> enemyDamageDealt = new Dictionary<string, double>();
        public static string userSelect { get; set; } = "";
        public static string playerSelectedEnemy { get; set; } = "";
        public static double playerDamage { get; set; } = 0.0;
        public Dictionary<string, double> enemyDamage = new Dictionary<string, double>();
        public static double playerFleeChance { get; set; } = 0.0;
        public Dictionary<string, double> enemyAggresion = new Dictionary<string, double>();
        public static int playerStrength { get; set; } = 0;
        public static int playerConstitution { get; set; } = 0;
        public static int playerDexterity { get; set; } = 0;
        public static int playerIntellegence { get; set; } = 0;
        public static int playerWisdom { get; set; } = 0;
        public static int playerCharisma { get; set; } = 0;
        public static Dictionary<string, int> enemyStrength = new Dictionary<string, int>();
        public static Dictionary<string, int> enemyConstitution = new Dictionary<string, int>();
        public static Dictionary<string, int> enemyDexterity = new Dictionary<string, int>();
        public static Dictionary<string, int> enemyIntellegence = new Dictionary<string, int>();
        public static Dictionary<string, int> enemyWisdom = new Dictionary<string, int>();
        public static Dictionary<string, int> enemyCharisma = new Dictionary<string, int>();
        //allows player to choose a option to do like attack or target
        public void CombatCommandChoice()
        {
            // playerSelectedEnemy is made so if the user dosn't choose who to select, it will be set by default
            playerSelectedEnemy = enemyNames[0];
            Console.WriteLine($"What would you like to Do (type the number for your choice): \n" + "1. Attack\n"
                + "2. Target\n" + "3. Flee\n" + "4. Disengage\n" + "5. Consider\n" + "6. Combat\n");
            //ux team
            string playerCombatReminder =
                "What would you like to Do (type the number for your choice): " +
                $"\n1.Attack {playerSelectedEnemy}" +
                "\n2.Change Target" +
                "\n3.Flee From Battle" +
                "\n4.Disengage From Fight" +
                "\n5.Consider Your Options" +
                "\n6.Combat Stats";
            userSelect = Console.ReadLine();
            switch (userSelect)
            {
                case "1":
                    Attack();
                    break;
                case "2":
                    Target();
                    break;
                case "3":
                    Flee();
                    break;
                case "4":
                    Disengage();
                    break;
                case "5":
                    Consider();
                    break;
                case "6":
                    Combat();
                    break;
            }
        }
        // attack command may or may not be replaced with Chase's, could be changed or edited
        public void Attack()
        {
            if (playerSelectedEnemy == null || playerSelectedEnemy == "")
            {
                Console.WriteLine("No enemy selected. Choosing first enemy avalible.");
               
                playerSelectedEnemy = enemyNames[0];
            }
            enemyHealth[playerSelectedEnemy] -= playerDamage;
            Console.WriteLine($"You hit {playerSelectedEnemy} for {playerDamage} healthpoints!");
            if (enemyHealth[playerSelectedEnemy] <= 0 && playerHealth > 0)
            {
                Console.WriteLine("You have won the fight!");
              
                
            }
            else if (enemyHealth[playerSelectedEnemy] > 0 && playerHealth <= 0)
            {
                Console.WriteLine("You have lost the fight!");                             
            }
        }
        //Allows the player to choose who to attack by targeting them
        public void Target()
        {
            Console.Write("Who would you like to target?: ");
            playerSelectedEnemy = Console.ReadLine();
            switch (playerSelectedEnemy)
            {
                case null:
                    Console.WriteLine("No target chosen. Choose a valid Target");
                    Target();
                    break;
                case "":
                    Console.WriteLine("No target chosen. Choose a valid Target");
                    Target();
                    break;
            }
            if (enemyNames.ContainsValue(playerSelectedEnemy) == false)
            {
                Console.WriteLine("This target does not exist. Choose a valid Target");
                Target();
            }
            Console.WriteLine($"{playerSelectedEnemy} is now targeted.");
        }
        //Flee Command will be replaced with Chase's one
        public void Flee()
        {
            if (enemyAggresion[playerSelectedEnemy] >= playerFleeChance)
            {
                Console.WriteLine("Your flee attempt was unsucsessful!");
               
               
            }
            else
            {
                Console.WriteLine("Your flee attempt was sucsessful!");
              
               
            }
        }
        //Disengage Command will be replaced with Chase's one
        public void Disengage()
        {
            if (enemyAggresion[playerSelectedEnemy] == 0.0)
            {
                Console.WriteLine($"{playerSelectedEnemy} has been calmed down.");
                         }
            else
            {
                Console.WriteLine($"Your disengage attempt has failed!");
               
            }
        }
        //Shows the enemies stats like strength and dexterity
        public void Consider()
        {
            Console.WriteLine("Who would you like to check/consider?: ");
            playerSelectedEnemy = Console.ReadLine();
            switch (playerSelectedEnemy)
            {
                case null:
                    Console.WriteLine("No target choosen to be considered. Choose a valid Target");
                    Consider();
                    break;
                case "":
                    Console.WriteLine("No target choosen to be considered. Choose a valid Target");
                    Consider();
                    break;
            }
            if (enemyNames.ContainsValue(playerSelectedEnemy) == false)
            {
                Console.WriteLine("This target does not exist. Choose a valid Target");
                Consider();
            }
            Console.WriteLine($"{playerSelectedEnemy} Stats: ");
            Console.WriteLine($"Strength: {enemyStrength[playerSelectedEnemy]}");
            Console.WriteLine($"Constitution: {enemyConstitution[playerSelectedEnemy]}");
            Console.WriteLine($"Dexterity: {enemyDexterity[playerSelectedEnemy]}");
            Console.WriteLine($"Intellegence: {enemyIntellegence[playerSelectedEnemy]}");
            Console.WriteLine($"Wisdom: {enemyWisdom[playerSelectedEnemy]}");
            Console.WriteLine($"Charisma: {enemyCharisma[playerSelectedEnemy]}");
        }
        //Shows status of combat and health, round and damage dealt from both sides
        public void Combat()
        {
            Console.WriteLine("Combat Status: ");
            Console.WriteLine($"Round: {RoundTiming.currentRounds}");
            Console.WriteLine($"Your Health: {playerHealth}");
            Console.WriteLine($"Your Damage Dealt: {damageDealt}");
            for (int i = 0; i < enemyNames.Count(); i++)
            {
                Console.WriteLine($"{enemyNames[i]} Health: {enemyHealth[i.ToString()]}");
                Console.WriteLine($"{enemyNames[i]} Damage Dealt: {enemyDamageDealt[i.ToString()]}\n");
            }
        }
    }

    internal class AttackCommand : ICommand
    {
        // This is the command a player would type to execute this command
        public string Name => "attack";

        // These are the aliases that can also be used to execute this command. This can be empty.
        public IEnumerable<string> Aliases => [];
        public string Help => "";

        private readonly GameState _instance = GameState.Instance;

        // What will happen when the command is executed
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;

            if (parameters.Count < 2)
            {
                player.WriteLine("Usage: attack <target>");
                return false;
            }

            // If PVP
            //Character target = Player.FindPlayer(parameters[1], _instance.Players);
            
            Character? target = Room.FindMob(parameters[1], player.GetRoom());

            if (target == null)
            {
                player.WriteLine("Target not found.");
                return false;
            }

            // TODO Check if already in combat with this target
               
            Battle b = new()
            { 
                Attacker = player, 
                Defender = target, 
                StartArea = player.GetArea(), 
                StartRoom = player.GetRoom() 
            };

            _instance.Battles.Add(b);
            //ux team edited this a bit
            player.WriteLine($"You have started attacking {target.Name}! Prepare for a battle for your lives!!");
            // If the command failed to run for some reason, return false
            return true;
        }
    }
}