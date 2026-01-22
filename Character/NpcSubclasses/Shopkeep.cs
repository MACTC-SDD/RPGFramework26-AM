using RPGFramework.Enums;
using System.ComponentModel;
using System.Transactions;
namespace RPGFramework

{
    internal class Shopkeep : NonPlayer
    {
        public Dictionary<string, int> ShopInventory { get; private set; } = new Dictionary<string, int>();
        // ItemID, Quantity
        public Shopkeep()
        {
        }

        public void IncrementItemQuantity(string index, int amount)
        {
            if((ShopInventory[index] + amount) < 0)
            {
                throw new IndexOutOfRangeException("Amount requested will cause it to go under 0");
            }
            ShopInventory[index] += amount;
        }
        public void AddItemToInventory(string index, int amount)
        {
            if (ShopInventory.ContainsKey(index))
            {
                IncrementItemQuantity(index, amount);
            }
            else
            {
                ShopInventory[index] = amount;
            }
        }

        public void BuyItem(string itemID, int quantity)
        {
            if(!ShopInventory.ContainsKey(itemID))
            {
                throw new KeyNotFoundException("Item not found in shop inventory");
            }
            if(ShopInventory[itemID] < quantity)
            {
                throw new InvalidOperationException("Not enough quantity in shop inventory");
            }
            ShopInventory[itemID] -= quantity;
            Gold += GameState.Instance.ItemCatalog[itemID].Value * quantity;
        }
    }
}
