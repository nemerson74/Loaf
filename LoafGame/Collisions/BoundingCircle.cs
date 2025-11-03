using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace LoafGame.Collisions
{
    /// <summary>
    /// A struct representing a bounding circle for collision detection.
    /// </summary>
    public struct BoundingCircle
    {
        /// <summary>
        /// The center of the bounding circle.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// The radius of the bounding circle
        /// </summary>
        public float Radius;

        /// <summary>
        /// Constructs a new bounding circle.
        /// </summary>
        /// <param name="center">The center of the new bounding circle</param>
        /// <param name="radius">The radius of the bounding circle</param>
        public BoundingCircle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Tests for collision between this and another Bounding Circle.
        /// </summary>
        /// <param name="other">The other bounding circle</param>
        /// <returns>True for collision</returns>
        public bool CollidesWith(BoundingCircle other)
        {
            return CollisionHelper.Collides(this, other);
        }

        /// <summary>
        /// Tests for collision between this and a bounding Rectangle.
        /// </summary>
        /// <param name="other">The bounding rectangle</param>
        /// <returns>True for collision</returns>
        public bool CollidesWith(BoundingRectangle other)
        {
            return CollisionHelper.Collides(this, other);
        }
    }
}
