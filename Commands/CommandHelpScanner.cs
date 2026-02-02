using System.Reflection;

namespace RPGFramework.Commands
{
    internal static class CommandHelpScanner
    {
        public static List<HelpEntry> GetAllHelpEntries(Assembly? assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();

            List<HelpEntry> entries = [];

            IEnumerable<Type> commandTypes = assembly
                .GetTypes()
                .Where(t =>
                typeof(ICommand).IsAssignableFrom(t)
                && !t.IsAbstract
                && !t.IsInterface);

            foreach (Type t in commandTypes.OrderBy(t => t.Name))
            {
                try
                {
                    ICommand? command = (ICommand?)Activator.CreateInstance(t);

                    if (command == null)
                        continue;

                    string category = command.Name.StartsWith('/')
                        ? "System Commands"
                        : "Commands";

                    entries.Add(new HelpEntry() { Topic = command.Name, Category = category, Content = command.Help }) ;
                }
                catch (Exception ex)
                {                    
                        entries.Add(new HelpEntry()
                        {
                            Topic = t.Name,
                            Category = "Command",
                            Content = $"Help unavailable for command ({t.Name}): {ex.GetType().Name}"
                        });                    
                }
            }
            return entries;
        }
    }
}

