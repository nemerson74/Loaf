using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTilemapPipeline
{
    public class HexTile
    {
        public enum TileType
        {
            Grassland,
            Badland,
            Desert,
            Forest
        }

        /// <summary>
        /// The terrain of this tile
        /// </summary>
        public TileType Terrain { get; init; }

        /// <summary>
        /// The index of the tile in the tileset
        /// </summary>
        public int TileIndex { get; init; }

        /// <summary>
        /// Gets the indices of the tiles surrounding the current tile.
        /// Starts at the top-left and goes clockwise.
        /// </summary>
        public int[] SurroundingTilesIndices { get; init; }

        /// <summary>
        /// Indicates whether the tile is currently highlighted.
        /// </summary>
        public bool IsHighlighted { get; set; } = false;

        /// <summary>
        /// Indicates whether the tile is walkable.
        /// </summary>
        public bool IsWalkable { get; init; } = false;

        /// <summary>
        /// Indicates whether the tile has a player on it.
        /// </summary>
        public bool HasPlayer { get; set; } = false;

        /// <summary>
        /// Indicates whether the tile has a road on it.
        /// </summary>
        public bool HasRoad { get; set; } = false;
    }
}
