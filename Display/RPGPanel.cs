
using Spectre.Console;

namespace RPGFramework.Display
{
    internal class RPGPanel
    {
        public static BoxBorder Border { get; private set; } = BoxBorder.Rounded;
        public static string HeaderColor { get; private set; } = "[bold yellow]";

        /// <summary>
        /// Get a Spectre Panel object using our default settings
        /// </summary>
        /// <param name="content"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static Panel GetPanel(string content, string header)
        {
            return new Panel(content)
            {
                Header = new PanelHeader($"{HeaderColor}Welcome[/]"),
                Border = Border

            };
        }
    }
}
