using LoafGame.Collisions;
using LoafGame.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Reflection.Metadata;

namespace LoafGame.Scenes;

public class OverworldScene : Scene
{
    private ScoreTracker scoreTracker = new ScoreTracker();
    private SpriteBatch _spriteBatch;
    private RedWorker redWorker;
    private HexTilemap _tilemap;
    private SpriteFont _font;
    private bool isSaved = false;
    private bool loadingFromSave = false;
    private SaveData loadedSave;
    private float textFadeTimer = 0f;
    private float textFadeOpacity = 1f;
    private bool debugFlag = false;
    private bool tutorialFlag = true;
    private int prevHighlightedTile = -1;
    private int currentHighlightedTile = -1;
    private int playerTile = -1;
    private Button buildButton, roadButton, saveButton;
    private Texture2D roadTexture;
    private Texture2D middleTexture;
    private bool gameOver = false;
    private int roadScore = 0;

    private float vw;
    private float vh;
    private float leftMargin;
    private float centerX;
    private float centerY;
    private float mapWidth;
    private float mapHeight;
    private float horizontalOffset = 0f;
    private float verticalOffset = 0f;

    private float buttonZoneThresh = 0f;

    private Vector2 startingPosition = Vector2.Zero;

    /// <summary>
    /// Initializes a new instance of the <see cref="OverworldScene"/> class.
    /// </summary>
    /// <param name="game">The game instance that this scene is associated with.</param>
    public OverworldScene(Game game) : base(game) { }

    public OverworldScene(Game game, SaveData save) : base(game) 
    {
        loadingFromSave = true;
        loadedSave = save;
    }

    public override void Initialize()
    {
        redWorker = new RedWorker()
        {
            Position = startingPosition,
            Direction = Direction.Down,
            Scale = 2f
        };
        var LOAF = Game as LOAF;
        vw = Game.GraphicsDevice.Viewport.Width / LOAF.GameScale;
        vh = Game.GraphicsDevice.Viewport.Height / LOAF.GameScale;
        leftMargin = vw * 0.02f;
        centerX = vw / 2;
        centerY = vh / 2;
        MediaPlayer.Stop();
        MediaPlayer.Volume = 0.5f;
        MediaPlayer.Play(LOAF.backgroundMusicOverworld);
        MediaPlayer.IsRepeating = true;
        buildButton = new Button() { Position = new Vector2(vw * 0.55f, vh * 0.05f), Text = "Build" };
        roadButton = new Button() { Position = new Vector2(vw * 0.7f, vh * 0.05f), Text = "Road" };
        saveButton = new Button() { Position = new Vector2(vw * 0.85f, vh * 0.05f), Text = "Save" };
        buttonZoneThresh = vh * 0.1f;
        base.Initialize();
    }

    public override void Reinitialize()
    {
        MediaPlayer.Stop();
        MediaPlayer.Play(LOAF.backgroundMusicOverworld);
        MediaPlayer.IsRepeating = true;

        if (_tilemap.GetTileTerrain(playerTile) == Enums.TileType.Forest)
        {
            if (scoreTracker.ForestPoints != 0) _tilemap.BuildTile(playerTile);
            LOAF.BuildSound.Play();
        }
        else if (_tilemap.GetTileTerrain(playerTile) == Enums.TileType.Desert)
        {
            //if (scoreTracker.DesertPoints != 0) _tilemap.BuildTile(playerTile);
            _tilemap.BuildTile(playerTile);
            LOAF.BuildSound.Play();
        }
        else if (_tilemap.GetTileTerrain(playerTile) == Enums.TileType.Badland)
        {
            //if (scoreTracker.BadlandPoints != 0) _tilemap.BuildTile(playerTile);
            _tilemap.BuildTile(playerTile);
            LOAF.BuildSound.Play();
        }
        else if (_tilemap.GetTileTerrain(playerTile) == Enums.TileType.Grassland)
        {
            //if (scoreTracker.GrasslandPoints != 0) _tilemap.BuildTile(playerTile);
            _tilemap.BuildTile(playerTile);
            LOAF.BuildSound.Play();
        }
        if (scoreTracker.IsComplete())
        {
            if (_tilemap.HasRoadPathWithAllBuildingTypes(out roadScore))
            {
                gameOver = true;
            }
        }
    }

    public override void LoadContent()
    {
        var LOAF = Game as LOAF;
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        _tilemap = Content.Load<HexTilemap>("tilemapkey");
        mapWidth = (_tilemap.TileWidth * _tilemap.MapWidth / 2);
        mapHeight = (_tilemap.TileHeight * _tilemap.MapHeight * 1.5f + _tilemap.TileHeight * 0.25f);
        verticalOffset = (vh - mapHeight) / 2f;
        horizontalOffset = 0f;
        roadTexture = Content.Load<Texture2D>("Overworld/road");
        middleTexture = Content.Load<Texture2D>("Overworld/RoadMiddle28x32");
        _tilemap.InitializeHexTiles(horizontalOffset, verticalOffset, 0, roadTexture, middleTexture);
        redWorker.LoadContent(Content);
        redWorker.Position = _tilemap.GetCenter(0) + new Vector2(-16, -16);
        _font = Content.Load<SpriteFont>("vergilia");
        if (loadingFromSave)
        {
            _tilemap.TakeHexState(loadedSave, out scoreTracker);
            redWorker.Position = _tilemap.GetCenter(_tilemap.GetPlayerIndex()) + new Vector2(-16, -16);
        }
        buildButton.LoadContent(Content);
        roadButton.LoadContent(Content);
        saveButton.LoadContent(Content);
    }

    public override void Update(GameTime gameTime)
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        debugFlag = LOAF.InputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space);
        Vector2 MousePos = LOAF.InputManager.Position / LOAF.GameScale;

        if (MousePos.Y < buttonZoneThresh)
        {
            BoundingPoint cursor = new BoundingPoint(MousePos);
            buildButton.Update(cursor.CollidesWith(buildButton.Bounds));
            roadButton.Update(cursor.CollidesWith(roadButton.Bounds));
            saveButton.Update(cursor.CollidesWith(saveButton.Bounds));

            if (LOAF.InputManager.LeftMouseClicked)
            {
                if (buildButton.Hover)
                {
                    if (Build(LOAF)) { /* scene change happens in Build */ }
                }

                if (roadButton.Hover)
                {
                    if (_tilemap.HasRoad(_tilemap.GetPlayerIndex()))
                    {
                        LOAF.DeniedSound.Play();
                    }
                    else
                    {
                        _tilemap.BuildRoad(_tilemap.GetPlayerIndex());
                        LOAF.BuildSound.Play();
                        if (scoreTracker.IsComplete() && _tilemap.HasRoadPathWithAllBuildingTypes(out roadScore))
                        {
                            gameOver = true;
                        }
                    }
                }

                if (saveButton.Hover)
                {
                    saveButton.PlayClickSound();
                    SaveGame.SaveOverworld(_tilemap.GiveHexState(scoreTracker));
                    isSaved = true;
                }
            }
        }
        else
        {
            // Tilemap hover and click handling outside button zone
            _tilemap.Update(MousePos, 0, (vh - mapHeight) / 2);
            playerTile = _tilemap.GetPlayerIndex();
            currentHighlightedTile = _tilemap.GetHighlightedTile();

            if (prevHighlightedTile != currentHighlightedTile)
            {
                prevHighlightedTile = currentHighlightedTile;
            }

            if (LOAF.InputManager.LeftMouseClicked)
            {
                if (!_tilemap.MovePlayer())
                {
                    LOAF.DeniedSound.Play();
                }
                else
                {
                    LOAF.FleeSound.Play();
                    redWorker.Position = _tilemap.GetHighlightedCenterVector() + new Vector2(-16, -16);
                    tutorialFlag = false;
                }
            }
        }

        // Return to title with Escape
        if (LOAF.InputManager.KeyClicked(Keys.Escape))
        {
            LOAF.ChangeScene(new TitleScene(LOAF));
            return;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var LOAF = Game as LOAF;
        Game.GraphicsDevice.Clear(Color.DarkSlateGray);
        _spriteBatch.Begin(transformMatrix: Matrix.CreateScale(LOAF.GameScale), blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
        
        _spriteBatch.DrawString(_font, "ESC to return, Left mouse to move to adjacent tiles, Spacebar for debug", new Vector2(vw * 0.01f, (vh - mapHeight) / 4), Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        buildButton.Draw(_spriteBatch);
        roadButton.Draw(_spriteBatch);
        saveButton.Draw(_spriteBatch);
        _tilemap.Draw(gameTime, _spriteBatch, 0, (vh - mapHeight) / 2);
        if (isSaved && textFadeTimer < 6f)
        {
            if (textFadeTimer < 3f)
            {
                textFadeOpacity = 1f;
            }
            else
            {
                textFadeOpacity = 1f - ((textFadeTimer - 3f) / 3f);
            }
            _spriteBatch.DrawString(_font, "Game Saved", new Vector2(vw * 0.9f, (vh - mapHeight) / 4), Color.Yellow * textFadeOpacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            textFadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        else if (isSaved && textFadeTimer >= 7f)
        {
            isSaved = false;
            textFadeTimer = 0f;
            textFadeOpacity = 1f;
        }

        if (debugFlag)
        {
            for (int y = 0; y < _tilemap.MapHeight; y++)
            {
                for (int x = 0; x < _tilemap.MapWidth; x++)
                {
                    Vector2 center = _tilemap.GetCenter(y * _tilemap.MapWidth + x);
                    string tileIndex = (_tilemap.TileIndices[y * _tilemap.MapWidth + x]).ToString();
                    string coordText = $"({x},{y})";
                    string indexText = $"[{y * _tilemap.MapWidth + x}]";
                    _spriteBatch.DrawString(_font, tileIndex, center + new Vector2(-5, -16), Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.DrawString(_font, coordText, center + new Vector2(-5, 0), Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.DrawString(_font, indexText, center + new Vector2(-5, 16), Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
            int[] indices = _tilemap.GetSurrounding(_tilemap.GetHighlightedTile());
            string indicesText = string.Join(", ", indices);
            _spriteBatch.DrawString(_font, "Surrounding Tiles Indices: " + indicesText, new Vector2(vw * 0.05f, vh - (vh - mapHeight) / 4), Color.Yellow * textFadeOpacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            string scoresText = string.Join(", ", scoreTracker.GetScores());
            _spriteBatch.DrawString(_font, "ScoresFGDB: " + scoresText, new Vector2(vw * 0.75f, vh - (vh - mapHeight) / 4), Color.Yellow * textFadeOpacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        if (tutorialFlag)
        {
            string titleText = "Build on at least 1 of each terrain and connect with roads";
            Vector2 titlePos = new Vector2(vw * 0.01f, vh - (vh - mapHeight) / 4 );
            _spriteBatch.DrawString(_font, titleText, titlePos, Color.Yellow, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
        }
        if(gameOver)
        {
            string gameOverText = "Victory! Minigame Score: " + scoreTracker.GetTotalScore() + " Road Score: " + roadScore/4;
            Vector2 gameOverPos = new Vector2(vw * 0.01f, vh - (vh - mapHeight) / 4);
            _spriteBatch.DrawString(_font, gameOverText, gameOverPos, Color.Yellow, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
        }

        redWorker.Draw(gameTime, _spriteBatch);

        _spriteBatch.End();
    }

    private bool Build(LOAF l)
    {
        if (debugFlag)
        {
            if (_tilemap.HasBuilding(_tilemap.GetPlayerIndex()) is true)
            {
                LOAF.DeniedSound.Play();
                return false;
            }
            else if (_tilemap.GetTileTerrain(_tilemap.GetPlayerIndex()) == Enums.TileType.Forest)
            {
                scoreTracker.ForestPoints += 1;
                Reinitialize();
            }
            else if (_tilemap.GetTileTerrain(_tilemap.GetPlayerIndex()) == Enums.TileType.Desert)
            {
                scoreTracker.DesertPoints += 1;
                Reinitialize();
            }
            else if (_tilemap.GetTileTerrain(_tilemap.GetPlayerIndex()) == Enums.TileType.Badland)
            {
                scoreTracker.BadlandPoints += 1;
                Reinitialize();
            }
            else if (_tilemap.GetTileTerrain(_tilemap.GetPlayerIndex()) == Enums.TileType.Grassland)
            {
                scoreTracker.GrasslandPoints += 1;
                Reinitialize();
            }
            else
            {
                LOAF.DeniedSound.Play();
                return false;
            }
            return true;
        }
        if (_tilemap.HasBuilding(_tilemap.GetPlayerIndex()) is true)
        {
            LOAF.DeniedSound.Play();
            return false;
        }
        else if (_tilemap.GetTileTerrain(_tilemap.GetPlayerIndex()) == Enums.TileType.Forest)
        {
            LOAF.ChangeScene(new TutorialScene(l, Enums.GameType.Carpentry, scoreTracker));
        }
        else if (_tilemap.GetTileTerrain(_tilemap.GetPlayerIndex()) == Enums.TileType.Desert)
        {
            LOAF.ChangeScene(new TutorialScene(l, Enums.GameType.Cactus, scoreTracker));
        }
        else if (_tilemap.GetTileTerrain(_tilemap.GetPlayerIndex()) == Enums.TileType.Badland)
        {
            LOAF.ChangeScene(new TutorialScene(l, Enums.GameType.Mining, scoreTracker));
        }
        else if (_tilemap.GetTileTerrain(_tilemap.GetPlayerIndex()) == Enums.TileType.Grassland)
        {
            LOAF.ChangeScene(new TutorialScene(l, Enums.GameType.Wheat, scoreTracker));
        }
        else
        {
            LOAF.DeniedSound.Play();
            return false;
        }
        return true;
    }
}