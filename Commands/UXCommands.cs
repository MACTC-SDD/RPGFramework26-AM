using RPGFramework.Display;
using Spectre.Console;

namespace RPGFramework.Commands
{
    /// <summary>
    /// Provides utility methods for retrieving user experience (UX) test commands.
    /// </summary>
    /// <remarks>This class is intended for internal use to aggregate and expose available UX test commands.
    /// The set of commands returned may change as new commands are added or existing ones are modified.</remarks>
    internal class UXCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                new UXCommand(),
                new UXColorCommand(),
                new UXDecorationCommand(),
                new UXPanelCommand(),
                // Add more test commands here as needed
            };
        }
    }

    /// <summary>
    /// This just lists the different commands we've added here in case we forget.
    /// These are mostly going to be test commands.
    /// </summary>
    internal class UXCommand : ICommand
    {
        // This is the command a player would type to execute this command
        public string Name => "/ux";

        // These are the aliases that can also be used to execute this command. This can be empty.
        public IEnumerable<string> Aliases => new List<string>() { };

        // Change code in here to experiment with the RPGPanel UX component
        public bool Execute(Character character, List<string> parameters)
        {
            // Exit if the caller isn't a player
            if (character is not Player player)
                return false;

            // This is an example of how we can use Spectre.Console to make a table
            // We'll put this inside our panel
            var table = new Table();
            table.AddColumn("Command");
            table.AddColumn("Description");
            table.AddRow("/ux", "This command");
            table.AddRow("/uxpanel 'title' 'the content'", "Use RPGPanel to create a panel");
            table.AddRow("/uxcolor", "Test different colors");
            table.AddRow("/uxdecoration", "Test different text decorations");

            string title = "UX Testing Commands";

            Panel panel = RPGPanel.GetPanel(table, title);
            player.Write(panel);

            return true;
        }
    }

    /// <summary>
    /// A test command for experimenting with the RPGPanel UX component.
    /// </summary>
    internal class UXPanelCommand : ICommand
    {
        // This is the command a player would type to execute this command
        public string Name => "/uxpanel";

        // These are the aliases that can also be used to execute this command. This can be empty.
        public IEnumerable<string> Aliases => new List<string>() { };

        // Change code in here to experiment with the RPGPanel UX component
        public bool Execute(Character character, List<string> parameters)
        {
            // Exit if the caller isn't a player
            if (character is not Player player)
                return false;

            string content = "This is the content inside the panel.\n";
            content += "You can customize this text by providing additional parameters.\n";
            content += "ie. /uxpanel 'a title' 'a bunch of content'";

            string title = "This Is The Title";

            if (parameters.Count > 1)
                title = parameters[1];

            if (parameters.Count > 2)
                content = parameters[2];

            Panel panel = RPGPanel.GetPanel(content, title);
            player.Write(panel);

            return true;
        }
    }

    /// <summary>
    /// A test commnd for experimenting with different spectre color codes.
    /// </summary>
    internal class UXColorCommand : ICommand
    {
        // This is the command a player would type to execute this command
        public string Name => "/uxcolor";

        // These are the aliases that can also be used to execute this command. This can be empty.
        public IEnumerable<string> Aliases => new List<string>() { "/uxcolors" };

        // Change code in here to experiment with the RPGPanel UX component
        public bool Execute(Character character, List<string> parameters)
        {
            // Exit if the caller isn't a player
            if (character is not Player player)
                return false;

            string content = "[red]This text is red![/]\n";
            content += "[blue]This text is blue![/]\n";
            content += "[blue on red]This text is blue on red![/]";
            // Try out some more!

            string title = "Color Testing";

            Panel panel = RPGPanel.GetPanel(content, title);
            player.Write(panel);

            return true;
        }
    }

    /// <summary>
    /// A test command for experimenting with different spectre markup codes
    /// </summary>
    internal class UXDecorationCommand : ICommand
    {
        // This is the command a player would type to execute this command
        public string Name => "/uxdecoration";

        // These are the aliases that can also be used to execute this command. This can be empty.
        public IEnumerable<string> Aliases => new List<string>() { "/uxdec", "/uxdecorations" };

        // Change code in here to experiment with different text decorations
        public bool Execute(Character character, List<string> parameters)
        {
            // Exit if the caller isn't a player
            if (character is not Player player)
                return false;

            string content = "[bold]This text is bold![/]\n";
            content += "[italic]This text is italic![/]\n";
            content += "[bold italic]This text is bold italic![/]\n";
            // Try out some more!


            string title = "Decoration Testing";

            Panel panel = RPGPanel.GetPanel(content, title);
            player.Write(panel);

            return true;
        }
    }
}
