namespace RPGFramework
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            TelnetServer server = new TelnetServer(5555);
            GameState.Instance.Start();
            await server.StartAsync();
        }
    }
}
