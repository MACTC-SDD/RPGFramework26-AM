using RPGFramework.Enums;

namespace RPGFramework
{
    /// <summary>
    /// Generally helpful methods
    /// </summary>
    internal class Utility
    {
        /// <summary>
        /// Check if player has permission to execute a command.
        /// For now, this means you have to have a role equal to or higher than the required role.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        
        public static int GetNextGlobalId()
        {
            return Guid.NewGuid().GetHashCode();
        }
        public static bool CheckPermission(Player player, PlayerRole role)
        {
            return player.Role >= role;
        }

        /// <summary>
        /// Creates a deep copy of the specified object by serializing and deserializing it using JSON.
        /// </summary>
        /// <remarks>NOTE: If you are cloning something with a unique id, the id of the cloned object
        /// will need up be updated.</remarks>
        /// <typeparam name="T">The type of the object to clone. Must be serializable by System.Text.Json.</typeparam>
        /// <param name="obj">The object to clone.</param>
        /// <returns>A deep copy of the specified object, or null if <paramref name="obj"/> is null.</returns>
        public static T? Clone<T>(T obj)
        {
            var serialized = System.Text.Json.JsonSerializer.Serialize(obj);
            return System.Text.Json.JsonSerializer.Deserialize<T>(serialized);
        }
    }
}
