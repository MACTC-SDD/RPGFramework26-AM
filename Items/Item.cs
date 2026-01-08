
using RPGFramework;

namespace RPGFramework
{
    internal class Item
    {
        public int Id { get; set; } = 0;
        public string Description { get; set; } = ""; // What you see when you look at it
        public string DisplayText { get; set; } = ""; // How it appears when in a room
        public bool IsDroppable { get; set; } // Can the item be dropped
        public bool IsGettable { get; set; } // Can the item be picked up
        public bool IsStackable { get; set; } = false;

        public int Level { get; set; } = 0;
        public string Name { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
        public int UsesRemaining { get; set; } = -1; // -1 means unlimited uses
        public double Value { get; set; } = 0;
        public double Weight { get; set; } = 0;
    }
}
