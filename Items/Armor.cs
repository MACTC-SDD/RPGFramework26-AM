
using Microsoft.VisualBasic;
using RPGFramework.Enums;

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



        //armor based damage reduction
        private void Stats() {

            //Highest armor tier
            //case armormaterial.somthing
            DamageReduction = 10;
            MaxDurability = 100;

            //Mid armor tier
            //case armormaterial.somthing
            DamageReduction = Random.Shared.Next(3, 6);
            MaxDurability = Random.Shared.Next(40, 50);

            //Low armor tier
            //case armormaterial.somthing
            DamageReduction = 3;
            MaxDurability = 25;
        }
        //end armor damage reduction

        //damage to durability 
        //may have to fix incomingDamage to Damage
        public int AbsorbDamage(int incomingDamage)
        {
            int reducedDamage = Math.Max(0, incomingDamage - DamageReduction);

            // durability loss based on hit strength
            Durability -= Math.Max(1, incomingDamage / 5);
            Durability = Math.Max(0, Durability);

            return reducedDamage;
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


