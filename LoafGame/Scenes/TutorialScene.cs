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
    public class TutorialScene : Scene
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

        string[] tutorialText;

        string limitsString;

        SpriteFont continueFont;
        SpriteFont tutorialFont;

        public enum TutorialType
        {
            Carpentry,
            Mining,
            Cactus,
            Wheat
        }

        private Enums.GameType CurrentTutorialType;
        ScoreTracker scoreTracker;

        public TutorialScene(Game game, Enums.GameType tutorialType, ScoreTracker scoreTracker = null) : base(game)
        {
            CurrentTutorialType = tutorialType;
            this.scoreTracker = scoreTracker;
            tutorialText = TutorialText.GetText(CurrentTutorialType);
        }

        public override void Initialize()
        {
            var LOAF = Game as LOAF;
            vw = Game.GraphicsDevice.Viewport.Width / LOAF.GameScale;
            vh = Game.GraphicsDevice.Viewport.Height / LOAF.GameScale;
            leftMargin = vw * 0.02f;
            centerX = vw / 2;

            float[] limits = CurrentTutorialType switch
            {
                Enums.GameType.Carpentry => Enums.CARPENTRY_LIMITS,
                Enums.GameType.Mining => Enums.MINING_LIMITS,
                Enums.GameType.Cactus => Enums.CACTUS_LIMITS,
                Enums.GameType.Wheat => Enums.WHEAT_LIMITS,
                _ => Enums.CARPENTRY_LIMITS
            };

            limitsString = $"3={limits[0]}s, 2={limits[1]}s, 1={limits[2]}s";

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
                if (CurrentTutorialType == Enums.GameType.Carpentry)
                {
                    LOAF.ChangeScene(new CarpentryScene(LOAF, scoreTracker));
                }
                if (CurrentTutorialType == Enums.GameType.Mining)
                {
                    LOAF.ChangeScene(new MiningScene(LOAF, scoreTracker));
                }
                if (CurrentTutorialType == Enums.GameType.Cactus)
                {
                    //LOAF.ChangeScene(new CactusScene(LOAF, scoreTracker));
                }
                if (CurrentTutorialType == Enums.GameType.Wheat)
                {
                    LOAF.ChangeScene(new WheatScene(LOAF, scoreTracker));
                }
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

            string limitsLine = "Times to aim for: " + limitsString;
            Vector2 limitsLineSize = tutorialFont.MeasureString(limitsLine);
            Vector2 limitsLinePos = new Vector2(centerX - limitsLineSize.X / 2f, vh * (0.2f + tutorialText.Length * 0.1f));
            _spriteBatch.DrawString(tutorialFont, limitsLine, limitsLinePos, Color.White, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

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