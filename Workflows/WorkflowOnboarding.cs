using RPGFramework.Commands;
using RPGFramework.Enums;
using RPGFramework.Geography;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPGFramework.Workflows
{
    internal class WorkflowOnboarding : IWorkflow
    {
        public bool inCombat = false;
        public int action { get; set; } 
        public int CurrentStep { get; set; } = 0;
        public string Description => "Guides new players through the initial setup and familiarization with the game mechanics.";
        public string Name => "Onboarding Workflow";
        public List<ICommand> PreProcessCommands { get; private set; } = [];
        public List<ICommand> PostProcessCommands { get; private set; } = [];
        public Dictionary<string, object> WorkflowData { get; set; } = new Dictionary<string, object>();
        public bool gamestarted = false;
        public string Chosenclass = "";
       
        public string started = "";
        public void Execute(Player player, List<string> parameters)
        {
            // 1. We'll assume we didn't get here if player exists, if they did that will have authenticated instead
            // 2. Gather player class
            // 3. Roll stats and loop until accepted
            // 4. Introduce basic commands

            // TODO: Rather than this giant switch statement , consider breaking each step
            // into its own method for clarity and maintainability.
            // TODO: Determine what happens if player logs out while workflow is active?
            //   Should Logout/Disconnect check for workflow? Should Workflow have a Rollback method?
            //   Or, should we Serialize Workflow with Player, at least for onboarding. Might be confusing.

            // The action we take will depend on the CurrentStep, we will store progress in WorkflowData

            switch (CurrentStep)
            {

                case 0:
                    // TODO: Make the name of the game configurable
                    player.WriteLine("Hello and welcome to the RPG World!");
                    player.WriteLine("Let's secure your account first. Please enter a password.");

                    GameState.Log(DebugLevel.Debug, $"{player.Name} is starting onboarding workflow.");
                    CurrentStep++;
                    break;
                case 1:
                    if (parameters.Count == 0)
                        player.WriteLine("No blank passwords allowed!");
                    else
                    {
                        player.SetPassword(parameters[0]);
                        player.WriteLine($"{player.Name} : Welcome to the game! Let's start by choosing your character class.");
                        player.WriteLine(
                             "============================================================================"
                           + "\n Warrior \tMage \tRogue" +
                             "\n============================================================================");

                        CurrentStep++;
                    }
                    break;

                case 2:
                    // Step 2: Gather player class and validate
                    string chosenClass = parameters.Count > 0 ? parameters[0].ToLower() : string.Empty;
                    if (chosenClass == "warrior" || chosenClass == "mage" || chosenClass == "rogue")
                    {
                        WorkflowData["ChosenClass"] = chosenClass;
                        player.WriteLine($"You have chosen the {chosenClass} class. Now Press Enter TWICE ONLYYYY!!! until text comes up :D");
                        // If class is valid, proceed, otherwise print message and stay on this step
                        // Placeholder logic
                        CurrentStep++;
                        Chosenclass = $"{chosenClass}";
                    }
                    else
                    {
                        player.WriteLine("Invalid class chosen. Please choose from: Warrior, Mage, Rogue!");

                    }
                    break;
                case 3:
                    // Step 2: Roll stats and loop until accepted
                    // Placeholder logic
                    CurrentStep++;
                    break;
                case 4:
                      
                    
                        // Onboarding complete
                        // TODO: Set PlayerClass (or maybe do that in step above) and save Player

                        player.WriteLine(Name + ": Onboarding complete! ");
                    player.WriteLine("============================================================================" +
                        "\nYour stats are:" +
                        "\nClass : " + WorkflowData["ChosenClass"] +
                        $"\nDexterity : {player.Dexterity}" +
                        "\n\tIncrease 'Dex' To Have A To Get The First Attack! " +
                        "\n{player.EquippedArmor}" +
                        $"\nHealth : {player.Health} out of {player.MaxHealth}" +
                        "\n\tYour Health Is Limited; Make Sure To Choose Your Actions Wisely! " +
                        $"\nConstitution : {player.Constitution}" +
                        "\n \tI don't remember what this is." +
                        $"\nGold : {player.Gold}" +
                        "\n\tYour Currency In Our Totally Great Text RPG Game; Spend It Wisely!" +
                        $"\nIntelligence : {player.Intelligence}" +
                        "\n\tIf This Was Based Of Who Put Me In Charge Of This It Would Be 0." +
                        $"\nLevel : {player.Level}" +
                        "\n\tHow Many Levels Gained Through Getting XP!" +
                        $"\nCurrent Playtime for user {player.Name}" +
                        $"\n\t{player.PlayTime} "
                        +
                        "\n\tHow Many Hours, Minutes, Se- you get it." +
                        $"\nCurrent Weapon : {player.PrimaryWeapon.Name}"
                        +
                        "\n\tThe Weapon You Pulled Out The Monster That Probably Has Some Type Of Diease You Don't Need To Be Getting Close To..." +
                        $"\nStrength : {player.Strength}" +
                        "\n\tHULK SMASHH, This Increases Your Damage... i think?" +
                        $"\nXP : {player.XP}" +
                        "\n\tThe Accumulated Souls Of The Innocent You've Aqquired." +
                        $"\nWisdom : {player.Wisdom}" +
                        "\n\tI've Heard This Increases With Age. either they lied, or im still 5." +
                        "\n============================================================================"
                        );
                    player.WriteLine("Type 'help' to see a list of available commands.");
                    player.WriteLine("For Best Quality, Please Set Screen To Full Size!" + "\nReady? (type yes if ready to start game)");


                    CurrentStep++;
                    break;
                case 5:

                   // if (player.Health <= 0)
                 //   {
                 //       player.Console!.Clear();
                //  player.WriteLine("Game Over..." + $"\nHope You Enjoyed {player.Name}");
                //   }
                 //   else
                //   {
                        player.Console!.Clear();
                        string output =
                            $"                                                 --Area : {player.AreaId}–-                   " +
                            "\n========================================================================================================================" +
                            $"\n                                             Room Level : {player.LocationId}" +
                            "\n========================================================================================================================" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n {player.Target} " +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n" +
                            "\n========================================================================================================================" +
                            "\n" +
                            "\n({action} Just :____!) <-- Player Action Goes Here! " +
                            "\n" +
                            "\n========================================================================================================================" +
                            "\n" +
                            $"\nPlayer Name: {player.Name}" +
                            $"\nXP:{player.XP}" +
                            $"\nLevel :{player.Level}" +
                            $"\nHealth :{player.Health}/{player.MaxHealth}" + $"\tGold :{player.Gold}" +
                            $"\nPlaytime : {player.PlayTime}" +
                            //$"\n Total Defence :?? "+
                            "\n" +
                            "\n========================================================================================================================" +
                            "\n" +
                            "\n--Equipment--";
                    // Equipped Armor Name
                        string helmetName = player.EquippedArmor.Find(o => o.Slot == ArmorSlot.Head)?.Name ?? "None";
                    string chestName = player.EquippedArmor.Find(o => o.Slot == ArmorSlot.Chest)?.Name ?? "None";
                    string legName = player.EquippedArmor.Find(o => o.Slot == ArmorSlot.Legs)?.Name ?? "None";
                    string backName = player.EquippedArmor.Find(o => o.Slot == ArmorSlot.Back)?.Name ?? "None";
                    // Equipped Armor Mats
                    string helmetMat = player.EquippedArmor.Find(o => o.Slot == ArmorSlot.Head)?.Material.ToString() ?? "None";
                    string chestMat = player.EquippedArmor.Find(o => o.Slot == ArmorSlot.Chest)?.Material.ToString() ?? "None";
                    string legMat = player.EquippedArmor.Find(o => o.Slot == ArmorSlot.Legs)?.Material.ToString() ?? "None";
                    string backMat = player.EquippedArmor.Find(o => o.Slot == ArmorSlot.Back)?.Material.ToString() ?? "None";
                    //Equipped Armor Type
                    string helmetType = player.EquippedArmor.Find(o => o.Slot == ArmorSlot.Head)?.Type.ToString() ?? "None";
                    string chestType = player.EquippedArmor.Find(o => o.Slot == ArmorSlot.Chest)?.Type.ToString() ?? "None";
                    string legType = player.EquippedArmor.Find(o => o.Slot == ArmorSlot.Legs)?.Type.ToString() ?? "None";
                    string backType = player.EquippedArmor.Find(o => o.Slot == ArmorSlot.Back)?.Type.ToString() ?? "None";
                    output +=
                            $"\nHelmet: {helmetName}" +
                            $"\n\tHelmet Armor Type : {helmetType}" +
                            $"\n\tHelmet Material : {helmetMat}" +
                            "\n"+
                            $"\nChestplate: {chestName}" +
                            $"\n\tChestplate Armor Type : {chestType}" +
                            $"\n\tChestplate Material : {chestMat}" +
                            "\n" +
                            $"\nLeggings: {legName}" +
                            $"\n\tLeggings Armor Type : {legType}" +
                            $"\n\tLeggings Material : {legMat}" +
                            "\n" +
                            $"\nBack Piece: {backName}" +
                            $"\n\tBack Piece Armor Type : {backType}" +
                            $"\n\tBack Piece Material : {backMat}" +
                            "\n" +
                            $"\nWeapon :{player.PrimaryWeapon.Name}" +
                            $"\n\tWeapon DMG : {player.PrimaryWeapon.Damage}" +
                            $"\n\tWeapon Material : {player.PrimaryWeapon.Material}" +                            
                            $"\n{player.PrimaryWeapon.DisplayText}" +                             
                            $"\n========================================================================================================================";

                        player.WriteLine(output);
                    
                   


                    player.CurrentWorkflow = null;

               //    }
                    break;
            } 
            
        }    
    }
}
