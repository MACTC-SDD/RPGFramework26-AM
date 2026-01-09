using System.IO;
using RPGFramework.Geography;

namespace RPGFramework.Persistence
{
    /// <summary>
    /// Provides JSON-based persistence for game data, including areas and players.
    /// </summary>
    /// <remarks> <see cref="JsonGamePersistence"/> implements the <see cref="IGamePersistence"/>
    /// interface to load and save game data using JSON files stored on disk. 
    /// Areas and players are persisted in separate directories, and each entity is 
    /// stored as an individual file. This class is not
    /// thread-safe. Concurrent calls to persistence methods may result in inconsistent 
    /// data if not externally synchronized.</remarks>
    internal sealed class JsonGamePersistence : IGamePersistence
    {
        #region Initialization Methods
        private static void CopyDirectoryIfMissing(string sourceDir, string destDir)
        {
            foreach (string sourcePath in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(sourceDir, sourcePath);
                string destPath = Path.Combine(destDir, relative);

                string? destParent = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrWhiteSpace(destParent))
                {
                    Directory.CreateDirectory(destParent);
                }

                // Never overwrite runtime data.
                if (!File.Exists(destPath))
                {
                    File.Copy(sourcePath, destPath);
                }
            }
        }

        private static void CreateStarterArea()
        {
            Area area = new Area
            {
                Id = 0,
                Name = "Starter Area",
                Description = "The first place new players enter."
            };

            Room room = new Room
            {
                Id = 0,
                AreaId = 0,
                Name = "The Void",
                Description = "You stand in a quiet, empty space. It is a safe place to begin."
            };

            area.Rooms.Add(room.Id, room);

            ObjectStorage.SaveObject(area, "data/areas/", $"{area.Name}.json");
        }

        public Task EnsureInitializedAsync(GamePersistenceInitializationOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string baseDir = AppContext.BaseDirectory;

            string runtimeDataDir = Path.Combine(baseDir, options.RuntimeDataRelativePath);
            string runtimeAreasDir = Path.Combine(runtimeDataDir, "areas");
            string runtimePlayersDir = Path.Combine(runtimeDataDir, "players");

            Directory.CreateDirectory(runtimeDataDir);
            Directory.CreateDirectory(runtimeAreasDir);
            Directory.CreateDirectory(runtimePlayersDir);

            if (!string.IsNullOrWhiteSpace(options.SeedDataRelativePath))
            {
                string seedDataDir = Path.Combine(baseDir, options.SeedDataRelativePath);

                // Avoid accidental "copy the folder into itself" scenarios.
                if (Directory.Exists(seedDataDir) &&
                    !PathsReferToSameDirectory(seedDataDir, runtimeDataDir))
                {
                    CopyDirectoryIfMissing(seedDataDir, runtimeDataDir);
                }
            }

            if (options.CreateStarterAreaIfMissing)
            {
                bool hasAnyAreaFile = Directory.EnumerateFiles(runtimeAreasDir, "*.json", SearchOption.TopDirectoryOnly).Any();
                if (!hasAnyAreaFile)
                {
                    CreateStarterArea();
                }
            }

            return Task.CompletedTask;
        }

        private static bool PathsReferToSameDirectory(string a, string b)
        {
            string fullA = Path.GetFullPath(a.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string fullB = Path.GetFullPath(b.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            return string.Equals(fullA, fullB, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Load Methods
        public Task<Area?> LoadAreaAsync(string areaName)
        {
            var area = ObjectStorage.LoadObject<Area>($"data/areas/",$"{areaName}");            
            return Task.FromResult(area);
        }

        public Task<IReadOnlyDictionary<int, Area>> LoadAreasAsync()
        {
            var areas = ObjectStorage.LoadAllObjects<Area>("data/areas/");
            var dict = areas.ToDictionary(a => a.Id);
            return Task.FromResult((IReadOnlyDictionary<int, Area>)dict);
        }

        public Task<IReadOnlyDictionary<string, Player>> LoadPlayersAsync()
        {
            var players = ObjectStorage.LoadAllObjects<Player>("data/players/");
            var dict = players.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            return Task.FromResult((IReadOnlyDictionary<string, Player>)dict);
        }
        #endregion

        #region Save Methods
        public Task SaveAreasAsync(IEnumerable<Area> areas)
        {
            foreach (var area in areas)
            {
                ObjectStorage.SaveObject(area, "data/areas/", $"{area.Name}.json");
            }

            return Task.CompletedTask;
        }

        public Task SavePlayersAsync(IEnumerable<Player> players)
        {
            foreach (var player in players)
            {
                ObjectStorage.SaveObject(player, "data/players/", $"{player.Name}.json");
            }

            return Task.CompletedTask;
        }

        public Task SavePlayerAsync(Player player)
        {
            ObjectStorage.SaveObject(player, "data/players/", $"{player.Name}.json");
            return Task.CompletedTask;
        }
        #endregion
    }
}
