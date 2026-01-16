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
                // Add more commands here as needed
            };
        }

        /*Creates, deletes, lists, and modifies mobs in the game world.*/
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
                        NpcCreate(player, parameters);
                        break;
                    case "delete":
                        NpcDelete(player, parameters);
                        break;
                    case "list":
                        ListMobs();
                        break;
                    case "set":
                        if (parameters[2].Equals("name"))
                        {
                            SetNpcName(player, parameters);
                        }
                        else if (parameters[2].Equals("desc"))
                        {
                            SetNpcDescription(player, parameters);
                        }
                        break;
                    default:
                        WriteUsage(player);
                        break;
                }

                return true;
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

            private static void ListMobs()
            {
                foreach (var mob in GameState.Instance.Mobs)
                {
                    Console.WriteLine($"Mob Name: {mob.Value.Name} Description: {mob.Value.Description}");
                }
                return;
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
                        if (parameters[2].Equals("name"))
                        {
                            SetNpcName(player, parameters);
                        }
                        else if (parameters[2].Equals("desc"))
                        {
                            SetNpcDescription(player, parameters);
                        }
                        break;
                    case "dialog":
                        if (parameters[2].Equals("add"))
                        {
                            NpcAddDialog(player, parameters);
                        }
                        else if (parameters[2].Equals("list") && parameters.Count == 5)
                        {
                            NpcListDialog(player, parameters);
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
                        break;
                    default:
                        WriteUsage(player);
                        break;
                }

                return true;
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
                foreach (var npc in GameState.Instance.Npcs)
                {
                    Console.WriteLine($"Npc Name: {npc.Value.Name} Description: {npc.Value.Description}");
                }
                return;
            }

            //Sets npc description that currently exists.
        }
        //Creates an entity of a NonPlayer type, adds to gamestate.
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
                player.WriteLine("Usage: /'<type>' create '<name>' '<description>'");
                return;
            }

            if (parameters[0].Equals("/npc"))
            {
                NonPlayer m = new NonPlayer();
                m.Name = parameters[2];
                m.Description = parameters[3];
                // check if key in dictionary
                if (GameState.Instance.Npcs.ContainsKey(m.Name))
                {
                    player.WriteLine("An Npc with that name already exists.");
                }
                else
                {
                    GameState.Instance.Npcs.Add(m.Name, m);
                }

                player.WriteLine("Npc created.");
            }
            else if (parameters[0].Equals("/mob"))
            {
                Mob m = new Mob();
                m.Name = parameters[2];
                m.Description = parameters[3];
                // check if key in dictionary
                if (GameState.Instance.Mobs.ContainsKey(m.Name))
                {
                    player.WriteLine("A mob with that name already exists.");
                }
                else
                {
                    GameState.Instance.Mobs.Add(m.Name, m);
                }

                player.WriteLine("Mob created.");

            }
        }

        //deletes an entity of a NonPlayer type, removes from gamestate.
        private static void NpcDelete(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }

            if (parameters[0].Equals("/npc"))
            {
                if (GameState.Instance.Npcs.ContainsKey(parameters[2]))
                {
                    GameState.Instance.Npcs.Remove(parameters[2]);
                }
                else
                {
                    player.WriteLine("An npc with that name doesn't exist.");
                }
            }
            else if (parameters[0].Equals("/mob"))
            {
                if (GameState.Instance.Mobs.ContainsKey(parameters[2]))
                {
                    GameState.Instance.Mobs.Remove(parameters[2]);
                }
                else
                {
                    player.WriteLine("A mob with that name doesn't exist.");
                }
            }
        }
        //sets nonplayer name
        private static void SetNpcName(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }

            if (parameters[0].Equals("/npc"))
            {
                NonPlayer temp = GameState.Instance.Npcs[parameters[3]];
                temp.Name = parameters[3];

                GameState.Instance.Npcs.Remove(parameters[3]);

                GameState.Instance.Npcs.Add(parameters[4], temp);
            }
            else if (parameters[0].Equals("/mob"))
            {
                Mob temp = GameState.Instance.Mobs[parameters[3]];
                temp.Name = parameters[3];
                GameState.Instance.Mobs.Remove(parameters[3]);
                GameState.Instance.Mobs.Add(parameters[4], temp);

            }
        }
        private static void SetNpcDescription(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }
            if (parameters[0] == "/npc")
            {
                if (GameState.Instance.Npcs.ContainsKey(parameters[3]))
                {
                    GameState.Instance.Npcs[parameters[3]].Description = parameters[4];
                }
            }
            else if (parameters[0] == "/mob")
            {
                if (GameState.Instance.Mobs.ContainsKey(parameters[3]))
                {
                    GameState.Instance.Mobs[parameters[3]].Description = parameters[4];
                }
            }
        }
        private static void DeleteNpcDialogLine(Player player, List<string> parameters)
        {
            if (!Utility.CheckPermission(player, PlayerRole.Admin))
            {
                player.WriteLine("You do not have permission to do that.");
                player.WriteLine("Your Role is: " + player.Role.ToString());
                return;
            }
            if (parameters[0] == "/npc")
            {
                string category = parameters[3].ToLower();
                GameState.Instance.Npcs[parameters[4]].DialogOptions[category].Remove(parameters[5]);
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
            if (parameters[0] == "/npc")
            {
                string category = parameters[3].ToLower();
                GameState.Instance.Npcs[parameters[4]].DialogOptions.Remove(category);
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
            if (parameters[0].Equals("/npc"))
            {
                foreach (var dialog in GameState.Instance.Npcs[parameters[4]].DialogOptions)
                {
                    player.WriteLine(dialog.Key);
                }
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

            if (parameters[0].Equals("/npc"))
            {
                string category = parameters[4].ToLower();
                foreach (var dialog in GameState.Instance.Npcs[parameters[3]].DialogOptions[category])
                {
                    player.WriteLine(dialog);
                }
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
            if (parameters[0].Equals("/npc"))
            {
                GameState.Instance.Npcs[parameters[3]].DialogOptions[parameters[4].ToLower()].Add(parameters[5]);
            }
        }
    }
}