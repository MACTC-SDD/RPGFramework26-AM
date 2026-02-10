
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

        //Mythril wearing check
        public static bool WearingMythril(Armor armor)
            {
            if (armor.Material == ArmorMaterial.Mythril
              )
            {
                return true; 
            }
            return false;
              
            }
         //Mythril wearing check end
         //armor type check
         public static bool WearingLight(Armor type)
        {
            if (type.Type == ArmorType.Light)
            {
                return true;
            }
            return false;

        }
        public static bool WearingMedium(Armor type)
        {
            if (type.Type == ArmorType.Medium)
            {
                return true;
            }
            return false;

        }
        public static bool WearingHeavy(Armor type)
        {
            if (type.Type == ArmorType.Heavy)
            {
                return true;
            }
            return false;

        }
        //amror type cheek end
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


 