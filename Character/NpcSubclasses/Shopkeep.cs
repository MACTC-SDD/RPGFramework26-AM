using RPGFramework.Enums;
using System.ComponentModel;
using System.Transactions;
namespace RPGFramework

{
    internal class Shopkeep : NonPlayer
    {
        public Dictionary<int, int> ShopInventory { get; private set; } = new Dictionary<int, int>();
        // ItemID, Quantity
        public Shopkeep()
        {
        }

        //not sure if its neccesary, but it doesn't hurt to have it.
        public Shopkeep(string name, string desc, int level, Dictionary<int, int> inventory,
            int locationID)
        {
            Name = name;
            Description = desc;
            Level = level;
            ShopInventory = inventory;
        }
    }
}
