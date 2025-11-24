using System;
using System.IO;
using System.Text.Json;
using LoafGame.Scenes;
using Microsoft.Xna.Framework;

namespace LoafGame
{
    public static class SaveGame
    {
        private static string SaveDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Loaf");
        private static string SavePath => Path.Combine(SaveDirectory, "overworld.json");

        public static void SaveOverworld(Scenes.OverworldScene scene)
        {
            try
            {
                Directory.CreateDirectory(SaveDirectory);
                var data = new SaveData { SavedScene = scene };
                var json = JsonSerializer.Serialize(data);
                File.WriteAllText(SavePath, json);
            }
            catch
            {
                // swallow exceptions for now or log if a logger exists
            }
        }

        public static bool TryLoadOverworld(out SaveData data)
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    data = new SaveData();
                    return false;
                }
                var json = File.ReadAllText(SavePath);
                var read = JsonSerializer.Deserialize<SaveData>(json);
                if (read == null)
                {
                    data = new SaveData();
                    return false;
                }
                data = read;
                return true;
            }
            catch
            {
                data = new SaveData();
                return false;
            }
        }
    }

    public class SaveData
    {
        public OverworldScene SavedScene { get; set; }
    }
}
