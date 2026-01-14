using System.Text.Json;
using RPGFramework.Enums;

namespace RPGFramework.Persistence
{
    internal class ObjectStorage
    {
        /// <summary>
        /// Save an object to a specified path and file name as JSON.
        /// </summary>
        /// <typeparam name="T">Type of object to save.</typeparam>
        /// <param name="obj">Object to save</param>
        /// <param name="path">Path to save file in</param>
        /// <param name="fileName">Name of file</param>
        public static void SaveObject<T>(T obj, string path, string fileName)
        {
            try
            {
                path = Path.Combine(AppContext.BaseDirectory, path);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string filePath = Path.Combine(path, fileName);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonString = JsonSerializer.Serialize(obj, options);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                GameState.Log(DebugLevel.Error, $"Error saving object:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Load an object of a given type from a specified path and file name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static T LoadObject<T>(string path, string fileName)
        {
            path = Path.Combine(AppContext.BaseDirectory, path);
            string filePath = Path.Combine(path, fileName);

            
            if (!File.Exists(filePath))
                throw new FileNotFoundException(
                    $"The file '{fileName}' doesn't exist in the directory '{filePath}'");


            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Load all objects of a given type from a directory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static List<T> LoadAllObjects<T>(string path)
        {
            path = Path.Combine(AppContext.BaseDirectory, path);

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"The directory '{path}' doesn't exist");

            List<T> objects = new List<T>();

            foreach (string file in Directory.EnumerateFiles(path))
            {
                string jsonString = File.ReadAllText(file);
                //Console.WriteLine(jsonString);
                objects.Add(JsonSerializer.Deserialize<T>(jsonString));
            }

            return objects;
        }
    }
}
