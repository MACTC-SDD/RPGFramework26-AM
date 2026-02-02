using RPGFramework;
using RPGFramework.Geography;
using RPGFramework.Enums;
namespace RPGFramework.Combat
{
    internal class Battle
    {
        public required Character Attacker { get; set; }
        public required Character Defender { get; set; }
        public Character? Initiative { get; private set; }

        public Character? NonInitiative { get; private set; }
        public DateTime? AttackTime { get; set; }
        public required Area StartArea { get; set; }
        public required Room StartRoom { get; set; }
        public BattleState BattleState { get; private set; } = BattleState.Combat;

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
            if (BattleState != BattleState.Combat) { return; }

            if (Attacker.Alive == false || Defender.Alive == false)
            {
                EndBattle();
            }

            //Initiatve player deals damage
            //IF non initiative player is still alive deal damage to initiative 



        }
        private void EndBattle()
        {
            BattleState = BattleState.CombatComplete;
        }
    }
}
