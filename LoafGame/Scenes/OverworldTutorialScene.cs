using LoafGame.Collisions;
using LoafGame.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using static LoafGame.Enums;

namespace LoafGame.Scenes
{
    public class OverworldTutorialScene : Scene
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;

        private float vw;
        private float vh;
        private float leftMargin;
        private float centerX;

        private bool barFilled = false;

        private const float barFillTime = 2.0f; // seconds
        private float barFillProgress = 0.0f;

        string[] tutorialText = new string[]
        {
            "Welcome to LOAF!",
            "Move around the map by clicking on adjacent tiles",
            "Build a building on one of each of the four terrains",
            "Build roads in and between all buildings to win"
        };
        SpriteFont continueFont;
        SpriteFont tutorialFont;

        public enum TutorialType
        {
            Carpentry,
            Mining,
            Cactus,
            Wheat
        }

        public OverworldTutorialScene(Game game) : base(game) { }

        public override void Initialize()
        {
            var LOAF = Game as LOAF;
            vw = Game.GraphicsDevice.Viewport.Width / LOAF.GameScale;
            vh = Game.GraphicsDevice.Viewport.Height / LOAF.GameScale;
            leftMargin = vw * 0.02f;
            centerX = vw / 2;

            float buttonRowY = vh * 0.5f;
            float buttonSpacing = vw * 0.15f;

            base.Initialize();
        }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            continueFont = Content.Load<SpriteFont>("vergilia");
            tutorialFont = Content.Load<SpriteFont>("tutorialFont");
            _pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public override void Update(GameTime gameTime)
        {
            var LOAF = Game as LOAF;
            if (LOAF == null) return;
            var input = LOAF.InputManager;

            if (input.LeftMouseDown)
            {
                barFillProgress += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (barFillProgress >= barFillTime)
                {
                    barFilled = true;
                }
            }
            else
            {
                barFillProgress = 0.0f;
                barFilled = false;
            }

            if (barFilled)
            {
                LOAF.ChangeScene(new OverworldScene(LOAF));
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var LOAF = Game as LOAF;

            Game.GraphicsDevice.Clear(Color.DarkSlateGray);
            _spriteBatch.Begin(transformMatrix: Matrix.CreateScale(LOAF.GameScale));
            float fontScale = 1f;

            _spriteBatch.DrawString(continueFont, "Hold Left Mouse Button to confirm.", new Vector2(leftMargin, vh * 0.0185f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            for (int i = 0; i < tutorialText.Length; i++)
            {
                string line = tutorialText[i];
                Vector2 lineSize = tutorialFont.MeasureString(line);
                Vector2 linePos = new Vector2(centerX - lineSize.X / 2f, vh * (0.2f + i * 0.1f));
                _spriteBatch.DrawString(tutorialFont, line, linePos, Color.White, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
            }

            // progress bar
            float progress = MathHelper.Clamp(barFillProgress / barFillTime, 0f, 1f);
            int barWidth = (int)(vw * 0.6f);
            int barHeight = (int)(vh * 0.04f);
            int barX = (int)(centerX - barWidth / 2f);
            int barY = (int)(vh * 0.78f);
            var bgRect = new Rectangle(barX, barY, barWidth, barHeight);
            var fillRect = new Rectangle(barX, barY, (int)(barWidth * progress), barHeight);
            _spriteBatch.Draw(_pixel, bgRect, Color.Black * 0.4f);
            _spriteBatch.Draw(_pixel, fillRect, Color.Yellow);

            _spriteBatch.End();
        }
    }
}