using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LoafGame
{
    public class FireballParticleSystem : ParticleSystem
    {
        IParticleEmitter _emitter;
        private string textureString;

        /// <summary>
        /// If the emitter is emitting particles
        /// </summary>s
        public bool Emitting { get; set; } = true;

        public FireballParticleSystem(Game game, IParticleEmitter emitter, string textureString) : base(game, 2000)
        {
            _emitter = emitter;
            this.textureString = textureString;
        }

        protected override void InitializeConstants()
        {
            textureFilename = textureString;

            minNumParticles = 5;
            maxNumParticles = 10;

            blendState = BlendState.Additive;
            DrawOrder = AdditiveBlendDrawOrder;
        }

        protected override void InitializeParticle(ref Particle p, Vector2 where)
        {
            var velocity = _emitter.Velocity * RandomHelper.NextFloat(0.7f, 1.3f);
            var acceleration = Vector2.UnitY * 2000;
            var scale = RandomHelper.NextFloat(1f, 2f);
            var lifetime = RandomHelper.NextFloat(0.05f, 0.25f);
            var rotation = RandomHelper.NextFloat(0, MathHelper.TwoPi);

            p.Initialize(where, velocity, acceleration, Color.White, scale: scale, lifetime: lifetime, rotation: rotation);
        }

        protected override void LoadContent()
        {
            textureFilename = textureString ?? textureFilename ?? "BALL";
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // only spawn new particles while Emitting is true
            if (Emitting)
            {
                AddParticles(_emitter.Position);
            }
        }
    }
}
