using System;
using System.Collections.Generic;
using System.Text;

namespace RPGFramework.data_seed.COmbat_Team
{


    public class hitChance
    {



        int HitChance = 50 + (Character.Dexterity -/*Enemy.Dexterity*/ ) * 5;
        HitChance = Math.Clamp(HitChance,5,95)
            int roll = Random.Shared.Next(1, 101);
        bool hit = roll <= HitChance;

    }

    public class WeaponStrengthDamage
    {
        int Damage = (Character.Strength +/*weapon.damage*/)
    }

    public class dodge
    {
        int DodgeChance = Character.dexterity

        if(HitChance.hit)
            {
       int Dodge = Random.Shared.Next(1, 101);
        bool dodgedroll = Dodge <= DodgeChance
            }

       if(Dodge = DodgeChance)
        {Console.WriteLine("attack was dodged");}
        else
{ Console.WriteLine("attack hit!")}


    

}
}
