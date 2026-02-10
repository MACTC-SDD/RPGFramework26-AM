using RPGFramework.Enums;

namespace RPGFramework
{
    internal class DialogGroup
    {
        public DialogGroupCategory Category { get; set; } = DialogGroupCategory.Idle;
        public List<string> DialogLines { get; set; } = [];
        public List<string> Keywords = [];

        public DialogGroup() { }

        #region Methods

        public void SetCategory(DialogGroupCategory category)
        {
            Category = category;
        }
        public void AddKeyword(string keyword)
        {
            if (!Keywords.Contains(keyword, StringComparer.OrdinalIgnoreCase))
            {
                Keywords.Add(keyword);
            }
        }

        public void RemoveKeyword(string keyword)
        {
            Keywords.RemoveAll(k => string.Equals(k, keyword, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasKeyword(string keyword)
        {
            return Keywords.Any(k => string.Equals(k, keyword, StringComparison.OrdinalIgnoreCase));
        }

        public List<string> GetKeywords()
        {
            return new List<string>(Keywords);
        }

        public void ClearKeywords()
        {
            Keywords.Clear();
        }

        public bool CheckKeywordsInText(string text)
        {
            foreach (var keyword in Keywords)
            {
                if (text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddDialogLine(string line)
        {
            if (!DialogLines.Contains(line))
            {
                DialogLines.Add(line);
            }
        }
        public void RemoveDialogLine(string line) {
            DialogLines.RemoveAll(l => string.Equals(l, line, StringComparison.OrdinalIgnoreCase));
        }
        public void ClearDialogLines()
        {
            DialogLines.Clear();
        }
        public void listDialogLines()
        {
            for (int i = 0; i < DialogLines.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {DialogLines[i]}");
            }
        }

        public string GetRandomDialogLine()
        {
            if (DialogLines.Count == 0)
            {
                return string.Empty;
            }
            Random random = new Random();
            int index = random.Next(DialogLines.Count);
            return DialogLines[index];
        }

        public bool HasDialogLine(string line)
        {
            return DialogLines.Any(l => string.Equals(l, line, StringComparison.OrdinalIgnoreCase));
        }

        public string GetDialogLine(int index)
        {
            if (index < 0 || index >= DialogLines.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            return DialogLines[index];
        }
        #endregion
    }
}
