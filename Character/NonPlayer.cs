
using RPGFramework.Enums;
using System.ComponentModel;
using System.Transactions;

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
        public Dictionary<string, List<string>> DialogOptions { get; protected set; } = new Dictionary<string, List<string>>();
        public int CurrentAggressionLevel { get; protected set; } = 0;
        public int MaxAggressionLevel { get; protected set; } = 10;
        public int MinAgressionLevel { get; protected set; } = 0;

        public NonPlayer()
        {
        }

        public void IncrementAgressionLevel(int amount)
        {
            CurrentAggressionLevel += amount;
        }
        
    }
}
