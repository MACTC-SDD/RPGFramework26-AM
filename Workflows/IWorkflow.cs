using System;
using System.Collections.Generic;
using System.Text;

namespace RPGFramework.Workflows
{
    internal interface IWorkflow
    {
        int CurrentStep { get; set; }
        string Description { get; }
        string Name { get; }
        Dictionary<string, object> WorkflowData { get; set; }

        void Execute(Player player, List<string> parameters);

    }
}
