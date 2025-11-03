using LoafGame.Collisions;
using LoafGame.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace LoafGame.Scenes;

public class CreditsScene : Scene
{
    private SpriteBatch _spriteBatch;
    private string creditsText;
    private float scrollOffset = 0f;
    private float scrollSpeed = 30f;

    public CreditsScene(Game game) : base(game) { }

    public override void Initialize()
    {

        base.Initialize();
    }

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        string readmePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "README.md");
        if (File.Exists(readmePath))
            creditsText = File.ReadAllText(readmePath);
        else
            creditsText = "Credits file not found.";
    }

    public override void Update(GameTime gameTime)
    {
        var loaf = Game as LOAF;
        if (loaf == null) return;
        var input = loaf.InputManager;

        // Right click to return
        if (input.PreviousRightMouseState == ButtonState.Released && input.CurrentRightMouseState == ButtonState.Pressed)
        {
            LOAF.ChangeScene(new TitleScene(loaf));
            return;
        }

        // Keyboard scroll
        if (input.IsKeyDown(Keys.Down))
            scrollOffset -= scrollSpeed;
        if (input.IsKeyDown(Keys.Up))
            scrollOffset += scrollSpeed;

        scrollOffset += input.ScrollWheelDelta / 4f;

        // Clamp top
        if (scrollOffset > 0) scrollOffset = 0;

        // Clamp bottom
        if (!string.IsNullOrEmpty(creditsText))
        {
            var font = Content.Load<SpriteFont>("hamburger");
            float fontScale = 0.85f; //needed for scrolling scaling
            float lineHeight = font.LineSpacing * fontScale;
            string[] lines = creditsText.Split('\n');
            float totalHeight = lines.Length * lineHeight;
            float minOffset = Math.Min(0, Game.GraphicsDevice.Viewport.Height / 2f - totalHeight); // /2 for scale
            if (scrollOffset < minOffset) scrollOffset = minOffset;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(transformMatrix: Matrix.CreateScale(1));

        SpriteFont font = Content.Load<SpriteFont>("hamburger");
        float fontScale = 0.75f;
        Vector2 position = new Vector2(20, 20 + scrollOffset);
        float lineHeight = font.LineSpacing * fontScale;

        if (!string.IsNullOrEmpty(creditsText))
        {
            string[] lines = creditsText.Split('\n');
            foreach (var line in lines)
            {
                _spriteBatch.DrawString(font, line, position, Color.White, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
                position.Y += lineHeight;
            }
        }
        _spriteBatch.DrawString(font, "Right click to return", new Vector2(20, 0), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

        _spriteBatch.End();
    }
}

