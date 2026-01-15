
using RPGFramework.Enums;
using static EquipmentBase;

namespace RPGFramework
{
    internal class Armor : Item
    {
        public ArmorMaterial Material { get; set; }
        public ArmorSlot Slot { get; set; }
        public ArmorType Type { get; set; }
        public int DamageReduction { get; set; }
        public int Durability { get; set; }
        public int MaxDurability { get; set; }
        public float DodgeChance { get; set; }
        public float HealthBonus { get; set; }

        public EquipmentSlot EquipmentSlot { get; private set; }
        public int Defense { get; private set; } = 0;

        public Armor(string name, EquipmentSlot slot, int defense)
        {
            Name = name;
            EquipmentSlot = slot;
            Defense = defense;
        }
        public ArmorRarity Rarity { get; set; }
    }
}
