
using RPGFramework.Enums;
using System;
using System.Drawing;

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

        public double Abilities { get; set; }

        public WeaponType Type { get; set; }

        public WeaponMaterial Material { get; set; }

        public bool IsTwoHanded { get; private set; }

        public Weapon (string name, int damage, bool isTwoHanded)
        {
            Name = name;
            Damage = damage;
            IsTwoHanded = isTwoHanded;
        }

        public WeaponRarity Rarity { get; set; }

        public Weapon()
        {
        }
        Dictionary<WeaponRarity, Color> rarityColors = new()
{
    { WeaponRarity.Common, Color.Gray },
    { WeaponRarity.Uncommon, Color.Green },
    { WeaponRarity.Rare, Color.Blue },
    { WeaponRarity.Epic, Color.Purple },
    { WeaponRarity.Legendary, Color.Orange },
    { WeaponRarity.Mythic, Color.Gold },
    { WeaponRarity.Eternal, Color.DarkRed }
};
        // TODO
        // Add attack properties (damage, speed, etc.)
        // Implement attack methods
        // Maybe some kind of Weapon generator (random stats, etc.)
    }
}
