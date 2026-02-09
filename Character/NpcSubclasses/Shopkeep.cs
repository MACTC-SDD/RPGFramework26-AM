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
            NpcType = NonPlayerType.Shopkeep;
            Tags.Add(NPCTag.Shopkeep);
        }
        #region --- Inventory Methods ---

        private int GetItemSellPrice(string itemID)
        {
            return ShopInventory[itemID];
        }
        public void SellItem(string itemID, int quantity)
        {
            if (ShopInventory.ContainsKey(itemID))
            {
                if (ShopInventory[itemID] >= quantity)
                {
                    ShopInventory[itemID] -= quantity;
                    if (ShopInventory[itemID] == 0)
                    {
                        ShopInventory.Remove(itemID);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Not enough items in inventory to sell.");
                }
            }
            else
            {
                throw new KeyNotFoundException("Item not found in inventory.");
            }
        }
        public void ClearInventory()
        {
            ShopInventory.Clear();
        }
        public void RemoveItemFromInventory(string index)
        {
            if (ShopInventory.ContainsKey(index)) {
                ShopInventory.Remove(index);
            }
        }
        public Dictionary<string, int> GetShopInventory()
        {
            return ShopInventory;
        }
        public int GetItemQuantity(string index)
        {
            return ShopInventory[index];
        }
        public bool HasItemInInventory(string index)
        {
            return ShopInventory.ContainsKey(index);
        }
        public void IncrementItemQuantity(string index)
        {
            ShopInventory[index]++;
        }
        public void AddItemToInventory(string index, int amount)
        {
            ShopInventory[index] = amount;
        }

        //Need to create selling methods later - Shelton
        #endregion
    }
}
