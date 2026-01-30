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
        private static void CopyDirectoryIfMissing(string sourceDir, string destDir, bool overwrite = false)
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

                // Never overwrite runtime data unless explicitly told to.
                if (!File.Exists(destPath) || overwrite)
                {
                    File.Copy(sourcePath, destPath, overwrite);
                }
            }
        }

        private static void CreateStarterArea()
        {
            Area area = new()
            {
                Id = 0,                
                Name = "Starter Area",
                Description = "The first place new players enter."
            };

            Room room = new()
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
            ArgumentNullException.ThrowIfNull(options);

            string baseDir = AppContext.BaseDirectory;

            string runtimeDataDir = Path.Combine(baseDir, options.RuntimeDataRelativePath);
            string runtimeAreasDir = Path.Combine(runtimeDataDir, "areas");
            string runtimePlayersDir = Path.Combine(runtimeDataDir, "players");
            string runtimeCatalogsDir = Path.Combine(runtimeDataDir, "catalogs");

            List<DirectoryInfo> dataDirectories = new List<DirectoryInfo>();
            dataDirectories.Add(Directory.CreateDirectory(runtimeDataDir));
            dataDirectories.Add(Directory.CreateDirectory(runtimeAreasDir));
            dataDirectories.Add(Directory.CreateDirectory(runtimePlayersDir));
            dataDirectories.Add(Directory.CreateDirectory(runtimeCatalogsDir));

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

            // If CopyFilesFromDataSeed was set, copy all files from seed directories to runtime directories.
            if (options.CopyFilesFromDataSeedToRuntimeData)
            {                 
                if (!string.IsNullOrWhiteSpace(options.SeedDataRelativePath))
                {
                    string seedDataDir = Path.Combine(baseDir, options.SeedDataRelativePath);
                    // Avoid accidental "copy the folder into itself" scenarios.
                    if (Directory.Exists(seedDataDir) &&
                        !PathsReferToSameDirectory(seedDataDir, runtimeDataDir))
                    {
                        CopyDirectoryIfMissing(seedDataDir, runtimeDataDir, options.CopyFilesFromDataSeedToRuntimeData);
                    }
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
            return Task.FromResult<Area?>(area);
        }

        public Task<IReadOnlyDictionary<int, Area>> LoadAreasAsync()
        {
            var areas = ObjectStorage.LoadAllObjects<Area>("data/areas/");
            /* var dict = areas.ToDictionary(a => a.Id);*/
            var dict = areas
         .GroupBy(a => a.Id)
         .Select(g => g.First())
         .ToDictionary(a => a.Id);
            return Task.FromResult((IReadOnlyDictionary<int, Area>)dict);
        }

        public Task<IReadOnlyDictionary<string, Player>> LoadPlayersAsync()
        {
            var players = ObjectStorage.LoadAllObjects<Player>("data/players/");
            var dict = players.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            return Task.FromResult((IReadOnlyDictionary<string, Player>)dict);
        }

        public Task<T?> LoadCatalogAsync<T>(string catalogName) where T : class
        {
            var catalog = ObjectStorage.LoadObject<T?>("data/catalogs/", $"{catalogName}.json");
            return Task.FromResult(catalog);
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

        public Task SaveCatalogAsync(object catalog, string catalogName)
        {
            ObjectStorage.SaveObject(catalog, "data/catalogs/", $"{catalogName}.json");
            return Task.CompletedTask;
        }
        #endregion
    }
}
