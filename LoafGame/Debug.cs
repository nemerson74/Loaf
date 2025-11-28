using LoafGame.Collisions;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoafGame
{
    public static class Debug
    {

        /// <summary>
        /// Draws a circle in the specified area to see a non-visible circle.
        /// </summary>
        /// <param name="_spriteBatch">The sprite batch to draw with</param>
        /// <param name="device">The Graphics Device of the game</param>
        /// <param name="center">The center of the circle to draw</param>
        /// <param name="radius">The radius of the circle to draw</param>
        /// <param name="color">The color of the outline</param>
        public static void DrawCircleOutline(SpriteBatch _spriteBatch, GraphicsDevice device, Vector2 center, float radius, Color color)
        {
            // draw 36 points
            int steps = 36;
            Texture2D pixel = new Texture2D(device, 1, 1);
            pixel.SetData(new[] { Color.White });
            for (int i = 0; i < steps; i++)
            {
                float a = i * (MathF.PI * 2f / steps);
                Vector2 p = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius;
                _spriteBatch.Draw(pixel, p, null, color, 0f, new Vector2(0.5f, 0.5f), 3f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a rectangle in the specified area to see a non-visible rectangle.
        /// </summary>
        /// <param name="_spriteBatch">The sprite batch to draw with</param>
        /// <param name="device">The Graphics Device of the game</param>
        /// <param name="rect">The rectangle to draw</param>
        /// <param name="color">The color of the rectangle</param>
        public static void DrawRectangleOutline(SpriteBatch _spriteBatch, GraphicsDevice device, BoundingRectangle rect, Color color)
        {
            // thickness
            int t = 2;
            Texture2D pixel = new Texture2D(device, 1, 1);
            pixel.SetData(new[] { Color.White });
            _spriteBatch.Draw(pixel, new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, t), color);
            _spriteBatch.Draw(pixel, new Rectangle((int)rect.X, (int)(rect.Y + rect.Height - t), (int)rect.Width, t), color);
            _spriteBatch.Draw(pixel, new Rectangle((int)rect.X, (int)rect.Y, t, (int)rect.Height), color);
            _spriteBatch.Draw(pixel, new Rectangle((int)(rect.X + rect.Width - t), (int)rect.Y, t, (int)rect.Height), color);
        }

        /// <summary>
        /// Draws a tiny circle in the specified area to see a non-visible point.
        /// </summary>
        /// <param name="_spriteBatch">The sprite batch to draw with</param>
        /// <param name="device">The Graphics Device of the game</param>
        /// <param name="center">The center of the point to draw</param>
        /// <param name="color">The color of the point</param>
        public static void DrawPoint(SpriteBatch _spriteBatch, GraphicsDevice device, Vector2 center, Color color)
        {
            float radius = 1;
            int steps = 8;
            Texture2D pixel = new Texture2D(device, 1, 1);
            pixel.SetData(new[] { Color.White });
            for (int i = 0; i < steps; i++)
            {
                float a = i * (MathF.PI * 2f / steps);
                Vector2 p = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius;
                _spriteBatch.Draw(pixel, p, null, color, 0f, new Vector2(0.5f, 0.5f), 3f, SpriteEffects.None, 0f);
            }
        }
    }
}
