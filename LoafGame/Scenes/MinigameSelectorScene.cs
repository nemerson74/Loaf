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
        newGameButton = new Button() { Position = new Vector2(Game.GraphicsDevice.Viewport.Width / 2 - 250, Game.GraphicsDevice.Viewport.Height / 2 - 180), Text = "New Game" };
        carpentryButton = new Button() { Position = new Vector2(Game.GraphicsDevice.Viewport.Width / 2 - 300, Game.GraphicsDevice.Viewport.Height / 2 - 90), Text = "Carpentry" };
        masonryButton = new Button() { Position = new Vector2(Game.GraphicsDevice.Viewport.Width / 2 - 200, Game.GraphicsDevice.Viewport.Height / 2 - 90), Text = "Masonry WIP" };
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
        Game.GraphicsDevice.Clear(Color.DarkSlateGray);
        _spriteBatch.Begin(transformMatrix: Matrix.CreateScale(1));
        newGameButton.Draw(_spriteBatch);
        carpentryButton.Draw(_spriteBatch);
        masonryButton.Draw(_spriteBatch);
        SpriteFont font = Content.Load<SpriteFont>("hamburger");
        float fontScale = 0.75f;

        _spriteBatch.DrawString(font, "Right click to return", new Vector2(20, 0), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

        _spriteBatch.End();
    }
}

