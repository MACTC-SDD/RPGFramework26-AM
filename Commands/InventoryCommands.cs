
using RPGFramework.Enums;
using System.Runtime.CompilerServices;

namespace RPGFramework.Commands
{
    internal class InventoryCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return
            [
                new InventoryCommand(),
                new AdminGetCommand(),
                // Add other communication commands here as they are implemented
            ];
        }
    }


    internal class InventoryCommand : ICommand
    {
        public string Name => "inventory"; // Change from "equip" to "inventory"
        public IEnumerable<string> Aliases => new List<string> { "inv" };

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;


            player.WriteLine("Your Inventory:");
            var items = player.PlayerInventory.Items;

            // Loop through 16 slots
            for (int i = 0; i < 16; i++)
            {
                if (items.ElementAtOrDefault(i) != null)
                {
                    var item = items[i];
                    player.WriteLine($"Slot {i + 1}: {item.Name}");
                }

                else
                {


                    player.WriteLine($"Slot {i + 1}: Empty");
                }

            }
            return true;
        }
      }
    internal class AdminGetCommand : ICommand
    {
        public string Name => "/Admin get";
        public IEnumerable<string> Aliases => new List<string> { "Ag" };

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player) return false;

            // 1. Check Input (Must have at least Command + ItemName)
            // parameters[0] is "Ag", parameters[1] is the item name
            if (parameters.Count < 2)
            {
                player.WriteLine("Usage: Ag [Item Name]");
                return true;
            }

            // 2. Reconstruct the name SKIPPING the command (Index 0)
            // We take the range starting at index 1, for the rest of the list
            List<string> itemWords = parameters.GetRange(1, parameters.Count - 1);
            string searchName = string.Join(" ", itemWords);

            Item? itemToAdd = null;

            // 3. Search Catalogs (Using Case-Insensitive Search)
            // This allows "blue" to find "Blue" or "BLUE"

            // Check Item Catalog
            var itemKey = GameState.Instance.ItemCatalog.Keys
                .FirstOrDefault(k => k.Equals(searchName, StringComparison.OrdinalIgnoreCase));

            if (itemKey != null)
            {
                itemToAdd = GameState.Instance.ItemCatalog[itemKey].Clone();
            }
            // Check Weapon Catalog
            else
            {
                var weaponKey = GameState.Instance.WeaponCatalog.Keys
                    .FirstOrDefault(k => k.Equals(searchName, StringComparison.OrdinalIgnoreCase));

                if (weaponKey != null)
                {
                    itemToAdd = GameState.Instance.WeaponCatalog[weaponKey].Clone();
                }
                // Check Armor Catalog
                else
                {
                    var armorKey = GameState.Instance.ArmorCatalog.Keys
                        .FirstOrDefault(k => k.Equals(searchName, StringComparison.OrdinalIgnoreCase));

                    if (armorKey != null)
                    {
                        itemToAdd = GameState.Instance.ArmorCatalog[armorKey].Clone();
                    }
                }
            }

            // 4. Handle Not Found
            if (itemToAdd == null)
            {
                player.WriteLine($"Error: Item '{searchName}' does not exist in any Catalog.");
                return true;
            }

            // 5. Add to Inventory
            if (player.PlayerInventory.AddItem(itemToAdd))
            {
                player.WriteLine($"You received: {itemToAdd.Name}");
                if (!string.IsNullOrEmpty(itemToAdd.Description))
                    player.WriteLine($"Description: {itemToAdd.Description}");
            }
            else
            {
                player.WriteLine("Your inventory is full!");
            }

            return true;
        }
    }
}
