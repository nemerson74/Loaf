using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace HexTilemapPipeline
{
    /// <summary>
    /// An importer for a basic tilemap file. The purpose of an importer to to load all important data 
    /// from a file into a content object; any processing of that data occurs in the subsequent content
    /// processor step. 
    /// </summary>
    [ContentImporter(".tmap", DisplayName = "HexTilemapImporter", DefaultProcessor = "HexTilemapProcessor")]
    public class BasicTilemapImporter : ContentImporter<HexTilemapContent>
    {
        public override HexTilemapContent Import(string filename, ContentImporterContext context)
        {
            // Create a new HexTilemapContent
            HexTilemapContent map = new();

            // Read in the map file and split along newlines 
            string data = File.ReadAllText(filename);
            var lines = data.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.RemoveEmptyEntries);

            // First line in the map file is the image file name,
            // we store it so it can be loaded in the processor
            map.TilesetImageFilename = lines[0].Trim();

            // Second line is the tileset image size
            var secondLine = lines[1].Split(',');
            map.TileWidth = int.Parse(secondLine[0]);
            map.TileHeight = int.Parse(secondLine[1]);

            // Third line is the map size (in tiles)
            var thirdLine = lines[2].Split(',');
            map.MapWidth = int.Parse(thirdLine[0]);
            map.MapHeight = int.Parse(thirdLine[1]);

            // Fourth line (and any wrapped continuation lines) are the map data (the indices of tiles in the map)
            // Join any remaining lines after the first three so indices can contain newlines/wrapping, then split.
            var indicesLines = lines.Skip(3);
            string indicesText = string.Join("", indicesLines).Trim();
            map.TileIndices = indicesText
                .Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(index => int.Parse(index.Trim()))
                .ToArray();

            // At this point, we've copied all of the file data into our
            // BasicTilemapContent object, so we pass it on to the processor
            return map;
        }
    }
}