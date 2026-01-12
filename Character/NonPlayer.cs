
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

        //Next to are paired together, the private in order to hold the actual value, the public to enforce clamping
        private int _currentAggressionLevel;
        public int CurrentAggressionLevel{
            get => _currentAggressionLevel;
            private set => _currentAggressionLevel = Math.Clamp(value, 0, 10);
        }
        public Dictionary<string, string[]> DialogOptions { get; set; } //Dialog options, added at creation time.

        public NonPlayer(string name, string shortDesc, string longDesc, int level, Dictionary<string, string[]> DialogOptions, int locationID)
        {
            Name = name;
            ShortDescription = shortDesc;
            LongDescription = longDesc;
            Level = level;
            this.DialogOptions = DialogOptions;
            LocationId = locationID;
        }
        
        //incriments the agression level by a set amount (negative values allowed)
        public void IncrimentAgressionLevel(int amount)
        {
            CurrentAggressionLevel += amount;
        }

        //feels self explanitory
        public int GetAggressionLevel(){return CurrentAggressionLevel;}

        //returns the two different descriptions
        public string GetShortDescription() { return ShortDescription; }
        public string GetLongDescription() { return LongDescription; }
        
    }
}
