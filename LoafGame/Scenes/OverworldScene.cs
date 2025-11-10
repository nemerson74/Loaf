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

    private float vw;
    private float vh;
    private float leftMargin;
    private float centerX;

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
        MediaPlayer.Stop();
        MediaPlayer.Play(LOAF.backgroundMusicOverworld);
        MediaPlayer.IsRepeating = true;
        base.Initialize();
    }

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        _tilemap = Content.Load<HexTilemap>("tilemapkey");
        redWorker.LoadContent(Content);
        _font = Content.Load<SpriteFont>("vergilia");
    }

    public override void Update(GameTime gameTime)
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        //cursor = new BoundingPoint(LOAF.InputManager.Position / LOAF.GameScale);
        _tilemap.Update(LOAF.InputManager.Position / LOAF.GameScale, 5f / LOAF.GameScale, 50f / LOAF.GameScale);

        //save on F5 press
        if (LOAF.InputManager.KeyClicked(Keys.F5))
        {
            SaveGame.SaveOverworld(redWorker.Position);
            isSaved = true;
        }

        if (LOAF.InputManager.LeftMouseClicked)
        {
            redWorker.Position = _tilemap.GetHighlightedCenterVector();
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

        _spriteBatch.DrawString(_font, "ESC to return, Left mouse to move, F5 to Save", new Vector2(30, 0), Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        _tilemap.Draw(gameTime, _spriteBatch, 5f / LOAF.GameScale, 50f / LOAF.GameScale);
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
            _spriteBatch.DrawString(_font, "Game Saved", new Vector2(400, 0), Color.Yellow * textFadeOpacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            textFadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        else if (isSaved && textFadeTimer >= 7f)
        {
            isSaved = false;
            textFadeTimer = 0f;
            textFadeOpacity = 1f;
        }
        redWorker.Draw(gameTime, _spriteBatch);

        _spriteBatch.End();
    }
}