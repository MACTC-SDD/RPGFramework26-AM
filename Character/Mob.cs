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

        public Mob(string name, string desc, int level, Dictionary<string, string[]> dialogOptions, int maxRoomsToChase,
            int locationID)
        {
            Name = name;
            Description = desc;
            Level = level;
            //LocationID = locationID; // Not sure what this is for (Area/Room?)
            //DialogOptions = dialogOptions;
            MaxRoomsToChase = maxRoomsToChase;
        }

        

        public bool CanChase()
        {
            return ChaseDistance < MaxRoomsToChase;
        }

    }
}