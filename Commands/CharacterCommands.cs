using RPGFramework.Enums;
using RPGFramework.Geography;
using RPGFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace RPGFramework.Commands
{
    internal class CharacterCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return
            [
                new MobBuilderCommand(),
                new NpcBuilderCommand(),
                new ShopKeepBuilderCommand(),
                new PlayerShopCommand(),
                // Add more commands here as needed
            ];
        }
    }

    #region MobBuilderCommand Class
    /*Creates, deletes, lists, and modifies mobs in the game world.*/
    internal class MobBuilderCommand : BaseNpcCommand, ICommand
    {
        public string Name => "/mob";

        public IEnumerable<string> Aliases => [];
        public string Help => "";

        public bool Execute(Character character, List<string> parameters)
        {
            _catalog = GameState.Instance.MobCatalog;
            _entityName = "mob";
            _entityType = typeof(Mob);

            if (character is not Player player)
            {
                return false;
            }
            if (Utility.CheckPermission(player, PlayerRole.Admin) == false)
            {
                player.WriteLine("You do not have permission to do that.");
                return false;
            }

            if (parameters.Count < 2)
            {
                WriteUsage(player);
                return false;
            }
            switch (parameters[1].ToLower())
            {
                case "create":
                    NpcCreate(player, parameters);
                    break;
                case "delete":
                    return NpcDelete(player, parameters);
                case "tag":
                    NpcTag(player, parameters);
                    break;
                case "list":
                    ListMobs(player);
                    break;
                case "set":
                    return SetNpcProperty(player, parameters);
                case "add":
                    if (parameters[2].Equals("armour"))
                    { 
                        AddArmour(player, parameters);
                    }
                    else if (parameters[2].Equals("item"))
                    {
                        GiveItem(player, parameters);
                    }
                    break;
                case "remove":
                    if (parameters[2].Equals("armour"))
                    {
                        RemoveArmour(player, parameters);
                    }
                    else if (parameters[2].Equals("item"))
                    {
                        RemoveItem(player, parameters);
                    }
                    break;
                case "level":
                    NonPlayer? npc = CheckForCatalogAndObject(player, parameters[2]);
                    if (npc == null)
                    {
                        break;
                    }
                    if (parameters[2].Equals("check"))
                    {
                        NonPlayer? npc2 = CheckForCatalogAndObject(player, parameters[3]);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        player.WriteLine($"{npc2.Name} is currently level {npc2.Level}");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                        break;
                    }
                    int amount;
                    int.TryParse(parameters[3], out amount);
                    npc.LevelUp(amount);
                    break;
                default:
                    WriteUsage(player);
                    break;
            }

            return true;
        }

        // CODE REVIEW: Shelton (PR #25) - NOTE, if you use /// for comments they will get included
        // in the editor intellisense. See below.
        //Prints all available commands.
        /// <summary>
        /// Displays a list of available mob-related commands and their usage to the specified player.
        /// </summary>

        private static void ListMobs(Player player)
        {
            foreach (var mob in GameState.Instance.MobCatalog)
            {
                player.WriteLine($"Mob Name: {mob.Value.Name} Description: {mob.Value.Description}");
            }
            return;
        }
    }
    #endregion

    #region NpcBuilderCommand Class
    internal class NpcBuilderCommand : BaseNpcCommand, ICommand
    {
        public string Name => "/npc";

        public IEnumerable<string> Aliases => [];
        public string Help => "";

        public bool Execute(Character character, List<string> parameters)
        {
            _catalog = GameState.Instance.NPCCatalog;
            _entityName = "npc";
            _entityType = typeof(NonPlayer);

            if (character is not Player player)
            {
                return false;
            }

            if (parameters.Count < 2)
            {
                WriteUsage(player);
                return false;
            }

            //Switches between the second parameter to determine command.
            switch (parameters[1].ToLower())
            {
                case "create":
                    NpcCreate(player, parameters);
                    break;
                case "delete":
                    NpcDelete(player, parameters);
                    break;
                case "list":
                    ListNpcs(player);
                    break;
                case "tag":
                    NpcTag(player, parameters);
                    break;
                case "set":
                    return SetNpcProperty(player, parameters);
                case "dialog":
                    NpcDialogCommands(player, parameters);
                    break;
                case "add":
                    if (parameters[2].Equals("armour"))
                    {
                        AddArmour(player, parameters);
                    }
                    else if (parameters[2].Equals("item"))
                    {
                        GiveItem(player, parameters);
                    }
                    break;
                case "remove":
                    if (parameters[2].Equals("armour"))
                    {
                        RemoveArmour(player, parameters);
                    }
                    else if (parameters[2].Equals("item"))
                    {
                        RemoveItem(player, parameters);
                    }
                    break;
                case "level":
                    NonPlayer? npc = CheckForCatalogAndObject(player, parameters[2]);
                    if (npc == null)
                    {
                        break;
                    }
                    if (parameters[2].Equals("check"))
                    {
                        player.WriteLine($"{npc.Name} is currently level {npc.Level}");
                        break;
                    }
                    int amount;
                    int.TryParse(parameters[3], out amount);
                    npc.LevelUp(amount);
                    player.WriteLine("Succesfully leveled up!");
                    break;
                default:
                    WriteUsage(player);
                    break;
            }
            return false;
        }
        private static void ListNpcs(Player player)
        {
            foreach (var npc in GameState.Instance.NPCCatalog)
            {
                player.WriteLine($"Npc Name: {npc.Value.Name} Description: {npc.Value.Description}");
            }
            return;
        }
    }
    #endregion

    #region ShopKeepBuilderCommand Class
    internal class ShopKeepBuilderCommand : BaseNpcCommand, ICommand
    {

        public string Name => "/shopkeep";

        public IEnumerable<string> Aliases => [];
        public string Help => "";

        public bool Execute(Character character, List<string> parameters)
        {
            _catalog = GameState.Instance.ShopkeepCatalog;
            _entityName = "shopkeep";
            _entityType = typeof(Shopkeep);

            if (character is not Player player)
            {
                return false;
            }

            if (parameters.Count < 2)
            {
                WriteUsage(player);
                return false;
            }

            //Switches between the second parameter to determine command.
            switch (parameters[1].ToLower())
            {
                case "create":
                    NpcCreate(player, parameters);
                    break;
                case "delete":
                    return NpcDelete(player, parameters);
                case "list":
                    ListShopKeeps(player);
                    break;
                case "set":
                    return SetNpcProperty(player, parameters);
                case "tag":
                    NpcTag(player, parameters);
                    break;
                case "inventory":
                    if (parameters[2].Equals("add"))
                    {
                        return AddItem(player, parameters);
                    }
                    break;
                case "dialog":
                    NpcDialogCommands(player, parameters);
                    break;
                case "add":
                    if (parameters[2].Equals("armour"))
                    {
                        AddArmour(player, parameters);
                    }
                    else if (parameters[2].Equals("item"))
                    {
                        GiveItem(player, parameters);
                    }
                    break;
                case "remove":
                    if (parameters[2].Equals("armour"))
                    {
                        RemoveArmour(player, parameters);
                    }
                    else if (parameters[2].Equals("item"))
                    {
                        RemoveItem(player, parameters);
                    }
                    break;
                case "level":
                    NonPlayer? npc = CheckForCatalogAndObject(player, parameters[2]);
                    if(npc == null)
                    {
                        break;
                    }
                    if (parameters[2].Equals("check"))
                    {
                        player.WriteLine($"{npc.Name} is currently level {npc.Level}");
                        break;
                    }
                    int amount;
                    int.TryParse(parameters[3], out amount);
                    npc.LevelUp(amount);
                    break;
                default:
                    WriteUsage(player);
                    break;
            }

            return false;
        }
        private static void ListShopKeeps(Player player)
        {
            foreach (var shop in GameState.Instance.ShopkeepCatalog)
            {
                player.WriteLine($"Shop Name: {shop.Value.Name} Description: {shop.Value.Description}");
            }
            return;
        }
        protected static bool AddItem(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
            {
                player.WriteLine("Usage: /shopkeep inventory add '<character'> '<itemID>' '<amount>'");
                return false;
            }

            if (parameters[0].Equals("/shopkeep"))
            {

                //Adds one to quantity if it exists already
                if (GameState.Instance.ShopkeepCatalog.ContainsKey(parameters[3]))
                {
                    Shopkeep? shop = (Shopkeep?)CheckForCatalogAndObject(player, parameters[3]);
                    if (shop == null)
                        return false;
                    
                    string itemID = parameters[4];
                    if(GameState.Instance.ItemCatalog.ContainsKey(itemID) == false)
                    {
                        player.WriteLine($"Item with ID '{itemID}' does not exist in the item catalog.");
                        return false;
                    }
                    if (shop.ShopInventory.ContainsKey(itemID))
                    {
                        int amount;
                        int.TryParse(parameters[5],out amount);
                        shop.IncrementItemQuantity(itemID, amount);
                        player.WriteLine("Added one of the item to the inventory!");
                    }
                    else
                    {
                        int itemAmount;
                        int.TryParse(parameters[5], out itemAmount);
                        shop.AddItemToInventory(itemID, itemAmount);
                        player.WriteLine("Item added to inventory!");
                    }
                    return true;
                }
                else
                {
                    player.WriteLine("Shopkeep does not exist!");
                    return false;
                }
            }
            return false;
        }
    }
    #endregion

    #region PlayerShopCommand Class
    internal class PlayerShopCommand : BaseNpcCommand, ICommand
    {

        public string Name => "shop";

        public IEnumerable<string> Aliases => [];
        public string Help => "";

        public bool Execute(Character character, List<string> parameters)
        {
            _catalog = GameState.Instance.ShopkeepCatalog;
            _entityName = "shop";
            _entityType = typeof(Shopkeep);

            if (character is not Player player)
            {
                return false;
            }

            if (parameters.Count < 2)
            {
                WriteUsageShop(player);
                return false;
            }

            //Switches between the second parameter to determine command.
            switch (parameters[1].ToLower())
            {
                case "list":
                    // List all shopkeeps or a specific shopkeep's inventory
                    if (parameters.Count == 2)
                    {
                        ListShopKeeps(player);
                    }
                    else if (parameters.Count == 3)
                    {
                        ListShopInventory(player, parameters[2]);
                    }
                    break;
                case "buy":
                    SellItemToPlayer(player, parameters);
                    break;
                default:
                    WriteUsageShop(player);
                    break;
            }

            return false;
        }

        protected static void WriteUsageShop(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine($"{_entityName} list");
            player.WriteLine($"{_entityName} list '<shopkeep>'");
            player.WriteLine($"{_entityName} buy '<shopkeep>' '<ItemName>' '<amount>'");
            player.WriteLine($"{_entityName} sell '<shopkeep>' '<ItemName>' '<amount>'");
        }

        protected static void ListShopKeeps(Player player)
        {
            player.WriteLine("Available Shopkeeps:");
            Room room = player.GetRoom();
            foreach (var shop in room.Npcs)
            {
                player.WriteLine($"Name: {shop.Name} Description: {shop.Description}");
            }
        }

        protected static void ListShopInventory(Player player, string shopkeepName)
        {
            Room room = player.GetRoom();
            NonPlayer? npc = room.GetNpcByName(shopkeepName);
            if (npc == null || npc is not Shopkeep shopkeep)
            {
                player.WriteLine($"Shopkeep '{shopkeepName}' not found in this room.");
                return;
            }
            player.WriteLine($"Inventory for Shopkeep '{shopkeep.Name}':");
            foreach (var itemEntry in shopkeep.ShopInventory)
            {
                string itemId = itemEntry.Key;
                int quantity = itemEntry.Value;
                if (GameState.Instance.ItemCatalog.ContainsKey(itemId))
                {
                    Item item = GameState.Instance.ItemCatalog[itemId];
                    player.WriteLine($"Item: {item.Name}, Description: {item.Description}, Quantity: {quantity}, Price: {item.Value} gold each");
                }
                else
                {
                    player.WriteLine($"Item ID: {itemId} (details not found), Quantity: {quantity}");
                }
            }
        }

        protected static void SellItemToPlayer(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
            {
                player.WriteLine("Usage: shop buy '<shopkeep>' '<ItemName>' '<amount>'");
                return;
            }
            string shopkeepName = parameters[2];
            string itemName = parameters[3];
            int amount;
            if (!int.TryParse(parameters[4], out amount) || amount <= 0)
            {
                player.WriteLine("Invalid amount specified.");
                return;
            }
            Room room = player.GetRoom();
            NonPlayer? npc = room.GetNpcByName(shopkeepName);
            if (npc == null || npc is not Shopkeep shopkeep)
            {
                player.WriteLine($"Shopkeep '{shopkeepName}' not found in this room.");
                return;
            }
            // Find the item by name in the shopkeep's inventory
            string? itemIdToBuy = null;
            foreach (var itemEntry in shopkeep.ShopInventory)
            {
                string itemId = itemEntry.Key;
                if (GameState.Instance.ItemCatalog.ContainsKey(itemId))
                {
                    Item item = GameState.Instance.ItemCatalog[itemId];
                    if (item.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                    {
                        itemIdToBuy = itemId;
                        break;
                    }
                }
            }
            if (itemIdToBuy == null)
            {
                player.WriteLine($"Item '{itemName}' not found in shopkeep '{shopkeepName}' inventory.");
                return;
            }
            shopkeep.SellItem(itemIdToBuy, amount);
            //add to player inventory logic would go here.
            //Adding this for extra emphasis since this is a key part of the buy process.
            for (int i = 0; i < amount; i++)
            {
                player.PlayerInventory.AddItem(itemIdToBuy);
            }
            player.WriteLine($"You have purchased {amount} of '{itemName}' from '{shopkeepName}'.");
        }

        protected static void BuyItemFromPlayer(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
            {
                player.WriteLine("Usage: shop buy '<shopkeep>' '<ItemName>' '<amount>'");
                return;
            }
            string shopkeepName = parameters[2];
            string itemName = parameters[3];
            int amount;
            if (!int.TryParse(parameters[4], out amount) || amount <= 0)
            {
                player.WriteLine("Invalid amount specified.");
                return;
            }
            Room room = player.GetRoom();
            NonPlayer? npc = room.GetNpcByName(shopkeepName);
            if (npc == null || npc is not Shopkeep shopkeep)
            {
                player.WriteLine($"Shopkeep '{shopkeepName}' not found in this room.");
                return;
            }
            for (int i = 0; i < amount; i++)
            {
                // Logic to remove item from player inventory and add to shopkeep inventory would go here.
                player.PlayerInventory.SellItem(itemName, player);
                shopkeep.AddItemToInventory(itemName, 1);
            }

        }
    }
    #endregion

    #region BaseNpcCommand Class
    internal abstract class BaseNpcCommand
    {
        protected static ICatalog? _catalog;
        protected static string _entityName = "";
        protected static Type _entityType = typeof(BaseNpcCommand);

        #region WriteUsage Method
        protected static void WriteUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine($"/{_entityName} set desc <'Name'> '<Description>'");
            player.WriteLine($"/{_entityName} set name <'CurrentName'> '<NewName>'");
            player.WriteLine($"/{_entityName} list");
            player.WriteLine($"/{_entityName} create '<name>' '<description>'");
            player.WriteLine($"/{_entityName} delete '<name>'");
            player.WriteLine($"/{_entityName} tag add '<name>' '<tag>'");
            player.WriteLine($"/{_entityName} tag delete '<name>' '<tag>'");
            player.WriteLine($"/{_entityName} tag list");
            player.WriteLine($"/{_entityName} tag list '<name>'");
            player.WriteLine($"/{_entityName} set location '<name>' '<locationid>");
            player.WriteLine($"/{_entityName} set area '<name>' '<areaid>");
            player.WriteLine($"/{_entityName} set weapon '<name>' '<weaponname>'");
            player.WriteLine($"/{_entityName} give armour '<name>' '<armourID>'");
            player.WriteLine($"/{_entityName} give item '<name>' '<itemID>'");
            player.WriteLine($"/{_entityName} remove armour '<name>' '<armourID>'");
            player.WriteLine($"/{_entityName} remove item '<name>' '<itemID>'");
            player.WriteLine($"/{_entityName} level '<name>' '<amount>'");
            player.WriteLine($"/{_entityName} level check '<name>'");
            if (_entityName == "shopkeep" || _entityName == "npc")
            {
                player.WriteLine($"/{_entityName} dialog list '<character>' '<category>'");
                player.WriteLine($"/{_entityName} dialog list '<character>'");
                player.WriteLine($"/{_entityName} dialog delete '<character>' '<category>'");
                player.WriteLine($"/{_entityName} dialog delete '<character>' '<category>' '<line to remove>'");
                player.WriteLine($"/{_entityName} dialog add '<character'> <category>' '<line to add>'");
                player.WriteLine($"/{_entityName} dialog add '<character'> <category>'");
                if (_entityName == "shopkeep")
                {
                    player.WriteLine($"/{_entityName} inventory add '<character'> '<itemID>'");
                    player.WriteLine($"/{_entityName} inventory delete '<character'> '<itemID>'");
                }
            }
        }

        #endregion

        #region Remove Item Method
        public static void RemoveItem(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
            {
                player.WriteLine($"Usage: /{_entityName} remove item '<name>' '<itemID>'");
                return;
            }
            string name = parameters[3];
            string itemId = parameters[4];
            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return;
            if (!GameState.Instance.ItemCatalog.ContainsKey(itemId))
            {
                player.WriteLine($"Item with ID '{itemId}' does not exist in the item catalog.");
                return;
            }
            bool removed = npc.PlayerInventory.RemoveItem(itemId);
            if (removed)
            {
                player.WriteLine($"Item '{itemId}' removed from {_entityName} '{name}' inventory.");
            }
            else
            {
                player.WriteLine($"Item '{itemId}' not found in {_entityName} '{name}' inventory.");
            }
        }
        #endregion

        #region RemoveArmour Method
        public static void RemoveArmour(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
            {
                player.WriteLine($"Usage: /{_entityName} remove armour '<name>' '<armourID>'");
                return;
            }
            string name = parameters[3];
            string armourId = parameters[4];
            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return;
            if (!GameState.Instance.ArmorCatalog.ContainsKey(armourId))
            {
                player.WriteLine($"Armour with ID '{armourId}' does not exist in the armour catalog.");
                return;
            }
            bool removed = npc.EquippedArmor.RemoveAll(a => a.Name.Equals(armourId, StringComparison.OrdinalIgnoreCase)) > 0;
            if (removed)
            {
                player.WriteLine($"Armour '{armourId}' removed from {_entityName} '{name}' inventory.");
            }
            else
            {
                player.WriteLine($"Armour '{armourId}' not found in {_entityName} '{name}' inventory.");
            }
        }
        #endregion

        #region AddArmour Method
        public static void AddArmour(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
            {
                player.WriteLine($"Usage: /{_entityName} give armour '<name>' '<armourID>'");
                return;
            }
            string name = parameters[3];
            string armourId = parameters[4];
            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return;
            if (!GameState.Instance.ArmorCatalog.ContainsKey(armourId))
            {
                player.WriteLine($"Armour with ID '{armourId}' does not exist in the armour catalog.");
                return;
            }
            npc.EquippedArmor.Add((Armor)GameState.Instance.ArmorCatalog[armourId].Clone());
            player.WriteLine($"Armour '{armourId}' added to {_entityName} '{name}' inventory.");
        }

        #endregion

        #region GiveItem Method
        public static void GiveItem(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
            {
                player.WriteLine($"Usage: /{_entityName} give item '<name>' '<itemID>'");
                return;
            }
            string name = parameters[3];
            string itemId = parameters[4];
            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return;
            if (!GameState.Instance.ItemCatalog.ContainsKey(itemId))
            {
                player.WriteLine($"Item with ID '{itemId}' does not exist in the item catalog.");
                return;
            }
            npc.PlayerInventory.AddItem(GameState.Instance.ItemCatalog[itemId].Clone());
            player.WriteLine($"Item '{itemId}' added to {_entityName} '{name}' inventory.");
        }

        #endregion

        #region NpcCreate Method
        //Creates an entity of a NonPlayer type, adds to gamestate.
        protected static bool NpcCreate(Player player, List<string> parameters)
        {
            // 0: /mob
            // 1: create
            // 2: name
            // 3: description
            if (parameters.Count < 4)
            {
                player.WriteLine($"Usage: /{_entityName} create '<name>' '<description>'");
                return false;
            }

            string name = parameters[2];
            string description = parameters[3];

            if (_catalog!.ContainsKey(name))
            {
                player.WriteLine($"{_entityName} with that name already exists.");
                return false;
            }

            NonPlayer npc = (NonPlayer)Activator.CreateInstance(_entityType)!;
            npc.Name = name; ;
            npc.Description = description;
            _catalog.Add(name, npc);

            player.WriteLine($"{_entityName} ({name}) created.");
            player.WriteLine("Npc created.");
            return true;
        }
        #endregion

        #region NpcDelete Method
        //deletes an entity of a NonPlayer type, removes from gamestate.
        protected static bool NpcDelete(Player player, List<string> parameters)
        {
            if (parameters.Count < 3)
            {
                player.WriteLine($"Usage: /{_entityName} delete '<name>'");
                return false;
            }

            if (_catalog!.Remove(parameters[2]))
            {
                player.WriteLine("Entity deleted.");
                return true;
            }

            player.WriteLine("Entity not found.");
            return false;
        }
        #endregion

        #region SetNpcProperty Method
        /// <summary>
        /// Sets a property of a NonPlayer entity. This includes anything that inherits from NonPlayer.
        /// However, if you need specific properties for a derived type, you may need to extend this method.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="parameters"></param>
        protected static bool SetNpcProperty(Player player, List<string> parameters)
        {
            // params are /'type' set <name> <property> <value> (5 total) 
            // Consider showing the value of the property if a value isn't supplied
            if (parameters.Count < 5)
            {
                player.WriteLine($"Usage: /{_entityName} set <property> <name> <value>");
                return false;
            }

            string name = parameters[3];
            string property = parameters[2].ToLower();
            string value = parameters[4];

            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return false;

            switch (property)
            {

                case "name":
                    npc.Name = value;
                    _catalog!.Remove(name);
                    _catalog.Add(value, npc);
                    player.WriteLine("Success!");
                    return true;
                case "desc":
                    npc.Description = value;
                    player.WriteLine("Success!");
                    return true;
                case "location":
                    if (int.TryParse(value, out int locationId))
                    {
                        npc.SetRoom(locationId);
                        player.WriteLine($"{_entityName} '{name}' location set to Room ID {locationId}.");
                        return true;
                    }
                    else
                    {
                        player.WriteLine("Invalid location ID. It must be a number.");
                        return false;
                    }
                case "area":
                    if (int.TryParse(value, out int areaId))
                    {
                        npc.AreaId = areaId;
                        player.WriteLine($"{_entityName} '{name}' area set to Area ID {areaId}.");
                        return true;
                    }
                    else
                    {
                        player.WriteLine("Invalid area ID. It must be a number.");
                        return false;
                    }
                // Add other properties here as needed
                case "weapon":
                    bool found = false;
                    foreach(string w in GameState.Instance.WeaponCatalog.Keys)
                    {
                        if(w.Equals(value))
                        {
                            npc.PrimaryWeapon = (Weapon)GameState.Instance.WeaponCatalog[w].Clone();
                            player.WriteLine($"{_entityName} '{name}' weapon set to '{value}'.");
                            found = true;
                        }
                    }
                    if (found) {
                        player.WriteLine($"{_entityName} '{name}' weapon set to '{value}'.");
                    }
                    else
                    {
                        player.WriteLine($"Weapon '{value}' not found in weapon catalog.");
                        return false;
                    }
                    return true;
                default:
                    player.WriteLine($"Property '{property}' is not recognized for {_entityName}.");
                    break;
            }

            return false;
        }
        #endregion

        #region DeleteNpcDialogLine Method
        protected static void DeleteNpcDialogLine(Player player, List<string> parameters)
        {
            // We shouldn't have to check for npc. We just won't include it in the options
            // for mobs or whatever if it isn't appropriate.
            if (parameters.Count < 6)
            {
                player.WriteLine("Usage: /npc dialog delete '<character>' '<category>' '<line to remove>'");
                return;
            }
            string name = parameters[3];
            string category = parameters[4];
            string description = parameters[5];

            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
            {   
                player.WriteLine($"{_entityName} '{name}' not found.");
                return;
            }
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            npc.GetDialogGroup(category).RemoveDialogLine(description);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        #endregion

        #region DeleteNpcDialogCategory Method
        protected static bool DeleteNpcDialogCategory(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
            {
                player.WriteLine($"Usage: /{_entityName} dialog delete '<character>' '<category>'");
                return false;
            }
            string name = parameters[3];
            string category = parameters[4];

            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return false;

#pragma warning disable CS8604 // Possible null reference argument.
            return npc.DialogGroups.Remove(npc.GetDialogGroup(category));
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion

        #region NpcListDialog Method
        protected static bool NpcListDialog(Player player, List<string> parameters)
        {
            string name = parameters[3];
            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
            {
                return false;
            }
            foreach (DialogGroup dialog in npc.DialogGroups)
            {
                player.WriteLine($"Category: {dialog.Category}");
            }
            return true;
        }
        #endregion

        #region NpcListCategoryDialog Method
        protected static bool NpcListCategoryDialog(Player player, List<string> parameters)
        {
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /npc dialog list '<character>' '<category>'");
                return false;
            }

            string name = parameters[3];
            string category = parameters[4];

            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
            {
                return false;
            }
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            DialogGroup dialogGroup = npc.GetDialogGroup(category);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            if (dialogGroup == null)
            {
                player.WriteLine($"Dialog category '{category}' not found for {_entityName} '{name}'.");
                return false;
            }
            else
            {
                foreach (var dialogLine in dialogGroup.DialogLines)
                {
                    player.WriteLine(dialogLine);
                }
                return true;
            }
        }
        #endregion

        #region CheckForCatalogAndObject Method
        protected static NonPlayer? CheckForCatalogAndObject(Player player, string key)
        {
            var catalog = _catalog as Catalog<string, NonPlayer>;
            if (!catalog!.TryGetValue(key, out NonPlayer? npc) || npc == null)
            {
                player.WriteLine($"{_entityName} '{key}' not found.");
                return null;
            }

            return npc;
        }
        #endregion

        #region NpcAddDialog Method
        protected static bool NpcAddDialog(Player player, List<string> parameters)
        {
            string name = parameters[3];
            string category = parameters[4];
            string dialogLine = parameters[5];
            player.WriteLine("Adding dialog");
            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return false;

            // Might need to check if category exists first
            if (!Enum.TryParse<DialogGroupCategory>(category, true, out DialogGroupCategory groupCategory))
            {
                player.WriteLine($"Dialog group {category} isn't valid.");
                return false;
            }
            if (npc.HasDialogGroup(groupCategory)){
                DialogGroup dialogCategory = npc.GetDialogGroup(category);
                if (!dialogCategory.HasDialogLine(dialogLine))
                {
                    dialogCategory.AddDialogLine(dialogLine);
                    player.WriteLine($"Dialog line added to category '{category}' for {_entityName} '{name}'.");
                }
                else
                {
                    player.WriteLine("Line already exists!");
                }
            }
            else
            {
                player.WriteLine($"Category does not exist");
            }
            return true;
        }
        #endregion

        #region NpcAddDialogCategory Method
        protected static bool NpcAddDialogCategory(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
            {
                player.WriteLine("Usage: /npc dialog add '<character'> <category>'");
                return false;
            }
            string name = parameters[3];
            string category = parameters[4];
            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return false;

            if (!Enum.TryParse<DialogGroupCategory>(category, true, out DialogGroupCategory groupCategory))
            {
                player.WriteLine($"Dialog group {category} isn't valid.");
                return false;
            }
            
            if (!npc.HasDialogGroup(groupCategory))
            {
                DialogGroup dialogGroup = new DialogGroup();
                dialogGroup.SetCategory(groupCategory);
                npc.DialogGroups.Add(dialogGroup);
            }
            else
            {
                player.WriteLine($"Dialog category '{category}' already exists for {_entityName} '{name}'.");
                return false;
            }
            player.WriteLine($"Dialog category '{category}' added to {_entityName} '{name}'.");
            return true;
        }
        #endregion

        #region AddTag Method
        protected static void AddTag(Player player, List<string> parameters)
        {
            if (parameters.Count < 4)
            {
                player.WriteLine($"Usage: /{_entityName} tag add '<name>' '<tag>'");
                return;
            }
            string name = parameters[3];
            string tag = parameters[4];
            Character? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
            {
                player.WriteLine($"Npc '{name}' not found.");
                return;
            }
            bool completed = npc.AddTag(tag);
            if (!completed)
            {
                player.WriteLine($"Tag '{tag}' is invalid or already exists on {_entityName} '{name}'.");
                return;
            }
            else
            {
                player.WriteLine($"Tag '{tag}' added to {_entityName} '{name}'.");
            }
        }
        #endregion

        #region RemoveTag Method
        protected static void RemoveTag(Player player, List<string> parameters)
        {
            if (parameters.Count < 4)
            {
                player.WriteLine($"Usage: /{_entityName} tag remove '<name>' '<tag>'");
                return;
            }
            string name = parameters[3];
            string tag = parameters[4];
            NPCTag tagEnum;
            if (!Enum.TryParse<NPCTag>(tag, true, out tagEnum))
            {
                player.WriteLine($"Tag '{tag}' is not a valid tag.");
                return;
            }
            Character? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return;
            bool completed = npc.RemoveTag(tagEnum);
            if (!completed)
            {
                player.WriteLine($"Tag '{tag}' does not exist on {_entityName} '{name}'.");
                return;
            }
            else
            {
                player.WriteLine($"Tag '{tag}' removed from {_entityName} '{name}'.");
            }
        }
        #endregion

        #region ListValidTags Method
        protected static void ListValidTags(Player player)
        {
            player.WriteLine("Valid Tags:");
            foreach (NPCTag tag in Enum.GetValues<NPCTag>())
            {
                string tagName = tag.ToString();
                player.WriteLine(tagName);
            }
        }
        #endregion

        #region ListTagsOnNPC Method
        // CODE REVIEW: Shelton PR #60 - I see where you are headed with GetTags returning a list of strings
        // I think that makes sense, but your foreach loop is still treating it like a list of NPCTags. 
        // I have modified GetTags method to return a sorted list of strings and adjusted this method accordingly.
        // Because this is on Character and applies equally to players and npcs I have adjusted the method name and parameter name to be more general as well.
        protected static void ListTagsOnCharacter(Player player, Character character)
        {
            player.WriteLine("Valid Tags:");

            foreach (string tag in character.GetTags())
            {
                player.WriteLine(tag);
            }
        }
        #endregion

        #region NpcTag Method
        protected static void NpcTag(Player player, List<string> parameters)
        {
            parameters[2] = parameters[2].ToLower();

            if (parameters[2].Equals("add"))
            {
                AddTag(player, parameters);
            }
            else if (parameters[2].Equals("remove") || parameters[2].Equals("delete"))
            {
                RemoveTag(player, parameters);
            }
            else if (parameters[2].Equals("list"))
            {
                if (parameters.Count == 3)
                {
                    ListValidTags(player);
                }
                else if (parameters.Count == 4)
                {
                    string name = parameters[3];
                    Character? npc = CheckForCatalogAndObject(player, name);
                    if (npc != null)
                    {
                        ListTagsOnCharacter(player, npc);
                    }
                }
            }
        }
        #endregion

        #region NpcDialogCommands Method
        protected static void NpcDialogCommands(Player player, List<string> parameters)
        {
            if (parameters[2].Equals("add") && parameters.Count == 6)
            {
                NpcAddDialog(player, parameters);
            }
            if (parameters[2].Equals("add") && parameters.Count == 5)
            {
                NpcAddDialogCategory(player, parameters);
            }
            if (parameters[2].Equals("list") && parameters.Count == 4)
            {
                NpcListDialog(player, parameters);
            }
            if (parameters[2].Equals("list") && parameters.Count == 5)
            {
                NpcListCategoryDialog(player, parameters);
            }
            if (parameters[2].Equals("delete") && parameters.Count == 5)
            {
                DeleteNpcDialogCategory(player, parameters);
            }
            if (parameters[2].Equals("delete") && parameters.Count == 6)
            {
                DeleteNpcDialogLine(player, parameters);
            }
        }
        #endregion
    }
}
#endregion