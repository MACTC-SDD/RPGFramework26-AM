using RPGFramework.Enums;
using System;

namespace RPGFramework

{
    internal class Weapon : Item
    {
        public double Damage { get; set; } = 0;
        public double OnehandDamage { get; set; } = 0;

        public double AttackTime { get; set; } = 1.0;

        public double TwohandDamage { get; set; }

        public double HeavyAttack {  get; set; }

        public double LightAttack { get; set; }

        public double Range { get; set; }

        public double Abilies { get; set; }

        public WeaponType Type { get; set; }

        public WeaponMaterial Material { get; set; }
        
        // TODO
        // Add attack properties (damage, speed, etc.)
        // Implement attack methods
        // Maybe some kind of Weapon generator (random stats, etc.)
    }
}
