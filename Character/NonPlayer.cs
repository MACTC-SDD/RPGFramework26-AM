
using RPGFramework.Enums;

namespace RPGFramework
{
    /// <summary>
    /// Represents a character in the game that is not controlled by a player.
    /// </summary>
    /// <remarks>Non-player characters (NPCs) may serve various roles such as quest givers, merchants, or
    /// enemies.</remarks>
    internal class NonPlayer : Character
    {
        //additional variables from NPCs
        public int MaxAgressionLevel { get; private set; } = 10; // (the maximum aggression level for this NPC)
        public int MinAgressionLevel { get; private set; } = 0; // (the minimum aggression level for this NPC)


        //Next two are paired together, the private in order to hold the actual value, the public to enforce clamping
        private int _currentAggressionLevel;
        public int CurrentAggressionLevel{
            get => _currentAggressionLevel;
            private set => _currentAggressionLevel = Math.Clamp(value, 0, 10);
        }
        public Dictionary<string, string[]> DialogOptions { get; set; } //Dialog options, added at creation time.
        public int PreviousAreaId { get; private set; } = 0;
        public NonPlayer(string name, string Desc, int level, Dictionary<string, string[]> DialogOptions, int locationID)
        {
            Name = name;
            Level = level;
        }

        public void IncrimentAgressionLevel(int amount)
        {
            CurrentAggressionLevel += amount;
        }
        
    }
}
