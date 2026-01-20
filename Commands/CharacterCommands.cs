using RPGFramework.Enums;
using RPGFramework.Geography;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RPGFramework.Commands
{
    internal class CharacterCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                new MobBuilderCommand(),
                new NpcBuilderCommand(),
                // Add more builder commands here as needed
            };
        }
    }

internal class MobBuilderCommand : ICommand
    {
        public string Name => "/mob";

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

            //Switches between the second parameter to determine command.
            switch (parameters[1].ToLower())
            {
                case "create":
                    MobCreate(player, parameters);
                    break;
                case "delete":
                    MobDelete(player, parameters);
                    break;
                case "list":
                    ListMobs();
                    break;
                case "set":
                    if (parameters[2].ToLower() == "name")
                    {
                        SetMobName(player, parameters);
                    }
                    else if (parameters[2].ToLower() == "desc")
                    {
                        SetMobDescription(player, parameters);
                    }
                    break;
                default:
                    WriteUsage(player);
                    break;
            }

            return true;
        }

        //Creates empty mob
        private static void MobCreate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }

            // 0: /mob
            // 1: create
            // 2: name
            // 3: description
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /mob create '<name>' '<description>'");
                return;
            }

            try
            {
                Mob m = new Mob();
                m.Name = parameters[2];
                m.Description = parameters[3];

                // Use a method or constructor to set Description, since the setter is protected
                // Assuming a method like SetDescription exists in NonPlayer or Mob

                // check if key in dictionary
                if (GameState.Instance.MobCatalog.ContainsKey(m.Name))
                {
                    player.WriteLine("A mob with that name already exists.");
                }
                else
                {
                    GameState.Instance.MobCatalog.Add(m.Name, m);
                }

                player.WriteLine("Mob created.");
            }
            catch (Exception ex)
            {
                player.WriteLine($"Error creating mob: {ex.Message}");
                player.WriteLine(ex.StackTrace ?? "");
            }
        }

        //Prints all available commands.
        private static void WriteUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine("/mob set desc <'Name'> '<Description>'");
            player.WriteLine("/mob set name <'CurrentName'> '<NewName>'");
            player.WriteLine("/mob list");
            player.WriteLine("/mob create '<name>' '<description>'");
            player.WriteLine("/mob delete '<name>'");
        }

        //Deletes a mob from the catalogue.
        private static void MobDelete(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }

            if (GameState.Instance.MobCatalog.ContainsKey(parameters[2]))
            {
                GameState.Instance.MobCatalog.Remove(parameters[2]);
            }
            else
            {
                player.WriteLine("A mob with that name doesn't exist.");
            }
        }

        //Set mobs name
        private static void SetMobName(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }

            Mob temp = GameState.Instance.MobCatalog[parameters[3]];
            temp.Name = parameters[3];

            GameState.Instance.MobCatalog.Remove(parameters[3]);

            GameState.Instance.MobCatalog.Add(parameters[4], temp);


        }
        private static void ListMobs()
        {
            foreach(var mob in GameState.Instance.MobCatalog)
            {
                Console.WriteLine($"Mob Name: {mob.Value.Name} Description: {mob.Value.Description}");
            }
            return;
        }

        //Sets mob description that currently exists.
        private static void SetMobDescription(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }
            if (GameState.Instance.MobCatalog.ContainsKey(parameters[3]))
            {
                GameState.Instance.MobCatalog[parameters[3]].Description = parameters[4];
            }

        }
    }
    internal class NpcBuilderCommand : ICommand
    {
        public string Name => "/npc";

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
                    if (parameters[2].ToLower() == "name")
                    {
                        SetNpcName(player, parameters);
                    }
                    else if (parameters[2].ToLower() == "desc")
                    {
                        SetNpcDescription(player, parameters);
                    }
                    break;
                case "dialog":
                    if (parameters[2].ToLower() == "add")
                    {
                        NpcAddDialog(player, parameters);
                    }
                    else if (parameters[2].ToLower() == "list" && parameters.Count == 5)
                    {
                        NpcListDialog(player, parameters);
                    }
                    else if (parameters[2].ToLower() == "list" && parameters.Count == 6)
                    {
                        NpcListCategoryDialog(player, parameters);
                    }
                    else if (parameters[2].ToLower() == "delete" && parameters.Count == 5)
                    {
                        DeleteNpcDialogCategory(player, parameters);
                    }
                    else if (parameters[2].ToLower() == "delete" && parameters.Count == 6)
                    {
                        DeleteNpcDialogLine(player, parameters);
                    }
                    break;
                default:
                    WriteUsage(player);
                    break;
            }

            return true;
        }

        //Creates empty mob
        private static void NpcCreate(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }

            // 0: /mob
            // 1: create
            // 2: name
            // 3: description
            if (parameters.Count < 4)
            {
                player.WriteLine("Usage: /mob create '<name>' '<description>'");
                return;
            }

            try
            {
                NonPlayer m = new NonPlayer();
                m.Name = parameters[2];
                m.Description = parameters[3];

                // Use a method or constructor to set Description, since the setter is protected
                // Assuming a method like SetDescription exists in NonPlayer or Mob

                // check if key in dictionary
                if (GameState.Instance.NPCCatalog.ContainsKey(m.Name))
                {
                    player.WriteLine("An Npc with that name already exists.");
                }
                else
                {
                    GameState.Instance.NPCCatalog.Add(m.Name, m);
                }

                player.WriteLine("Npc created.");
            }
            catch (Exception ex)
            {
                player.WriteLine($"Error creating Npc: {ex.Message}");
                player.WriteLine(ex.StackTrace ?? "");
            }
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

        private static void DeleteNpcDialogLine(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }
            try
            {
                string category = parameters[3].ToLower();
                GameState.Instance.NPCCatalog[parameters[4]].DialogOptions[category].Remove(parameters[5]);
            }
            catch (Exception ex)
            {
                player.WriteLine($"Error deleting dialog!: {ex.Message}");
                player.WriteLine(ex.StackTrace ?? "");
            }
        }
        private static void DeleteNpcDialogCategory(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }
            try
            {
                string category = parameters[3].ToLower();
                GameState.Instance.NPCCatalog[parameters[4]].DialogOptions.Remove(category);
            }
            catch (Exception ex)
            {
                player.WriteLine($"Error deleting dialog!: {ex.Message}");
                player.WriteLine(ex.StackTrace ?? "");
            }
        }
        private static void NpcListDialog(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }
            string category = parameters[3].ToLower();
            foreach (var dialog in GameState.Instance.NPCCatalog[parameters[4]].DialogOptions)
            {
                player.WriteLine(dialog.Key);
            }
        }
        private static void NpcListCategoryDialog(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }
            string category = parameters[3].ToLower();
            foreach (var dialog in GameState.Instance.NPCCatalog[parameters[4]].DialogOptions[category])
            {
                player.WriteLine(dialog);
            }
        }
        private static void NpcAddDialog(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }
            string category = parameters[3].ToLower();
            GameState.Instance.NPCCatalog[parameters[4]].DialogOptions[category].Add(parameters[5]);
        }

        //Deletes an NPC from the catalogue.
        private static void NpcDelete(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }

            if (GameState.Instance.NPCCatalog.ContainsKey(parameters[2]))
            {
                GameState.Instance.NPCCatalog.Remove(parameters[2]);
            }
            else
            {
                player.WriteLine("An npc with that name doesn't exist.");
            }
        }

        //Set npcs name
        private static void SetNpcName(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }

            NonPlayer temp = GameState.Instance.NPCCatalog[parameters[3]];
            temp.Name = parameters[3];

            GameState.Instance.NPCCatalog.Remove(parameters[3]);

            GameState.Instance.NPCCatalog.Add(parameters[4], temp);


        }
        private static void ListNpcs()
        {
            foreach (var npc in GameState.Instance.NPCCatalog)
            {
                Console.WriteLine($"Npc Name: {npc.Value.Name} Description: {npc.Value.Description}");
            }
            return;
        }

        //Sets npc description that currently exists.
        private static void SetNpcDescription(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }
            if (GameState.Instance.NPCCatalog.ContainsKey(parameters[3]))
            {
                GameState.Instance.NPCCatalog[parameters[3]].Description = parameters[4];
            }

        }
    }
}