using RPGFramework;
using RPGFramework.Geography;
using RPGFramework.Enums;
using RPGFramework.Interfaces;
using System.Dynamic;

namespace RPGFramework.Combat
{
    internal class Battle
    {
        public  Character Attacker { get; set; }
        public  Character Defender { get; set; }
        public Character? Initiative { get; private set; }
        public Character? NonInitiative { get; private set; }
        public DateTime? AttackTime { get; set; }
        public DateTime? StartTime { get; private set; } = DateTime.Now;
        public bool IsAttacking { get; set; }
        public  Area StartArea { get; set; }
        public  Room StartRoom { get; set; }
        public BattleState BattleState { get; private set; } = BattleState.Combat;
        public 

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
        
        public Battle()
        {
            AttackTime = DateTime.Now;
            RollInitiative();
            Attacker!.Target = Defender;
        }

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
            //if (BattleState != BattleState.Combat) { return; }

            // Check if both alive
            //if (Attacker.Alive == false || Defender.Alive == false)
            //{
            //    EndBattle();
            //}

            GameState.Log(DebugLevel.Debug, $"Processing turn for battle between {Attacker.Name} and {Defender.Name}");
            // Are they in same room



            // Calculate init player damage done and apply to non init player
            
            public static int CalculateDamage(int Damage, int Strength, int DamageMultiplier)
            {
                // 1. Scale with attacker stats
                float damage = Damage + Strength * 0.5f;

                // 2. Apply multipliers
                damage *= DamageMultiplier;
            }

            if (Battle Attacker.Turn == true)
            {
                Battle calculateDamage -= ;
            }
            else
            {

            }
           
                    
                    
             
            
                    
                    
                    
            // 3. Apply defense

            //need to make a target defense variable that grabs the targets defense
            //damage -= Target.Defense;

            // 4. Apply damage reduction
            //same thing here but for damage reduction
            //damage *= (1f - defender.DamageReduction);


}


            // Is non init player still alive?
            if (Attacker != null || Defender != null)
            {
                Console.WriteLine("Opponent left");
                EndBattle();
            }
            else
            {
                
            }

            // Calculate non init player damage done and apply to init player

            // Check if battle has timed out (maybe do that first?)


        }
        private void EndBattle()
        {
            BattleState = BattleState.CombatComplete;
        }
    }


}
