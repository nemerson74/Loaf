using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoafGame
{
    public class HexTilemap
    {
        /// <summary>
        /// The map width
        /// </summary>
        public int MapWidth { get; init; }

        /// <summary>
        /// The map height
        /// </summary>
        public int MapHeight { get; init; }

        /// <summary>
        /// The width of a tile in the map
        /// </summary>
        public int TileWidth { get; init; }

        /// <summary>
        /// The height of a tile in the map
        /// </summary>
        public int TileHeight { get; init; }

        /// <summary>
        /// The texture containing the tiles
        /// </summary>
        public Texture2D TilesetTexture { get; init; }

        /// <summary>
        /// The rectangles representing each tile in the tileset
        /// </summary>
        public Rectangle[] Tiles { get; init; }

        /// <summary>
        /// The indices of the tiles in the tileset
        /// </summary>
        public int[] TileIndices { get; init; }

        /// <summary>
        /// Draws the tile-based map to the screen using the specified game time and sprite batch.
        /// </summary>
        /// <remarks>This method renders a hexagonal tile map where tiles are laid out in a pointy-top
        /// configuration.</remarks>
        /// <param name="gameTime">The current game time.</param>
        /// <param name="spriteBatch">The sprite batch.</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, float horizontalOffset, float verticalOffset)
        {
            //for pointy-top hex tiles laid out in columns
            float horizontalSpacing = TileWidth * 0.5f - 1;
            float sideLength = TileHeight / 2f;
            float verticalSpacing = TileHeight + sideLength - 2; // full tile height between rows


            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    // Indices start at 1, so shift by 1 for array coordinates
                    int index = TileIndices[y * MapWidth + x] - 1;

                    // Index of -1 (shifted from 0) should not be drawn
                    if (index == -1) continue;

                    //odd columns are offset vertically by half a tile.
                    float posX = x * horizontalSpacing + horizontalOffset;
                    float posY = y * verticalSpacing + ((x & 1) == 1 ? sideLength * 1.5f - 1f : 0f) + verticalOffset;

                    spriteBatch.Draw(
                        TilesetTexture,
                        new Rectangle(
                            (int)posX,
                            (int)posY,
                            TileWidth,
                            TileHeight
                            ),
                        Tiles[index],
                        Color.White
                        );
                }
            }
        }
    }
}
