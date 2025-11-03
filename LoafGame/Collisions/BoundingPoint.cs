using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LoafGame.Collisions
{
    /// <summary>
    /// A struct representing a bounding rectangle for collision detection.
    /// </summary>
    public struct BoundingPoint
    {
        /// <summary>
        /// X coordinate of the bounding Rectangle
        /// </summary>
        public float X;
        /// <summary>
        /// Y coordinate of the bounding Rectangle
        /// </summary>
        public float Y;

        /// <summary>
        /// Builds a new bounding rectangle from x, y
        /// </summary>
        /// <param name="x">The x coordinates of the rectangle</param>
        /// <param name="y">The y coordinates of the rectangle</param>
        public BoundingPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Builds a new bounding rectangle from a position vector
        /// </summary>
        /// <param name="position">Vector of the </param>
        public BoundingPoint(Vector2 position)
        {
            X = position.X;
            Y = position.Y;
        }

        /// <summary>
        /// Tests for collision between this and a Bounding Rectangle.
        /// </summary>
        /// <param name="rect">The bounding rectangle</param>
        /// <returns>True for collision</returns>
        public bool CollidesWith(BoundingRectangle rect)
        {
            return X >= rect.Left &&
                   X <= rect.Right &&
                   Y >= rect.Top &&
                   Y <= rect.Bottom;
        }

        /// <summary>
        /// Tests for collision between this and a Bounding Circle.
        /// </summary>
        /// <param name="other">The bounding Circle</param>
        /// <returns>True for collision</returns>
        public bool CollidesWith(BoundingCircle other)
        {
            return Math.Pow(other.Radius, 2) >= Math.Pow(other.Center.X - this.X, 2) + Math.Pow(other.Center.Y - this.Y, 2);
        }
    }
}
