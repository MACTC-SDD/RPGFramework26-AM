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
        }

        //not sure if its neccesary, but it doesn't hurt to have it.
        public Mob(string name, string desc, int level, Dictionary<string, string[]> dialogOptions, int maxRoomsToChase)
        {
            Name = name;
            Description = desc;
            Level = level;
            MaxRoomsToChase = maxRoomsToChase;
        }

        public bool CanChase()
        {
            return ChaseDistance < MaxRoomsToChase;
        }

    }
}