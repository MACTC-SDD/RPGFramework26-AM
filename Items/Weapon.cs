using RPGFramework.Enums;
namespace RPGFramework

{
    internal class Weapon : Item
    {
        public double Damage { get; set; } = 0;
        public WeaponType WeaponType { get; set; }

        // TODO
        // Add attack properties (damage, speed, etc.)
        // Implement attack methods
        // Maybe some kind of Weapon generator (random stats, etc.)
    }
}
