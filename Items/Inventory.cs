using System.Text.Json.Serialization;


namespace RPGFramework.Items
{
    internal class Inventory
    {
        [JsonInclude] public List<Item> Items { get; internal set; } = [];
        [JsonInclude] public int MaxSlots { get; private set; } = 16;
        public bool AddItem(Item item)
        {
            if (Items.Count >= MaxSlots) return false;
            Items.Add(item);
            return true;
        }
    }

}
