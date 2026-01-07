
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
        public string SongDescription { get; private set; } = "";
        public int CurrentAgressionLevel { get; private set; } = 0; // (the higher the value, the more aggressive actions can be taken)

        public NonPlayer(string name, string shortDesc, string longDesc, int level)
        {
            Name = name;
            ShortDescription = shortDesc;
            LongDescription = longDesc;
            Level = level;
        }
    }
}
/*    
 Notes:
-Random NPC generation can be added later
-Spawn limit?
-behavior rules
 
 */
