
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
                new GetCommand(),
                new DropCommand(),
                new AdminGetCommand()
                // Add other communication commands here as they are implemented
            ];
        }
    }
    internal class GetCommand : ICommand
    {
        public string Name => "get";
        public IEnumerable<string> Aliases => new List<string> { "take", "grab" };
        public string Help => "Usage: get [Item Name]\nPicks up an item from the current room and adds it to your inventory.";

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player) return false;

            // 1. Check if the player actually typed an item name
            if (parameters.Count < 2)
            {
                player.WriteLine("Get what?");
                return true;
            }

            // 2. Combine parameters into the search name (handles "get iron sword")
            string searchName = string.Join(" ", parameters.Skip(1));

            // 3. Get the current room and look for the item
            var room = player.GetRoom();
            var itemInRoom = room.Items.FirstOrDefault(i => i.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase));

            // 4. Handle item not found
            if (itemInRoom == null)
            {
                player.WriteLine($"There is no '{searchName}' here.");
                return true;
            }

            // 5. Try to add to inventory
            // We don't "Clone()" here because we are moving the physical object
            if (player.PlayerInventory.AddItem(itemInRoom))
            {
                // Remove it from the room ground
                room.Items.Remove(itemInRoom);

                player.WriteLine($"You pick up the {itemInRoom.Name}.");
            }
            else
            {
                player.WriteLine("Your inventory is full! You can't carry any more.");
            }

            return true;
        }
    }
    internal class DropCommand : ICommand
    {
        public string Name => "drop";
        public IEnumerable<string> Aliases => new List<string> { "discard", "put" };
        public string Help => "Usage: drop [Item Name]\nRemoves an item from your inventory and leaves it in the current room.";

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player) return false;

            // 1. Check if they specified what to drop
            if (parameters.Count < 2)
            {
                player.WriteLine("Drop what?");
                return true;
            }

            // 2. Combine parameters (handles multi-word items like "Rusty Iron Sword")
            string searchName = string.Join(" ", parameters.Skip(1));

            // 3. Find the item in the player's inventory
            // We look inside player.PlayerInventory.Items
            var itemToDrop = player.PlayerInventory.Items
                .FirstOrDefault(i => i.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase));

            // 4. Handle item not found in pockets
            if (itemToDrop == null)
            {
                player.WriteLine($"You aren't carrying a '{searchName}'.");
                return true;
            }

            // 5. THE REVERSE HANDOFF
            // Remove from player's inventory list
            player.PlayerInventory.Items.Remove(itemToDrop);

            // Add to the room's item list
            var room = player.GetRoom();
            room.Items.Add(itemToDrop);

            player.WriteLine($"You WHIPPED the [yellow]{itemToDrop.Name}[/] at the floor with the utmost force and velocity you could muster.");
            return true;
        }

        public bool Execute(Character character, List<int> parameters) => throw new NotImplementedException();
    }
    internal class InventoryCommand : ICommand
    {
        public string Name => "inventory"; // Change from "equip" to "inventory"
        public IEnumerable<string> Aliases => new List<string> { "inv" };
        public string Help => "Usage: inventory (or inv)\nDisplays the contents of your inventory, showing each slot and its item if occupied.";

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
        public string Help => "Usage: Ag [Item Name]\nAdds the specified item to your inventory. The item name is case-insensitive and can be found in the Item, Weapon, or Armor Catalogs.";

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
