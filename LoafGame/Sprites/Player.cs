using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LoafGame
{
    internal class Player
    {
        #region Private
        private Texture2D texture;
        private double animationTimer;
        private short animationFrame = 1;
        #endregion
        #region Properties
        /// <summary>
        /// Facing direction of the worker
        /// </summary>
        public Direction Direction;

        /// <summary>
        /// Where is the worker?
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The scale of the worker sprite
        /// </summary>
        public float Scale = 1f;
        #endregion

        /// <summary>
        /// Loads the worker sprite texture
        /// </summary>
        /// <param name="content">the content manager to load with</param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("MiniWorldSprites\\MiniWorldSprites\\Characters\\Workers\\RedWorker\\FarmerRed");
        }

        /// <summary>
        /// Update the worker sprite to walk in a pattern
        /// </summary>
        /// <param name="gameTime">the game time</param>
        public void Update(GameTime gameTime)
        {
            //Move worker - go forward in current direction
            switch (Direction)
            {
                case Direction.Down:
                    Position += new Vector2(0, 1) * 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
                case Direction.Right:
                    Position += new Vector2(1, 0) * 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
                case Direction.Up:
                    Position += new Vector2(0, -1) * 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
                case Direction.Left:
                    Position += new Vector2(-1, 0) * 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
            }
        }

        /// <summary>
        /// Draws the animted sprite
        /// </summary>
        /// <param name="gameTime">the game time</param>
        /// <param name="spriteBatch">SpriteBatch to draw with</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //update animation timer
            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;
            //update animation frame every 0.3 seconds
            if (animationTimer > 0.3)
            {
                animationFrame++;
                if (animationFrame > 4) animationFrame = 1;
                animationTimer -= 0.3;
            }
            // draw the sprite
            var source = new Rectangle(animationFrame * 16, (int)Direction * 16, 16, 16);
            //spriteBatch.Draw(texture, Position, source, Color.White);
            spriteBatch.Draw(
                texture,
                Position,
                source,
                Color.White,
                0f,
                Vector2.Zero,
                Scale,
                SpriteEffects.None,
                0f
            );
        }
    }
}
