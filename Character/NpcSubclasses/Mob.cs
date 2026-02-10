using RPGFramework.Enums;
using System.ComponentModel;
using System.Transactions;

namespace RPGFramework
{
    internal class Mob : NonPlayer
    {
        public int ChaseDistance { get; private set; } = 0;
        public int MaxRoomsToChase { get; private set; } = 5;

        public Mob()
        {
            NpcType = NonPlayerType.Mob;
            Tags.Add(NPCTag.Mob);
        }
        public bool CanChase()
        {
            return ChaseDistance < MaxRoomsToChase;
        }

    }
}