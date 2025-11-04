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

        // Index in the map array (y * MapWidth + x) of the currently highlighted tile, or -1 if none
        private int highlightedTile = -1;

        /// <summary>
        /// Updates the tilemap state. Determines which tile the provided mouse position is closest to
        /// by comparing distances to each tile center and stores that tile as highlighted.
        /// </summary>
        /// <param name="mousePosition">Mouse position in screen coordinates.</param>
        /// <param name="horizontalOffset">Horizontal offset applied when drawing the map.</param>
        /// <param name="verticalOffset">Vertical offset applied when drawing the map.</param>
        public void Update(Vector2 mousePosition, float horizontalOffset, float verticalOffset)
        {
            if (MapWidth <= 0 || MapHeight <= 0) 
            {
                highlightedTile = -1;
                return;
            }

            // for pointy-top hex tiles laid out in columns
            float horizontalSpacing = TileWidth * 0.5f - 1;
            float sideLength = TileHeight / 2f;
            float verticalSpacing = TileHeight + sideLength - 2; // full tile height between rows

            float bestDistSq = float.MaxValue;
            int bestIndex = -1;
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    float posX = x * horizontalSpacing + horizontalOffset;
                    float posY = y * verticalSpacing + ((x & 1) == 1 ? sideLength * 1.5f - 1f : 0f) + verticalOffset;

                    // center of this tile
                    float centerX = posX + TileWidth * 0.5f;
                    float centerY = posY + TileHeight * 0.5f;

                    float dx = mousePosition.X - centerX;
                    float dy = mousePosition.Y - centerY;
                    float distSq = dx * dx + dy * dy;

                    if (distSq < bestDistSq)
                    {
                        bestDistSq = distSq;
                        bestIndex = y * MapWidth + x;
                    }
                }
            }

            highlightedTile = bestIndex;
        }

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
                    // Indices start at 1, so shift to get tile index
                    int index = TileIndices[y * MapWidth + x] - 1;

                    // Index of -1 (shifted from 0) should not be drawn
                    if (index == -1) continue;

                    //odd columns are offset vertically by half a tile.
                    float posX = x * horizontalSpacing + horizontalOffset;
                    float posY = y * verticalSpacing + ((x & 1) == 1 ? sideLength * 1.5f - 1f : 0f) + verticalOffset;

                    // If this map cell is the highlighted one, tint it green
                    int mapCellIndex = y * MapWidth + x;
                    Color tint = (mapCellIndex == highlightedTile) ? Color.LightGreen : Color.White;

                    spriteBatch.Draw(
                        TilesetTexture,
                        new Rectangle(
                            (int)posX,
                            (int)posY,
                            TileWidth,
                            TileHeight
                            ),
                        Tiles[index],
                        tint
                        );
                }
            }
        }

        /// <summary>
        /// Calculates the center position of the currently highlighted tile in the map.
        /// </summary>
        /// <returns>A <see cref="Vector2"/> representing the center position of the highlighted tile.  Returns <see
        /// cref="Vector2.Zero"/> if no tile is highlighted.</returns>
        public Vector2 GetHighlightedCenterVector()
        {
            if (highlightedTile == -1) return Vector2.Zero;

            int x = highlightedTile % MapWidth;
            int y = highlightedTile / MapWidth;

            // Calculate the center position of the highlighted tile
            float horizontalSpacing = TileWidth * 0.5f - 1;
            float sideLength = TileHeight / 2f;
            float verticalSpacing = TileHeight + sideLength - 2;

            float posX = x * horizontalSpacing;
            float posY = y * verticalSpacing + ((x & 1) == 1 ? sideLength * 1.5f - 1f : 0f);

            return new Vector2(posX + TileWidth * 0.5f, posY + TileHeight * 0.5f);
        }
    }
}
