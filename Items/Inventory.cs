using System;
using System.Collections.Generic;
using System.Text;



namespace RPGFramework.Items
{
    internal class Inventory
    {
        public List<Item> Items { get; internal set; } = [];
        public int MaxSlots { get; private set; } = 16;
        public bool AddItem(Item item)
        {
            if (Items.Count >= MaxSlots) return false;
            Items.Add(item);
            return true;
        }
    }

}
