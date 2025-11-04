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
    private Button masonryButton;
    private BoundingPoint cursor;
    public MinigameSelectorScene(Game game) : base(game) { }

    public override void Initialize()
    {
        float vw = Game.GraphicsDevice.Viewport.Width;
        float vh = Game.GraphicsDevice.Viewport.Height;

        float leftMargin = vw * 0.02f;
        float centerX = vw / 2;

        float buttonRowY = vh * 0.5f;
        float buttonSpacing = vw * 0.15f;

        newGameButton = new Button() { Position = new Vector2(centerX, buttonRowY - buttonSpacing), Text = "New" };
        carpentryButton = new Button() { Position = new Vector2(centerX - buttonSpacing, buttonRowY + buttonSpacing), Text = "Carpentry" };
        masonryButton = new Button() { Position = new Vector2(centerX + buttonSpacing, buttonRowY + buttonSpacing), Text = "wip" };
        base.Initialize();
    }

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        newGameButton.LoadContent(Content);
        carpentryButton.LoadContent(Content);
        masonryButton.LoadContent(Content);
    }

    public override void Update(GameTime gameTime)
    {
        var loaf = Game as LOAF;
        if (loaf == null) return;
        var input = loaf.InputManager;

        cursor = new BoundingPoint(input.Position);
        newGameButton.Update(cursor.CollidesWith(newGameButton.Bounds));
        carpentryButton.Update(cursor.CollidesWith(carpentryButton.Bounds));
        masonryButton.Update(cursor.CollidesWith(masonryButton.Bounds));

        // Right click to return
        if (input.PreviousRightMouseState == ButtonState.Released && input.CurrentRightMouseState == ButtonState.Pressed)
        {
            LOAF.ChangeScene(new TitleScene(loaf));
            return;
        }

        if (input.KeyClicked(Keys.Escape))
        {
            LOAF.ChangeScene(new TitleScene(loaf));
            return;
        }

        // Left click to select
        if (input.LeftMouseClicked)
        {
            if (newGameButton.Hover)
            {
                newGameButton.PlayClickSound();
                LOAF.ChangeScene(new OverworldScene(loaf));
            }
            if (carpentryButton.Hover)
            {
                carpentryButton.PlayClickSound();
                LOAF.ChangeScene(new CarpentryScene(loaf));
            }
            if (masonryButton.Hover)
            {
                masonryButton.PlayClickSound();
                //LOAF.ChangeScene(new LoadScene(LOAF));
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        float vw = Game.GraphicsDevice.Viewport.Width;
        float vh = Game.GraphicsDevice.Viewport.Height;
        float leftMargin = vw * 0.02f;
        float centerX = vw / 2;

        Game.GraphicsDevice.Clear(Color.DarkSlateGray);
        _spriteBatch.Begin(transformMatrix: Matrix.CreateScale(1));
        newGameButton.Draw(_spriteBatch);
        carpentryButton.Draw(_spriteBatch);
        masonryButton.Draw(_spriteBatch);
        SpriteFont font = Content.Load<SpriteFont>("vergilia");
        float fontScale = 1f;

        _spriteBatch.DrawString(font, "Right click to return", new Vector2(20, 0), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

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

