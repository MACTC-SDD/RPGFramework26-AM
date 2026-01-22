using System;
using System.Collections.Generic;
using System.Text;



namespace RPGFramework.Items
{
    internal class Inventory
    {
        public List<string> InventorySlots { get; set; } = new List<string>();
        public void SetSlotValue(int index, string value)
        {
            if (index < 0 || index >= InventorySlots.Count)
                throw new IndexOutOfRangeException("Inventory Is Full");

            InventorySlots[index] = value;
        }
    }

}
