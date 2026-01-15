using RPGFramework;

namespace RPGFramework
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            GameState gameState = GameState.Instance;
            await gameState.Start();
        }
    }
}

