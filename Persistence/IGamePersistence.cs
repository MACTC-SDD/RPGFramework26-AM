using RPGFramework.Geography;

namespace RPGFramework.Persistence
{
    /// <summary>
    /// This will let us create different persistence mechanisms (e.g., file-based, database-based, etc.)
    /// Initially, we will implement a file-based persistence mechanism.
    /// But we may want to implement a SQLite / Entity Framework based persistence mechanism later.
    /// Add methods as needed to store additional game state information.
    /// </summary>
    internal interface IGamePersistence
    {
        /// <summary>
        /// Ensures the persistence store is ready to be used (directories exist, database created/migrated, seed data copied, etc.).
        /// Implementations that do not require initialization can implement this as a no-op.
        /// </summary>
        Task EnsureInitializedAsync(GamePersistenceInitializationOptions options);

        /// <summary>
        /// Asynchronously loads all available areas and returns them as a read-only dictionary.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a read-only dictionary mapping area IDs to <see cref="Area"/> instances. 
        /// The dictionary is empty if no areas are available.</returns>
        Task<IReadOnlyDictionary<int, Area>> LoadAreasAsync();

        /// <summary>
        /// Asynchronously loads the area with the specified identifier.
        /// </summary>
        /// <param name="areaId">The unique identifier of the area to load. 
        /// Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains the <see cref="Area"/> object if
        /// found; otherwise, <see langword="null"/>.</returns>
        Task<Area?> LoadAreaAsync(string areaName);

        /// <summary>
        /// Asynchronously loads all players and returns a read-only dictionary of player 
        /// identifiers and their corresponding <see cref="Player"/> objects.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a read-only dictionary mapping
        /// player IDs to <see cref="Player"/> instances. 
        /// The dictionary is empty if no players are found.</returns>
        Task<IReadOnlyDictionary<string, Player>> LoadPlayersAsync();

        /// <summary>
        /// Asynchronously saves the specified collection of areas to the data store.
        /// </summary>
        /// <param name="areas">The collection of <see cref="Area"/> objects to save. 
        /// Cannot be null. Each area in the collection will be persisted.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task SaveAreasAsync(IEnumerable<Area> areas);

        /// <summary>
        /// Asynchronously saves a collection of player records to the data store.
        /// </summary>
        /// <param name="players">The collection of <see cref="Player"/> objects to save. 
        /// Cannot be null. Each player in the collection will be persisted.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task SavePlayersAsync(IEnumerable<Player> players);

        /// <summary>
        /// Asynchronously saves the specified player to the data store.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> instance to save. Cannot be <c>null</c>.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task SavePlayerAsync(Player player);
    }

    /// <summary>
    /// Provides configuration options for initializing game persistence, including paths for runtime and seed data
    /// folders and behavior for creating a starter area.
    /// </summary>
    /// <remarks>Use this type to specify how the game should locate and prepare its data directories during
    /// startup. The options control where live game data is stored, whether initial seed data should be copied, and
    /// whether a minimal starter area is automatically created if no area data is present. All paths are relative to
    /// the application's base directory.</remarks>
    internal sealed class GamePersistenceInitializationOptions
    {
        /// <summary>
        /// Runtime data folder relative to <see cref="AppContext.BaseDirectory"/>.
        /// This is where the game reads/writes live data.
        /// Default: "data"
        /// </summary>
        public string RuntimeDataRelativePath { get; init; } = "data";

        /// <summary>
        /// Optional seed folder relative to <see cref="AppContext.BaseDirectory"/>.
        /// If present, files are copied into the runtime data folder only when missing.
        /// Default: "data_seed"
        /// </summary>
        public string? SeedDataRelativePath { get; init; } = "data_seed";

        /// <summary>
        /// If true, creates a minimal starter area if the runtime areas folder has no area json files
        /// after seed copy.
        /// Default: true
        /// </summary>
        public bool CreateStarterAreaIfMissing { get; init; } = true;
    }
}
