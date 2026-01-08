
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
        public int CurrentAgressionLevel { get; private set { field = Math.Clamp(value, 0, 10); } } = 0; // (the higher the value, the more aggressive actions can be taken)
        public Dictionary<string, string[]> DialogOptions { get; set; } //Dialog options, added at creation time.

        public NonPlayer(string name, string shortDesc, string longDesc, int level, Dictionary<string, string[]> DialogOptions)
        {
            Name = name;
            ShortDescription = shortDesc;
            LongDescription = longDesc;
            Level = level;
            this.DialogOptions = DialogOptions;
        }
        
        public void IncrimentAgressionLevel(int amount)
        {
            CurrentAgressionLevel += amount;
        }

        //feels self explanitory
        public int GetAgressionLevel(){return CurrentAgressionLevel;}

        //returns the two different descriptions
        public string GetShortDescription() { return ShortDescription; }
        public string GetLongDescription() { return LongDescription; }
        
    }
}
