using RPGFramework.Enums;

namespace RPGFramework.Items
{
    internal class StarterBow : Weapon
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
                Type = WeaponType.Bow,
                Material = WeaponMaterial.Wood,
                Range = 20,
                Damage = 5
            };
        }
    }
}
