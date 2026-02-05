using System;
using System.Collections.Generic;
using System.Text;



namespace RPGFramework.Items
{
    internal class Inventory
    {
        public List<Item> InventorySlots { get; set; } = new List<Item>();
        //Constants can't be changed, so we define max slots here. -Shelton
        private const int MaxSlots = 16;

        //Modified because InventorySlots is a list, which doesnt have a max designed by default. -Shelton
        public void SetSlotValue(int index, string value)
        {
            if (index < 0 || index >= MaxSlots)
                throw new IndexOutOfRangeException("Inventory Is Full");

            Item item = GameState.Instance.ItemCatalog[value];
            InventorySlots[index] = item;
        }
        public Inventory() { }

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

        public bool HasItem(string item)
        {
            Item check = InventorySlots.Find(i => i.Name == item);
            if(check == null)
                return false;
            return true;
        }
        //End of added NPC team methods
    }

}
