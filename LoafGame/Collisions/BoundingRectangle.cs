using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LoafGame.Collisions
{
    /// <summary>
    /// A struct representing a bounding rectangle for collision detection.
    /// </summary>
    public struct BoundingRectangle
    {
        /// <summary>
        /// Width of the bounding circle
        /// </summary>
        public float Width;
        /// <summary>
        /// Height of the bounding circle
        /// </summary>
        public float Height;
        /// <summary>
        /// X coordinate of the bounding Rectangle
        /// </summary>
        public float X;
        /// <summary>
        /// Y coordinate of the bounding Rectangle
        /// </summary>
        public float Y;
        public float Left => X;
        public float Right => X + Width;
        public float Top => Y;
        public float Bottom => Y + Height;

        /// <summary>
        /// Gets or sets the position (X,Y) as a Vector2.
        /// </summary>
        public Vector2 Position
        {
            get => new Vector2(X, Y);
            set { X = value.X; Y = value.Y; }
        }

        /// <summary>
        /// Builds a new bounding rectangle from x, y, width and height
        /// </summary>
        /// <param name="x">The x coordinates of the rectangle</param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public BoundingRectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Builds a new bounding rectangle from a position vector, width and height
        /// </summary>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public BoundingRectangle(Vector2 position, float width, float height)
        {
            X = position.X;
            Y = position.Y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Move the rectangle to the given position.
        /// </summary>
        public void MoveTo(Vector2 position)
        {
            X = position.X;
            Y = position.Y;
        }

        /// <summary>
        /// Move the rectangle to the given coordinates.
        /// </summary>
        public void MoveTo(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Move the rectangle by delta.
        /// </summary>
        public void Translate(Vector2 delta)
        {
            X += delta.X;
            Y += delta.Y;
        }

        /// <summary>
        /// Move the rectangle by dx, dy.
        /// </summary>
        public void Translate(float dx, float dy)
        {
            X += dx;
            Y += dy;
        }

        /// <summary>
        /// Tests for collision between this and another Bounding Rectangle.
        /// </summary>
        /// <param name="other">The other bounding rectangle</param>
        /// <returns>True for collision</returns>
        public bool CollidesWith(BoundingRectangle other)
        {
            return CollisionHelper.Collides(this, other);
        }

        /// <summary>
        /// Tests for collision between this and a Bounding Circle.
        /// </summary>
        /// <param name="other">The bounding Circle</param>
        /// <returns>True for collision</returns>
        public bool CollidesWith(BoundingCircle other)
        {
            return CollisionHelper.Collides(other, this);
        }

        /// <summary>
        /// Detects a collision between a rectangle and a point
        /// </summary>
        /// <param name="r">The rectangle</param>
        /// <param name="p">The point</param>
        /// <returns>true on collision, false otherwise</returns>
        public static bool Collides(BoundingRectangle r, BoundingPoint p)
        {
            return p.X >= r.X && p.X <= r.X + r.Width && p.Y >= r.Y && p.Y <= r.Y + r.Height;
        }
    }
}
