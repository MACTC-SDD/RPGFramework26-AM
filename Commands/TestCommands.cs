namespace RPGFramework.Commands
{
    internal class TestCommands
    {
        public static List<ICommand> GetAllCommands()
        {
            return new List<ICommand>
            {
                new TestItemSizeCommand(),
                new LadCommand()
                // Add more test commands here as needed
            };
        }
    }

    internal class LadCommand : ICommand
    {
        public string Name => "play" ;
        public IEnumerable<string> Aliases => new List<string>() { };
        public bool Execute(Character character, List<string> parameters)
        {
            if (character is Player player)
            {
                player.WriteLine("                                                                                           MENU\r\n========================================================================\r\n\r\n                                   TITLE\r\n========================================================================\r\n\r\n                                                        ( SETTINGS)      START        (HELP) \r\n                                                                                               QUIT\r\n\r\n\r\n                ============================================================\r\n");
            }
            return true;
        }
    }

    /// <summary>
    /// Measures the memory usage of creating a large number of <see cref="Item"/> instances.
    /// </summary>
    /// <remarks>This command is intended for diagnostic or testing purposes to estimate the memory footprint
    /// of <see cref="Item"/> objects. When executed, it creates 100,000 <see cref="Item"/> instances, calculates the
    /// total memory used, and outputs the results to the player if the character is a player.</remarks>
    internal class TestItemSizeCommand : ICommand
    {
        public string Name => "testitemsize";
        public IEnumerable<string> Aliases => new List<string>() { };
        public bool Execute(Character character, List<string> parameters)
        {
            long startMem = GC.GetTotalMemory(true);
            List<Item> items = new List<Item>();
            for (int i = 0; i < 100000; i++)
            {
                Item item = new Item
                {
                    Id = i,
                    Name = "Item " + i,
                    Description = "This is item number " + i,
                    DisplayText = "You see item number " + i,
                    Level = i % 10,
                    Value = (i % 100) * 1.5,
                    Weight = (i % 50) * 0.75,
                    IsDroppable = true,
                    IsGettable = true,
                    UsesRemaining = -1
                };
                items.Add(item);
            }
            long endMem = GC.GetTotalMemory(true);

            if (character is Player player)
            {
                player.WriteLine($"Created {items.Count} items.");
                player.WriteLine($"Memory used: {endMem - startMem} bytes.");
                player.WriteLine($"Average per item: {(endMem - startMem) / (double)items.Count} bytes.");
            }
            return true;
        }
    }
}
