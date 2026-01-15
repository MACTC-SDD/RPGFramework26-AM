using RPGFramework.Enums;
using RPGFramework.Geography;
using System;
using System.Collections.Generic;
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
                player.WriteLine("Your Role is: " + player.PlayerRole.ToString());
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
            catch (Exception ex)
            {
                player.WriteLine($"Error creating mob: {ex.Message}");
                player.WriteLine(ex.StackTrace);
            }
        }

        //Prints all available commands.
        private static void WriteUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine("/mob set '<desc>' <'Name'> '<Description>'");
            player.WriteLine("/mob set '<name>' <'CurrentName'> '<NewName>'");
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
                player.WriteLine("Your Role is: " + player.PlayerRole.ToString());
                return;
            }

            if (GameState.Instance.Mobs.ContainsKey(parameters[2]))
            {
                GameState.Instance.Mobs.Remove(parameters[2]);
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
                player.WriteLine("Your Role is: " + player.PlayerRole.ToString());
                return;
            }

            Mob temp = GameState.Instance.Mobs[parameters[3]];
            temp.Name = parameters[3];

            GameState.Instance.Mobs.Remove(parameters[3]);

            GameState.Instance.Mobs.Add(parameters[4], temp);


        }
        private static void ListMobs()
        {
            foreach(var mob in GameState.Instance.Mobs)
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
                player.WriteLine("Your Role is: " + player.PlayerRole.ToString());
                return;
            }
            if (GameState.Instance.Mobs.ContainsKey(parameters[3]))
            {
                GameState.Instance.Mobs[parameters[3]].Description = parameters[4];
            }

        }
    }
}
