using LoafGame.Collisions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoafGame
{
    internal class Button
    {
        #region Private
        private Texture2D texture;
        private float scale = 0.5f;
        private bool hover;
        private string text = "Unset";
        private BoundingRectangle bounds = new BoundingRectangle(new Vector2(0, 0), 150, 90);
        private SpriteFont font;
        private bool wasHover = false;
        #endregion

        #region Properties
        /// <summary>
        /// Where is the button? (interpreted as the center point)
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// True if cursor is over the button
        /// </summary>
        public bool Hover
        {
            set { hover = value; }
            get { return hover; }
        }

        /// <summary>
        /// Text for the button
        /// </summary>
        public string Text {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// The bounds of the button
        /// </summary>
        public BoundingRectangle Bounds
        {
            get { return bounds; }
        }
        #endregion

        /// <summary>
        /// Loads the button sprite texture
        /// </summary>
        /// <param name="content">the content manager to load with</param>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Button");
            font = content.Load<SpriteFont>("hamburger");
            bounds.Width = texture.Width * scale;
            bounds.Height = texture.Height * scale;
        }

        /// <summary>
        /// Update the button based on mouse location
        /// </summary>
        /// <param name="collision">is the mouse hovering?</param>
        public void Update(bool collision)
        {
            // Position is the center; bounds store top-left for collision
            bounds.X = Position.X - bounds.Width / 2f;
            bounds.Y = Position.Y - bounds.Height / 2f;
            wasHover = hover;
            hover = collision;
            if (!wasHover && hover && LoafGame.LOAF.ButtonHoverSound != null)
            {
                LoafGame.LOAF.ButtonHoverSound.Play();
            }
        }

        /// <summary>
        /// Draws the button
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw with</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            if (hover)
            {
                spriteBatch.Draw(texture, Position, null, Color.Green, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(texture, Position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            //draw text centered on the button
            if (font != null && !string.IsNullOrEmpty(text))
            {
                Vector2 textSize = font.MeasureString(text);
                Vector2 textPosition = Position - textSize / 2f;
                spriteBatch.DrawString(font, text, textPosition, Color.Black);
            }
        }

        /// <summary>
        /// Plays the click sound effect.
        /// </summary>
        public void PlayClickSound()
        {
            if (LoafGame.LOAF.ButtonClickSound != null)
            {
                LoafGame.LOAF.ButtonClickSound.Play();
            }
        }
    }
}
