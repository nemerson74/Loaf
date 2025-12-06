using LoafGame.Collisions;
using LoafGame.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace LoafGame.Scenes;

public class MinigameSelectorScene : Scene
{
    private SpriteBatch _spriteBatch;
    private Button newGameButton;
    private Button carpentryButton;
    private Button miningButton;
    private BoundingPoint cursor;

    private float vw;
    private float vh;
    private float leftMargin;
    private float centerX;

    public MinigameSelectorScene(Game game) : base(game) { }

    public override void Initialize()
    {
        var LOAF = Game as LOAF;
        vw = Game.GraphicsDevice.Viewport.Width / LOAF.GameScale;
        vh = Game.GraphicsDevice.Viewport.Height / LOAF.GameScale;
        leftMargin = vw * 0.02f;
        centerX = vw / 2;

        float buttonRowY = vh * 0.5f;
        float buttonSpacing = vw * 0.15f;

        newGameButton = new Button() { Position = new Vector2(centerX, buttonRowY - buttonSpacing), Text = "New" };
        carpentryButton = new Button() { Position = new Vector2(centerX - buttonSpacing, buttonRowY + buttonSpacing), Text = "Carpentry" };
        miningButton = new Button() { Position = new Vector2(centerX + buttonSpacing, buttonRowY + buttonSpacing), Text = "Mining" };
        base.Initialize();
    }

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        newGameButton.LoadContent(Content);
        carpentryButton.LoadContent(Content);
        miningButton.LoadContent(Content);
    }

    public override void Update(GameTime gameTime)
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        var input = LOAF.InputManager;

        cursor = new BoundingPoint(input.Position / LOAF.GameScale);
        newGameButton.Update(cursor.CollidesWith(newGameButton.Bounds));
        carpentryButton.Update(cursor.CollidesWith(carpentryButton.Bounds));
        miningButton.Update(cursor.CollidesWith(miningButton.Bounds));

        // Right click to return
        if (input.PreviousRightMouseState == ButtonState.Released && input.CurrentRightMouseState == ButtonState.Pressed)
        {
            LOAF.ChangeScene(new TitleScene(LOAF));
            return;
        }

        if (input.KeyClicked(Keys.Escape))
        {
            LOAF.ChangeScene(new TitleScene(LOAF));
            return;
        }

        // Left click to select
        if (input.LeftMouseClicked)
        {
            if (newGameButton.Hover)
            {
                newGameButton.PlayClickSound();
                LOAF.ChangeScene(new OverworldScene(LOAF));
            }
            if (carpentryButton.Hover)
            {
                carpentryButton.PlayClickSound();
                LOAF.ChangeScene(new CarpentryScene(LOAF));
            }
            if (miningButton.Hover)
            {
                miningButton.PlayClickSound();
                LOAF.ChangeScene(new MiningScene(LOAF));
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var LOAF = Game as LOAF;

        Game.GraphicsDevice.Clear(Color.DarkSlateGray);
        _spriteBatch.Begin(transformMatrix: Matrix.CreateScale(LOAF.GameScale));
        newGameButton.Draw(_spriteBatch);
        carpentryButton.Draw(_spriteBatch);
        miningButton.Draw(_spriteBatch);
        SpriteFont font = Content.Load<SpriteFont>("vergilia");
        float fontScale = 1f;

        _spriteBatch.DrawString(font, "Right click to return", new Vector2(leftMargin, vh * 0.0185f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

        string mainText = "Main Game (WIP)";
        Vector2 titleSize = font.MeasureString(mainText);
        Vector2 titlePos = new Vector2(centerX - titleSize.X / 2f, vh * 0.1f);
        _spriteBatch.DrawString(font, mainText, titlePos, Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

        string minigameText = "Minigame Selector";
        Vector2 minigameTextSize = font.MeasureString(minigameText);
        Vector2 minigameTextPos = new Vector2(centerX - minigameTextSize.X / 2f, vh * 0.6f);
        _spriteBatch.DrawString(font, minigameText, minigameTextPos, Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

        _spriteBatch.End();
    }
}

