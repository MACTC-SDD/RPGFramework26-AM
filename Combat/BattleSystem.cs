using RPGFramework;
namespace RPGFramework.Combat
{
    // =========================
    // GAME STATE
    // =========================

    public enum CombatState
    {
        Exploration,
        Combat
    }

    // =========================
    // COMMANDS (NETWORK PAYLOADS)
    // =========================

    public enum CombatCommandType
    {
        Attack,
        Disengage,
        Flee,
    }

    public class CombatCommand
    {
        public int PlayerId { get; }
        public CombatCommandType Type { get; }
        public int TargetId { get; }

        public CombatCommand(int playerId, CombatCommandType type, int targetId)
        {
            PlayerId = playerId;
            Type = type;
            TargetId = targetId;
        }
    }




    // =========================
    // COMBAT MANAGER (SERVER AUTHORITY)
    // =========================

    internal class CombatManager
    {
        //ux team
     
        public CombatState State { get; private set; } = CombatState.Exploration;
        public List<Character> Combatants { get; private set; } = [];
        public int TurnIndex { get; private set; }

        public Character CurrentTurn => Combatants[TurnIndex];

        // Start combat (server only)
        public void StartCombat(IEnumerable<Character> combatants)
        {
            State = CombatState.Combat;
            Combatants = combatants.ToList();

            /*
            foreach (var c in Combatants)
                c.RollInitiative();

            Combatants = Combatants
                .OrderByDescending(c => c.Initiative)
                .ToList();
            */
            TurnIndex = 0;

            BroadcastState();
        }

        // Receive command from network
        public void ReceiveCommand(CombatCommand command)
        {
            if (State != CombatState.Combat)
                return;

            // 🔒 Authority check
            //if (command.PlayerId != CurrentTurn.Id)
            {
                Console.WriteLine("❌ Invalid command: not your turn.");
                string playerAction = $"Hey! Wait your turn!";
                return;
            }

            ResolveCommand(command);
            AdvanceTurn();
        }
        private void AttemptDisengage(Character actor)
        {
            Random rng = new Random();

            // Base chance 30% + 5% per Charisma point
            int chance = 30 + (5 * actor.Charisma);
            int roll = rng.Next(1, 101); // 1-100

            if (roll <= chance)
            {
                Console.WriteLine($"{actor.Name} successfully disengaged from combat! 🏃‍♂️");
                string playerAction = $"You walk away from the battle.";
                Combatants.Remove(actor);

                // If player leaves, skip to next turn
                if (TurnIndex >= Combatants.Count)
                    TurnIndex = 0;

                BroadcastState();
            }
            else
            {
                Console.WriteLine($"{actor.Name} failed to disengage!");
                string playerAction = $"The enemy chucked a rock at your head, tempting you to stay in the fight.";
            }
        }

        private void Flee(Character actor)
        {
            Random rng = new Random();

            // Base chance 30% + 5% per Dexterity point
            int chance = 50 + (5 * actor.Dexterity);
            int roll = rng.Next(1, 101); // 1-100

            if (roll <= chance)
            {
                Console.WriteLine($"{actor.Name} successfully Fled combat! 🏃‍♂️");
                string playerAction = "You flee from the fight!";
                Combatants.Remove(actor);

                // If player leaves, skip to next turn
                if (TurnIndex >= Combatants.Count)
                    TurnIndex = 0;

                BroadcastState();
            }
            else
            {
                Console.WriteLine($"{actor.Name} failed to Flee!");
                string playerAction = "The enemy blocked your pathway!";
            }
        }

        private void ResolveCommand(CombatCommand command)
        {
            var actor = CurrentTurn;
            //var target = Combatants.FirstOrDefault(c => c.Id == command.TargetId);
            NonPlayer target = new(); // Placeholder for target lookup

            if (target == null)
            {
                Console.WriteLine("❌ Invalid target.");
                string playerAction = "Cannot Target That";
                return;
            }

            switch (command.Type)
            {
                case CombatCommandType.Attack:
                    Console.WriteLine($"{actor.Name} attacks {target.Name}!");
                    string playerAction = $"{actor.Name} hit {target.Name} for {actor.PrimaryWeapon.Damage}!";
                    break;

                case CombatCommandType.Disengage:
                    AttemptDisengage(actor);
                    break;

                case CombatCommandType.Flee:
                    Flee(actor);
                    break;
            }
        }

        private void AdvanceTurn()
        {
            TurnIndex = (TurnIndex + 1) % Combatants.Count;
            BroadcastState();
        }

        private void BroadcastState()
        {
            Console.WriteLine($"\n▶ {CurrentTurn.Name}'s turn");
            string playerTurnReminder = $"Its {CurrentTurn.Name} Turn!";
        }
    }

    // =========================
    // SERVER SIMULATION
    // =========================
    /*
    class Program
    {
        static void Main()
        {
            // Simulated player IDs (would come from network login)
            var player1Id = Guid.NewGuid();
            var player2Id = Guid.NewGuid();

            var player1 = new PlayerCharacter(player1Id, "Hero", 3);
            var player2 = new PlayerCharacter(player2Id, "Rogue", 4);

            var combat = new CombatManager();
            combat.StartCombat(new[] { player1, player2 });

            // Simulated network commands
            combat.ReceiveCommand(new CombatCommand(
                player1Id,
                CommandType.Attack,
                player2Id
            ));

            combat.ReceiveCommand(new CombatCommand(
                player2Id,
                CommandType.Attack,
                player1Id
            ));
        }
    }*/
}


