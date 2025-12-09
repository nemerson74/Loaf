using LoafGame.Collisions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using System;
using System.IO;
using System.Linq;
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

    private int _score = 0;
    private ScoreTracker _scoreTracker;
    private ScoreTimer _scoreTimer;
    private bool debugFlag = false;
    Random random = new Random();
    Vector2 shakeOffset = Vector2.Zero;

    private float vw;
    private float vh;
    private float leftMargin;
    private float centerX;

    private SpriteBatch _spriteBatch;
    private SpriteFont font;

    //private BoundingPoint cursor;

    Texture2D hammerTexture;
    private int hammerFrame = 0;
    private float glowDelay;
    private BoundingCircle headCircleLeft;
    private BoundingCircle headCircleRight;

    Texture2D nailTexture;
    private BoundingRectangle nailBounds;
    private BoundingRectangle nailBounds2;
    private int[] nailHitCounter = new int[3];
    private float lastNailHitTime = 0f;
    private float lastSideNailHitTime = 0f;

    private Vector2[] boardPositions = new Vector2[BOARD_COUNT];
    private Vector2[] nailPositions = new Vector2[NAIL_COUNT];
    private Vector2[] sideNailPositions = new Vector2[SIDE_NAIL_COUNT];
    private int[] nailRandom = new int[NAIL_COUNT];
    private int[] sideNailRandom = new int[SIDE_NAIL_COUNT];
    private int nailIndex = 0;
    private int sideNailIndex = 0;
    private float[] nailProgress = new float[NAIL_COUNT];
    private float[] sideNailProgress = new float[SIDE_NAIL_COUNT];

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

    private bool screenShakeFlag = false;
    private float screenShakeTimer = 0f;

    private bool gameOverFlag = false;

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
    private const float BOARD_DRAW_SCALE = 8f;
    private const int BOARD_COUNT = 12;
    private const int SIDE_NAIL_COUNT = 4;
    private const int NAIL_COUNT = 4;
    private const float NAIL_HIT_THRESHOLD = 45f;

    // two head circles for hammer
    private static readonly Vector2 HEAD_LEFT_SRC = new Vector2(5f, 3f);
    private static readonly Vector2 HEAD_RIGHT_SRC = new Vector2(11f, 3f);
    private const float HEAD_CIRCLE_RADIUS = 2.3f; // in source pixels, will be scaled by HAMMER_DRAW_SCALE

    private static readonly float[] BOARD_ROTATIONS = new float[12] { 0f, 0f, 0f, 0f, 0f, 0f, MathF.PI / 2, MathF.PI / 2, MathF.PI / 2, MathF.PI / 2, MathF.PI / 2, MathF.PI / 2 };
    private static readonly float[] SIDE_NAIL_ROTATIONS = new float[4] { MathF.PI / 2, MathF.PI / 2, MathF.PI + MathF.PI / 2, MathF.PI + MathF.PI / 2 };

    private float currentMaxVelocity = MAX_VELOCITY_1;

    // revolution tracking
    private float accumulatedCW = 0f;
    private float accumulatedCCW = 0f;
    private int revolutionsCW = 0;
    private int revolutionsCCW = 0;

    private Texture2D pixel;
    private FireballParticleSystem fireballred;
    private FireballParticleSystem fireballs;

    public CarpentryScene(Game game, ScoreTracker scoreTracker = null) : base(game)
    {
        _scoreTracker = scoreTracker;
    }

    public override void Initialize()
    {
        prevAnchor = Vector2.Zero;
        prevAngle = angle;

        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        fireballs = new FireballParticleSystem(LOAF, this, "fireballnormalgabe") { Emitting = false };
        LOAF.Components.Add(fireballs);

        fireballred = new FireballParticleSystem(LOAF, this, "fireballred") { Emitting = false };
        LOAF.Components.Add(fireballred);

        vw = Game.GraphicsDevice.Viewport.Width / LOAF.GameScale;
        vh = Game.GraphicsDevice.Viewport.Height / LOAF.GameScale;
        leftMargin = vw * 0.02f;
        centerX = vw / 2;

        boardPositions = new Vector2[12]
        {
            new Vector2(0 * 16 * BOARD_DRAW_SCALE, vh - 11 * BOARD_DRAW_SCALE),
            new Vector2(1 * 16 * BOARD_DRAW_SCALE, vh - 11 * BOARD_DRAW_SCALE),
            new Vector2(2 * 16 * BOARD_DRAW_SCALE, vh - 11 * BOARD_DRAW_SCALE),
            new Vector2(vw - 1 * 16 * BOARD_DRAW_SCALE, vh - 11 * BOARD_DRAW_SCALE),
            new Vector2(vw - 2 * 16 * BOARD_DRAW_SCALE, vh - 11 * BOARD_DRAW_SCALE),
            new Vector2(vw - 3 * 16 * BOARD_DRAW_SCALE, vh - 11 * BOARD_DRAW_SCALE),
            new Vector2(11 * BOARD_DRAW_SCALE, vh * 0.5f - 0 * 16 * BOARD_DRAW_SCALE),
            new Vector2(11 * BOARD_DRAW_SCALE, vh * 0.5f - 1 * 16 * BOARD_DRAW_SCALE),
            new Vector2(11 * BOARD_DRAW_SCALE, vh * 0.5f - 2 * 16 * BOARD_DRAW_SCALE),
            new Vector2(vw + 5 * BOARD_DRAW_SCALE, vh * 0.5f - 0 * 16 * BOARD_DRAW_SCALE),
            new Vector2(vw + 5 * BOARD_DRAW_SCALE, vh * 0.5f - 1 * 16 * BOARD_DRAW_SCALE),
            new Vector2(vw + 5 * BOARD_DRAW_SCALE, vh * 0.5f - 2 * 16 * BOARD_DRAW_SCALE)
        };

        nailPositions = new Vector2[4]
        {
            new Vector2(1 * 16 * BOARD_DRAW_SCALE - 0.5f * 16 * NAIL_DRAW_SCALE, vh - 5 * BOARD_DRAW_SCALE - 16 * NAIL_DRAW_SCALE),
            new Vector2(2 * 16 * BOARD_DRAW_SCALE - 0.5f * 16 * NAIL_DRAW_SCALE, vh - 5 * BOARD_DRAW_SCALE - 16 * NAIL_DRAW_SCALE),
            new Vector2(vw - 2 * 16 * BOARD_DRAW_SCALE - 0.5f * 16 * NAIL_DRAW_SCALE, vh - 5 * BOARD_DRAW_SCALE - 16 * NAIL_DRAW_SCALE),
            new Vector2(vw - 1 * 16 * BOARD_DRAW_SCALE - 0.5f * 16 * NAIL_DRAW_SCALE, vh - 5 * BOARD_DRAW_SCALE - 16 * NAIL_DRAW_SCALE)
        };

        sideNailPositions = new Vector2[4]
        {
            new Vector2(5 * BOARD_DRAW_SCALE + 16 * NAIL_DRAW_SCALE, vh * 0.5f - 0 * 16 * BOARD_DRAW_SCALE - 0.5f * 16 * NAIL_DRAW_SCALE),
            new Vector2(5 * BOARD_DRAW_SCALE + 16 * NAIL_DRAW_SCALE, vh * 0.5f - 1 * 16 * BOARD_DRAW_SCALE - 0.5f * 16 * NAIL_DRAW_SCALE),
            new Vector2(vw - 16 * NAIL_DRAW_SCALE - 5 * BOARD_DRAW_SCALE, vh * 0.5f - -1 * 16 * BOARD_DRAW_SCALE - 16 * NAIL_DRAW_SCALE - 0.5f * 16 * NAIL_DRAW_SCALE),
            new Vector2(vw - 16 * NAIL_DRAW_SCALE - 5 * BOARD_DRAW_SCALE, vh * 0.5f - 0 * 16 * BOARD_DRAW_SCALE - 16 * NAIL_DRAW_SCALE - 0.5f * 16 * NAIL_DRAW_SCALE)
        };

        nailRandom = CreateRandomlySortedArray(0, NAIL_COUNT - 1);
        sideNailRandom = CreateRandomlySortedArray(0, SIDE_NAIL_COUNT - 1);

        MediaPlayer.Stop();
        MediaPlayer.Volume = 0.5f;
        MediaPlayer.Play(LOAF.backgroundMusicMinigame);
        MediaPlayer.IsRepeating = true;

        _scoreTimer = new ScoreTimer(Enums.GameType.Carpentry);

        base.Initialize();
    }

    public override void LoadContent()
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;

        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        hammerTexture = Content.Load<Texture2D>("hammer");
        nailTexture = Content.Load<Texture2D>("nail");
        woodTexture = Content.Load<Texture2D>("wood");

        hammerWhoosh = Content.Load<SoundEffect>("35_Miss_Evade_02");
        hammerHit = Content.Load<SoundEffect>("39_Block_03");

        font = Content.Load<SpriteFont>("vergilia");

        headCircleLeft = new BoundingCircle(Vector2.Zero, HEAD_CIRCLE_RADIUS * HAMMER_DRAW_SCALE);
        headCircleRight = new BoundingCircle(Vector2.Zero, HEAD_CIRCLE_RADIUS * HAMMER_DRAW_SCALE);

        nailBounds = new BoundingRectangle(new Vector2(100, 100), nailTexture.Width * NAIL_DRAW_SCALE, nailTexture.Height - 1 * NAIL_DRAW_SCALE);
        nailBounds2 = new BoundingRectangle(new Vector2(200, 200), nailTexture.Height - 1 * NAIL_DRAW_SCALE, nailTexture.Width * NAIL_DRAW_SCALE);

        _scoreTimer.LoadContent(Content, LOAF.GameScale);

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

        #region hammerstuff

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
            Velocity = headCircleRight.Center - Position;
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
        #endregion

        if (!gameOverFlag)
        {
            //update the nail bounds at current nail position
            if (!(nailIndex == NAIL_COUNT - 1 && nailProgress[nailRandom[nailIndex]] >= NAIL_HIT_THRESHOLD))
            {
                nailBounds.Position = new Vector2(nailPositions[nailRandom[nailIndex]].X, nailPositions[nailRandom[nailIndex]].Y + nailProgress[nailRandom[nailIndex]]);
            }

            //update the nail bounds at current nail position
            if (!(sideNailIndex == NAIL_COUNT - 1 && sideNailProgress[sideNailRandom[sideNailIndex]] >= NAIL_HIT_THRESHOLD))
            {
                if (sideNailRandom[sideNailIndex] <= 1)
                {
                    nailBounds2.Position = new Vector2(sideNailPositions[sideNailRandom[sideNailIndex]].X - sideNailProgress[sideNailRandom[sideNailIndex]] - (nailTexture.Height - 1 * NAIL_DRAW_SCALE), sideNailPositions[sideNailRandom[sideNailIndex]].Y);
                }
                else
                {
                    nailBounds2.Position = new Vector2(sideNailPositions[sideNailRandom[sideNailIndex]].X + sideNailProgress[sideNailRandom[sideNailIndex]], sideNailPositions[sideNailRandom[sideNailIndex]].Y - NAIL_DRAW_SCALE * nailTexture.Width);
                }
            }
            //check for nail hits
            lastNailHitTime += dt;
            if (Math.Abs(angularVelocity) > 3f && lastNailHitTime > 0.3f && !(nailIndex == NAIL_COUNT - 1 && nailProgress[nailRandom[nailIndex]] >= NAIL_HIT_THRESHOLD))
            {
                //to the left of the nail and swinging CW
                if (anchor.X < nailBounds.X && angularVelocity > 0)
                {
                    if (headCircleRight.CollidesWith(nailBounds))
                    {
                        NailHit();
                    }
                }
                //to the right of the nail and swinging CCW
                if (anchor.X > nailBounds.X + nailBounds.Width && angularVelocity < 0)
                {
                    if (headCircleLeft.CollidesWith(nailBounds))
                    {
                        NailHit();
                    }
                }
                //to the left of the nail and swinging CCW
                if (anchor.X < nailBounds.X && angularVelocity < 0)
                {
                    if (headCircleLeft.CollidesWith(nailBounds))
                    {
                        NailBadHit();
                    }
                }
                //to the right of the nail and swinging CW
                if (anchor.X > nailBounds.X + nailBounds.Width && angularVelocity > 0)
                {
                    if (headCircleRight.CollidesWith(nailBounds))
                    {
                        NailBadHit();
                    }
                }
            }
            //check for side nail hits
            lastSideNailHitTime += dt;
            if (Math.Abs(angularVelocity) > 3f && lastSideNailHitTime > 0.3f && !(sideNailIndex == NAIL_COUNT - 1 && sideNailProgress[sideNailRandom[sideNailIndex]] >= NAIL_HIT_THRESHOLD))
            {
                //above nail and swinging CW
                if (anchor.Y < nailBounds2.Y && angularVelocity > 0)
                {
                    if (headCircleRight.CollidesWith(nailBounds2))
                    {
                        if (sideNailRandom[sideNailIndex] <= 1) SideNailHit(); else SideNailBadHit();
                    }
                }
                //below nail and swinging CCW
                if (anchor.Y > nailBounds2.Y + nailBounds2.Height && angularVelocity < 0)
                {
                    if (headCircleLeft.CollidesWith(nailBounds2))
                    {
                        if (sideNailRandom[sideNailIndex] <= 1) SideNailHit(); else SideNailBadHit();
                    }
                }
                //above and swinging CCW
                if (anchor.Y < nailBounds2.Y && angularVelocity < 0)
                {
                    if (headCircleLeft.CollidesWith(nailBounds2))
                    {
                        if (sideNailRandom[sideNailIndex] <= 1) SideNailBadHit(); else SideNailHit();
                    }
                }
                //below nail and swinging CW
                if (anchor.Y > nailBounds2.Y + nailBounds2.Height && angularVelocity > 0)
                {
                    if (headCircleRight.CollidesWith(nailBounds2))
                    {
                        if (sideNailRandom[sideNailIndex] <= 1) SideNailBadHit(); else SideNailHit();
                    }
                }
            }
            if ((sideNailIndex == NAIL_COUNT - 1 && sideNailProgress[sideNailRandom[sideNailIndex]] >= NAIL_HIT_THRESHOLD) && (nailIndex == NAIL_COUNT - 1 && nailProgress[nailRandom[nailIndex]] >= NAIL_HIT_THRESHOLD))
            {
                _score = _scoreTimer.ReturnScore();
                gameOverFlag = true;
            }
        }


        if (screenShakeFlag)
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
                screenShakeFlag = false;
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
            if (_scoreTracker != null) _scoreTracker.ForestPoints = _score;
            LOAF.ChangeScene(new TitleScene(LOAF));
            return;
        }

        _scoreTimer.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.DarkGreen);
        var LOAF = Game as LOAF;
        // combine scale and screen-shake translation into the view matrix
        Matrix viewMatrix = Matrix.CreateScale(LOAF.GameScale) * Matrix.CreateTranslation(shakeOffset.X, shakeOffset.Y, 0f);
        _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

        Rectangle sourceRect = new Rectangle(0, 0, FRAME_WIDTH, FRAME_HEIGHT);
        switch (hammerFrame)
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
        //draw the score timer
        _scoreTimer.Draw(_spriteBatch);
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
        //draw the downward nail
        for (int i = 0; i < NAIL_COUNT; i++)
        {
            if (i == nailRandom[nailIndex] || nailProgress[i] > 0)
            {
                _spriteBatch.Draw(
                    nailTexture,
                    new Vector2(
                        nailPositions[i].X,
                        nailPositions[i].Y + nailProgress[i]
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
        }
        //draw the sideward nail left
        for (int i = 0; i < SIDE_NAIL_COUNT-2; i++)
        {
            if (i == sideNailRandom[sideNailIndex] || sideNailProgress[i] > 0)
            {
                _spriteBatch.Draw(
                    nailTexture,
                    new Vector2(
                        sideNailPositions[i].X - sideNailProgress[i],
                        sideNailPositions[i].Y
                    ),
                    null,
                    Color.White,
                    SIDE_NAIL_ROTATIONS[i],
                    Vector2.Zero,
                    NAIL_DRAW_SCALE,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        //draw the sideward nail right
        for (int i = 2; i < SIDE_NAIL_COUNT; i++)
        {
            if (i == sideNailRandom[sideNailIndex] || sideNailProgress[i] > 0)
            {
                _spriteBatch.Draw(
                    nailTexture,
                    new Vector2(
                        sideNailPositions[i].X + sideNailProgress[i],
                        sideNailPositions[i].Y
                    ),
                    null,
                    Color.White,
                    SIDE_NAIL_ROTATIONS[i],
                    Vector2.Zero,
                    NAIL_DRAW_SCALE,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        //draw the planks
        for (int i = 0; i < BOARD_COUNT; i++)
        {
            _spriteBatch.Draw(
                woodTexture,
                boardPositions[i],
                null,
                Color.White,
                BOARD_ROTATIONS[i],
                Vector2.Zero,
                BOARD_DRAW_SCALE,
                SpriteEffects.None,
                0f
            );
        }
        float fontScale = 1f;
        _spriteBatch.DrawString(font, "Mouse Buttons to Rotate, Spacebar: DEBUG", new Vector2(vw * 0.6f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        _spriteBatch.DrawString(font, "ESC to return", new Vector2(vw * 0.1f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

        if (gameOverFlag)
        {
            string doneString = "All nails hammered! Well done!";
            Vector2 lineSize = font.MeasureString(doneString);
            Vector2 linePos = new Vector2(centerX - lineSize.X / 2f, vh/2f);
            _spriteBatch.DrawString(font, doneString, linePos, Color.Yellow, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
        }

        if (debugFlag)
        {
            Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, headCircleLeft.Center, headCircleLeft.Radius, Color.Red);
            Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, headCircleRight.Center, headCircleLeft.Radius, Color.Red);
            Debug.DrawRectangleOutline(_spriteBatch, Game.GraphicsDevice, nailBounds, Color.Cyan);
            Debug.DrawRectangleOutline(_spriteBatch, Game.GraphicsDevice, nailBounds2, Color.Cyan);
            Debug.DrawPoint(_spriteBatch, Game.GraphicsDevice, anchor, Color.Orange);

            _spriteBatch.DrawString(font, "CW: " + revolutionsCW.ToString(), new Vector2(vw * 0.25f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            _spriteBatch.DrawString(font, "CCW: " + revolutionsCCW.ToString(), new Vector2(vw * 0.35f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            _spriteBatch.DrawString(font, "Speed: " + ((int)angularVelocity).ToString(), new Vector2(vw * 0.45f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
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

    private static int[] CreateRandomlySortedArray(int start, int end)
    {
        // Calculate the count of numbers in the range
        int count = end - start + 1;

        // Generate a sequence of numbers within the range, then shuffle them randomly
        // and convert to an array.
        int[] randomlySortedArray = Enumerable.Range(start, count)
                                            .OrderBy(i => Guid.NewGuid())
                                            .ToArray();

        return randomlySortedArray;
    }

    private void NailHit()
    {
        lastNailHitTime = 0f;
        hammerHit.Play(1f, Math.Min(1f - Math.Abs(angularVelocity) / 12, 1f), 0f);
        nailProgress[nailRandom[nailIndex]] += (int)Math.Abs(angularVelocity) - 2;
        if (nailProgress[nailRandom[nailIndex]] >= NAIL_HIT_THRESHOLD)
        {
            //move to next nail
            nailIndex++;
            nailIndex = Math.Min(nailIndex, NAIL_COUNT - 1);
            screenShakeFlag = true;
        }
    }

    private void NailBadHit()
    {
        lastNailHitTime = 0f;
        hammerHit.Play(1f, Math.Min(1f - Math.Abs(angularVelocity) / 12, 1f), 0f);
        nailProgress[nailRandom[nailIndex]] -= (int)Math.Abs(angularVelocity) - 3;
    }

    private void SideNailHit()
    {
        lastSideNailHitTime = 0f;
        hammerHit.Play(1f, Math.Min(1f - Math.Abs(angularVelocity) / 12, 1f), 0f);
        sideNailProgress[sideNailRandom[sideNailIndex]] += (int)Math.Abs(angularVelocity) - 2;
        if (sideNailProgress[sideNailRandom[sideNailIndex]] >= NAIL_HIT_THRESHOLD)
        {
            //move to next nail
            sideNailIndex++;
            sideNailIndex = Math.Min(sideNailIndex, SIDE_NAIL_COUNT - 1);
            screenShakeFlag = true;
        }
    }

    private void SideNailBadHit()
    {
        lastSideNailHitTime = 0f;
        hammerHit.Play(1f, Math.Min(1f - Math.Abs(angularVelocity) / 12, 1f), 0f);
        sideNailProgress[sideNailRandom[sideNailIndex]] -= (int)Math.Abs(angularVelocity) - 3;
    }
}