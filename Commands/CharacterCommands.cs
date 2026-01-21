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
                // Add more commands here as needed
            ];
        }
    }

    // CODE REVIEW: Shelton (PR #25) - Unless there is a highly specific reason to have
    // nested classes, they should be top-level classes. This improves readability
    // and maintainability. I have refactored the classes to be top-level below. 561
    // Nesting can also hide structural issues, such as the fact that all of your NPCcommands
    // are located under the ShopKeepBuilderCommand class, which is proably not the intent.
    // A nicer way to handle this might be to create a base class for NPC commands that
    // MobBuilderCommand and ShopKeepBuilderCommand ad NpcBuilderCommand inherit from.
    // I am going to refactor accordingly so you can see how that would look. The new parent class
    // is called BaseNpcCommand.
    // I also added one master permission at the top of each execute method to reduce
    // redundancy.

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

            // CODE REVIEW: Shelton (PR #25) - Added permission check for admin
            // This assumes all /mob commands require admin permissions
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

            //Switches between the second parameter to determine command.
            switch (parameters[1].ToLower())
            {
                case "create":
                    NpcCreate(player, parameters);
                    break;
                case "delete":
                    return NpcDelete(player, parameters);
                case "list":
                    ListMobs();
                    break;
                // CODE REVIEW: Shelton (PR #25) - Because there might be many properties to set,
                // we should put that logic in a method like NPCSetProperty (I will refactor accordingly)
                // so you can review
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
        private static void WriteUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine("/mob set desc <'Name'> '<Description>'");
            player.WriteLine("/mob set name <'CurrentName'> '<NewName>'");
            player.WriteLine("/mob list");
            player.WriteLine("/mob create '<name>' '<description>'");
            player.WriteLine("/mob delete '<name>'");
        }

        private static void ListMobs()
        {
            foreach (var mob in GameState.Instance.MobCatalog)
            {
                Console.WriteLine($"Mob Name: {mob.Value.Name} Description: {mob.Value.Description}");
            }
            return;
        }
    }

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
                case "set":
                    return SetNpcProperty(player, parameters);
                case "dialog":
                    if (parameters[2].Equals("add"))
                    {
                        return NpcAddDialog(player, parameters);
                    }
                    else if (parameters[2].Equals("list") && parameters.Count == 5)
                    {
                        return NpcListDialog(player, parameters);
                    }
                    else if (parameters[2].Equals("list") && parameters.Count == 6)
                    {
                        return NpcListCategoryDialog(player, parameters);
                    }
                    else if (parameters[2].Equals("delete") && parameters.Count == 5)
                    {
                        return DeleteNpcDialogCategory(player, parameters);
                    }
                    else if (parameters[2].Equals("delete") && parameters.Count == 6)
                    {
                        return DeleteNpcDialogLine(player, parameters);
                    }
                    break;
                default:
                    WriteUsage(player);
                    break;
            }

            return false;
        }

        //Prints all available commands.
        private static void WriteUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine("/npc set desc <'Name'> '<Description>'");
            player.WriteLine("/npc set name <'CurrentName'> '<NewName>'");
            player.WriteLine("/npc list");
            player.WriteLine("/npc dialog list '<character>' '<category>'");
            player.WriteLine("/npc dialog list '<character>'");
            player.WriteLine("/npc dialog delete '<character>' '<category>'");
            player.WriteLine("/npc dialog delete '<character>' '<category>' '<line to remove>'");
            player.WriteLine("/npc dialog add '<character'> <category>' '<line to add>'");
            player.WriteLine("/npc create '<name>' '<description>'");
            player.WriteLine("/npc delete '<name>'");
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
        public string Name => "/npc";

        public IEnumerable<string> Aliases => [];

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

                case "inventory":
                    if (parameters[2].Equals("add")) {
                        return AddNpcItem(player, parameters);
                    }
                    break;
                // For longer commands with a lot of optiosn like this, we might send this to another method
                case "dialog":
                    if (parameters[2].Equals("add"))
                    {
                        return NpcAddDialog(player, parameters);
                    }
                    else if (parameters[2].Equals("list") && parameters.Count == 5)
                    {
                        return NpcListDialog(player, parameters);
                    }
                    else if (parameters[2].Equals("list") && parameters.Count == 6)
                    {
                        return NpcListCategoryDialog(player, parameters);
                    }
                    else if (parameters[2].Equals("delete") && parameters.Count == 5)
                    {
                        return DeleteNpcDialogCategory(player, parameters);
                    }
                    else if (parameters[2].Equals("delete") && parameters.Count == 6)
                    {
                        return DeleteNpcDialogLine(player, parameters);
                    }
                    break;
                default:
                    WriteUsage(player);
                    break;
            }

            return false;
        }
        //Prints all available commands.
        private static void WriteUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine("/shopkeep set desc <'Name'> '<Description>'");
            player.WriteLine("/shopkeep set name <'CurrentName'> '<NewName>'");
            player.WriteLine("/shopkeep list");
            player.WriteLine("/shopkeep dialog list '<character>' '<category>'");
            player.WriteLine("/shopkeep dialog list '<character>'");
            player.WriteLine("/shopkeep dialog delete '<character>' '<category>'");
            player.WriteLine("/shopkeep dialog delete '<character>' '<category>' '<line to remove>'");
            player.WriteLine("/shopkeep dialog add '<character'> <category>' '<line to add>'");
            player.WriteLine("/shopkeep inventory add '<character'> '<itemID>'"); //to add
            player.WriteLine("/shopkeep inventory delete '<character'> '<itemID>'"); //to add
            player.WriteLine("/shopkeep create '<name>' '<description>'");
            player.WriteLine("/shopkeep delete '<name>'");
        }

        private static void ListShopKeeps()
        {
            foreach (var shop in GameState.Instance.ShopCatalog)
            {
                Console.WriteLine($"Shop Name: {shop.Value.Name} Description: {shop.Value.Description}");
            }
            return;
        }
    }
    #endregion

    #region BaseNpcCommand Class
    internal abstract class BaseNpcCommand
    {
        protected static ICatalog? _catalog;
        protected static string _entityName = "";
        protected static Type _entityType;

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
                player.WriteLine($"Usage: /{_entityName} set <name> <property> <value>");
                return false;
            }

            string name = parameters[2];
            string property = parameters[3].ToLower();
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
                case "description":
                    npc.Description = value;
                    return true;
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

        #region AddNpcItem Method
        // CODE REVIEW: Shelton (PR #25) - If this truly applies to shopkeeps we should move it to that class.
        protected static bool AddNpcItem(Player player, List<string> parameters)
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
                    int.TryParse(parameters[4], out int itemID);

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
        #endregion
    }
    #endregion
}

