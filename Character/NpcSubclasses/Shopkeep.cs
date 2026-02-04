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
            Tags.Add("Shopkeep");
        }

        public void IncrementItemQuantity(string index)
        {
            ShopInventory[index]++;
        }
        public void AddItemToInventory(string index)
        {
            ShopInventory[index] = 1;
        }

        //Need to create selling methods later - Shelton
    }
}
