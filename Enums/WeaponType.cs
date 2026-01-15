
namespace RPGFramework.Enums
{
    // CODE REVIEW: Liam (PR #21)
    // Before you start actually creating weapons, alphabatize this list.
    internal enum WeaponType
    {
        LongSword,
        Shortsword,
        Claymore,
        Cutlass,
        BattleAxe,
        Hatchet,
        Mace,
        Warhammer,
        Staff,
        Wands,
        Dagger,
        Spear,
        Bow,
        Magicbook,
        Shield,
        DualShield,
        Crossbow,
        Slings,
        Throwingknives,
        Clubs,
        Halberd,
        Whip,
        Kusarigama,
        Katana,
        Rapier,
        Trident,
        WarScythe,
        MilitaryFork,
        Pocketofsand,
        Chair,
        Fryingpan,
        Wakizashi,
        TableLeg,
        Saber,
        FlintLock
    }

    internal enum WeaponMaterial
    {
        Wood,
        Bone,
        Rusty,
        Stone,
        Iron,
        Steel,
        Mythril,
        Obsidian,
        Copper,
        Bronze,
        DragonBone,
        Crystal,
        Tungsten,
        VoidMetal,
        StarMetal,
        Gold,
        Basalt,
        MoonSilver,
        Sand,
        admin,
        burner,
        DragonScale,
        MetorMetal,
        EnchatedSteel

    }
    internal enum ArmorMaterial
    {
        Cloth,
        Bronze,
        Copper,
        Gold,
        Basalt,
        Bone,
        Rusty,
        DragonScale,
        DragonBone,
        EnchantedSteel,
        Crystal,
        Leather,
        Iron,
        Steel,
        Mythril,
        Obsidian,
        Tungsten,
        StarMetal,
        MeteorSteel,
        VoidMetal,
        MoonSilver
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
    public enum EquipmentSlot
    {
        Head,
        Chest,
        Legs,
        Back,
        MainHand,
        OffHand
    }
    public enum WeaponRarity
    {
        Common, 
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic,
        Eternal
    }
    public enum ArmorRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }
}
    
