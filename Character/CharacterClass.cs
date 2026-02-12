
using RPGFramework.Enums;
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

        }
        //class crits
        public  bool IsKnight()
        {
            //CharacterClass Type = new CharacterClass();
            //if (Type.ClassType == Classes.Knight)
            //{
            //    return true;
            //}
            //return false;
            return ClassType == Classes.Knight;          
        }
        public  bool IsHealer()
        {
            return ClassType == Classes.Healer; ;
        }
        public  bool IsThief()
        {
            return ClassType == Classes.Thief;
        }
        public  bool IsMage()
        {
            
            return ClassType == Classes.Mage;
        }
        public  bool IsArcher()
        {
            return ClassType == Classes.Archer;
           

        }
    }

}
