
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
            switch (Type) { 

            //Highest armor tier
            case ArmorType.Heavy:
                DamageReduction = 10;
                MaxDurability = 100;
                break;

                //Mid armor tier
                case ArmorType.Medium:
                    DamageReduction = Random.Shared.Next(3, 6);
                MaxDurability = Random.Shared.Next(40, 50);
                    break;

                //Low armor tier
                case ArmorType.Light:
                    DamageReduction = 3;
                MaxDurability = 25;
                    break;

            }
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

            //end damage to durability
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
        Back
    }
    internal enum ArmorType
    {
        Light,
        Medium,
        Heavy
    }
}


