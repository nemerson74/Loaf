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
        /// Gets the collection of hexagonal tiles associated with the current instance.
        /// </summary>
        private HexTile[] hexTiles { get; set; }

        /// <summary>
        /// Gets the collection of centers tiles associated with the tiles.
        /// </summary>
        private Vector2[] Centers { get; set; }

        private Texture2D roadTexture;

        private Texture2D middleTexture;

        // Index in the map array (y * MapWidth + x) of the currently highlighted tile, or -1 if none
        private int highlightedTile = 0;

        private static readonly Vector2[] ROAD_POSITIONS = new Vector2[]
        {
            new Vector2(-8f, -14f),
            new Vector2(8f, -14f),
            new Vector2(20f, 0f),
            new Vector2(8f, 14f),
            new Vector2(-8f, 14f),
            new Vector2(-20f, 0f)
        };

        private static readonly float[] ROAD_ANGLES = new float[]
        {
            240f,
            300f,
            0f,
            60f,
            120f,
            180f
        };

        /// <summary>
        /// Initializes the hexagonal tiles for the map.
        /// </summary>
        public void InitializeHexTiles(float horizontalOffset, float verticalOffset, int startingTile, Texture2D roadTexture, Texture2D middleTexture)
        {
            highlightedTile = startingTile;
            hexTiles = new HexTile[MapWidth * MapHeight];
            Centers = new Vector2[MapWidth * MapHeight];
            this.roadTexture = roadTexture;
            this.middleTexture = middleTexture;

            float horizontalSpacing = TileWidth * 0.5f;
            float sideLength = TileHeight / 2f;
            float verticalSpacing = TileHeight + sideLength - 2;

            int _tileIndex = 0;
            bool _leftColumn = false;
            bool _rightColumn = false;
            bool _topRow = false;
            bool _bottomRow = false;
            bool _oddRow = false;
            int _thisIndex = 0;
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    _thisIndex = y * MapWidth + x;

                    _tileIndex = TileIndices[_thisIndex];
                    Enums.TileType _terrain = Enums.TileType.Grassland;
                    if (_tileIndex == 1)
                    {
                        _terrain = Enums.TileType.Grassland;
                    }
                    else if (_tileIndex == 3)
                    {
                        _terrain = Enums.TileType.Badland;
                    }
                    else if (_tileIndex == 5)
                    {
                        _terrain = Enums.TileType.Desert;
                    }
                    else
                    {
                        _terrain = Enums.TileType.Forest;
                    }

                    float posX = x * horizontalSpacing + horizontalOffset;
                    float posY = y * verticalSpacing + ((x & 1) == 1 ? sideLength * 1.5f : 0f) + verticalOffset;

                    // center of this tile
                    float centerX = posX + TileWidth * 0.5f;
                    float centerY = posY + TileHeight * 0.5f;
                    Centers[y * MapWidth + x] = new Vector2(centerX, centerY);
                    _leftColumn = x == 0 || x == 1;
                    _rightColumn = x == MapWidth - 1 || x == MapWidth -2;
                    _topRow = y == 0 && x % 2 == 0;
                    _bottomRow = y == MapHeight - 1 && x % 2 != 0;
                    _oddRow = (_thisIndex - y * MapWidth) % 2 != 0;
                    if (_oddRow)
                    {
                        hexTiles[_thisIndex] = new HexTile
                        {
                            TileIndex = _tileIndex,
                            SurroundingTilesIndices = new int[]
                            {
                                _topRow ? -1 : _thisIndex - 1,
                                _topRow ? -1 : _thisIndex + 1,
                                _rightColumn ? -1 : _thisIndex + 2,
                                _bottomRow ? -1 : MapWidth + _thisIndex + 1,
                                _bottomRow ? -1 : MapWidth + _thisIndex - 1,
                                _leftColumn ? -1 : _thisIndex - 2
                            },
                            Terrain = _terrain
                        };
                    }
                    else
                    {
                        hexTiles[_thisIndex] = new HexTile
                        {
                            TileIndex = _tileIndex,
                            SurroundingTilesIndices = new int[]
                            {
                                (_topRow || _leftColumn) ? -2 : _thisIndex - 1 - MapWidth,
                                (_topRow || _rightColumn) ? -2 : _thisIndex + 1 - MapWidth,
                                _rightColumn ? -2 : _thisIndex + 2,
                                (_rightColumn) ? -2 : _thisIndex + 1,
                                (_leftColumn) ? -2 : _thisIndex - 1,
                                _leftColumn ? -2 : _thisIndex - 2
                            },
                            Terrain = _terrain
                        };
                    }

                    hexTiles[_thisIndex].Center = new Vector2(centerX, centerY);
                }
            }
            MovePlayer();
        }

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

            float bestDistSq = float.MaxValue;
            int bestIndex = -1;
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    // center of this tile
                    float centerX = Centers[y * MapWidth + x].X;
                    float centerY = Centers[y * MapWidth + x].Y;

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
            float horizontalSpacing = TileWidth * 0.5f;
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
                    float posY = y * verticalSpacing + ((x & 1) == 1 ? sideLength * 1.5f : 0f) + verticalOffset;

                    // If this map cell is the highlighted one, tint it green
                    int mapCellIndex = y * MapWidth + x;
                    Color tint = Color.White;
                    if (mapCellIndex == highlightedTile)
                    {
                        if (hexTiles[mapCellIndex].IsWalkable)
                        {
                            tint = Color.LightGreen;
                        }
                        else
                        {
                            if (hexTiles[mapCellIndex].HasPlayer)
                            {
                                tint = Color.White;
                            }
                            else
                            {
                                tint = Color.LightGray;
                            }
                        }
                    }
                    //Color tint = (mapCellIndex == highlightedTile) ? Color.LightGreen : Color.White;
                    if (!hexTiles[mapCellIndex].HasBuilding)
                    {
                        spriteBatch.Draw(
                        TilesetTexture,
                        new Rectangle(
                            (int)posX,
                            (int)posY,
                            TileWidth,
                            TileHeight
                            ),
                        Tiles[index],
                        tint,
                        0f,
                        Vector2.Zero,
                        SpriteEffects.None,
                        0f
                        );
                        if (hexTiles[mapCellIndex].HasRoad)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                int neighborIndex = hexTiles[mapCellIndex].SurroundingTilesIndices[i];
                                if (neighborIndex >= 0 && hexTiles[neighborIndex].HasRoad)
                                {
                                    spriteBatch.Draw(
                                        roadTexture,
                                        new Vector2(posX + TileWidth / 2f, posY + TileHeight / 2f) + ROAD_POSITIONS[i],
                                        null,
                                        Color.White,
                                        MathHelper.ToRadians(ROAD_ANGLES[i]),
                                        new Vector2(roadTexture.Width / 2f, roadTexture.Height / 2f),
                                        0.5f,
                                        SpriteEffects.None,
                                        0f
                                        );
                                }
                            }
                            spriteBatch.Draw(
                                middleTexture,
                                new Vector2(posX + TileWidth / 2f, posY + TileHeight / 2f),
                                null,
                                Color.White,
                                0f,
                                new Vector2(middleTexture.Width / 2f, middleTexture.Height / 2f),
                                0.5f,
                                SpriteEffects.None,
                                0f
                                );
                        }
                    }
                    if (hexTiles[mapCellIndex].HasBuilding)
                    {
                        spriteBatch.Draw(
                        TilesetTexture,
                        new Rectangle(
                            (int)posX,
                            (int)posY,
                            TileWidth,
                            TileHeight
                            ),
                        Tiles[index],
                        tint,
                        0f,
                        Vector2.Zero,
                        SpriteEffects.None,
                        0f
                        );
                    }
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
            return hexTiles[highlightedTile].Center;
        }

        /// <summary>
        /// Moves the player to the highlighted tile.
        /// </summary>
        public bool MovePlayer()
        {
            if (highlightedTile == -1) return false;
            if (!hexTiles[highlightedTile].IsWalkable)
            {
                return false;
            }
            foreach (HexTile tile in hexTiles)
            {
                tile.HasPlayer = false;
                tile.IsWalkable = false;
            }
            hexTiles[highlightedTile].HasPlayer = true;
            foreach (int index in hexTiles[highlightedTile].SurroundingTilesIndices)
            {
                if (index >= 0)
                {
                    hexTiles[index].IsWalkable = true;
                }
            }
            return true;
        }

        /// <summary>
        /// Build a building on the specified tile index.
        /// </summary>
        /// <param name="index">The specified tile index.</param>
        public void BuildTile(int index)
        {
            if (index == -1) return;
            if (hexTiles[index].HasBuilding) return;
            Enums.TileType terrain = GetTileTerrain(index);
            if (HasRoad(index))
            {
                if (terrain == Enums.TileType.Forest)
                {
                    TileIndices[index] = 19;
                    hexTiles[index].TileIndex = 19;
                }
                else if (terrain == Enums.TileType.Desert)
                {
                    TileIndices[index] = 22;
                    hexTiles[index].TileIndex = 22;
                }
                else if (terrain == Enums.TileType.Badland)
                {
                    TileIndices[index] = 21;
                    hexTiles[index].TileIndex = 21;
                }
                else if (terrain == Enums.TileType.Grassland)
                {
                    TileIndices[index] = 20;
                    hexTiles[index].TileIndex = 20;
                }
            }
            else
            {
                if (terrain == Enums.TileType.Forest)
                {
                    TileIndices[index] = 13;
                    hexTiles[index].TileIndex = 13;
                }
                else if (terrain == Enums.TileType.Desert)
                {
                    TileIndices[index] = 16;
                    hexTiles[index].TileIndex = 16;
                }
                else if (terrain == Enums.TileType.Badland)
                {
                    TileIndices[index] = 15;
                    hexTiles[index].TileIndex = 15;
                }
                else if (terrain == Enums.TileType.Grassland)
                {
                    TileIndices[index] = 14;
                    hexTiles[index].TileIndex = 14;
                }
            }
            hexTiles[index].HasBuilding = true;
        }

        /// <summary>
        /// Build a road on the specified tile index.
        /// </summary>
        /// <param name="index">The specified tile index.</param>
        public void BuildRoad(int index)
        {
            if (index == -1) return;
            if (hexTiles[index].HasRoad) return;
            Enums.TileType terrain = GetTileTerrain(index);
            if (HasBuilding(index))
            {
                if (terrain == Enums.TileType.Forest)
                {
                    TileIndices[index] = 19;
                    hexTiles[index].TileIndex = 19;
                }
                else if (terrain == Enums.TileType.Desert)
                {
                    TileIndices[index] = 22;
                    hexTiles[index].TileIndex = 22;
                }
                else if (terrain == Enums.TileType.Badland)
                {
                    TileIndices[index] = 21;
                    hexTiles[index].TileIndex = 21;
                }
                else if (terrain == Enums.TileType.Grassland)
                {
                    TileIndices[index] = 20;
                    hexTiles[index].TileIndex = 20;
                }
            }
            hexTiles[index].HasRoad = true;
        }

        public bool HasBuilding(int index)
        {
            if (index == -1) return false;
            return hexTiles[index].HasBuilding;
        }

        public bool HasRoad(int index)
        {
            if (index == -1) return false;
            return hexTiles[index].HasRoad;
        }

        public Enums.TileType GetTileTerrain(int index)
        {
            if (index == -1) return Enums.TileType.Forest;
            return hexTiles[index].Terrain;
        }

        /// <summary>
        /// Moves the player to the highlighted tile.
        /// </summary>
        public int GetPlayerIndex()
        {
            for (int i = 0; i < hexTiles.Length; i++)
            {
                if (hexTiles[i].HasPlayer == true) return i;
            }
            return -1;
        }

        public Vector2 GetCenter(int index)
        {
            if (index == -1) return Vector2.Zero;
            return Centers[index];
        }

        public int[] GetSurrounding(int index)
        {
            if (index == -1) return new int[] { 0, 0, 0, 0, 0, 0 };
            return new int[]
            {
                hexTiles[index].SurroundingTilesIndices[0],
                hexTiles[index].SurroundingTilesIndices[1],
                hexTiles[index].SurroundingTilesIndices[2],
                hexTiles[index].SurroundingTilesIndices[3],
                hexTiles[index].SurroundingTilesIndices[4],
                hexTiles[index].SurroundingTilesIndices[5]
            };
        }

        public int GetHighlightedTile()
        {
            return highlightedTile;
        }

        public SaveData GiveHexState(ScoreTracker scoreTracker)
        {
            SaveData saveData = new SaveData();
            saveData.s_Terrain = new Enums.TileType[hexTiles.Length];
            saveData.s_TileIndex = new int[hexTiles.Length];
            saveData.s_Center = new Vector2[hexTiles.Length];
            saveData.s_SurroundingTilesIndices = new int[hexTiles.Length][];
            saveData.s_IsHighlighted = new bool[hexTiles.Length];
            saveData.s_IsWalkable = new bool[hexTiles.Length];
            saveData.s_HasPlayer = new bool[hexTiles.Length];
            saveData.s_HasBuilding = new bool[hexTiles.Length];
            saveData.s_HasRoad = new bool[hexTiles.Length];
            for (int i = 0; i < hexTiles.Length; i++)
            {
                saveData.s_Terrain[i] = hexTiles[i].Terrain;
                saveData.s_TileIndex[i] = hexTiles[i].TileIndex;
                saveData.s_Center[i] = hexTiles[i].Center;
                saveData.s_SurroundingTilesIndices[i] = new int[]
                {
                    hexTiles[i].SurroundingTilesIndices[0],
                    hexTiles[i].SurroundingTilesIndices[1],
                    hexTiles[i].SurroundingTilesIndices[2],
                    hexTiles[i].SurroundingTilesIndices[3],
                    hexTiles[i].SurroundingTilesIndices[4],
                    hexTiles[i].SurroundingTilesIndices[5]
                };
                saveData.s_IsHighlighted[i] = (i == highlightedTile);
                saveData.s_IsWalkable[i] = hexTiles[i].IsWalkable;
                saveData.s_HasPlayer[i] = hexTiles[i].HasPlayer;
                saveData.s_HasBuilding[i] = hexTiles[i].HasBuilding;
                saveData.s_HasRoad[i] = hexTiles[i].HasRoad;

            }
            saveData.s_scores = scoreTracker.GetScores();
            return saveData;
        }

        public void TakeHexState(SaveData data, out ScoreTracker scoreTracker)
        {
            for (int i = 0; i < hexTiles.Length; i++)
            {
                hexTiles[i].Terrain = data.s_Terrain[i];
                hexTiles[i].TileIndex = data.s_TileIndex[i];
                TileIndices[i] = data.s_TileIndex[i];
                hexTiles[i].Center = data.s_Center[i];
                hexTiles[i].SurroundingTilesIndices = new int[]
                {
                    data.s_SurroundingTilesIndices[i][0],
                    data.s_SurroundingTilesIndices[i][1],
                    data.s_SurroundingTilesIndices[i][2],
                    data.s_SurroundingTilesIndices[i][3],
                    data.s_SurroundingTilesIndices[i][4],
                    data.s_SurroundingTilesIndices[i][5]
                };
                hexTiles[i].IsWalkable = data.s_IsWalkable[i];
                hexTiles[i].HasPlayer = data.s_HasPlayer[i];
                hexTiles[i].HasBuilding = data.s_HasBuilding[i];
                hexTiles[i].HasRoad = data.s_HasRoad[i];
                if (data.s_IsHighlighted[i])
                {
                    highlightedTile = i;
                }
            }
            scoreTracker = new ScoreTracker();
            scoreTracker.TakeScores(data.s_scores);
        }

        public bool HasRoadPathWithAllBuildingTypes(out int roadCount)
        {
            roadCount = 0;
            if (hexTiles == null || hexTiles.Length == 0) return false;

            var required = new HashSet<int> { 19, 20, 21, 22 };
            var visited = new bool[hexTiles.Length];

            for (int i = 0; i < hexTiles.Length; i++)
            {
                if (!hexTiles[i].HasRoad || visited[i])
                    continue;

                var queue = new Queue<int>();
                queue.Enqueue(i);
                visited[i] = true;

                var found = new HashSet<int>();
                int componentCount = 0;

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    componentCount++;

                    if (hexTiles[current].HasBuilding)
                    {
                        int idx = TileIndices[current];
                        if (required.Contains(idx))
                        {
                            found.Add(idx);
                            if (found.Count == required.Count)
                            {
                                roadCount = componentCount;
                                return true;
                            }
                        }
                    }

                    var neighbors = hexTiles[current].SurroundingTilesIndices;
                    for (int n = 0; n < neighbors.Length; n++)
                    {
                        int neighbor = neighbors[n];
                        if (neighbor >= 0 && !visited[neighbor] && hexTiles[neighbor].HasRoad)
                        {
                            visited[neighbor] = true;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return false;
        }
    }

}
