


using Spectre.Console;
using System.Reflection;

internal static class DisplaySettings
    {
        public static string AnnouncementColor { get; set; } = "[red]";

        #region Map Settings
        public static string RoomMapIcon { get; set; } = "■";
        public static string RoomMapIconColor { get; set; } = "[green]";
        public static string YouAreHereMapIcon { get; set; } = "🙂";
        public static string YouAreHereMapIconColor { get; set; } = "[bold black]";
        #endregion
 
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
}





