using System;
using System.Collections.Generic;
using System.Text;

namespace RPGFramework.Items.ItemDefinitions
{
    internal class starter_bow
    {
        public static Weapon Create()
        {
            return new Weapon
            {
                Id = 2,
                Description = "A basic wooden bow used by noivce adventurers",
                DisplayText = "",
                IsDroppable = true,
                IsGettable = true,
                IsStackable = false,
                Level = 1,
                Name = "Starter Bow",
                UsesRemaining = 0,
                Value = 0,
                Type = Weapon.WeaponType.Bow,
                Material = Weapon.WeaponMaterial.Wood,
                Range = 20,
                Damage = 5
            };
        }
    }
