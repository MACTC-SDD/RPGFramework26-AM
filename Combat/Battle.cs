using RPGFramework;
using RPGFramework.Geography;
using RPGFramework.Enums;
using RPGFramework.Interfaces;
using System.Dynamic;
using System.Diagnostics.CodeAnalysis;
using System;


namespace RPGFramework
{
    internal class Battle
    {
        public Character Attacker { get; set; }
        public Character Defender { get; set; }
        public Character? Initiative { get; private set; }
        public Character? NonInitiative { get; private set; }
        public DateTime AttackTime { get; set; } = DateTime.Now;
        public DateTime StartTime { get; private set; } = DateTime.Now;

        public Area StartArea { get; set; }
        public Room StartRoom { get; set; }
        public BattleState BattleState { get; private set; } = BattleState.Combat;

        private int _timeoutSeconds = 120;



        public Battle(Character attacker, Character defender, Area startArea, Room startRoom)
        {
            Attacker = attacker;
            Defender = defender;
            StartArea = startArea;
            StartRoom = startRoom;
            AttackTime = DateTime.Now;
            RollInitiative();
            Attacker.Target = Defender;
            Defender.Target = Attacker;
        }

        /*public Battle()
        {
            AttackTime = DateTime.Now;
            RollInitiative();
            Attacker?.Target = Defender;
        }
        */

        private void RollInitiative()
        {
            Random random = new Random();
            int p1 = random.Next(1, 21) + Attacker.Dexterity;
            int p2 = random.Next(1, 21) + Defender.Dexterity;
            if (p1 >= p2)
            {
                Initiative = Attacker;
                NonInitiative = Defender;
            }
            else
            {
                Initiative = Defender;
                NonInitiative = Attacker;
            }
        }
        public void ProcessTurn()
        {
            GameState.Log(DebugLevel.Debug, $"Processing turn for battle between {Attacker.Name} and {Defender.Name}");
            //if (BattleState != BattleState.Combat) { return; }

            // Check if both alive
            if (Attacker.Alive == false || Defender.Alive == false)
            {
                EndBattle();
            }

            if (Attacker.GetRoom() != Defender.GetRoom())
            {
                // Can't attack because in different rooms, battle goes on
                return;
            }

            if (CheckForTimeout())
            {
                EndBattle();
                return;
            }

            AttackTime = DateTime.Now;
            Room r = Initiative!.GetRoom();

            int damage = Initiative!.GetDamage();
            NonInitiative!.TakeDamage(damage);

            Comm.SendToRoom(r, $"{Initiative.Name} hits {NonInitiative.Name} for {damage} damage!");

            // Is non init player still alive?
            if (!NonInitiative.Alive)
            {
                Comm.SendToRoom(r, $"{Initiative.Name} just killed {NonInitiative.Name}");
                EndBattle();
                return;
            }

            damage = NonInitiative.GetDamage();
            Comm.SendToRoom(r, $"{NonInitiative.Name} hits {Attacker.Name} for {damage} damage!");

            // Is defender player still alive?
            if (!Initiative.Alive)
            {
                Comm.SendToRoom(r, $"{NonInitiative.Name} just killed {Initiative.Name}");
                EndBattle();
                return;
            }
        }

        private void EndBattle()
        {
            BattleState = BattleState.CombatComplete;
            Attacker.Target = null;
            Defender.Target = null;
            GameState.Instance.Battles.Remove(this);
        }

        private bool CheckForTimeout()
        {
            // Check if current time - attackTime > timeoutSeconds
            TimeSpan t = DateTime.Now - AttackTime;
            if (t.TotalSeconds > _timeoutSeconds)
                return true;

            return false;
        }

    }


}


    /*
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
    */


