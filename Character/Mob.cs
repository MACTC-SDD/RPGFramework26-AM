using RPGFramework.Enums;
using System.ComponentModel;

namespace RPGFramework
{
    internal class Mob : NonPlayer
    {
        public int ChaseDistance { get; private set; } = 0;
        public int MaxRoomsToChase { get; private set; } = 5;

        public Mob(string name, string Desc, int level, Dictionary<string, string[]> dialogOptions, int maxRoomsToChase,
            int locationID)
            : base(name, Desc, level, dialogOptions, locationID)
        {
            MaxRoomsToChase = maxRoomsToChase;
        }

        public Mob()
        {
            
        }

        public bool CanChase()
        {
            return ChaseDistance < MaxRoomsToChase;
        }

    }
}