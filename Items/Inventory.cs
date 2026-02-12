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
            if (index < 0) { string playerInvenReminder = "Inventory Is Full!"; }
        }

        public bool AddItem(Item item)
        {
            if (Items.Count >= MaxSlots) return false;
            Items.Add(item);
            return true;
            //ux team
            if (Items.Count >= MaxSlots) { string playeraction = $"No Space In Inventory To Add Item!"; }
        }

        // CODE REVIEW: Shelton - we might want to discuss. In this case we are giving
        // the player the actual copy from the catalog, meaning if it got degraded or modified
        // it's modifying the catalog version, which isn't the intent. 
        // Check out Utility.Clone which will take an object and give you a copy of it (not just a reference to the same thing)
        //Added for the sake of sell commands (remove if you hate) -Shelton
        public bool AddItem(string item)
        {
            if (InventorySlots.Count >= MaxSlots)
                return false;
            Item itemNew = GameState.Instance.ItemCatalog[item];
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
                //ux team
                string playeraction = $"You Sold {itemToSell} for {itemToSell.Value}!";
            }
        }

        public bool HasItem(string item)
        {
            Item check = InventorySlots.Find(i => i.Name == item);
            if (check == null) 
            //ux team
            if (false) { string playerAction = $"Cannot Find Item"; }
            return false;
            return true;
        
        }
        //End of added NPC team methods
    }

}
