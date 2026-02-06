using RPGFramework.Display;
using RPGFramework.Enums;
using RPGFramework.Items;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace RPGFramework.Commands
{
    internal class ItemCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                new ListInventoryCommand(),
                new ItemBuildCommand(),
                new ArmorBuildCommand(),
                new WeaponBuildCommand(),
                // Add more builder commands here as needed
            };
        }
    }

    internal class ListInventoryCommand : ICommand
    {
        public string Name => "inventory";
        public IEnumerable<string> Aliases => new List<string>() { "inv" };
        public string Help => "";
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
            {
                return false;
            }
            player.WriteLine("Inventory:");
            for (int i = 0; i < 16; i++)
            {
                player.WriteLine($"Slot {i}: ");
            }
            return true;
        }

        public bool Execute(Character character, List<int> parameters)
        {
            throw new NotImplementedException();
        }
    }
    internal class ItemBuildCommand : ICommand
    {
        private static Item CreateItemFromTemplate(Item template)
        {
            Item newItem;
            int newId = Utility.GetNextGlobalId();

            if (template is Armor a)
            {
                newItem = new Armor
                {
                    Id = newId,
                    Name = a.Name,
                    Description = a.Description,
                    Slot = a.Slot,
                    Material = a.Material,
                    Type = a.Type,
                    DamageReduction = a.DamageReduction,
                    MaxDurability = a.MaxDurability,
                    Durability = a.MaxDurability
                };
            }
            else if (template is Weapon w)
            {
                newItem = new Weapon
                {
                    Id = newId,
                    Name = w.Name,
                    Description = w.Description,
                    Damage = w.Damage,
                    AttackTime = w.AttackTime,
                    Range = w.Range,
                    Type = w.Type,
                    Material = w.Material
                };
            }
            else
            {
                newItem = new Item { Id = newId, Name = template.Name, Description = template.Description };
            }

            newItem.DisplayText = template.DisplayText;
            newItem.IsGettable = template.IsGettable;
            newItem.IsDroppable = template.IsDroppable;
            return newItem;
        }
        public string Name => "/item";

        public IEnumerable<string> Aliases => Array.Empty<string>();
        public string Help => "";

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
            {
                return false;
            }

            if (parameters.Count < 2)
            {
                WriteUsage(player);
                return false;
            }

            switch (parameters[1].ToLower())
            {
                case "description":
                    ItemSetDescription(player, parameters);
                    break;
                case "name":
                    ItemSetName(player, parameters);
                    break;
                case "create":
                    ItemCreate(player, parameters);
                    break;
                case "delete":
                    ItemDelete(player, parameters);
                    break;
                case "list":
                    ItemList(player);
                    break;
                default:
                    WriteUsage(player);
                    break;
                case "clear":
                    player.GetRoom().Items.Clear();
                    player.WriteLine("[green]Room cleared of all items.[/]");
                    return true;
                case "spawn":
                    if (parameters.Count < 3)
                    {
                        player.WriteLine("Usage: /item spawn <item_name>");
                        return true;
                    }

                    string templateName = string.Join(" ", parameters.Skip(2)).Trim();

                    Item? template = null;
                    if (GameState.Instance.ItemCatalog.TryGetValue(templateName, out var foundItem))
                        template = foundItem;
                    else if (GameState.Instance.WeaponCatalog.TryGetValue(templateName, out var foundWeapon))
                        template = foundWeapon;
                    else if (GameState.Instance.ArmorCatalog.TryGetValue(templateName, out var foundArmor))
                        template = foundArmor;

                    if (template == null)
                    {
                        player.WriteLine($"Could not find '{templateName}' in any catalog.");
                        return true;
                    }

                    Item newItem = CreateItemFromTemplate(template);

                    // Ensure the name is copied to the new instance
                    newItem.Name = template.Name;
                    if (string.IsNullOrEmpty(newItem.DisplayText))
                        newItem.DisplayText = $"{newItem.DisplayText}";

                    player.GetRoom().Items.Add(newItem);
                    player.WriteLine($"You spawned a [yellow]{newItem.Name}[/].");
                    return true;
            }

            return true;
        }

        private static void WriteUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine("/item description '<set item desc to this>'");
            player.WriteLine("/item name '<set item name to this>'");
            player.WriteLine("/item create '<name>' '<description>''");
            player.WriteLine("/item delete '<name>'");
            player.WriteLine("/item list");
            player.WriteLine("/item spawn <name>");
        }

        private static void ItemCreate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }

            // 0: /item
            // 1: create
            // 2: name
            // 3: description

            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /item create '<name>' '<description>'");
                return;
            }
                Item newItem = new Item
                {
                    Name = parameters[2],
                    Description = parameters[3]
                };
            if (GameState.Instance.ItemCatalog.ContainsKey(newItem.Name))
            {

            }
            else
            {
                GameState.Instance.ItemCatalog.Add(newItem.Name, newItem);
            }
                // Here you would typically add the item to a database or game world
                player.WriteLine($"Item '{newItem.Name}' created successfully with description: {newItem.Description}");
            
        }

        private static void ItemDelete(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            // Parameters:
            // 0: /item
            // 1: delete
            // 2: name
            if (parameters.Count < 3)
            {
                player.WriteLine("Usage: /item delete '<name>'");
                return;
            }

            string itemName = parameters[2];

            if (GameState.Instance.ItemCatalog.Remove(itemName))
            {
                player.WriteLine($"Item '{itemName}' was successfully chucked into The Twilight Zone, never to be seen again.");
            }
            else
            {
                player.WriteLine($"Item '{itemName}' not found in the Item Catalog.");
            }
        }

        private static void ItemList(Player player)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            var catalog = GameState.Instance.ItemCatalog;

            if (catalog.Count == 0)
            {
                player.WriteLine("The item catalog is currently empty.");
                return;
            }

            player.WriteLine("Current Item Catalog:");
            foreach (var itemName in catalog.Keys)
            {
                player.WriteLine($"- {itemName}");
            }
        }

        private static void ItemSetDescription(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            var item = GameState.Instance.ItemCatalog[parameters[3]];
            if (item is IDescribable describableItem)
            {
                if (parameters.Count < 4)
                {
                    player.WriteLine(describableItem.Description);
                }
                else
                {
                    describableItem.Description = parameters[3];
                    player.WriteLine("Item description set.");
                }
            }
            else
            {
                player.WriteLine("No item selected or item does not support naming.");
            }
        }

        private static void ItemSetName(Player player, List<string> parameters)
        {
            if (!GameState.Instance.ItemCatalog.TryGetValue(parameters[3], out Item? item) || item == null)
            {
                player.WriteLine("Item not found.");
                return;
            }


            if (parameters.Count < 3)
            {
                // Roundabout, we know Item has a Name, no need for IDescribable here
                // Fix: Avoid possible null reference by using null-coalescing operator
                //player.WriteLine(describableItem.Name?.ToString() ?? string.Empty);
            }
            else
            {
                item.Name = parameters[2];
                player.WriteLine("Item name set.");
            }
        }

        public bool Execute(Character character, List<int> parameters)
        {
            throw new NotImplementedException();
        }
    }
    internal class ArmorBuildCommand : ICommand
    {
        public string Name => "/armor";

        public IEnumerable<string> Aliases => Array.Empty<string>();
        public string Help => "";

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
            {
                return false;
            }

            if (parameters.Count < 2)
            {
                WriteUsage(player);
                return false;
            }

            switch (parameters[1].ToLower())
            {
                case "description":
                    ArmorSetDescription(player, parameters);
                    break;
                case "name":
                    ArmorSetName(player, parameters);
                    break;
                case "create":
                    ArmorCreate(player, parameters);
                    break;
                case "delete":
                    ArmorDelete(player, parameters);
                    break;
                case "list":
                    ArmorList(player);
                    break;
                default:
                    WriteUsage(player);
                    break;
            }

            return true;
        }

        private static void WriteUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine("/armor description '<set item desc to this>'");
            player.WriteLine("/armor name '<set item name to this>'");
            player.WriteLine("/armor create '<name>' '<description>''");
            player.WriteLine("/armor delete '<name>'");
            player.WriteLine("/armor list");
        }

        private static void ArmorCreate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }

            // 0: /item
            // 1: create
            // 2: name
            // 3: description
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /armor create '<name>' '<description>'");
                return;
            }
            Armor newArmor = new Armor
            {
                Name = parameters[2],
                Description = parameters[3]
            };
            if (GameState.Instance.ArmorCatalog.ContainsKey(newArmor.Name))
            {

            }
            else
            {
                GameState.Instance.ArmorCatalog.Add(newArmor.Name, new Armor());
            }
            // Here you would typically add the item to a database or game world
            player.WriteLine($"Armor '{newArmor.Name}' created successfully with description: {newArmor.Description}");

        }

        private static void ArmorDelete(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            // Parameters:
            // 0: /armor
            // 1: delete
            // 2: name
            if (parameters.Count < 3)
            {
                player.WriteLine("Usage: /armor delete '<name>'");
                return;
            }

            string armorName = parameters[2];

            if (GameState.Instance.ArmorCatalog.Remove(armorName))
            {
                player.WriteLine($"You just deleted Armor '{armorName}'...that had to hurt...");
            }
            else
            {
                player.WriteLine($"Armor '{armorName}' doesn't exist...");
            }
        }

        private static void ArmorList(Player player)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            var catalog = GameState.Instance.ArmorCatalog;

            if (catalog.Count == 0)
            {
                player.WriteLine("What the... It looks like the Armor Catalog is currently EMPTY.");
                return;
            }

            player.WriteLine("Current Armor Catalog:");
            foreach (var armorName in catalog.Keys)
            {
                player.WriteLine($"- {armorName}");
            }
        }

        private static void ArmorSetDescription(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            if (parameters.Count < 3)
            {
                //player.WriteLine(player.GetArmor().Description);
            }
            else
            {
                //player.GetArmor().Description = parameters[2];
                player.WriteLine("Armor description set.");
            }
        }

        private static void ArmorSetName(Player player, List<string> parameters)
        {

            if (parameters.Count < 3)
            {
                // player.WriteLine(player.GetArmor().Name);
            }
            else
            {
                //player.GetItem().Name = parameters[2];
                player.WriteLine("Item name set.");
            }
        }

        public bool Execute(Character character, List<int> parameters)
        {
            throw new NotImplementedException();
        }
    }
    internal class WeaponBuildCommand : ICommand
    {
        public string Name => "/weapon";

        public IEnumerable<string> Aliases => Array.Empty<string>();
        public string Help => "";

        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
            {
                return false;
            }

            if (parameters.Count < 2)
            {
                WriteUsage(player);
                return false;
            }

            switch (parameters[1].ToLower())
            {
                case "description":
                    WeaponSetDescription(player, parameters);
                    break;
                case "name":
                    WeaponSetName(player, parameters);
                    break;
                case "create":
                    WeaponCreate(player, parameters);
                    break;
                case "damage":
                    WeaponSetDamage(player, parameters);
                    break;
                case "delete":
                    WeaponDelete(player, parameters);
                    break;
                case "list":
                    WeaponList(player);
                    break;
                default:
                    WriteUsage(player);
                    break;
                
            }

            return true;
        }

        private static void WriteUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine("/weapon description '<set item desc to this>'");
            player.WriteLine("/weapon name '<set item name to this>'");
            player.WriteLine("/weapon create '<name>' '<description>''");
            player.WriteLine("/weapon '<name>' set damage '<set weapon damage to this>'");
            player.WriteLine("/weapon delete '<name>'");
            player.WriteLine("/weapon list");
        }

        private static bool WeaponCreate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return false;
            }

            // 0: /weapon
            // 1: create
            // 2: name
            // 3: description
            // 4: damage
            // 5: attack time
            // 6: range
            // 7: type
            // 8: material
            if (parameters.Count < 9)
            {
                player.WriteLine("Usage: /weapon create '<name>' '<description>' ' <damage>' '<attack time>' '<range>' '<type>' '<material>'");
                return false;
            }

            if (!Int32.TryParse(parameters[4], out int damage))
            {
                player.WriteLine("Invalid damage value.");
                return false;
            }
            if (!Int32.TryParse(parameters[5], out int attackspeed))
            {
                player.WriteLine("Invalid Attack Speed value.");
                return false;
            }
            if (!Int32.TryParse(parameters[6], out int range))
            {
                player.WriteLine("Invalid range value.");
                return false;
            }
            if (!Enum.TryParse(parameters[7], true, out WeaponType type))
            {
                player.WriteLine("Invalid weapon type.");
                return false;
            }
            if (!Enum.TryParse(parameters[8], true, out WeaponMaterial material))
            {
                player.WriteLine("Invalid weapon material.");
                return false;
            }
            Weapon newWeapon = new Weapon
            {
                Name = parameters[2],
                Description = parameters[3],
                Damage = damage,
                AttackTime = attackspeed,
                Range = range,
                Type = type,
                Material = material,
            };
            if (GameState.Instance.WeaponCatalog.ContainsKey(newWeapon.Name))
            {
                player.WriteLine("A weapon with that name already exists.");
                return false;
            }
            else
            {
                GameState.Instance.WeaponCatalog.Add(newWeapon.Name, newWeapon);
                player.WriteLine($"Weapon '{newWeapon.Name}' created successfully with description: {newWeapon.Description}");
                return true;
            }
        }

        private static void WeaponDelete(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            // Parameters:
            // 0: /weapon
            // 1: delete
            // 2: name
            if (parameters.Count < 3)
            {
                player.WriteLine("Usage: /weapon delete '<name>'");
                return;
            }

            string weaponName = parameters[2];

            if (GameState.Instance.WeaponCatalog.Remove(weaponName))
            {
                player.WriteLine($"You have removed Weapon '{weaponName}' from existance");
            }
            else
            {
                player.WriteLine($"Weapon '{weaponName}' doesn't exist");
            }
        }

        private static void WeaponList(Player player)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            var catalog = GameState.Instance.WeaponCatalog;

            if (catalog.Count == 0)
            {
                player.WriteLine("The Weapon Catalog is a blank, empty page.");
                return;
            }

            player.WriteLine("Current Weapon Catalog:");
            foreach (var weaponName in catalog.Keys)
            {
                player.WriteLine($"- {weaponName}");
            }
        }

        // Here you would typically add the item to a database or game world

        // 0: /weapon
        // 1: 'Weapon name'
        // 2: set
        // 3: (property name)
        // 4: (property value)

        private static void WeaponSetName(Player player, List<string> parameters)
        {

            if (parameters.Count < 2)
            {
                player.WriteLine("You must specify a weapon name.");
                return;
            }
            else
            {
                // = GameState.Instance.WeaponCatalog[parameters[2]];
                player.WriteLine("Weapon name set (but not really yet).");
            }
        }
        private static void WeaponSetDescription(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 3)
            {
                player.WriteLine("Not enough parameters");
                return;
            }

            var weapon = GameState.Instance.WeaponCatalog[parameters[2]];
                if (weapon == null)
                {
                    player.WriteLine("No weapon selected.");
                    return;
                }

                if (parameters.Count < 4)
                {
                    player.WriteLine(weapon.Description);
                }
                else
                {
                    weapon.Description = parameters[3];
                    player.WriteLine("Weapon description set.");
                }
        }
        private static void WeaponSetDamage(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 4)
            {
                player.WriteLine("Not enough parameters");
                return;
            }

            var weapon = GameState.Instance.WeaponCatalog[parameters[2]];
            if (weapon == null)
            {
                player.WriteLine("No weapon selected.");
                return;
            }

            if (parameters.Count < 5)
            {
                player.WriteLine($"Weapon Damage:{weapon.Damage}");
                return;
            }
            else
            {
            
            }
        }



        public bool Execute(Character character, List<int> parameters)
        {
            throw new NotImplementedException();
        }
    }
}

