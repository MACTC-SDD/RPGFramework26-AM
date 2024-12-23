using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGFramework
{
    /// <summary>
    /// Generally helpful methods
    /// </summary>
    public class Utility
    {
        /// <summary>
        /// Check if player has permission to execute a command.
        /// For now, this means you have to have a role equal to or higher than the required role.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static bool CheckPermission(Player player, PlayerRole role)
        {
            return player.Role >= role;
        }
    }
}
