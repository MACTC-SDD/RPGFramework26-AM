using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGFramework
{
    public class Item
    {
        public int Id { get; set; } = 0;
        public string Description { get; set; } = "";
        public string Name { get; set; } = "";
        public double Value { get; set; } = 0;
        public double Weight { get; set; } = 0;
    }
}
