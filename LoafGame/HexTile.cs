using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoafGame
{
    public class HexTile
    {

        /// <summary>
        /// Gets the type of terrain represented by this tile.
        /// </summary>
        public Enums.TileType Terrain { get; init; }

        /// <summary>
        /// The index of the tile in the tileset
        /// </summary>
        public int TileIndex { get; init; }

        /// <summary>
        /// Gets the center point of the object as a two-dimensional vector.
        /// </summary>
        public Vector2 Center { get; set; } = Vector2.Zero;

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
        public bool IsWalkable { get; set; } = true;

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
