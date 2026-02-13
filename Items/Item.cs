using RPGFramework;
using System.Text.Json.Serialization;

namespace RPGFramework
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Weapon), "weapon")]
    [JsonDerivedType(typeof(Armor), "armor")]
    internal class Item : IDescribable
    {
        internal EquipmentBase slot;

        public int Id { get; set; } = 0;
        public string Description { get; set; } = ""; // What you see when you look at it
        public string DisplayText { get; set; } = ""; // How it appears when in a room
        public bool IsDroppable { get; set; } // Can the item be dropped
        public bool IsGettable { get; set; } // Can the item be picked up
        public bool IsStackable { get; set; } = false;
        public int Level { get; set; } = 0;
        public string Name { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
        public int UsesRemaining { get; set; } = -1; // -1 means unlimited uses
        public double Value { get; set; } = 0;
        public double Weight { get; set; } = 0;
        public bool IsConsumable { get; set; } = false;
        public int HealAmount { get; set; } = 0;

        public virtual Item Clone()
        {
            // 1. Create a shallow copy of the object (copies numbers, names, bools)
            Item newItem = (Item)this.MemberwiseClone();

            // 2. Create a NEW list for tags so the clone doesn't share tags with the original
            newItem.Tags = new List<string>(this.Tags);

            return newItem;
        }
        internal static Item ItemCreation(int areaId, int v1, int v2)
        {
            throw new NotImplementedException();
        }
        public int StrengthBonus { get; internal set; }
        public int DefenseBonus { get; internal set; }
    }
}
