using RPGFramework.Display;
using RPGFramework.Enums;
using RPGFramework.Items;
using System;
using System.Collections.Generic;
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
        public string Name => "/item";

        public IEnumerable<string> Aliases => Array.Empty<string>();

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
                default:
                    WriteUsage(player);
                    break;
            }

            return true;
        }

        private static void WriteUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine("/item description '<set item desc to this>'");
            player.WriteLine("/item name '<set item name to this>'");
            player.WriteLine("/item create '<name>' '<description>''");
        }

        private static void ItemCreate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.PlayerRole.ToString());
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

                // Here you would typically add the item to a database or game world
                player.WriteLine($"Item '{newItem.Name}' created successfully with description: {newItem.Description}");
            
        }

        private static void ItemSetDescription(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                return;
            }

            if (parameters.Count < 3)
            {
                //player.WriteLine(player.GetItem().Description);
            }
            else
            {
                //player.GetItem().Description = parameters[2];
                player.WriteLine("Room description set.");
            }
        }

        private static void ItemSetName(Player player, List<string> parameters)
        {

            if (parameters.Count < 3)
            {
                player.WriteLine(player.GetRoom().Name);
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
    internal class ArmorBuildCommand : ICommand
    {
        public string Name => "/armor";

        public IEnumerable<string> Aliases => Array.Empty<string>();

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
        }

        private static void ArmorCreate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.PlayerRole.ToString());
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

            // Here you would typically add the item to a database or game world
            player.WriteLine($"Armor '{newArmor.Name}' created successfully with description: {newArmor.Description}");

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
        }

        private static void WeaponCreate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.PlayerRole.ToString());
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
            Weapon newWeapon = new Weapon
            {
                Name = parameters[2],
                Description = parameters[3]
            };

            // Here you would typically add the item to a database or game world
            player.WriteLine($"Weapon '{newWeapon.Name}' created successfully with description: {newWeapon.Description}");

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
                //player.WriteLine(player.GetWeapon().Description);
            }
            else
            {
                //player.GetWeapon().Description = parameters[2];
                player.WriteLine("Weapon description set.");
            }
        }

        private static void WeaponSetName(Player player, List<string> parameters)
        {

            if (parameters.Count < 3)
            {
                // player.WriteLine(player.GetWeapon().Name);
            }
            else
            {
                //player.GetWeapon().Name = parameters[2];
                player.WriteLine("Weapon name set.");
            }
        }

        public bool Execute(Character character, List<int> parameters)
        {
            throw new NotImplementedException();
        }
    }



}

