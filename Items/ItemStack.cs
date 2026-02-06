using System;
using System.Collections.Generic;
using System.Text;

namespace RPGFramework
{
    internal class ItemStack
    {
        public List<Item> Items { get; set; } = [];

        public void AddItem(Item item)
        {
            Items.Add(item);
        }

        public void RemoveItem(Item item)
        {
            Items.Remove(item);
        }

        public void DisplayStack()
        {
            if (Items.Count == 0)
            {
                Console.WriteLine("The stack is empty.");
                return;
            }

            Console.WriteLine("Items in stack:");
            foreach (var item in Items)
            {
                Console.WriteLine($"- {item.Name}");
            }
        }
    }
}
