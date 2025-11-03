using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace LoafGame.Collisions
{

    public static class CollisionHelper
    {
        /// <summary>
        /// Detects collision between two bounding circles
        /// </summary>
        /// <param name="A">First circle</param>
        /// <param name="B">Second circle</param>
        /// <returns>True for collision</returns>
        public static bool Collides(BoundingCircle A, BoundingCircle B)
        {
            float radii = A.Radius + B.Radius;
            return Vector2.DistanceSquared(A.Center, B.Center) <= radii * radii;
        }

        /// <summary>
        /// Detects collision between two bounding rectangles
        /// </summary>
        /// <param name="A">First rectangle</param>
        /// <param name="B">Second rectangle</param>
        /// <returns>True for collision</returns>
        public static bool Collides(BoundingRectangle A, BoundingRectangle B)
        {
            return !(A.Right < B.Left || A.Left > B.Right
                     || A.Top > B.Bottom || A.Bottom < B.Top);
        }

        /// <summary>
        /// Detects collision between a bounding circle and a bounding rectangle
        /// </summary>
        /// <param name="C">The Circle</param>
        /// <param name="R">The Rectangle</param>
        /// <returns>True for collision</returns>
        public static bool Collides(BoundingCircle C, BoundingRectangle R)
        {
            float nearestX = MathHelper.Clamp(C.Center.X, R.Left, R.Right);
            float nearestY = MathHelper.Clamp(C.Center.Y, R.Top, R.Bottom);
            float deltaX = C.Center.X - nearestX;
            float deltaY = C.Center.Y - nearestY;
            return (Math.Pow(deltaX,2) + Math.Pow(deltaY, 2)) < Math.Pow(C.Radius, 2);
        }

        /// <summary>
        /// Detects collision between a bounding rectangle and a bounding circle
        /// </summary>
        /// <param name="R">The Rectangle</param>
        /// <param name="C">The Circle</param>
        /// <returns>True for collision</returns>
        public static bool Collides(BoundingRectangle R, BoundingCircle C)
        {
            return Collides(C, R);
        }
    }
}
