
using RPGFramework.Enums;
using System.Runtime.CompilerServices;
using System.Linq;

namespace RPGFramework.Commands
{
    internal class InventoryCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return
            [
                new InventoryCommand(),
                new UseCommand(),
                new ItemGetCommand(),
                new ItemRemoveCommand(),
                new GetCommand(),
                new DropCommand(),
                new GiveCommand(),
                // Add other communication commands here as they are implemented
            ];
        }
    }

    internal class UseCommand : ICommand
    {
        public string Name => "use";
        public IEnumerable<string> Aliases => new List<string> { "consume", "eat", "drink" };
        public string Help => "Usage: use [Item Name]\nUses a consumable item to restore health.";

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player) return false;

            if (parameters.Count < 2)
            {
                player.WriteLine("Use what?");
                return true;
            }

            string itemName = string.Join(" ", parameters.Skip(1));
            var item = player.PlayerInventory.Items
                .FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                player.WriteLine($"You don't have a '{itemName}'.");
                return true;
            }

            if (!item.IsConsumable)
            {
                player.WriteLine($"You can't use the {item.Name}.");
                return true;
            }

            // --- UPDATED HEALING LOGIC ---

            // We use the .Heal() method because 'Health' is protected.
            // This also automatically ensures we don't go over MaxHealth!
            player.Heal(item.HealAmount);

            player.WriteLine($"You used the [cyan]{item.Name}[/] and healed for [green]{item.HealAmount}[/] health!");
            player.WriteLine($"Current Health: [green]{player.Health}/{player.MaxHealth}[/]");

            // -----------------------------

            player.PlayerInventory.RemoveItem(item);
            return true;
        }
    }

    internal class GetCommand : ICommand
    {
        public string Name => "get";
        public IEnumerable<string> Aliases => new List<string> { "take", "grab", "nab", "snatch", "pocket" };
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
                player.WriteLine($"There is no [red]'{searchName}'[/] here.");
                return true;
            }

            // 5. Try to add to inventory
            // We don't "Clone()" here because we are moving the physical object
            if (player.PlayerInventory.AddItem(itemInRoom))
            {
                // Remove it from the room ground
                room.Items.Remove(itemInRoom);

                player.WriteLine($"You pick up the [yellow]{itemInRoom.Name}[/].");
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
                player.WriteLine($"You aren't carrying a [red]'{searchName}'[/].");
                return true;
            }

            // 5. THE REVERSE HANDOFF
            // Remove from player's inventory list
            player.PlayerInventory.Items.Remove(itemToDrop);

            // Add to the room's item list
            var room = player.GetRoom();
            room.Items.Add(itemToDrop);

            player.WriteLine($"You [bold red]WHIPPED[/] the [yellow]{itemToDrop.Name}[/] at the floor with the utmost force and velocity you could muster.");
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
    internal class ItemGetCommand : ICommand
    {
        public string Name => "/itemget";
        public IEnumerable<string> Aliases => new List<string> { "Ag" };
        public string Help => "Usage: Ag [Item Name]\nAdds the specified item to your inventory. The item name is case-insensitive and can be found in the Item, Weapon, or Armor Catalogs.";

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player) return false;
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return false;
            }

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
    internal class ItemRemoveCommand : ICommand
    {
        public string Name => "/itemremove";
        public IEnumerable<string> Aliases => new List<string> { "Ar", "rm" };
        public string Help => "Usage: Ar [Item Name]\nTrashes an item from your inventory. Does not affect the global catalog.";

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player) return false;
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return false;
            }
            // 1. Check Input
            if (parameters.Count < 2)
            {
                player.WriteLine("Usage: Ar [Item Name]");
                return true;
            }

            // 2. Get the name user typed (e.g., "Iron Sword")
            // parameters[0] is "Ar", so we grab everything after that.
            List<string> itemWords = parameters.GetRange(1, parameters.Count - 1);
            string searchName = string.Join(" ", itemWords);

            // 3. Search ONLY the Player's Inventory
            // We are NOT looking at GameState.Instance.ItemCatalog here.
            Item? itemToDelete = player.PlayerInventory.Items
                .FirstOrDefault(i => i.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase));

            // 4. Handle Not Found
            if (itemToDelete == null)
            {
                player.WriteLine($"You are not carrying an item named '{searchName}'.");
                return true;
            }

            // 5. Remove the instance
            // This calls the method we fixed in Step 1
            bool success = player.PlayerInventory.RemoveItem(itemToDelete);

            if (success)
            {
                player.WriteLine($"You successfully trashed the {itemToDelete.Name}.");
            }
            else
            {
                player.WriteLine("Something went wrong removing the item.");
            }

            return true;
        }
    }
    internal class GiveCommand : ICommand
    {
        public string Name => "give";
        public IEnumerable<string> Aliases => new List<string> { "transfer", "send" };
        public string Help => "Usage: give [Player Name] [Item Name]\nTransfers an item from your inventory to another player.";

        public bool Execute(Character character, List<string> parameters)
        {
            // Ensure the executor is a Player
            if (character is not Player sourcePlayer) return false;

            // 1. Validate Input
            // Expected syntax: give <PlayerName> <ItemName>
            if (parameters.Count < 3)
            {
                sourcePlayer.WriteLine("Usage: give [Player Name] [Item Name]");
                return true;
            }

            string targetName = parameters[1];
            // Combine all words after the player name to get the item name (e.g., "Iron Sword")
            string itemName = string.Join(" ", parameters.Skip(2));

            // 2. Find the Target Player
            // We use the static method you provided in Player.cs. 
            // This looks through the entire GameState.Instance.Players dictionary.
            if (!Player.TryFindPlayer(targetName, GameState.Instance.Players, out Player? targetPlayer) || targetPlayer == null)
            {
                sourcePlayer.WriteLine($"Could not find a player named '[red]{targetName}[/]'.");
                return true;
            }

            // 3. Self-Check
            if (sourcePlayer == targetPlayer)
            {
                sourcePlayer.WriteLine("You cannot give items to yourself.");
                return true;
            }

            // 4. Find the Item in Source Player's Inventory
            // We look specifically in the 'Items' list of the player initiating the command.
            Item? itemToGive = sourcePlayer.PlayerInventory.Items
                .FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (itemToGive == null)
            {
                sourcePlayer.WriteLine($"You do not have an item named '{itemName}'.");
                return true;
            }

            // 5. Execute Transfer
            // FIRST: Remove the specific instance from the source player.
            // We use the 'internal' overload of RemoveItem(Item item) to ensure we remove this exact object.
            bool removed = sourcePlayer.PlayerInventory.RemoveItem(itemToGive);

            if (removed)
            {
                // SECOND: Add that same instance to the target player.
                targetPlayer.PlayerInventory.Items.Add(itemToGive);

                // 6. Notify Both Parties
                sourcePlayer.WriteLine($"You sent [cyan]{itemToGive.Name}[/] to [green]{targetPlayer.Name}[/].");

                // Because 'targetPlayer' is a valid Player object, we can write directly to their console
                // even if they are halfway across the map.
                targetPlayer.WriteLine($"[green]{sourcePlayer.Name}[/] sent you a [cyan]{itemToGive.Name}[/]!");
            }
            else
            {
                // This catches rare edge cases where the item might have been lost during processing
                sourcePlayer.WriteLine("[red]An error occurred while trying to transfer the item.[/]");
            }

            return true;
        }
    }
}
