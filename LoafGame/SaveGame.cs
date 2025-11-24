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

        public static void SaveOverworld(SaveData data)
        {
            try
            {
                Directory.CreateDirectory(SaveDirectory);
                var options = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };
                var json = JsonSerializer.Serialize(data, options);
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
                var options = new JsonSerializerOptions { IncludeFields = true };
                var read = JsonSerializer.Deserialize<SaveData>(json, options);
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
        public Enums.TileType[] s_Terrain;
        public int[] s_TileIndex;
        public Vector2[] s_Center;
        public int[][] s_SurroundingTilesIndices;
        public bool[] s_IsHighlighted;
        public bool[] s_IsWalkable;
        public bool[] s_HasPlayer;
        public bool[] s_HasBuilding;
        public bool[] s_HasRoad;
    }
}
