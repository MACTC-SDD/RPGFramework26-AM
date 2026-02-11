
using System.Net.Security;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;

namespace RPGFramework
{
    /// <summary>
    /// Type or class of character (e.g., Warrior, Mage, Thief).
    /// </summary>
    internal class CharacterClass
    {
        public Enums.Classes ClassType { get; set; } = Enums.Classes.Knight;

        public CharacterClass()
        {
        }

        public void SetClass(Character player, Enums.Classes classType)
        {
            if (classType < Enums.Classes.Knight || classType > Enums.Classes.Healer)
            {
                throw new ArgumentOutOfRangeException(nameof(classType), "Invalid class type specified.");
            }
            ClassType = classType;
            if (ClassType == Enums.Classes.Mage)
            {
                player.IncrimentIntelligence(3);
                player.IncrimentWisdom(2);
            }
            else if(ClassType == Enums.Classes.Knight)
            {
                player.IncrimentStrength(3);
                player.IncrimentConstitution(2);
            }
            else if (ClassType == Enums.Classes.Thief)
            {
                player.IncrimentDexterity(3);
                player.IncrimentCharisma(2);
            }
            else if (ClassType == Enums.Classes.Archer)
            {
                player.IncrimentDexterity(3);
                player.IncrimentIntelligence(2);
            }
            else if (ClassType == Enums.Classes.Healer)
            {
                player.IncrimentWisdom(3);
                player.IncrimentIntelligence(2);
            }

        }
        public override string ToString()
        {
            return ClassType.ToString();
        }
    }
}
