using RPGFramework.Enums;
using System.ComponentModel;

namespace RPGFramework
{
    internal class Mob : NonPlayer
    {
        public int RoomsChased { get; private set; } = 0;
        public int MaxRoomsToChase { get; private set; } = 5;

        public Mob(string name, string shortDesc, string longDesc, int level, Dictionary<string, string[]> dialogOptions, int maxRoomsToChase,
            int locationID)
            : base(name, shortDesc, longDesc, level, dialogOptions, locationID)
        {
            MaxRoomsToChase = maxRoomsToChase;
        }

    }
}