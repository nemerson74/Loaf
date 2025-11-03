using Microsoft.Xna.Framework;

namespace LoafGame
{
    /// <summary>
    /// A class representing a single particle in a particle system 
    /// </summary>
    public struct Particle
    {
        /// <summary>
        /// The current position of the particle. Default (0,0).
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The current velocity of the particle. Default (0,0).
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// The current acceleration of the particle. Default (0,0).
        /// </summary>
        public Vector2 Acceleration;

        /// <summary>
        /// The current rotation of the particle. Default 0.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The current angular velocity of the particle. Default 0.
        /// </summary>
        public float AngularVelocity;

        /// <summary>
        /// The current angular acceleration of the particle. Default 0.
        /// </summary>
        public float AngularAcceleration;

        /// <summary>
        /// The current scale of the particle.  Default 1.
        /// </summary>
        public float Scale;

        /// <summary>
        /// The current lifetime of the particle (how long it will "live").  Default 1s.
        /// </summary>
        public float Lifetime;

        /// <summary>
        /// How long this particle has been alive 
        /// </summary>
        public float TimeSinceStart;

        /// <summary>
        /// The current color of the particle. Default White
        /// </summary>
        public Color Color;

        /// <summary>
        /// If this particle is still alive, and should be rendered
        /// <summary>
        public bool Active => TimeSinceStart < Lifetime;



        /// <summary>
        /// Sets the particle up for first use, restoring defaults
        /// </summary>
        public void Initialize(Vector2 where, float lifetime = 1, float scale = 1, float rotation = 0, float angularVelocity = 0, float angularAcceleration = 0)
        {
            this.Position = where;
            this.Velocity = Vector2.Zero;
            this.Acceleration = Vector2.Zero;
            this.Rotation = rotation;
            this.AngularVelocity = angularVelocity;
            this.AngularAcceleration = angularAcceleration;
            this.Scale = scale;
            this.Color = Color.White;
            this.Lifetime = lifetime;
            this.TimeSinceStart = 0f;
        }

        /// <summary>
        /// Sets the particle up for first use 
        /// </summary>
        public void Initialize(Vector2 position, Vector2 velocity, float lifetime = 1, float scale = 1, float rotation = 0, float angularVelocity = 0, float angularAcceleration = 0)
        {
            this.Position = position;
            this.Velocity = velocity;
            this.Acceleration = Vector2.Zero;
            this.Lifetime = lifetime;
            this.TimeSinceStart = 0f;
            this.Scale = scale;
            this.Rotation = rotation;
            this.AngularVelocity = angularVelocity;
            this.AngularAcceleration = angularAcceleration;
            this.Color = Color.White;
        }

        /// <summary>
        /// Sets the particle up for first use 
        /// </summary>
        public void Initialize(Vector2 position, Vector2 velocity, Vector2 acceleration, float lifetime = 1, float scale = 1, float rotation = 0, float angularVelocity = 0, float angularAcceleration = 0)
        {
            this.Position = position;
            this.Velocity = velocity;
            this.Acceleration = acceleration;
            this.Lifetime = lifetime;
            this.TimeSinceStart = 0f;
            this.Scale = scale;
            this.Rotation = rotation;
            this.AngularVelocity = angularVelocity;
            this.AngularAcceleration = angularAcceleration;
            this.Color = Color.White;
        }

        /// <summary>
        /// Sets the particle up for first use 
        /// </summary>
        public void Initialize(Vector2 position, Vector2 velocity, Vector2 acceleration, Color color, float lifetime = 1, float scale = 1, float rotation = 0, float angularVelocity = 0, float angularAcceleration = 0)
        {
            this.Position = position;
            this.Velocity = velocity;
            this.Acceleration = acceleration;
            this.Lifetime = lifetime;
            this.TimeSinceStart = 0f;
            this.Scale = scale;
            this.Rotation = rotation;
            this.AngularVelocity = angularVelocity;
            this.AngularAcceleration = angularAcceleration;
            this.Color = color;
        }
    }
}
