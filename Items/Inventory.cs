using System.Text.Json;
using System.Text.Json.Serialization;


namespace RPGFramework.Items
{
    internal class Inventory
    {
        public List<Item> InventorySlots { get; set; } = new List<Item>();
        [JsonInclude] public List<Item> Items { get; internal set; } = [];
        [JsonInclude] public int MaxSlots { get; private set; } = 16;
      
        //Modified because InventorySlots is a list, which doesnt have a max designed by default. -Shelton
        public void SetSlotValue(int index, string value)
        {
            if (index < 0 || index >= MaxSlots)
                throw new IndexOutOfRangeException("Inventory Is Full");

            Item item = GameState.Instance.ItemCatalog[value];
            InventorySlots[index] = item;
        }

        public bool AddItem(Item item)
        {
            if (Items.Count >= MaxSlots) return false;
            Items.Add(item);
            return true;
        }

        //Added for the sake of sell commands (remove if you hate) -Shelton
        public bool AddItem(string item)
        {
            if (InventorySlots.Count >= MaxSlots)
                return false;
            
            Item itemNew = GameState.Instance.ItemCatalog[item].Clone();
            InventorySlots.Add(itemNew);
            return true;
        }
        public bool RemoveItem(string item)
        {
            Item itemToRemove = InventorySlots.Find(i => i.Name == item);
            if (itemToRemove == null)
                return false;
            return InventorySlots.Remove(itemToRemove);
        }

        public void SellItem(string item, Player player)
        {
            Item itemToSell = InventorySlots.Find(i => i.Name == item);
            if (itemToSell != null)
            {
                InventorySlots.Remove(itemToSell);
                player.Gold += ((int) itemToSell.Value / 2);
            }
        }

        public bool HasItem(string item)
        {
            Item check = InventorySlots.Find(i => i.Name == item);
            if(check == null)
                return false;
            return true;
        }

        //End of added NPC team methods
        internal bool RemoveItem(Item itemToRemove)
        {
            if (Items.Contains(itemToRemove))
            {
                Items.Remove(itemToRemove);
                return true;
            }
            return false;
        }
    }

}
