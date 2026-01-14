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

            switch (parameters[1].ToLower())
            {
                case "create":
                    MobCreate(player, parameters);
                    break;
                default:
                    WriteUsage(player);
                    break;
            }

            return true;
        }
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
                // Use a method or constructor to set Description, since the setter is protected
                // Assuming a method like SetDescription exists in NonPlayer or Mob
                if (m is NonPlayer nonPlayer)
                {
                    // If a method to set description exists, use it:
                    // nonPlayer.SetDescription(parameters[3]);
                    // If not, you need to add such a method to NonPlayer or Mob.
                    throw new InvalidOperationException("Cannot set Description. No accessible setter or method found.");
                }

                // The following line is unrelated to Mob creation and seems to be a copy-paste error:
                // player.GetRoom().AddExits(player, exitDirection, parameters[5], room);
                // Remove or replace with actual mob placement logic if needed.

                player.WriteLine("Mob created.");
            }
            catch (Exception ex)
            {
                player.WriteLine($"Error creating mob: {ex.Message}");
                player.WriteLine(ex.StackTrace);
            }
        }
        private static void WriteUsage(Player player)
        {
            player.WriteLine("Usage: ");
            player.WriteLine("/mob description '<set mob>'");
            player.WriteLine("/mob name '<set mob name to this>'");
            player.WriteLine("/mob create '<name>' '<description>'");
        }
    }
}
