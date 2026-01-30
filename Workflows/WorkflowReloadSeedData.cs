using RPGFramework;
using RPGFramework.Commands;
using RPGFramework.Enums;
using RPGFramework.Persistence;
namespace RPGFramework.Workflows
{
    internal class WorkflowReloadSeedData : IWorkflow
    {
        public int CurrentStep { get; set; } = 0;
        public string Description => "Forces a user to confirm that they want to reload seed data before doing it.";
        public string Name => "Reload Seed Data Workflow";
        public List<ICommand> PreProcessCommands { get; private set; } = [];
        public List<ICommand> PostProcessCommands { get; private set; } = [];

        public Dictionary<string, object> WorkflowData { get; set; } = new Dictionary<string, object>();
        public void Execute(Player player, List<string> parameters)
        {
            // Step 0, they've been prompted to confirm, step 0 is processing it.
            if (CurrentStep == 0)
            {
                if (parameters.Count == 0 || parameters[0] != "YES!")
                {
                    player.WriteLine("You must confirm that you want to reload seed data by typing 'YES!'. If that's really what you wanted to do, try again.");
                    return;
                }

                GameState.Persistence.EnsureInitializedAsync(new GamePersistenceInitializationOptions()
                {
                    CopyFilesFromDataSeedToRuntimeData = true
                });

                _ = GameState.Instance.LoadAllAreas();
                _ = GameState.Instance.LoadAllPlayers();
                _ = GameState.Instance.LoadCatalogs();

                GameState.Log(DebugLevel.Warning, $"{player.Name} has confirmed reloading seed data.");

                player.WriteLine("Seed data reloaded successfully.");
                player.CurrentWorkflow = null;
                CurrentStep++;
            }

        }
    }
}
