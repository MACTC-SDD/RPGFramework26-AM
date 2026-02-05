using System;
using System.Collections.Generic;
using System.Text;



namespace RPGFramework.Items
{
    internal class Inventory
    {
        public List<string> InventorySlots { get; set; } = new List<string>();
        //Constants can't be changed, so we define max slots here. -Shelton
        private const int MaxSlots = 20;

        //Modified because InventorySlots is a list, which doesnt have a max designed by default. -Shelton
        public void SetSlotValue(int index, string value)
        {
            if (index < 0 || index >= MaxSlots)
                throw new IndexOutOfRangeException("Inventory Is Full");

            InventorySlots[index] = value;
        }
        public Inventory() { }

        //Added for the sake of sell commands (remove if you hate) -Shelton
        public bool AddItem(string item)
        {
            if (InventorySlots.Count >= MaxSlots)
                return false;
            InventorySlots.Add(item);
            return true;
        }
        public bool RemoveItem(string item)
        {
            return InventorySlots.Remove(item);
        }

        public bool HasItem(string item)
        {
            return InventorySlots.Contains(item);
        }
        //End of added NPC team methods
    }

}
