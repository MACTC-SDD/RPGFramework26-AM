using RPGFramework.Enums;
using RPGFramework.Geography;
using RPGFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
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
                    ListMobs();
                    break;
                case "set":
                    return SetNpcProperty(player, parameters);
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

        private static void ListMobs()
        {
            foreach (var mob in GameState.Instance.MobCatalog)
            {
                Console.WriteLine($"Mob Name: {mob.Value.Name} Description: {mob.Value.Description}");
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
                    ListNpcs();
                    break;
                case "tag":
                    NpcTag(player, parameters);
                    break;
                case "set":
                    return SetNpcProperty(player, parameters);
                case "dialog":
                    NpcDialogCommands(player, parameters);
                    break;
                default:
                    WriteUsage(player);
                    break;
            }
            return false;
        }
        private static void ListNpcs()
        {
            foreach (var npc in GameState.Instance.NPCCatalog)
            {
                Console.WriteLine($"Npc Name: {npc.Value.Name} Description: {npc.Value.Description}");
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

        public bool Execute(Character character, List<string> parameters)
        {
            _catalog = GameState.Instance.ShopCatalog;
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
                    ListShopKeeps();
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
                // For longer commands with a lot of optiosn like this, we might send this to another method
                case "dialog":
                    NpcDialogCommands(player, parameters);
                    break;
                default:
                    WriteUsage(player);
                    break;
            }

            return false;
        }
        private static void ListShopKeeps()
        {
            foreach (var shop in GameState.Instance.ShopCatalog)
            {
                Console.WriteLine($"Shop Name: {shop.Value.Name} Description: {shop.Value.Description}");
            }
            return;
        }
        protected static bool AddItem(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
            {
                player.WriteLine("Usage: /shopkeep inventory add '<character'> '<itemID>'");
                return false;
            }

            if (parameters[0].Equals("/shopkeep"))
            {

                //Adds one to quantity if it exists already
                if (GameState.Instance.ShopCatalog.ContainsKey(parameters[3]))
                {
                    Shopkeep shop = GameState.Instance.ShopCatalog[parameters[3]];
                    string itemID = parameters[4];
                    if (shop.ShopInventory.ContainsKey(itemID))
                    {
                        shop.IncrementItemQuantity(itemID);
                        player.WriteLine("Added one of the item to the inventory!");
                    }
                    else
                    {
                        shop.AddItemToInventory(itemID);
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

    #region BaseNpcCommand Class
    internal abstract class BaseNpcCommand
    {
        protected static ICatalog? _catalog;
        protected static string _entityName = "";
        protected static Type _entityType;

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
            if (_entityName == "shopkeep" || _entityName == "npc")
            {
                player.WriteLine($"/{_entityName} dialog list '<character>' '<category>'");
                player.WriteLine($"/{_entityName} dialog list '<character>'");
                player.WriteLine($"/{_entityName} dialog delete '<character>' '<category>'");
                player.WriteLine($"/{_entityName} dialog delete '<character>' '<category>' '<line to remove>'");
                player.WriteLine($"/{_entityName} dialog add '<character'> <category>' '<line to add>'");
                if (_entityName == "shopkeep")
                {
                    player.WriteLine($"/{_entityName} inventory add '<character'> '<itemID>'");
                    player.WriteLine($"/{_entityName} inventory delete '<character'> '<itemID>'");
                }
            }
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
                    return true;
                case "desc":
                    npc.Description = value;
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
                default:
                    player.WriteLine($"Property '{property}' is not recognized for {_entityName}.");
                    break;
            }

            return false;
        }
        #endregion

        #region DeleteNpcDialogLine Method
        protected static bool DeleteNpcDialogLine(Player player, List<string> parameters)
        {
            // We shouldn't have to check for npc. We just won't include it in the options
            // for mobs or whatever if it isn't appropriate.
            if (parameters.Count < 6)
            {
                player.WriteLine("Usage: /npc dialog delete '<category>' '<character>' '<line to remove>'");
                return false;
            }
            string name = parameters[4];
            string category = parameters[3].ToLower();
            string description = parameters[5];

            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return false;

            return npc.DialogOptions[category].Remove(description);
        }
        #endregion

        #region DeleteNpcDialogCategory Method
        protected static bool DeleteNpcDialogCategory(Player player, List<string> parameters)
        {
            if (parameters.Count < 5)
            {
                player.WriteLine($"Usage: /{_entityName} dialog delete '<category>' '<character>'");
                return false;
            }
            string name = parameters[4];
            string category = parameters[3].ToLower();

            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return false;

            return npc.DialogOptions.Remove(category);
        }
        #endregion

        #region NpcListDialog Method
        protected static bool NpcListDialog(Player player, List<string> parameters)
        {
            // Need to look at the format for this one, I wasn't sure of the parameters
            if (parameters[0].Equals("/npc"))
            {
                foreach (var dialog in GameState.Instance.NPCCatalog[parameters[4]].DialogOptions)
                {
                    player.WriteLine(dialog.Key);
                }
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
            string category = parameters[4].ToLower();

            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return false;

            foreach (var dialog in GameState.Instance.NPCCatalog[parameters[3]].DialogOptions[category])
            {
                player.WriteLine(dialog);
            }
            return true;
        }
        #endregion

        #region CheckForCatalogAndObject Method
        protected static NonPlayer? CheckForCatalogAndObject(Player player, object key)
        {
            if (_catalog is null || !_catalog.ContainsKey(key))
            {
                player.WriteLine($"{_entityName} '{key}' not found.");
                return null;
            }
            if (_catalog[key] is not NonPlayer npc)
            {
                player.WriteLine($"{_entityName} '{key}' is not a valid NonPlayer entity.");
                return null;
            }
            return npc;
        }
        #endregion

        #region NpcAddDialog Method
        protected static bool NpcAddDialog(Player player, List<string> parameters)
        {
            if (parameters.Count < 6)
            {
                player.WriteLine("Usage: /npc dialog add '<character'> <category>' '<line to add>'");
                return false;
            }

            string name = parameters[3];
            string category = parameters[4].ToLower();
            string dialogLine = parameters[5];

            NonPlayer? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return false;

            // Might need to check if category exists first
            npc.DialogOptions[category].Add(dialogLine);
            player.WriteLine($"Dialog line added to category '{category}' for {_entityName} '{name}'.");
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
            string name = parameters[2];
            string tag = parameters[3];
            Character? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return;
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
            string name = parameters[2];
            string tag = parameters[3];
            Character? npc = CheckForCatalogAndObject(player, name);
            if (npc == null)
                return;
            bool completed = npc.RemoveTag(tag);
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
            NonPlayer character = new NonPlayer();
            player.WriteLine("Valid Tags:");
            foreach (string tag in character.ValidTags)
            {
                player.WriteLine(tag);
            }
        }
        #endregion

        #region ListTagsOnNPC Method
        protected static void ListTagsOnNPC(Player player, Character npc)
        {
            player.WriteLine("Valid Tags:");
            foreach (string tag in npc.GetTags())
            {
                player.WriteLine(tag);
            }
        }
        #endregion

        #region NpcTag Method
        protected static void NpcTag(Player player, List<string> parameters)
        {

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
                        ListTagsOnNPC(player, npc);
                    }
                }
            }
        }
        #endregion

        #region NpcDialogCommands Method
        protected static void NpcDialogCommands(Player player, List<string> parameters)
        {
            if (parameters[2].Equals("add"))
            {
                NpcAddDialog(player, parameters);
                return;
            }
            else if (parameters[2].Equals("list") && parameters.Count == 5)
            {
                NpcListDialog(player, parameters);
                return;
            }
            else if (parameters[2].Equals("list") && parameters.Count == 6)
            {
                NpcListCategoryDialog(player, parameters);
            }
            else if (parameters[2].Equals("delete") && parameters.Count == 5)
            {
                DeleteNpcDialogCategory(player, parameters);
            }
            else if (parameters[2].Equals("delete") && parameters.Count == 6)
            {
                DeleteNpcDialogLine(player, parameters);
            }
        }
        #endregion
    }
}
#endregion