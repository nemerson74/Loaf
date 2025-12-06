using LoafGame.Collisions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoafGame
{
    public class Rotator
    {
        #region Properties
        /// <summary>
        /// Effect of gravity on the rotator. Set to 0 if parallel to ground.
        /// </summary>
        public float Gravity { get; set; } = 100f;

        /// <summary>
        /// General drag to simulate air resistance and friction.
        /// </summary>
        public float Damping { get; set; } = 0.995f;

        /// <summary>
        /// The mass of the head of the tool.
        /// </summary>
        public float HeadMass { get; set; } = 3.0f;

        /// <summary>
        /// The mass of the handle of the tool.
        /// </summary>
        public float HandleMass { get; set; } = 0.5f;

        /// <summary>
        /// The distance from anchor to COM of the head.
        /// </summary>
        public float HeadRadius { get; set; } = 14f;

        /// <summary>
        /// The distance from anchor to COM of the handle.
        /// </summary>
        public float HandleRadius { get; set; } = 9f;

        /// <summary>
        /// Mouse poosition.
        /// </summary>
        public Vector2 Anchor 
        {
            get
            {
                return anchor;
            }
        }

        /// <summary>
        /// Amount of speed stages. Set to 0 for no speed limit.
        /// </summary>
        public int SpeedStageCount { get; set; } = 3;

        /// <summary>
        /// Holds the max velocity for each stage, index 0 is the first stage limit.
        /// </summary>
        public float[] MaxVelocities { get; set; } = { 6f, 9f, 12f };

        /// <summary>
        /// Speed threshold where the revolutions reset.
        /// </summary>
        public float MinDecayVelocity { get; set; } = 0.5f;

        /// <summary>
        /// Height of the frame for the drawing box around the tool.
        /// </summary>
        public int FrameHeight { get; set; } = 16;

        /// <summary>
        /// Width of the frame for the drawing box around the tool.
        /// </summary>
        public int FrameWidth { get; set; } = 16;

        /// <summary>
        /// Where the cursor attaches to the tool.
        /// </summary>
        public Vector2 CursorOrigin { get; set; } = new Vector2(7f, 15f);

        /// <summary>
        /// The center of the left bounding circle.
        /// </summary>
        public Vector2 LeftCollisionOrigin { get; set; } = new Vector2(1f, 4f);

        /// <summary>
        /// the center of the right bounding circle.
        /// </summary>
        public Vector2 RightCollisionOrigin { get; set; } = new Vector2(15f, 4f);

        /// <summary>
        /// Base radius of the collision circles.
        /// </summary>
        public float CollisionRadius { get; set; } = 2.3f;

        /// <summary>
        /// Scale of the tool.
        /// </summary>
        public float Scale { get; set; } = 5f;

        /// <summary>
        /// Scale of the tool.
        /// </summary>
        public float GameScale { get; set; } = 1f;

        /// <summary>
        /// The magnitude of the effect of mouse input on rotation.
        /// </summary>
        public float ClickTorque { get; set; } = 500000f;

        /// <summary>
        /// How many revolutions CW have passed in current direction.
        /// </summary>
        public float revolutionsCW { get; set; } = 0f;

        /// <summary>
        /// How many revolutions CCW have passed in current direction.
        /// </summary>
        public float revolutionsCCW { get; set; } = 0f;

        /// <summary>
        /// Cirrent Angular Velocity of the rotator.
        /// </summary>
        public float AngularVelocity { get; private set; } = 0f;

        /// <summary>
        /// The name of the texture to load.
        /// </summary>
        public string TextureName { get; set; } = "hammer";

        /// <summary>
        /// The name of the tool whoose sound to load.
        /// </summary>
        public string ToolWhooshSoundString { get; set; } = "35_Miss_Evade_02";

        /// <summary>
        /// The name of the tool hit sound to load.
        /// </summary>
        public string ToolHitSoundString { get; set; } = "39_Block_03";
        #endregion
        #region private variables
        private float angle = 0.5f;
        private float prevAngle = 0f;
        private float currentVelocity;
        private float currentMaxVelocity;
        private float angularAcceleration;
        private float accumulatedCW = 0f;
        private float accumulatedCCW = 0f;
        private float gameScale;
        private float glowDelay;
        private Vector2 anchor;
        private static SoundEffect toolWhoosh;
        private static SoundEffect toolHit;
        private Texture2D toolTexture;
        private int toolFrame = 0;
        public BoundingCircle leftBoundingCircle;
        public BoundingCircle rightBoundingCircle;
        #endregion

        public Rotator() {}
        private void Initialize(float gameScale)
        {
            prevAngle = angle;
            this.gameScale = gameScale;
            leftBoundingCircle = new BoundingCircle(Vector2.Zero, CollisionRadius * Scale);
            rightBoundingCircle = new BoundingCircle(Vector2.Zero, CollisionRadius * Scale);
        }

        public void LoadContent(ContentManager content, float gameScale)
        {
            Initialize(gameScale);
            toolTexture = content.Load<Texture2D>(TextureName);
            toolWhoosh = content.Load<SoundEffect>(ToolWhooshSoundString);
            toolHit = content.Load<SoundEffect>(ToolHitSoundString);
        }

        public void Update(GameTime gameTime, InputManager input)
        {
            //follow the mouse (in world units; input.Position is in screen units)
            anchor = input.Position / gameScale;

            // update head circle centers to follow the rotated/scaled sprite
            float drawRotation = angle - MathF.PI;
            // compute local offsets from the cursor origin, rotated, then scaled by sprite Scale
            Vector2 leftLocal = Rotate(LeftCollisionOrigin - CursorOrigin, drawRotation) * Scale;
            Vector2 rightLocal = Rotate(RightCollisionOrigin - CursorOrigin, drawRotation) * Scale;
            // place circles at sprite position plus local offsets
            leftBoundingCircle.Center = anchor + leftLocal;
            rightBoundingCircle.Center = anchor + rightLocal;
            // set radii
            leftBoundingCircle.Radius = CollisionRadius;
            rightBoundingCircle.Radius = CollisionRadius;

            //delta time
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f) dt = 1f / 60f;

            // (m * g * r) for handle and head depending on angle
            float gravityTorque = -((HandleMass * Gravity * HandleRadius) + (HeadMass * Gravity * HeadRadius)) * (float)Math.Sin(angle);

            // (m * r^2) for handle and head
            float inertia = (HandleMass * HandleRadius * HandleRadius) + (HeadMass * HeadRadius * HeadRadius);
            if (inertia <= 0.0001f) inertia = 0.0001f;

            //mouse button control for torque
            float clickTorque = 0f;
            if (input.LeftMouseDown)
            {
                clickTorque += ClickTorque * dt;
            }
            if (input.RightMouseDown)
            {
                clickTorque -= ClickTorque * dt;
            }

            // total acceleration from mouse and gravity
            angularAcceleration = (clickTorque + gravityTorque) / inertia;

            // integrate angular motion
            AngularVelocity += angularAcceleration * dt;
            AngularVelocity = MathHelper.Clamp(AngularVelocity, -currentMaxVelocity, currentMaxVelocity);
            angle += AngularVelocity * dt;

            // revolution tracking
            float delta = WrapAngle(angle - prevAngle);
            if (delta > 0f)
            {
                // CCW rotation
                accumulatedCCW += delta;
                while (accumulatedCCW >= MathF.PI * 2f)
                {
                    revolutionsCCW++;
                    accumulatedCCW -= MathF.PI * 2f;
                    revolutionsCCW = Math.Min(revolutionsCCW, 12);
                    revolutionsCW = 0;
                    if (Math.Abs(AngularVelocity) > currentMaxVelocity * 0.7f)
                        toolWhoosh.Play(1f, Math.Max(1f - Math.Abs(AngularVelocity), 0.5f), 0f);
                }
            }
            else if (delta < 0f)
            {
                // CW rotation
                accumulatedCW += -delta;
                while (accumulatedCW >= MathF.PI * 2f)
                {
                    revolutionsCW++;
                    accumulatedCW -= MathF.PI * 2f;
                    revolutionsCW = Math.Min(revolutionsCW, 12);
                    revolutionsCCW = 0;
                    if (Math.Abs(AngularVelocity) > currentMaxVelocity * 0.7f)
                        toolWhoosh.Play(1f, Math.Max(1f - Math.Abs(AngularVelocity) / 12, 0.5f), 0f);
                }
            }
            prevAngle = angle;

            // update hammer frame/max velocity based on revolutions
            if (revolutionsCW >= 6)
            {
                currentMaxVelocity = MaxVelocities[2];
                //Velocity = headCircleLeft.Center - Position;
                //Position = headCircleLeft.Center * LOAF.GameScale;
                toolFrame = 2;
            }
            else if (revolutionsCW >= 3)
            {
                currentMaxVelocity = MaxVelocities[1];
                //Velocity = headCircleLeft.Center - Position;
                //Position = headCircleLeft.Center * LOAF.GameScale;
                toolFrame = 1;
            }
            else
            {
                if (revolutionsCCW == 0) currentMaxVelocity = MaxVelocities[0];
                if (revolutionsCCW == 0) toolFrame = 0;
            }

            if (revolutionsCCW >= 6)
            {
                currentMaxVelocity = MaxVelocities[2];
                //Velocity = headCircleRight.Center - Position;
                //Position = headCircleRight.Center * LOAF.GameScale;
                toolFrame = 5;
            }
            else if (revolutionsCCW >= 3)
            {
                currentMaxVelocity = MaxVelocities[1];
                //Velocity = headCircleRight.Center - Position;
                //Position = headCircleRight.Center * LOAF.GameScale;
                toolFrame = 4;
            }
            else
            {
                if (revolutionsCW == 0) currentMaxVelocity = MaxVelocities[0];
                if (revolutionsCW == 0) toolFrame = 3;
            }

            // damping
            if (!(input.LeftMouseDown || input.RightMouseDown))
            {
                AngularVelocity *= Damping;
            }

            //decay the rotations
            glowDelay += dt;
            if (MathF.Abs(AngularVelocity) < 6f && glowDelay > 0.5f)
            {
                glowDelay = 0f;
                revolutionsCW = Math.Max(0, revolutionsCW - 1);
                revolutionsCCW = Math.Max(0, revolutionsCCW - 1);
            }

            //speed drops below MinDecayVelocity, reset to stage 1
            if (Math.Abs(AngularVelocity) < MinDecayVelocity)
            {
                currentMaxVelocity = MaxVelocities[0];
                revolutionsCCW = 0;
                accumulatedCCW = 0f;
                revolutionsCW = 0;
                accumulatedCW = 0f;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Rectangle sourceRect = new Rectangle(0, 0, FrameWidth, FrameHeight);
            switch (toolFrame)
            {
                case 0:
                    sourceRect = new Rectangle(0, 0, FrameWidth, FrameHeight);
                    break;
                case 1:
                    sourceRect = new Rectangle(FrameWidth, 0, FrameWidth, FrameHeight);
                    break;
                case 2:
                    sourceRect = new Rectangle(FrameWidth * 2, 0, FrameWidth, FrameHeight);
                    break;
                case 3:
                    sourceRect = new Rectangle(0, FrameHeight, FrameWidth, FrameHeight);
                    break;
                case 4:
                    sourceRect = new Rectangle(FrameWidth, FrameHeight, FrameWidth, FrameHeight);
                    break;
                case 5:
                    sourceRect = new Rectangle(FrameWidth * 2, FrameHeight, FrameWidth, FrameHeight);
                    break;
            }
            //draw the tool
            spriteBatch.Draw(
                toolTexture,
                anchor,
                sourceRect,
                Color.White,
                angle - MathF.PI,
                CursorOrigin,
                Scale,
                SpriteEffects.None,
                0f
            );
        }

        public void PlayHitSound()
        {
            toolHit.Play(1f, 0f, 0f);
        }

        public void Rebound()
        {
            AngularVelocity = -AngularVelocity * 0.2f;
        }

        private static float WrapAngle(float radians)
        {
            while (radians <= -MathF.PI) radians += 2f * MathF.PI;
            while (radians > MathF.PI) radians -= 2f * MathF.PI;
            return radians;
        }

        private static Vector2 Rotate(Vector2 v, float angle)
        {
            float c = MathF.Cos(angle);
            float s = MathF.Sin(angle);
            return new Vector2(v.X * c - v.Y * s, v.X * s + v.Y * c);
        }
    }
}