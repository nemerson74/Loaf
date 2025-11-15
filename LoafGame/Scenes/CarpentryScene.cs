using LoafGame.Collisions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpDX.DXGI;
using System;
using System.IO;
using System.Reflection.Metadata;

namespace LoafGame.Scenes;

public class CarpentryScene : Scene, IParticleEmitter
{
    /// <summary>
    /// Position of the particle emitter in world space.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Velocity of the emitted particles.
    /// </summary>
    public Vector2 Velocity { get; set; }

    private bool debugFlag = false;
    Random random = new Random();
    Vector2 shakeOffset = Vector2.Zero;

    private float vw;
    private float vh;
    private float leftMargin;
    private float centerX;

    private SpriteBatch _spriteBatch;

    //private BoundingPoint cursor;

    Texture2D hammerTexture;
    private int hammerFrame = 0;
    private float glowDelay;
    private BoundingCircle headCircleLeft;
    private BoundingCircle headCircleRight;

    Texture2D nailTexture;
    private BoundingRectangle nailBounds;
    private int[] nailHitCounter = new int[3];
    private int nailIndex = 0;
    private float lastNailHitTime = 0f;

    Texture2D woodTexture;

    private static SoundEffect hammerWhoosh;
    private static SoundEffect hammerHit;

    private Vector2 anchor;
    private Vector2 prevAnchor;
    private float angle = 0.5f;
    private float prevAngle = 0f;
    private float angularVelocity;
    private float prevangularAcceleration = 0f;
    private float angularAcceleration;

    private bool screenShakeflag = false;
    private float screenShakeTimer = 0f;

    // --- constants ---
    private const float GRAVITY = 100f;
    private const float DAMPING = 0.995f;

    private const float MASS_HANDLE = 0.5f;
    private const float MASS_HEAD = 3.0f;

    private const float R_HANDLE = 9f;
    private const float R_HEAD = 14f;

    private const float MAX_VELOCITY_1 = 6f;
    private const float MAX_VELOCITY_2 = 9f;
    private const float MAX_VELOCITY_3 = 12f;

    private const float CLICK_TORQUE_STRENGTH = 500000f;

    private const float SCREEN_SHAKE_DURATION = 0.5f;

    private const int FRAME_WIDTH = 16;
    private const int FRAME_HEIGHT = 16;
    private static readonly Vector2 HAMMER_ORIGIN = new Vector2(7f, 15f);
    private const float HAMMER_DRAW_SCALE = 5f;
    private const float NAIL_DRAW_SCALE = 4f;

    // two head circles for hammer
    private static readonly Vector2 HEAD_LEFT_SRC = new Vector2(5f, 3f);
    private static readonly Vector2 HEAD_RIGHT_SRC = new Vector2(11f, 3f);
    private const float HEAD_CIRCLE_RADIUS = 2.3f; // in source pixels, will be scaled by HAMMER_DRAW_SCALE

    private float currentMaxVelocity = MAX_VELOCITY_1;

    // revolution tracking
    private float accumulatedCW = 0f;
    private float accumulatedCCW = 0f;
    private int revolutionsCW = 0;
    private int revolutionsCCW = 0;

    private Texture2D pixel;
    private FireballParticleSystem fireballred;
    private FireballParticleSystem fireballs;
    Coin coin;

    public CarpentryScene(Game game) : base(game) { }

    public override void Initialize()
    {
        base.Initialize();
        prevAnchor = Vector2.Zero;
        prevAngle = angle;

        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        fireballs = new FireballParticleSystem(LOAF, this, "fireballnormalgabe") { Emitting = false};
        LOAF.Components.Add(fireballs);

        fireballred = new FireballParticleSystem(LOAF, this, "fireballred") { Emitting = false };
        LOAF.Components.Add(fireballred);

        vw = Game.GraphicsDevice.Viewport.Width / LOAF.GameScale;
        vh = Game.GraphicsDevice.Viewport.Height / LOAF.GameScale;
        leftMargin = vw * 0.02f;
        centerX = vw / 2;

        MediaPlayer.Stop();
        MediaPlayer.Volume = 0.5f;
        MediaPlayer.Play(LOAF.backgroundMusicMinigame);
        MediaPlayer.IsRepeating = true;

        coin = new Coin(LOAF);
        base.Initialize();
    }

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        hammerTexture = Content.Load<Texture2D>("hammer");
        nailTexture = Content.Load<Texture2D>("nail");
        woodTexture = Content.Load<Texture2D>("wood");

        headCircleLeft = new BoundingCircle(Vector2.Zero, HEAD_CIRCLE_RADIUS * HAMMER_DRAW_SCALE);
        headCircleRight = new BoundingCircle(Vector2.Zero, HEAD_CIRCLE_RADIUS * HAMMER_DRAW_SCALE);

        nailBounds = new BoundingRectangle(new Vector2(100, 100), nailTexture.Width * NAIL_DRAW_SCALE, nailTexture.Height-1 * NAIL_DRAW_SCALE);

        hammerWhoosh = Content.Load<SoundEffect>("35_Miss_Evade_02");
        hammerHit = Content.Load<SoundEffect>("39_Block_03");

        // debug pixel
        pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
    }

    public override void Update(GameTime gameTime)
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        var input = LOAF.InputManager;
        debugFlag = input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space);
        coin.Update(gameTime);

        //follow the mouse
        anchor = input.Position / LOAF.GameScale;

        //update head circle centers to follow the rotated/scaled hammer sprite
        float drawScale = HAMMER_DRAW_SCALE;
        float drawRotation = angle - MathF.PI; // same rotation used for drawing
        Vector2 leftLocal = Rotate(HEAD_LEFT_SRC - HAMMER_ORIGIN, drawRotation) * drawScale;
        Vector2 rightLocal = Rotate(HEAD_RIGHT_SRC - HAMMER_ORIGIN, drawRotation) * drawScale;
        headCircleLeft.Center = anchor + leftLocal;
        headCircleRight.Center = anchor + rightLocal;
        headCircleLeft.Radius = HEAD_CIRCLE_RADIUS * drawScale;
        headCircleRight.Radius = HEAD_CIRCLE_RADIUS * drawScale;

        //update the nail bounds at current nail position
        nailBounds.Position = new Vector2(
            (nailIndex + 1) * 16 * 6 - 14,
            vh * 0.85f - 10 + nailHitCounter[nailIndex] * 1
        );

        //delta time
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (dt <= 0f) dt = 1f / 60f;

        // (m * g * r) for handle and head depending on angle
        float gravityTorque = -((MASS_HANDLE * GRAVITY * R_HANDLE) + (MASS_HEAD * GRAVITY * R_HEAD)) * (float)Math.Sin(angle);

        // (m * r^2) for handle and head
        float inertia = (MASS_HANDLE * R_HANDLE * R_HANDLE) + (MASS_HEAD * R_HEAD * R_HEAD);
        if (inertia <= 0.0001f) inertia = 0.0001f;

        //mouse button control for torque
        float clickTorque = 0f;
        if (input.LeftMouseDown)
        {
            clickTorque += CLICK_TORQUE_STRENGTH * dt;
        }
        if (input.RightMouseDown)
        {
            clickTorque -= CLICK_TORQUE_STRENGTH * dt;
        }

        // total acceleration from mouse and gravity
        angularAcceleration = (clickTorque + gravityTorque) / inertia;

        // integrate angular motion
        angularVelocity += angularAcceleration * dt;
        angularVelocity = MathHelper.Clamp(angularVelocity, -currentMaxVelocity, currentMaxVelocity);
        angle += angularVelocity * dt;

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
                if (Math.Abs(angularVelocity) > currentMaxVelocity * 0.7f)
                    hammerWhoosh.Play(1f, Math.Max(1f - Math.Abs(angularVelocity), 0.5f), 0f);
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
                if (Math.Abs(angularVelocity) > currentMaxVelocity * 0.7f)
                    hammerWhoosh.Play(1f, Math.Max(1f - Math.Abs(angularVelocity) / 12, 0.5f), 0f);
            }
        }
        prevAngle = angle;

        // update hammer frame/max velocity based on revolutions
        if (revolutionsCW >= 6)
        {
            currentMaxVelocity = MAX_VELOCITY_3;
            Velocity = headCircleLeft.Center - Position;
            Position = headCircleLeft.Center * LOAF.GameScale;
            hammerFrame = 2;
        }
        else if (revolutionsCW >= 3)
        {
            currentMaxVelocity = MAX_VELOCITY_2;
            Velocity = headCircleLeft.Center - Position;
            Position = headCircleLeft.Center * LOAF.GameScale;
            hammerFrame = 1;
        }
        else
        {
            if (revolutionsCCW == 0) currentMaxVelocity = MAX_VELOCITY_1;
            if (revolutionsCCW == 0) hammerFrame = 0;
        }

        if (revolutionsCCW >= 6)
        {
            currentMaxVelocity = MAX_VELOCITY_3;
            Velocity = headCircleRight.Center  - Position;
            Position = headCircleRight.Center * LOAF.GameScale;
            hammerFrame = 5;
        }
        else if (revolutionsCCW >= 3)
        {
            currentMaxVelocity = MAX_VELOCITY_2;
            Velocity = headCircleRight.Center - Position;
            Position = headCircleRight.Center * LOAF.GameScale;
            hammerFrame = 4;
        }
        else
        {
            if (revolutionsCW == 0) currentMaxVelocity = MAX_VELOCITY_1;
            if (revolutionsCW == 0) hammerFrame = 3;
        }

        // damping
        if (!(input.LeftMouseDown || input.RightMouseDown))
        {
            angularVelocity *= DAMPING;
        }

        //decay the rotations
        glowDelay += dt;
        if (MathF.Abs(angularVelocity) < 6f && glowDelay > 0.5f)
        {
            glowDelay = 0f;
            revolutionsCW = Math.Max(0, revolutionsCW - 1);
            revolutionsCCW = Math.Max(0, revolutionsCCW - 1);
        }

        //speed drops below 0.5, reset to stage 1
        if (Math.Abs(angularVelocity) < 0.5f)
        {
            currentMaxVelocity = MAX_VELOCITY_1;
            revolutionsCCW = 0;
            accumulatedCCW = 0f;
            revolutionsCW = 0;
            accumulatedCW = 0f;
        }

        //turn off emitter depending on speed
        if (MathF.Abs(angularVelocity) < 8f)
        {
            fireballred.Emitting = false;
            fireballs.Emitting = false;
        }
        else
        {
            if (MathF.Abs(angularVelocity) < 10f)
            {
                fireballred.Emitting = false;
                fireballs.Emitting = true;
            }
            else
            {
                fireballred.Emitting = true;
                fireballs.Emitting = false;
            }
        }

        //check for nail hits
        lastNailHitTime += dt;
        if (Math.Abs(angularVelocity) > 3f && lastNailHitTime > 0.3f)
        {
            //to the left of the nail and swinging CW
            if (anchor.X < nailBounds.X && angularVelocity > 0)
            {
                if (headCircleRight.CollidesWith(nailBounds))
                {
                    lastNailHitTime = 0f;
                    hammerHit.Play(1f, Math.Min(1f - Math.Abs(angularVelocity) / 12, 1f), 0f);
                    nailHitCounter[nailIndex] += (int)Math.Abs(angularVelocity) - 3;
                    if (nailHitCounter[nailIndex] >= 45)
                    {
                        //move to next nail
                        nailIndex++;
                        nailIndex = Math.Min(nailIndex, 2);
                        screenShakeflag = true;
                    }
                }
            }
            //to the right of the nail and swinging CCW
            if (anchor.X > nailBounds.X + nailBounds.Width && angularVelocity < 0)
            {
                if (headCircleLeft.CollidesWith(nailBounds))
                {
                    lastNailHitTime = 0f;
                    hammerHit.Play(1f, Math.Min(1f - Math.Abs(angularVelocity) / 12, 1f), 0f);
                    nailHitCounter[nailIndex] += (int)Math.Abs(angularVelocity) - 3;
                    if (nailHitCounter[nailIndex] >= 45)
                    {
                        //move to next nail
                        nailIndex++;
                        nailIndex = Math.Min(nailIndex, 2);
                        screenShakeflag = true;
                    }
                }
            }
            //to the left of the nail and swinging CCW
            if (anchor.X < nailBounds.X && angularVelocity < 0)
            {
                if (headCircleLeft.CollidesWith(nailBounds))
                {
                    lastNailHitTime = 0f;
                    hammerHit.Play(1f, Math.Min(1f - Math.Abs(angularVelocity) / 12, 1f), 0f);
                    nailHitCounter[nailIndex] -= (int)Math.Abs(angularVelocity) - 3;
                }
            }
            //to the right of the nail and swinging CW
            if (anchor.X > nailBounds.X + nailBounds.Width && angularVelocity > 0)
            {
                if (headCircleRight.CollidesWith(nailBounds))
                {
                    lastNailHitTime = 0f;
                    hammerHit.Play(1f, Math.Min(1f - Math.Abs(angularVelocity) / 12, 1f), 0f);
                    nailHitCounter[nailIndex] -= (int)Math.Abs(angularVelocity) - 3;
                }
            }
        }

        if (screenShakeflag)
        {
            screenShakeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            float shakeMagnitude = 1f;
            //decrease the shake
            float currentMagnitude = shakeMagnitude * (1f - (screenShakeTimer / SCREEN_SHAKE_DURATION));

            //generate new offset
            float shakeAngle = (float)random.NextDouble() * MathHelper.TwoPi;
            shakeOffset = new Vector2(
                (float)Math.Cos(shakeAngle) * currentMagnitude,
                (float)Math.Sin(shakeAngle) * currentMagnitude);

            if (screenShakeTimer >= SCREEN_SHAKE_DURATION)
            {
                screenShakeflag = false;
                screenShakeTimer = 0;
                shakeOffset = Vector2.Zero;
            }
        }

        // Return to title with Escape
        if (input.IsKeyDown(Keys.Escape))
        {
            fireballred.Emitting = false;
            fireballs.Emitting = false;
            if (fireballred != null)
            {
                LOAF.Components.Remove(fireballred);
                fireballred.Dispose();
                fireballred = null;
            }
            if (fireballs != null)
            {
                LOAF.Components.Remove(fireballs);
                fireballs.Dispose();
                fireballs = null;
            }
            LOAF.ChangeScene(new TitleScene(LOAF));
            return;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.DarkSlateGray);
        var LOAF = Game as LOAF;
        // combine scale and screen-shake translation into the view matrix
        Matrix viewMatrix = Matrix.CreateScale(LOAF.GameScale) * Matrix.CreateTranslation(shakeOffset.X, shakeOffset.Y, 0f);
        _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

        Rectangle sourceRect = new Rectangle(0, 0, FRAME_WIDTH, FRAME_HEIGHT);
        switch(hammerFrame)
        {
            case 0:
                sourceRect = new Rectangle(0, 0, FRAME_WIDTH, FRAME_HEIGHT);
                break;
            case 1:
                sourceRect = new Rectangle(FRAME_WIDTH, 0, FRAME_WIDTH, FRAME_HEIGHT);
                break;
            case 2:
                sourceRect = new Rectangle(FRAME_WIDTH * 2, 0, FRAME_WIDTH, FRAME_HEIGHT);
                break;
            case 3:
                sourceRect = new Rectangle(0, FRAME_HEIGHT, FRAME_WIDTH, FRAME_HEIGHT);
                break;
            case 4:
                sourceRect = new Rectangle(FRAME_WIDTH, FRAME_HEIGHT, FRAME_WIDTH, FRAME_HEIGHT);
                break;
            case 5:
                sourceRect = new Rectangle(FRAME_WIDTH * 2, FRAME_HEIGHT, FRAME_WIDTH, FRAME_HEIGHT);
                break;
        }
        //draw the hammer
        _spriteBatch.Draw(
            hammerTexture,
            anchor,
            sourceRect,
            Color.White,
            angle - MathF.PI,
            HAMMER_ORIGIN,
            HAMMER_DRAW_SCALE,
            SpriteEffects.None,
            0f
        );
        //draw the nails
        for (int i = 0; i < nailIndex+1; i++)
        {
            _spriteBatch.Draw(
                nailTexture,
                new Vector2(
                    (i + 1) * 16 * 6 - 14,
                    vh * 0.85f - 10 + nailHitCounter[i] * 1
                ),
                null,
                Color.White,
                0f,
                Vector2.Zero,
                NAIL_DRAW_SCALE,
                SpriteEffects.None,
                0f
            );
        }
        //draw the planks
        for (int i = 0; i < nailIndex + 2; i++)
        {
            _spriteBatch.Draw(
                woodTexture,
                new Vector2(
                    i * 16 * 6 + 10,
                    vh * 0.85f
                    ),
                null,
                Color.White,
                0f,
                Vector2.Zero,
                8f,
                SpriteEffects.None,
                0f
            );
        }
        SpriteFont font = Content.Load<SpriteFont>("vergilia");
        float fontScale = 1f;
        _spriteBatch.DrawString(font, "Mouse Buttons to Rotate, Spacebar: DEBUG", new Vector2(vw * 0.7f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        _spriteBatch.DrawString(font, "ESC to return to title", new Vector2(vw * 0.02f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

        if (nailHitCounter[2] >= 45)
        {
            _spriteBatch.DrawString(font, "All nails hammered! Well done!", new Vector2(50, 100), Color.Lime, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
        }

        coin.Draw();

        if (debugFlag)
        {
            DrawCircleOutline(headCircleLeft.Center, headCircleLeft.Radius, Color.Red);
            DrawCircleOutline(headCircleRight.Center, headCircleRight.Radius, Color.Red);
            DrawRectangleOutline(nailBounds, Color.Cyan);

            _spriteBatch.DrawString(font, "CW: " + revolutionsCW.ToString(), new Vector2(vw * 0.22f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            _spriteBatch.DrawString(font, "CCW: " + revolutionsCCW.ToString(), new Vector2(vw * 0.32f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            _spriteBatch.DrawString(font, "Speed: " + ((int)angularVelocity).ToString(), new Vector2(vw * 0.42f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        }
        _spriteBatch.End();
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

    //draw a circle outline by plotting points
    //move these to a class later
    private void DrawCircleOutline(Vector2 center, float radius, Color color)
    {
        // draw 36 points
        int steps = 36;
        for (int i = 0; i < steps; i++)
        {
            float a = i * (MathF.PI * 2f / steps);
            Vector2 p = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius;
            _spriteBatch.Draw(pixel, p, null, color, 0f, new Vector2(0.5f, 0.5f), 3f, SpriteEffects.None, 0f);
        }
    }

    // draw a rectangle outline by plotting points
    private void DrawRectangleOutline(BoundingRectangle rect, Color color)
    {
        // thickness
        int t = 2;
        _spriteBatch.Draw(pixel, new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, t), color);
        _spriteBatch.Draw(pixel, new Rectangle((int)rect.X, (int)(rect.Y + rect.Height - t), (int)rect.Width, t), color);
        _spriteBatch.Draw(pixel, new Rectangle((int)rect.X, (int)rect.Y, t, (int)rect.Height), color);
        _spriteBatch.Draw(pixel, new Rectangle((int)(rect.X + rect.Width - t), (int)rect.Y, t, (int)rect.Height), color);
    }
}