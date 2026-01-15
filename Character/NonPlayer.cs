
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

        public NonPlayer(string name, string shortDesc, string longDesc, int level)
        {
            Name = name;
            Description = Desc;
            Level = level;
        }

        public NonPlayer()
        {
            Name ="Generic NPC";
            Level = 1;
            DialogOptions = new Dictionary<string, string[]>();
            LocationId = 0;
        }

        //incriments the agression level by a set amount (negative values allowed)
        public void IncrimentAgressionLevel(int amount)
        {
            CurrentAggressionLevel += amount;
        }

        //feels self explanitory
        public int GetAggressionLevel(){return CurrentAggressionLevel;}

        //returns the two different descriptions
        
    }
}
