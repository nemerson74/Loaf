using LoafGame.Collisions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LoafGame
{
    public class ScoreTimer
    {
        private Enums.GameType gameType;
        private static readonly int SCORE_MAX = 3;
        private static readonly int SCORE_MIN = 1;

        private float elapsedTime = 0f;
        private float _gameScale = 1f;
        private bool _running = false;
        private int? _lastScore = null;

        private SpriteFont font;

        public ScoreTimer(Enums.GameType gameType)
        {
            this.gameType = gameType;
        }

        private void Initialize(float gameScale)
        {
            _gameScale = gameScale;
            elapsedTime = 0f;
            _running = true;
            _lastScore = null;
        }

        public void LoadContent(ContentManager content, float gameScale)
        {
            Initialize(gameScale);
            font = content.Load<SpriteFont>("vergilia");
        }

        public void Start() => _running = true;
        public void Stop() => _running = false;
        public void Reset()
        {
            elapsedTime = 0f;
            _running = false;
            _lastScore = null;
        }

        public void Update(GameTime gameTime)
        {
            if (_running)
            {
                elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (font == null) return;
            var vp = spriteBatch.GraphicsDevice.Viewport;
            float vw = vp.Width / _gameScale;
            float vh = vp.Height / _gameScale;
            float centerX = vw / 2f;

            string timeString = "Time: " + elapsedTime.ToString("F2");
            Vector2 lineSize = font.MeasureString(timeString);
            Vector2 linePos = new Vector2(centerX - lineSize.X / 2f, vh / 3f);
            spriteBatch.DrawString(font, timeString, linePos, Color.Yellow, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);

            if (_lastScore.HasValue)
            {
                string scoreText = "Score: " + _lastScore.Value + "/3";
                Vector2 scoreSize = font.MeasureString(scoreText);
                Vector2 scorePos = new Vector2(centerX - scoreSize.X / 2f, vh / 3f + scoreSize.Y * 1.5f);
                spriteBatch.DrawString(font, scoreText, scorePos, Color.Yellow, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
            }
        }

        public int ReturnScore()
        {
            // stop timer when scoring
            _running = false;

            // Faster completion yields higher score. If above max limit, score is 0.
            float[] limits = gameType switch
            {
                Enums.GameType.Carpentry => Enums.CARPENTRY_LIMITS,
                Enums.GameType.Mining => Enums.MINING_LIMITS,
                Enums.GameType.Cactus => Enums.CACTUS_LIMITS,
                Enums.GameType.Wheat => Enums.WHEAT_LIMITS,
                _ => Enums.CARPENTRY_LIMITS
            };

            int score;
            if (elapsedTime <= limits[0]) score = SCORE_MAX; // best time
            else if (elapsedTime <= limits[1]) score = SCORE_MAX - 1;
            else if (elapsedTime <= limits[2]) score = SCORE_MIN; // minimum passing score
            else score = 0; // failed to meet time limit

            _lastScore = score; // record for drawing after this call
            return score;
        }

        public float ElapsedSeconds => elapsedTime;
    }
}
