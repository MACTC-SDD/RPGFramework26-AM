using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RPGFramework.Combat_Stuff
{
    internal class CombatCommands
    {
        // variables not set to actual stats yet
        // will make it add the enemies stats to the list later
        public static double playerHealth { get; set; } = 100.0;
        public List<string> enemyNames = new List<string>();
        public List<double> enemyHealth = new List<double>();
        public static double damageDealt { get; set; } = 0.0;
        public List<double> enemyDamageDealt = new List<double>();
        public static string userSelect { get; set; } = "";
        public static string playerSelectedEnemy { get; set; } = "";
        public static double playerDamage { get; set; } = 0.0;
        public List<double> enemyDamage = new List<double>();
        public static double playerFleeChance { get; set; } = 0.0;
        public List<double> enemyAggresion = new List<double>();
        public static string playerConsideredEnemy { get; set; } = "";
        public static int playerStrength { get; set; } = 0;
        public static int playerConstitution { get; set; } = 0;
        public static int playerDexterity { get; set; } = 0;
        public static int playerIntellegence { get; set; } = 0;
        public static int playerWisdom { get; set; } = 0;
        public static int playerCharisma { get; set; } = 0;
        public static List<int> enemyStrength = new List<int>();
        public static List<int> enemyConstitution = new List<int>();
        public static List<int> enemyDexterity = new List<int>();
        public static List<int> enemyIntellegence = new List<int>();
        public static List<int> enemyWisdom = new List<int>();
        public static List<int> enemyCharisma = new List<int>();
        public void CombatCommandChoice()
        {
            // these are set if the user dosn't choose who to select, it will be set by default
            playerSelectedEnemy = enemyNames[0];
            playerConsideredEnemy = enemyNames[0];

            Console.WriteLine($"What would you like to Do (type the number for your choice): \n" + "1. Attack\n"
                + "2. Target\n" + "3. Flee\n" + "4. Disengage\n" + "5. Consider\n" + "6. Combat\n");
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
            enemyHealth[enemyNames.IndexOf(playerSelectedEnemy)] -= playerDamage;
            Console.WriteLine($"You hit {playerSelectedEnemy} for {playerDamage} healthpoints!");
            if (enemyHealth[enemyNames.IndexOf(playerSelectedEnemy)] <= 0 && playerHealth > 0)
            {
                Console.WriteLine("You have won the fight!");
            }
            else if (enemyHealth[enemyNames.IndexOf(playerSelectedEnemy)] > 0 && playerHealth <= 0)
            {
                Console.WriteLine("You have lost the fight!");
            }
        }
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
            if (enemyNames.IndexOf(playerSelectedEnemy) == -1)
            {
                Console.WriteLine("This target does not exist. Choose a valid Target");
                Target();
            }
            Console.WriteLine($"{playerSelectedEnemy} is now targeted.");
        }
        //Flee Command will be replaced with Chase's one
        public void Flee()
        {
            if (enemyAggresion[enemyNames.IndexOf(playerSelectedEnemy)] >= playerFleeChance)
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
            if (enemyAggresion[enemyNames.IndexOf(playerSelectedEnemy)] == 0.0)
            {
                Console.WriteLine($"{playerSelectedEnemy} has been calmed down.");
            }
            else
            {
                Console.WriteLine($"Your disengage attempt has failed!");
            }
        }
        public void Consider()
        {
            Console.WriteLine("Who would you like to check/consider?: ");
            playerConsideredEnemy = Console.ReadLine();
            switch (playerConsideredEnemy)
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
            if (enemyNames.IndexOf(playerConsideredEnemy) == -1)
            {
                Console.WriteLine("This target does not exist. Choose a valid Target");
                Consider();
            }
            Console.WriteLine($"{playerConsideredEnemy} Stats: ");
            Console.WriteLine($"Strength: {enemyStrength[enemyNames.IndexOf(playerSelectedEnemy)]}");
            Console.WriteLine($"Constitution: {enemyConstitution[enemyNames.IndexOf(playerSelectedEnemy)]}");
            Console.WriteLine($"Dexterity: {enemyDexterity[enemyNames.IndexOf(playerSelectedEnemy)]}");
            Console.WriteLine($"Intellegence: {enemyIntellegence[enemyNames.IndexOf(playerSelectedEnemy)]}");
            Console.WriteLine($"Wisdom: {enemyWisdom[enemyNames.IndexOf(playerSelectedEnemy)]}");
            Console.WriteLine($"Charisma: {enemyCharisma[enemyNames.IndexOf(playerSelectedEnemy)]}");
        }
        public void Combat()
        {
            Console.WriteLine("Combat Status: ");
            Console.WriteLine($"Round: {RoundTiming.currentRounds}");
            Console.WriteLine($"Your Health: {playerHealth}");
            Console.WriteLine($"Your Damage Dealt: {damageDealt}");
            for (int i = 0; i < enemyNames.Count(); i++)
            {
                Console.WriteLine($"{enemyNames[i]} Health: {enemyHealth[enemyNames.IndexOf(playerSelectedEnemy)]}");
                Console.WriteLine($"{enemyNames[i]} Damage Dealt: {enemyDamageDealt[enemyNames.IndexOf(playerSelectedEnemy)]}\n");
            }
        }
    }
}