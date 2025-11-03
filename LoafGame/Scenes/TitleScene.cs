using LoafGame.Collisions;
using LoafGame.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Reflection.Metadata;

namespace LoafGame.Scenes;

public class TitleScene : Scene
{
    private SpriteBatch _spriteBatch;

    private RedWorker redWorker;
    private Button startButton;
    private Button loadButton;
    private Button creditsButton;
    private BoundingPoint cursor;

    private SpriteFont friedolin;
    private SpriteFont exitText;

    /// <summary>
    /// Initializes a new instance of the <see cref="TitleScene"/> class.
    /// </summary>
    /// <param name="game">The game instance that this scene is associated with.</param>
    public TitleScene(Game game) : base(game) { }

    public override void Initialize()
    {
        float vw = Game.GraphicsDevice.Viewport.Width;
        float vh = Game.GraphicsDevice.Viewport.Height;

        float leftMargin = vw * 0.02f;
        float centerX = vw / 2;

        redWorker = new RedWorker() { Position = new Vector2(leftMargin + vw * 0.0172f, vh * 0.45f),
            Direction = Direction.Right,
            Scale = 3f};

        float buttonRowY = vh * 0.75f;
        float buttonSpacing = vw * 0.15f;

        startButton = new Button() { Position = new Vector2(centerX - buttonSpacing, buttonRowY), Text = "Start" };
        loadButton = new Button() { Position = new Vector2(centerX, buttonRowY), Text = "Load" };
        creditsButton = new Button() { Position = new Vector2(centerX + buttonSpacing, buttonRowY), Text = "Credits" };

        MediaPlayer.Play(LOAF.backgroundMusicTitle);
        MediaPlayer.IsRepeating = true;
        base.Initialize();
    }

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        friedolin = Content.Load<SpriteFont>("friedolin");
        exitText = Content.Load<SpriteFont>("hamburger");
        redWorker.LoadContent(Content);
        startButton.LoadContent(Content);
        loadButton.LoadContent(Content);
        creditsButton.LoadContent(Content);
    }

    public override void Update(GameTime gameTime)
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        redWorker.Update(gameTime);
        cursor = new BoundingPoint(LOAF.InputManager.Position);
        startButton.Update(cursor.CollidesWith(startButton.Bounds));
        loadButton.Update(cursor.CollidesWith(loadButton.Bounds));
        creditsButton.Update(cursor.CollidesWith(creditsButton.Bounds));

        if (LOAF.InputManager.LeftMouseClicked)
        {
            if (startButton.Hover)
            {
                startButton.PlayClickSound();
                LOAF.ChangeScene(new MinigameSelectorScene(LOAF));
            }
            if (loadButton.Hover)
            {
                loadButton.PlayClickSound();
            }
            if (creditsButton.Hover)
            {
                creditsButton.PlayClickSound();
                LOAF.ChangeScene(new CreditsScene(LOAF));
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.DarkSlateGray);

        _spriteBatch.Begin(transformMatrix: Matrix.CreateScale(1), samplerState: SamplerState.PointClamp);

        float vw = Game.GraphicsDevice.Viewport.Width;
        float vh = Game.GraphicsDevice.Viewport.Height;
        float leftMargin = vw * 0.02f;
        float centerX = vw / 2;

        redWorker.Draw(gameTime, _spriteBatch);

        startButton.Draw(_spriteBatch);
        loadButton.Draw(_spriteBatch);
        creditsButton.Draw(_spriteBatch);

        string title = "Life of a Foreman";
        Vector2 titleSize = friedolin.MeasureString(title);
        Vector2 titlePos = new Vector2(centerX - titleSize.X / 2f, vh * 0.12f);
        _spriteBatch.DrawString(friedolin, title, titlePos, Color.Goldenrod);

        _spriteBatch.DrawString(exitText, "Press ESC to Exit", new Vector2(leftMargin, vh * 0.0185f), Color.Beige);

        _spriteBatch.End();
    }
}