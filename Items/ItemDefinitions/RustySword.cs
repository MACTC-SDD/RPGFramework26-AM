using System;
using System.Collections.Generic;
using System.Text;
using RPGFramework.Items;

namespace RPGFramework.Items
{
    // CODE REVIEW: Liam (PR #21)
    // We won't end up creating weapons this way, but maybe this is old code?
    // Discuss with me and we'll remove after you do.
    internal static class RustySword
    {
        public static Weapon Create() 
        {
            return new Weapon
            {
                Id = 1,
                Description = "Watch your toes for tetanus",
                DisplayText = "",
                IsDroppable = true,
                IsGettable = true,
                IsStackable = false,
                Level = 1,
                Name = "Rusty Sword",
                UsesRemaining = 0,
                Value = 0,
                Type = WeaponType.Sword,
                Material = WeaponMaterial.Rusty
                
            };
        }
    } 
}