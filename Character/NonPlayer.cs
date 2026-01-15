
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

        // CODE REVIEW: Shelton - PR #18
        // There is really no reason to be fields, and you already have all of this information
        // as properties with private set, so I removed these methods.
        // I also moved them to the top with the rest of the fields/properties.
        // You can delete this comment once you've reviewed.
        //public int GetAgressionLevel() { return CurrentAgressionLevel; } // Not needed you already have CurrentAgressionLevel property
        //public string GetShortDescription() { return ShortDescription; }
        //public string GetLongDescription() { return LongDescription; }

        // CODE REVIEW: Shelton - PR #18
        // We want to make sure we have a default constructor for serialization purposes (and in general).
        // I added it, so you can just delete this comment.
        public NonPlayer()
        {
        }

        public NonPlayer(string name, string shortDesc, string longDesc, int level)
        {
            Name = name;
            ShortDescription = shortDesc;
            LongDescription = longDesc;
            Level = level;
        }

        public void IncrementAgressionLevel(int amount)
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


        
    }
}
