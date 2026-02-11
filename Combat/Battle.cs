using RPGFramework;
using RPGFramework.Geography;
using RPGFramework.Enums;
using RPGFramework.Interfaces;
using System.Dynamic;
using System.Diagnostics.CodeAnalysis;
using System;

namespace RPGFramework.Combat
{
    internal class Battle
    {
        public Character Player { get; private set; }
        public List<Character> Enemies { get; private set; }

        private int turnIndex = 0;
        public Character CurrentTurn => turnIndex == 0 ? Player : Enemies[turnIndex - 1];

        public BattleState BattleState { get; private set; } = BattleState.Combat;

        public Battle(Character player, List<Character> enemies)
        {
            Player = player;
            Enemies = enemies;

            // Set targets
            foreach (var enemy in Enemies)
                enemy.Target = Player;

            Player.Target = Enemies.FirstOrDefault();
        }

        public void ShowStatus()
        {
            Console.WriteLine($"\n▶ Battle Status:");
            Console.WriteLine($"Player: {Player.Name} | HP: {Player.Health}/{Player.MaxHealth}");
            foreach (var enemy in Enemies)
            {
                Console.WriteLine($"{enemy.Name} | HP: {enemy.Health}/{enemy.MaxHealth}");
            }
        }

        public void ProcessCommand(string command)
        {
            if (BattleState != BattleState.Combat)
            {
                Console.WriteLine("The battle is over!");
                return;
            }

            switch (command.ToLower())
            {
                case "attack":
                    ExecuteAttack(CurrentTurn);
                    break;

                case "target":
                    ChooseTarget();
                    break;

                case "flee":
                    AttemptFlee();
                    break;

                case "consider":
                    ConsiderTarget();
                    break;

                default:
                    Console.WriteLine("Unknown command. Try: attack, target, flee, consider");
                    break;
            }

            AdvanceTurn();
            CheckEndBattle();
        }

        private void ExecuteAttack(Character attacker)
        {
            Character target = attacker.Target ?? Enemies.First();

            if (!attacker.WillHit())
            {
                Console.WriteLine($"{attacker.Name} misses {target.Name}!");
                return;
            }

            if (target.WillDodge())
            {
                Console.WriteLine($"{target.Name} dodges {attacker.Name}'s attack!");
                return;
            }

            int baseDamage = (int)(attacker.PrimaryWeapon?.Damage ?? 1);
            int damage = baseDamage + attacker.Strength;

            target.TakeDamage(damage);
            Console.WriteLine($"{attacker.Name} hits {target.Name} for {damage} damage!");
        }

        private void ChooseTarget()
        {
            Console.WriteLine("Who would you like to target?");
            string input = Console.ReadLine();

            var target = Enemies.FirstOrDefault(e => e.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (target == null)
            {
                Console.WriteLine("Invalid target. Defaulting to first enemy.");
                target = Enemies.First();
            }

            Player.Target = target;
            Console.WriteLine($"{Player.Name} is now targeting {target.Name}");
        }

        private void ConsiderTarget()
        {
            Character target = Player.Target ?? Enemies.First();
            Console.WriteLine($"\n{target.Name} Stats:");
            Console.WriteLine($"HP: {target.Health}/{target.MaxHealth}");
            Console.WriteLine($"Strength: {target.Strength}, Dexterity: {target.Dexterity}, Constitution: {target.Constitution}");
        }

        private void AttemptFlee()
        {
            int fleeChance = 50 + Player.Dexterity * 2;
            int roll = Random.Shared.Next(1, 101);

            if (roll <= fleeChance)
            {
                Console.WriteLine("You successfully fled the battle!");
                BattleState = BattleState.CombatComplete;
            }
            else
            {
                Console.WriteLine("You failed to flee!");
            }
        }

        private void AdvanceTurn()
        {
            turnIndex++;

            // Enemy turns
            if (turnIndex > Enemies.Count)
                turnIndex = 0;

            if (CurrentTurn != Player)
            {
                // Simple AI: attack player
                ExecuteAttack(CurrentTurn);
            }
        }

        private void CheckEndBattle()
        {
            if (!Player.Alive)
            {
                Console.WriteLine("You have been defeated!");
                BattleState = BattleState.CombatComplete;
                return;
            }

            if (Enemies.All(e => !e.Alive))
            {
                Console.WriteLine("All enemies defeated!");
                BattleState = BattleState.CombatComplete;
                return;
            }
        }
    }
}

