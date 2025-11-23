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
    private SpriteBatch _spriteBatch;
    private RedWorker redWorker;
    private HexTilemap _tilemap;
    private BoundingPoint cursor;
    private SpriteFont _font;
    private bool isSaved = false;
    private float textFadeTimer = 0f;
    private float textFadeOpacity = 1f;
    private bool debugFlag = false;
    private bool tutorialFlag = true;
    private int prevHighlightedTile = -1;
    private int playerTile = -1;

    private float vw;
    private float vh;
    private float leftMargin;
    private float centerX;
    private float centerY;
    private float mapWidth;
    private float mapHeight;
    private float horizontalOffset = 0f;
    private float verticalOffset = 0f;

    private Vector2 startingPosition = Vector2.Zero;

    /// <summary>
    /// Initializes a new instance of the <see cref="OverworldScene"/> class.
    /// </summary>
    /// <param name="game">The game instance that this scene is associated with.</param>
    public OverworldScene(Game game) : base(game) { }

    public OverworldScene(Game game, SaveData save) : base(game)
    {
        if (save != null)
        {
            startingPosition = new Vector2(save.X, save.Y);
        }
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
        MediaPlayer.Play(LOAF.backgroundMusicOverworld);
        MediaPlayer.IsRepeating = true;
        base.Initialize();
    }

    public override void Reinitialize()
    {
        MediaPlayer.Stop();
        MediaPlayer.Play(LOAF.backgroundMusicOverworld);
        MediaPlayer.IsRepeating = true;
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
        _tilemap.InitializeHexTiles(horizontalOffset, verticalOffset, 0);
        redWorker.LoadContent(Content);
        redWorker.Position = _tilemap.GetCenter(0) + new Vector2(-16, -16);
        _font = Content.Load<SpriteFont>("vergilia");
    }

    public override void Update(GameTime gameTime)
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        debugFlag = LOAF.InputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space);
        //cursor = new BoundingPoint(LOAF.InputManager.Position / LOAF.GameScale);
        _tilemap.Update(LOAF.InputManager.Position / LOAF.GameScale, 0, (vh - mapHeight) / 2);
        playerTile = _tilemap.GetPlayerIndex();

        if (prevHighlightedTile != _tilemap.GetHighlightedTile())
        {
            //maybe play moving sound here
        }

        //save on F5 press
        if (LOAF.InputManager.KeyClicked(Keys.F5))
        {
            SaveGame.SaveOverworld(redWorker.Position);
            isSaved = true;
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

        if (LOAF.InputManager.RightMouseClicked)
        {
            if (_tilemap.GetTileTerrain(_tilemap.GetPlayerIndex()) == Enums.TileType.Forest)
            {
                LOAF.ChangeScene(new CarpentryScene(LOAF));
            }
            else
            {
                LOAF.DeniedSound.Play();
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
        _spriteBatch.Begin(transformMatrix: Matrix.CreateScale(LOAF.GameScale), blendState: BlendState.AlphaBlend);
        
        _spriteBatch.DrawString(_font, "ESC to return, Left mouse to move to adjacent tiles, Right mouse to build, F5 to Save, Spacebar for debug", new Vector2(vw * 0.05f, (vh - mapHeight) / 4), Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
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
            _spriteBatch.DrawString(_font, "Game Saved", new Vector2(vw * 0.8f, (vh - mapHeight) / 4), Color.Yellow * textFadeOpacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
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
        }

        if (tutorialFlag)
        {
            string titleText = "Go to a forest and build a lumbermill";
            Vector2 titleSize = _font.MeasureString(titleText);
            Vector2 titlePos = new Vector2(centerX - titleSize.X / 4f, vh * 0.5f);
            _spriteBatch.DrawString(_font, titleText, titlePos, Color.Yellow, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
        }

        redWorker.Draw(gameTime, _spriteBatch);

        _spriteBatch.End();
    }
}