using RPGFramework.Commands;
using RPGFramework.Enums;
using RPGFramework.Geography;
using System.Numerics;
using System.Reflection;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace RPGFramework.Combat_Stuff
{
    // allows player to start a attack
    internal class AttackCommand : ICommand
    {
        // This is the command a player would type to execute this command
        public string Name => "attack";

        // These are the aliases that can also be used to execute this command. This can be empty.
        public IEnumerable<string> Aliases => [];
        public string Help => "";

        private readonly GameState _instance = GameState.Instance;

        // What will happen when the command is executed
        public bool Execute(Character character, List<string> parameters)
        {
            string playerSelectedEnemy = parameters[1];
            if (character is not Player player)
                return false;

            if (parameters.Count < 2)
            {
                player.WriteLine("Usage: attack <target>");
                return false;
            }

            // If PVP
            //Character target = Player.FindPlayer(parameters[1], _instance.Players);

            Character? target = Room.FindMob(parameters[1], player.GetRoom());

            if (target == null)
            {
                player.WriteLine("Target not found.");
                return false;
            }

            // TODO Check if already in combat with this target
            if (target.Name == parameters[1] && _instance.Battles[0] != null)
            {
                player.WriteLine("Already in combat with this target.");
                return false;
            }

            Battle b = new(player, target, player.GetArea(), player.GetRoom());

            _instance.Battles.Add(b);
            player.WriteLine($"You have started attacking {playerSelectedEnemy}!");
            // If the command failed to run for some reason, return false
            return true;
        }
    }

    //Allows the player to choose who to attack by targeting them
    internal class TargetCommand : ICommand
    {
        public string Name => "target";
        public IEnumerable<string> Aliases => [];
        public string Help => "";
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;
            string playerSelectedEnemy = parameters[1];
            Mob? target = Room.FindMob(playerSelectedEnemy, player.GetRoom());
            if (target == null)
            {
                player.Write("Who would you like to target?");
                return false;
            }
            player.WriteLine("Changing target not yet implemented.");
            /*
             * Battle b = new Battle()
            {
                Attacker = player,
                Defender = target,
                StartArea = player.GetArea(),
                StartRoom = player.GetRoom()
            };
            GameState.Instance.Battles.Add(b);
            player.WriteLine($"{playerSelectedEnemy} is now targeted!");
            */
            return true;
        }
    }
    //Shows the enemies stats like strength and dexterity
    internal class ConsiderCommand : ICommand
    {
        public string Name => "consider";
        public IEnumerable<string> Aliases => [];
        public string Help => "";
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;

            string playerSelectedEnemy = parameters[1];
            Mob? target = Room.FindMob(playerSelectedEnemy, player.GetRoom());
            if (target == null)
            {
                player.WriteLine("Who would you like to check/consider?");
                return false;
            }

            player.WriteLine($"{playerSelectedEnemy} Stats vs {player.Name} Stats: ");
            player.WriteLine($"Strength: {target.Strength}, {player.Strength}");
            player.WriteLine($"Constitution: {target.Constitution}, {player.Constitution}");
            player.WriteLine($"Dexterity: {target.Dexterity}, {player.Dexterity}");
            player.WriteLine($"Intelligence: {target.Intelligence}, {player.Intelligence}");
            player.WriteLine($"Wisdom: {target.Wisdom}, {player.Wisdom}");
            player.WriteLine($"Charisma: {target.Charisma}, {player.Charisma}");
            player.WriteLine($"Level: {target.Level}, {player.Level}");
            return true;
        }
    }
    //Shows status of combat with health, round and damage dealt from both sides
    internal class CombatCheckCommand : ICommand
    {
        public string Name => "combatstatus";
        public IEnumerable<string> Aliases => [];
        public string Help => "";
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is not Player player)
                return false;

            player.WriteLine("combatstatus not yet implemented");
            return true;

            string playerSelectedEnemy = parameters[1];
            Mob? target = Room.FindMob(playerSelectedEnemy, player.GetRoom());
            if (target == null)
            {
                player.WriteLine("You are not in a battle so what is there to check?");
                return false;
            }

            player.WriteLine("Combat Status: ");
            player.WriteLine($"Round: {RoundTiming.currentRounds}");
            player.WriteLine($"Participants: {player.Name}, {playerSelectedEnemy}");
            player.WriteLine($"{player.Name} Health: {player.Health}");
            player.WriteLine($"{playerSelectedEnemy} Health: {target.Health}");
            // add armor and weapon durability later
            player.WriteLine($"{player.Name} Weapon:  {player.PrimaryWeapon}");
            //player.WriteLine($"{player.Name} Weapon Durability:  {player.PrimaryWeapon}");
            player.WriteLine($"{playerSelectedEnemy} Weapon: {player.PrimaryWeapon}");
            //player.WriteLine($"{playerSelectedEnemy} Weapon Durability: {player.PrimaryWeapon}");
            player.WriteLine($"{player.Name} Armor: {player.EquippedArmor}");
            //player.WriteLine($"{player.Name} Armor Durability: {player.EquippedArmor}");
            player.WriteLine($"{playerSelectedEnemy} Armor: {target.EquippedArmor}");
            //player.WriteLine($"{playerSelectedEnemy} Armor Durability: {target.EquippedArmor}");
            return true;
        }
    }
}