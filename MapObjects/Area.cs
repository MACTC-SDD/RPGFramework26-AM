using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGFramework.MapObjects
{
    public class Area
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public Dictionary<int, Exit> Exits { get; set; } = new();

        public Dictionary<int, Room> Rooms { get; set; } = new();
    }
}
