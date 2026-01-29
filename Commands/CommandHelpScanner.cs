using System.Reflection;

namespace RPGFramework.Commands
{
    internal static class CommandHelpScanner
    {
        public static List<string> GetAllHelpEntries(Assembly? assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();

            List<string> entries = [];

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

                    entries.Add(command.Help);
                }
                catch (Exception ex)
                {
                    
                        entries.Add($"Help unavailable for command ({t.Name}): {ex.GetType().Name}");
                    
                }
            }
            return entries;
        }
    }
}

