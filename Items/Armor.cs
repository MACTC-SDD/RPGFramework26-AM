
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
        public int Defense { get; private set; }

        public Armor(string name, EquipmentSlot slot, int defense)
        {
            Name = name;
            EquipmentSlot = slot;
            Defense = defense;
        }


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
