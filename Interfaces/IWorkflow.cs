using RPGFramework.Commands;

namespace RPGFramework.Workflows
{
    internal interface IWorkflow
    {
        int CurrentStep { get; set; }
        string Description { get; }
        string Name { get; }
        List<ICommand> PreProcessCommands { get;  }
        List<ICommand> PostProcessCommands { get;  }
        Dictionary<string, object> WorkflowData { get; set; }

        void Execute(Player player, List<string> parameters);

    }
}
