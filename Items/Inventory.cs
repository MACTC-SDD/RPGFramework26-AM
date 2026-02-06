using System;
using System.Collections.Generic;
using System.Text;



namespace RPGFramework.Items
{
    internal class Inventory
    {
        public List<string> InventorySlots { get; set; } = new List<string>();
        public List<Item> Items { get; internal set; } = [];
        public int MaxSlots { get; private set; } = 16;

        public void SetSlotValue(int index, string value)
        {
            if (index < 0 || index >= InventorySlots.Count)
                throw new IndexOutOfRangeException("Inventory Is Full");

            InventorySlots[index] = value;
        }

        public bool AddItem(Item item)
        {
            if (Items.Count >= MaxSlots) return false;
            Items.Add(item);
            return true;
        }
    }

}
