
using Spectre.Console;

namespace RPGFramework.Display
{
    internal class RPGPanel
    {
        // TODO These should come from DisplaySettings
        public static BoxBorder Border { get; private set; } = BoxBorder.Rounded;
        public static string HeaderColor { get; private set; } = "[bold yellow]";

        /// <summary>
        /// Get a Spectre Panel object using our default settings
        /// </summary>
        /// <param name="content"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static Panel GetPanel(string content, string title)
        {
            return new Panel(content)
            {
                Header = new PanelHeader($"{HeaderColor}{title}[/]"),
                Border = Border
            };
        }

        public static Panel GetPanel(Spectre.Console.Rendering.IRenderable content, string title)
        {
            return new Panel(content)
            {
                Header = new PanelHeader($"{HeaderColor}{title}[/]"),
                Border = Border
            };
        }
    }
}
