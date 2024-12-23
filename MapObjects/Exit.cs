using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGFramework.MapObjects
{
    public class Exit
    {
        public int Id { get; set; } = 0;
        public Direction ExitDirection { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int SourceRoomId { get; set; }
        public int DestinationRoomId { get; set; }

        /// <summary>
        /// Finds the highest Exit ID for the current area in GameState and returns one higher
        /// This could lead to gaps in the ID sequence if we delete Exits, but that's ok for now.
        /// </summary>
        /// <returns></returns>
        public static int GetNextId(Area a)
        {
            return GetNextId(a.Id);         
        }

        public static int GetNextId(int areaId)
        {
            if (GameState.Instance.Areas[areaId].Exits.Count == 0)
            {
                return 0;
            }
            return GameState.Instance.Areas[areaId].Exits.Keys.Max() + 1;
            // Return one higher
        }

    }
}
