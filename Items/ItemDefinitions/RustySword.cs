using System;
using System.Collections.Generic;
using System.Text;
using RPGFramework.Items;

namespace RPGFramework.Items.ItemDefinitions
{
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
                
            };
        } 
    } 
}