
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
        public string ShortDescription { get; private set; } = "";
        public string LongDescription { get; private set; } = "";
        public int CurrentAgressionLevel { get; private set; } = 0; // (the higher the value, the more aggressive actions can be taken)
        public int MaxAgressionLevel { get; private set; } = 10; // (the maximum aggression level for this NPC)
        public int MinAgressionLevel { get; private set; } = 0; // (the minimum aggression level for this NPC)

        public NonPlayer(string name, string shortDesc, string longDesc, int level)
        {
            Name = name;
            ShortDescription = shortDesc;
            LongDescription = longDesc;
            Level = level;
        }

        public void IncrimentAgressionLevel(int amount)
        {
            if (amount < MaxAgressionLevel || amount > -MaxAgressionLevel)
            {
                CurrentAgressionLevel += amount;
            }
            else if (amount + CurrentAgressionLevel > MaxAgressionLevel)
            {
                CurrentAgressionLevel = MaxAgressionLevel;
            }
            else if (CurrentAgressionLevel - amount < MinAgressionLevel)
            {
                CurrentAgressionLevel = MinAgressionLevel;
            }
            else
            {
                CurrentAgressionLevel += amount;
            }
        }
        //feels self explanitory
        public int GetAgressionLevel(){return CurrentAgressionLevel;}

        //returns the two different descriptions
        public string GetShortDescription() { return ShortDescription; }
        public string GetLongDescription() { return LongDescription; }
        
    }
}
