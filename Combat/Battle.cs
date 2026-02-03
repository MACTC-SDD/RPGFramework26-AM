using RPGFramework;
using RPGFramework.Geography;
using RPGFramework.Enums;
using RPGFramework.Interfaces;

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

        public  Area StartArea { get; set; }
        public  Room StartRoom { get; set; }
        public BattleState BattleState { get; private set; } = BattleState.Combat;

        public Battle(Character attacker, Character defender, Area startArea, Room startRoom)
        {
            Attacker = attacker;
            Defender = defender;
            StartArea = startArea;
            StartRoom = startRoom;
            AttackTime = DateTime.Now;
            RollInitiative();
            Attacker.Target = Defender;
        }
        
        public Battle()
        {
            AttackTime = DateTime.Now;
            RollInitiative();
            Attacker?.Target = Defender;
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

            // Is non init player still alive?

            // Calculate non init player damage done and apply to init player

            // Check if battle has timed out (maybe do that first?)


        }
        private void EndBattle()
        {
            BattleState = BattleState.CombatComplete;
        }
    }


}
