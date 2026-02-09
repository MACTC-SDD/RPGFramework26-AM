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
                new ItemBuildCommand(),
                new ArmorBuildCommand(),
                new WeaponBuildCommand(),
                // Add more builder commands here as needed
            };
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
            player.WriteLine("Usage: /item create '<name>' '<description>' '<isdroppable>' '<isgettable>' '<isstackable>' '<level>' '<value>' '<weight>'");
            player.WriteLine("/item description '<set item desc to this>'");
            player.WriteLine("/item name '<set item name to this>'");
            player.WriteLine("/item create '<name>' '<description>''");
            player.WriteLine("/item delete '<name>'");
            player.WriteLine("/item list");
            player.WriteLine("/item spawn <name>");
        }

        private static bool ItemCreate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return false;
            }

            // 0: /item
            // 1: create
            // 2: name
            // 3: description
            // 4: IsDroppable?
            // 5: IsGettable?
            // 6: IsStackable?
            // 7: Level
            // 8: Value
            // 9: Weight
            if (parameters.Count < 5)
            {
                player.WriteLine("Usage: /item create '<name>' '<description>'");
                return false;
            }
            if (!bool.TryParse(parameters[4], out bool isdroppable))
            {
                player.WriteLine("Invalid IsDroppable value.");
                return false;
            }
            if (!bool.TryParse(parameters[5], out bool isgettable))
            {
                player.WriteLine("Invalid IsGettable value.");
                return false;
            }
            if (!bool.TryParse(parameters[6], out bool isstackable))
            {
                player.WriteLine("Invalid IsStackable value.");
                return false;
            }
            if (!Int32.TryParse(parameters[7], out int level))
            {
                player.WriteLine("Invalid level value.");
                return false;
            }
            if (!Int32.TryParse(parameters[8], out int value))
            {
                player.WriteLine("Invalid value value.");
                return false;
            }
            if (!Int32.TryParse(parameters[9], out int weight))
            {
                player.WriteLine("Invalid weight value.");
                return false;
            }
            Item newItem = new Item
            {
                Name = parameters[2],
                Description = parameters[3],
                IsDroppable = isdroppable,
                IsGettable = isgettable,
                IsStackable = isstackable,
                Level = level,
                Value = value,
                Weight = weight
            };
            if (GameState.Instance.ItemCatalog.ContainsKey(newItem.Name))
            {
                player.WriteLine("A weapon with that name already exists.");
                return false;
            }
            else
            {
                GameState.Instance.ItemCatalog.Add(newItem.Name, newItem);
                player.WriteLine($"Weapon '{newItem.Name}' created successfully with description: {newItem.Description}");
                return true;
            }
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
                case "material":
                    ArmorSetMaterial(player, parameters);
                    break;
                case "slot":
                    ArmorSetSlot(player, parameters);
                    break;
                case "type":
                    ArmorSetType(player, parameters);
                    break;
                case "damage":
                    ArmorSetDamageReduction(player, parameters);
                    break;
                case "durability":
                    ArmorSetDurability(player, parameters);
                    break;
                case "dodge":
                    ArmorSetDodgeChance(player, parameters);
                    break;
                case "health":
                    ArmorSetHealthBonus(player, parameters);
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
            player.WriteLine("Usage: /armor material '<armor name>' '<new material>'");
            player.WriteLine("Usage: /armor type '<armor name>' '<new type>'");
            player.WriteLine("Usage: /armor damage '<armor name>' '<new damage reduction>'");
            player.WriteLine("Usage: /armor durability '<armor name>' '<new durability>'");
            player.WriteLine("Usage: /armor dodge '<armor name>' '<new dodge chance>'");
            player.WriteLine("/armor delete '<name>'");
            player.WriteLine("/armor list");
        }

        private static bool ArmorCreate(Player player, List<string> parameters)
        {
            // 1. Permission Check
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return false;
            }

            // 2. Check for the minimum required arguments (Name + Desc)
            // 0: /armor, 1: create, 2: name, 3: description
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /armor create '<name>' '<description>'");
                player.WriteLine("Optional: ... '<material>' '<slot>' '<type>' '<dmg>' '<durability>' '<dodge>' '<health>'");
                return false;
            }

            // 3. Set up DEFAULT values
            // These will be used if the user ONLY types the name and description.
            ArmorMaterial material = ArmorMaterial.Cloth; // Default
            ArmorSlot slot = ArmorSlot.Chest;             // Default
            ArmorType type = ArmorType.Light;             // Default
            int damagereduction = 0;
            int durability = 100;
            int maxdurability = durability;
            float dodgechance = 0.0f;
            float healthbonus = 0.0f;

            // 4. ONLY parse the extra stats if the user provided them (11 arguments total)
            if (parameters.Count >= 11)
            {
                if (!Enum.TryParse(parameters[4], true, out material))
                {
                    player.WriteLine($"Invalid armor material: {parameters[4]}");
                    return false;
                }
                if (!Enum.TryParse(parameters[5], true, out slot))
                {
                    player.WriteLine($"Invalid armor slot: {parameters[5]}");
                    return false;
                }
                if (!Enum.TryParse(parameters[6], true, out type))
                {
                    player.WriteLine($"Invalid armor type: {parameters[6]}");
                    return false;
                }
                if (!Int32.TryParse(parameters[7], out damagereduction))
                {
                    player.WriteLine($"Invalid damage reduction: {parameters[7]}");
                    return false;
                }
                if (!Int32.TryParse(parameters[8], out durability))
                {
                    player.WriteLine($"Invalid durability: {parameters[8]}");
                    return false;
                }
                if (!float.TryParse(parameters[9], out dodgechance))
                {
                    player.WriteLine($"Invalid dodge chance: {parameters[9]}");
                    return false;
                }
                if (!float.TryParse(parameters[10], out healthbonus))
                {
                    player.WriteLine($"Invalid health bonus: {parameters[10]}");
                    return false;
                }
            }
            // Safety check: If they typed more than just Name/Desc, but not enough for full stats
            else if (parameters.Count > 4)
            {
                player.WriteLine("Error: You provided some stats but missed others.");
                player.WriteLine("Usage: /armor create '<name>' '<description>' '<material>' '<slot>' '<type>' '<dmgreduction>' '<maxdurability>' '<dodge chance>' '<health bonus>'");
                return false;
            }

            // 5. Create the Object
            // It will use either the defaults we set at step 3, OR the parsed values from step 4
            Armor newArmor = new Armor
            {
                Name = parameters[2],
                Description = parameters[3],
                Material = material,
                Slot = slot,
                Type = type,
                DamageReduction = damagereduction,
                MaxDurability = durability,
                DodgeChance = dodgechance,
                HealthBonus = healthbonus
            };

            // 6. Save to Catalog
            if (GameState.Instance.ArmorCatalog.ContainsKey(newArmor.Name))
            {
                player.WriteLine("An armor with that name already exists.");
                return false;
            }
            else
            {
                GameState.Instance.ArmorCatalog.Add(newArmor.Name, newArmor);
                player.WriteLine($"Armor '{newArmor.Name}' created successfully!");

                // Let the user know if defaults were used
                if (parameters.Count == 4)
                {
                    player.WriteLine("(Created with default stats because none were provided)");
                }
                return true;
            }
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
        private static void ArmorSetMaterial (Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /armor material '<armor name>' '<new material>'");
                return;
            }
            string targetName = parameters[2];
            string materialInput = parameters[3];

            if (targetName == null)
            {
                player.WriteLine("No armor selected.");
                return;
            }

            if (Enum.TryParse(materialInput, true, out ArmorMaterial newMaterial))
            {
                // 3. Update the existing item
                GameState.Instance.ArmorCatalog[targetName].Material = newMaterial;

                player.WriteLine($"Successfully changed '{targetName}' material to {newMaterial}.");
            }
            else
            {
                // Helpful error message listing valid options
                string validOptions = string.Join(", ", Enum.GetNames(typeof(ArmorMaterial)));
                player.WriteLine($"'{materialInput}' is not a valid material.");
                player.WriteLine($"Valid options are: {validOptions}");
            }
        }
        private static void ArmorSetType(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /armor type '<armor name>' '<new type>'");
                return;
            }
            string targetName = parameters[2];
            string armortype = parameters[3];

            if (targetName == null)
            {
                player.WriteLine("No armor selected.");
                return;
            }

            if (Enum.TryParse(armortype, true, out ArmorType newArmortype))
            {
                // 3. Update the existing item
                GameState.Instance.ArmorCatalog[targetName].Type = newArmortype;

                player.WriteLine($"Successfully changed '{targetName}' armor type to {newArmortype}.");
            }
            else
            {
                // Helpful error message listing valid options
                string validOptions = string.Join(", ", Enum.GetNames(typeof(ArmorMaterial)));
                player.WriteLine($"'{armortype}' is not a valid material.");
                player.WriteLine($"Valid options are: {validOptions}");
            }
        }
        private static void ArmorSetSlot(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /armor slot '<armor name>' '<new slot>'");
                return;
            }
            string targetName = parameters[2];
            string armorslot = parameters[3];

            if (targetName == null)
            {
                player.WriteLine("No armor selected.");
                return;
            }

            if (Enum.TryParse(armorslot, true, out ArmorSlot newArmorslot))
            {
                // 3. Update the existing item
                GameState.Instance.ArmorCatalog[targetName].Slot = newArmorslot;

                player.WriteLine($"Successfully changed '{targetName}' armor type to {newArmorslot}.");
            }
            else
            {
                // Helpful error message listing valid options
                string validOptions = string.Join(", ", Enum.GetNames(typeof(ArmorMaterial)));
                player.WriteLine($"'{armorslot}' is not a valid material.");
                player.WriteLine($"Valid options are: {validOptions}");
            }
        }
        private static void ArmorSetDamageReduction(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /armor damage '<armor name>' '<new damage reduction>'");
                return;
            }
            string targetName = parameters[2];
            string damagereduction = parameters[3];

            if (targetName == null)
            {
                player.WriteLine("No armor selected.");
                return;
            }

            if (int.TryParse(damagereduction, out int newDamageReduction))
            {
                // 3. Update the existing item
                GameState.Instance.ArmorCatalog[targetName].DamageReduction = newDamageReduction;

                player.WriteLine($"Successfully changed '{targetName}' damage reduction to {newDamageReduction}.");
            }
            else
            {
                // Helpful error message listing valid options
                player.WriteLine($"'{damagereduction}' is not a valid damage reduction.");
               
            }
        }
        private static void ArmorSetDurability(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /armor durability '<armor name>' '<new durability>'");
                return;
            }
            string targetName = parameters[2];
            string durability = parameters[3];
            string maxdurability = durability;

            if (targetName == null)
            {
                player.WriteLine("No armor selected.");
                return;
            }

            if (int.TryParse(durability, out int newDurability))
            {
                // 3. Update the existing item
                GameState.Instance.ArmorCatalog[targetName].Durability = newDurability;
                GameState.Instance.ArmorCatalog[targetName].MaxDurability = newDurability;

                player.WriteLine($"Successfully changed '{targetName}' durability to {newDurability}.");
                player.WriteLine($"Successfully changed '{targetName}' max durability to {newDurability}.");
            }
            else
            {
                // Helpful error message listing valid options
                player.WriteLine($"'{durability}' is not a valid durability.");

            }
        }
        private static void ArmorSetDodgeChance(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /armor dodge '<armor name>' '<new dodge chance>'");
                return;
            }
            string targetName = parameters[2];
            string dodgechance = parameters[3];

            if (targetName == null)
            {
                player.WriteLine("No armor selected.");
                return;
            }

            if (float.TryParse(dodgechance, out float newDodgeChance))
            {
                // 3. Update the existing item
                GameState.Instance.ArmorCatalog[targetName].DodgeChance = newDodgeChance;

                player.WriteLine($"Successfully changed '{targetName}' dodge chance to {newDodgeChance}.");
            }
            else
            {
                // Helpful error message listing valid options
                player.WriteLine($"'{dodgechance}' is not a valid dodge chance.");

            }
        }
        private static void ArmorSetHealthBonus(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /armor dodge '<armor name>' '<new dodge chance>'");
                return;
            }
            string targetName = parameters[2];
            string healthbonus = parameters[3];

            if (targetName == null)
            {
                player.WriteLine("No armor selected.");
                return;
            }

            if (float.TryParse(healthbonus, out float newHealthBonus))
            {
                // 3. Update the existing item
                GameState.Instance.ArmorCatalog[targetName].HealthBonus = newHealthBonus;

                player.WriteLine($"Successfully changed '{targetName}' health bonus to {newHealthBonus}.");
            }
            else
            {
                // Helpful error message listing valid options
                player.WriteLine($"'{healthbonus}' is not a valid dodge chance.");

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
                case "material":
                    WeaponSetMaterial(player, parameters);
                    break;
                case "type":
                    WeaponSetType(player, parameters);
                    break;
                case "durability":
                    WeaponSetDurability(player, parameters);
                    break;
                case "isdroppable":
                    WeaponIsDroppable(player, parameters);
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
            player.WriteLine("Usage: /weapon material '<weapon name>' '<new material>'");
            player.WriteLine("Usage: /weapon type '<weapon name>' '<new type>'");
            player.WriteLine("Usage: /weapon durability '<weapon name>' '<new durability>'");
            player.WriteLine("Usage: /weapon create '<name>' '<desc>' <dmg> <speed> <range> <type> <material>");
            player.WriteLine("Usage: /weapon isdroppable '<weapon name>' '<new droppable>'");
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
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /weapon create '<name>' '<description>'");
                player.WriteLine("Optional: ... '<isdroppable>' '<isgettable>' '<isstackable>' '<level>' '<value>' '<value>' '<weight>' '<damage>' '<attackspeed>' '<range>'");
                return false;
            }
            // 3. Set up DEFAULT values
            // These will be used if the user ONLY types the name and description.



            WeaponMaterial material = WeaponMaterial.Iron; // Default
            WeaponType type = WeaponType.Sword;             // Default
            bool isdroppable = false;
            bool isgettable = true;
            bool isstackable = false;
            int level = 1;
            int value = 10;
            int weight = 5;
            int damage = 1;
            int attackspeed = 1;
            int range = 1;

            if (parameters.Count >= 15) { 
            if (!bool.TryParse(parameters[4], out  isdroppable))
            {
                player.WriteLine("Invalid IsDroppable value.");
                return false;
            }
            if (!bool.TryParse(parameters[5], out isgettable))
            {
                player.WriteLine("Invalid IsGettable value.");
                return false;
            }
            if (!bool.TryParse(parameters[6], out isstackable))
            {
                player.WriteLine("Invalid IsStackable value.");
                return false;
            }
            if (!Int32.TryParse(parameters[7], out  level))
            {
                player.WriteLine("Invalid level value.");
                return false;
            }
            if (!Int32.TryParse(parameters[8], out  value))
            {
                player.WriteLine("Invalid value value.");
                return false;
            }
            if (!Int32.TryParse(parameters[9], out  weight))
            {
                player.WriteLine("Invalid weight value.");
                return false;
            }
            if (!Int32.TryParse(parameters[10], out  damage))
            {
                player.WriteLine("Invalid damage value.");
                return false;
            }
            if (!Int32.TryParse(parameters[11], out attackspeed))
            {
                player.WriteLine("Invalid Attack Speed value.");
                return false;
            }
            if (!Int32.TryParse(parameters[12], out range))
            {
                player.WriteLine("Invalid range value.");
                return false;
            }
            if (!Enum.TryParse(parameters[13], true, out  type))
            {
                player.WriteLine("Invalid weapon type.");
                return false;
            }
            if (!Enum.TryParse(parameters[14], true, out material))
            {
                player.WriteLine("Invalid weapon material.");
                return false;
            }
        }
            else if (parameters.Count > 4)
            {
                player.WriteLine("Error: You provided some stats but missed others.");
                player.WriteLine("Usage: /armor create '<name>' '<description>' '<isdroppable>' '<isgettable>' '<isstackable>' '<level>' '<value>' '<value>' '<weight>' '<damage>' '<attackspeed>' '<range>'");
                return false;
            }

            
            Weapon newWeapon = new Weapon
            {
                Name = parameters[2],
                Description = parameters[3],
                IsDroppable = isdroppable,
                IsGettable = isgettable,
                IsStackable = isstackable,
                Level = level,

                Value = value,
                Weight = weight,
                Damage = damage,
                AttackTime = attackspeed,
                Range = range,
                Type = type,
                Material = material,
            };

            if (GameState.Instance.WeaponCatalog.ContainsKey(newWeapon.Name))
            {
                player.WriteLine("An weapon with that name already exists.");
                return false;
            }
            else
            {
                GameState.Instance.WeaponCatalog.Add(newWeapon.Name, newWeapon);
                player.WriteLine($"Weapon '{newWeapon.Name}' created successfully!");

                // Let the user know if defaults were used
                if (parameters.Count == 4)
                {
                    player.WriteLine("(Created with default stats because none were provided)");
                }
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
                player.WriteLine("not enough vals");
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

        private static void WeaponSetMaterial(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /weapon material '<weapon name>' '<new material>'");
                return;
            }
            string targetName = parameters[2];
            string materialInput = parameters[3];

            if (targetName == null)
            {
                player.WriteLine("No weapon selected.");
                return;
            }

            if (Enum.TryParse(materialInput, true, out WeaponMaterial newMaterial))
            {
                // 3. Update the existing item
                GameState.Instance.WeaponCatalog[targetName].Material = newMaterial;

                player.WriteLine($"Successfully changed '{targetName}' material to {newMaterial}.");
            }
            else
            {
                // Helpful error message listing valid options
                string validOptions = string.Join(", ", Enum.GetNames(typeof(WeaponMaterial)));
                player.WriteLine($"'{materialInput}' is not a valid material.");
                player.WriteLine($"Valid options are: {validOptions}");
            }
        }
        private static void WeaponSetType(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /weapon type '<weapon name>' '<new type>'");
                return;
            }
            string targetName = parameters[2];
            string weapontype = parameters[3];

            if (targetName == null)
            {
                player.WriteLine("No weapon selected.");
                return;
            }

            if (Enum.TryParse(weapontype, true, out WeaponType newWeapontype))
            {
                // 3. Update the existing item
                GameState.Instance.WeaponCatalog[targetName].Type = newWeapontype;

                player.WriteLine($"Successfully changed '{targetName}' weapon type to {newWeapontype}.");
            }
            else
            {
                // Helpful error message listing valid options
                string validOptions = string.Join(", ", Enum.GetNames(typeof(WeaponMaterial)));
                player.WriteLine($"'{weapontype}' is not a valid material.");
                player.WriteLine($"Valid options are: {validOptions}");
            }
        }

        private static void WeaponSetDurability(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /weapon durability '<weapon name>' '<new durability>'");
                return;
            }
            string targetName = parameters[2];
            string durability = parameters[3];

            if (string.IsNullOrEmpty(targetName))
            {
                player.WriteLine("No weapon selected.");
                return;
            }

            if (int.TryParse(durability, out int newDurability))
            {
                if (GameState.Instance.WeaponCatalog.TryGetValue(targetName, out Weapon? weapon) && weapon != null)
                {
                    weapon.Durability = newDurability;
                    weapon.MaxDurability = newDurability;

                    player.WriteLine($"Successfully changed '{targetName}' durability to {newDurability}.");
                    player.WriteLine($"Successfully changed '{targetName}' max durability to {newDurability}.");
                }
                else
                {
                    player.WriteLine($"Weapon '{targetName}' not found.");
                }
            }
            else
            {
                player.WriteLine($"'{durability}' is not a valid damage reduction.");
            }
        }
        private static void WeaponIsDroppable (Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /weapon isdroppable '<weapon name>' '<new isdroppable>'");
                return;
            }
            string targetName = parameters[2];
            string isdroppable = parameters[3];

            if (targetName == null)
            {
                player.WriteLine("No weapon selected.");
            }
        }



        public bool Execute(Character character, List<int> parameters)
       {
            throw new NotImplementedException();
        }
    }
}

