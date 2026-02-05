
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
        public static string Knight { get; private set; }
        public int Health { get; set; } = 150;
        public int Strength { get; set; } = 50;
        public int ProtectionLevel { get; set; } = 5;
        public int Magic { get; set; } = 0;
    }


{ 
public static string Mage { get; private set; }
    public int Health { get; set; } = 80;
    public int Strength { get; set; } = 10;
    public int ProtectionLevel { get; set; } = 1;
    public int Magic { get; set; } = 100;

{
    public static string Thief { get; private set; }
    public int Health { get; set; } = 100;
    public int Strength { get; set; } = 30;
    public int ProtectionLevel { get; set; } = 3;
    public int Magic { get; set; } = 20;
}
    { 
    
    public static string Brute { get; private set; }
public int health { get; set; } = 200;
public int strength { get; set; } = 100;
public int protection_level { get; set; } = 10;
public int magic { get; set; } = 0;



    }
}
