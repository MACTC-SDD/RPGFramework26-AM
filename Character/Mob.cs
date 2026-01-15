using RPGFramework.Enums;
using System.ComponentModel;

namespace RPGFramework
{
    internal class Mob : NonPlayer
    {
        public int ChaseDistance { get; private set; } = 0;
        public int MaxRoomsToChase { get; private set; } = 5;

        public Mob()
        {
        }

        public bool CanChase()
        {
            return ChaseDistance < MaxRoomsToChase;
        }

    }
}