
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



    }
    internal enum ArmorMaterial
    {
        Cloth,

        Leather,
        Iron,
        Steel,
        Mythril
    }
    internal enum ArmorSlot
    {
        Head,
        Chest,
        Legs,
        back
    }
    internal enum ArmorType
    {
        Light,
        Medium,
        Heavy
    }
}
