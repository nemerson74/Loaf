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

public class MiningScene : Scene, IParticleEmitter
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
    private float _gameScale;

    private float vw;
    private float vh;
    private float leftMargin;
    private float centerX;

    private SpriteBatch _spriteBatch;
    private Texture2D rockTexture, oreTexture;
    private float lastHitTime = 0;


    private bool screenShakeflag = false;
    private float screenShakeTimer = 0f;

    private Rotator pickaxe = new Rotator(){ TextureName = "Mining/pickaxe" };

    private int oreMined = 0;
    private Vector2[] rockPositionsRight = new Vector2[ROCK_COUNT/2];
    private Vector2[] orePositionsRight = new Vector2[ORE_COUNT/2];
    private BoundingCircle[] rockCollisionsRight = new BoundingCircle[ROCK_COUNT/2];
    private BoundingCircle[] oreCollisionsRight = new BoundingCircle[ORE_COUNT/2];
    private Vector2[] rockPositionsLeft = new Vector2[ROCK_COUNT/2];
    private Vector2[] orePositionsLeft = new Vector2[ORE_COUNT/2];
    private BoundingCircle[] rockCollisionsLeft = new BoundingCircle[ROCK_COUNT/2];
    private BoundingCircle[] oreCollisionsLeft = new BoundingCircle[ORE_COUNT/2];

    // --- constants ---
    private const float SCREEN_SHAKE_DURATION = 0.5f;
    private const int ROCK_COUNT = 42;
    private const int ORE_COUNT = 6;
    private const float ROCK_DRAW_SCALE = 5f;

    // two head circles for hammer

    // revolution tracking
    private FireballParticleSystem fireballred;
    private FireballParticleSystem fireballs;

    public MiningScene(Game game, ScoreTracker scoreTracker = null) : base(game) 
    {
        _scoreTracker = scoreTracker;
    }

    public override void Initialize()
    {

        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        _gameScale = LOAF.GameScale;
        fireballs = new FireballParticleSystem(LOAF, this, "fireballnormalgabe") { Emitting = false};
        LOAF.Components.Add(fireballs);

        fireballred = new FireballParticleSystem(LOAF, this, "fireballred") { Emitting = false };
        LOAF.Components.Add(fireballred);

        vw = Game.GraphicsDevice.Viewport.Width / _gameScale;
        vh = Game.GraphicsDevice.Viewport.Height / _gameScale;
        leftMargin = vw * 0.02f;
        centerX = vw / 2;

        // constrain spawn ranges to the right side and within vertical bounds
        // right side ranges
        float minXRight = vw * 0.55f;
        float minXRightOre = vw * 0.65f;
        float maxXRight = vw * 0.95f;
        // left side ranges
        float maxXLeft = vw * 0.35f;
        float minXLeft = vw * 0.00f;
        float minXLeftOre = vw * 0.00f;
        float minY1 = vh * 0.1f;
        float maxY1 = vh * 0.3f;
        float minY2 = vh * 0.3f;
        float maxY2 = vh * 0.6f;
        float minY3 = vh * 0.6f;
        float maxY3 = vh * 0.9f;

        // rocks: fill all indices without gaps
        // RIGHT SIDE ROCKS
        int rocksPerBandRight = (ROCK_COUNT/2) / 3;
        int rockRemainderRight = (ROCK_COUNT/2) - rocksPerBandRight * 3;
        int rIndex = 0;
        for (int i = 0; i < rocksPerBandRight + (rockRemainderRight > 0 ? 1 : 0); i++, rIndex++)
        {
            float xPos = (float)(minXRight + random.NextDouble() * (maxXRight - minXRight));
            float yPos = (float)(minY1 + random.NextDouble() * (maxY1 - minY1));
            rockPositionsRight[rIndex] = new Vector2(xPos, yPos);
        }
        for (int i = 0; i < rocksPerBandRight + (rockRemainderRight > 1 ? 1 : 0); i++, rIndex++)
        {
            float xPos = (float)(minXRight + random.NextDouble() * (maxXRight - minXRight));
            float yPos = (float)(minY2 + random.NextDouble() * (maxY2 - minY2));
            rockPositionsRight[rIndex] = new Vector2(xPos, yPos);
        }
        for (int i = 0; i < rocksPerBandRight; i++, rIndex++)
        {
            float xPos = (float)(minXRight + random.NextDouble() * (maxXRight - minXRight));
            float yPos = (float)(minY3 + random.NextDouble() * (maxY3 - minY3));
            rockPositionsRight[rIndex] = new Vector2(xPos, yPos);
        }

        // RIGHT SIDE ores: fill all indices without gaps
        int oresPerBandRight = (ORE_COUNT/2) / 3;
        int oreRemainderRight = (ORE_COUNT/2) - oresPerBandRight * 3;
        int oIndex = 0;
        for (int i = 0; i < oresPerBandRight + (oreRemainderRight > 0 ? 1 : 0); i++, oIndex++)
        {
            float xPos = (float)(minXRightOre + random.NextDouble() * (maxXRight - minXRightOre));
            float yPos = (float)(minY1 + random.NextDouble() * (maxY1 - minY1));
            orePositionsRight[oIndex] = new Vector2(xPos, yPos);
        }
        for (int i = 0; i < oresPerBandRight + (oreRemainderRight > 1 ? 1 : 0); i++, oIndex++)
        {
            float xPos = (float)(minXRightOre + random.NextDouble() * (maxXRight - minXRightOre));
            float yPos = (float)(minY2 + random.NextDouble() * (maxY2 - minY2));
            orePositionsRight[oIndex] = new Vector2(xPos, yPos);
        }
        for (int i = 0; i < oresPerBandRight; i++, oIndex++)
        {
            float xPos = (float)(minXRightOre + random.NextDouble() * (maxXRight - minXRightOre));
            float yPos = (float)(minY3 + random.NextDouble() * (maxY3 - minY3));
            orePositionsRight[oIndex] = new Vector2(xPos, yPos);
        }

        // LEFT SIDE ROCKS
        int rocksPerBandLeft = (ROCK_COUNT/2) / 3;
        int rockRemainderLeft = (ROCK_COUNT/2) - rocksPerBandLeft * 3;
        int rlIndex = 0;
        for (int i = 0; i < rocksPerBandLeft + (rockRemainderLeft > 0 ? 1 : 0); i++, rlIndex++)
        {
            float xPos = (float)(minXLeft + random.NextDouble() * (maxXLeft - minXLeft));
            float yPos = (float)(minY1 + random.NextDouble() * (maxY1 - minY1));
            rockPositionsLeft[rlIndex] = new Vector2(xPos, yPos);
        }
        for (int i = 0; i < rocksPerBandLeft + (rockRemainderLeft > 1 ? 1 : 0); i++, rlIndex++)
        {
            float xPos = (float)(minXLeft + random.NextDouble() * (maxXLeft - minXLeft));
            float yPos = (float)(minY2 + random.NextDouble() * (maxY2 - minY2));
            rockPositionsLeft[rlIndex] = new Vector2(xPos, yPos);
        }
        for (int i = 0; i < rocksPerBandLeft; i++, rlIndex++)
        {
            float xPos = (float)(minXLeft + random.NextDouble() * (maxXLeft - minXLeft));
            float yPos = (float)(minY3 + random.NextDouble() * (maxY3 - minY3));
            rockPositionsLeft[rlIndex] = new Vector2(xPos, yPos);
        }

        // LEFT SIDE ores
        int oresPerBandLeft = (ORE_COUNT/2) / 3;
        int oreRemainderLeft = (ORE_COUNT/2) - oresPerBandLeft * 3;
        int olIndex = 0;
        for (int i = 0; i < oresPerBandLeft + (oreRemainderLeft > 0 ? 1 : 0); i++, olIndex++)
        {
            float xPos = (float)(minXLeftOre + random.NextDouble() * (maxXLeft - minXLeftOre));
            float yPos = (float)(minY1 + random.NextDouble() * (maxY1 - minY1));
            orePositionsLeft[olIndex] = new Vector2(xPos, yPos);
        }
        for (int i = 0; i < oresPerBandLeft + (oreRemainderLeft > 1 ? 1 : 0); i++, olIndex++)
        {
            float xPos = (float)(minXLeftOre + random.NextDouble() * (maxXLeft - minXLeftOre));
            float yPos = (float)(minY2 + random.NextDouble() * (maxY2 - minY2));
            orePositionsLeft[olIndex] = new Vector2(xPos, yPos);
        }
        for (int i = 0; i < oresPerBandLeft; i++, olIndex++)
        {
            float xPos = (float)(minXLeftOre + random.NextDouble() * (maxXLeft - minXLeftOre));
            float yPos = (float)(minY3 + random.NextDouble() * (maxY3 - minY3));
            orePositionsLeft[olIndex] = new Vector2(xPos, yPos);
        }

        for (int i = 0; i < ROCK_COUNT/2; i++)
        {
            rockCollisionsRight[i] = new BoundingCircle(rockPositionsRight[i], 0f);
            rockCollisionsLeft[i] = new BoundingCircle(rockPositionsLeft[i], 0f);
        }
        for (int i = 0; i < ORE_COUNT/2; i++)
        {
            oreCollisionsRight[i] = new BoundingCircle(orePositionsRight[i], 0f);
            oreCollisionsLeft[i] = new BoundingCircle(orePositionsLeft[i], 0f);
        }

        MediaPlayer.Stop();
        MediaPlayer.Volume = 0.5f;
        MediaPlayer.Play(LOAF.backgroundMusicMinigame);
        MediaPlayer.IsRepeating = true;

        _scoreTimer = new ScoreTimer(Enums.GameType.Mining);

        base.Initialize();
    }

    public override void LoadContent()
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        pickaxe.LoadContent(LOAF.Content, _gameScale);
        rockTexture = LOAF.Content.Load<Texture2D>("Mining/Rock");
        oreTexture = LOAF.Content.Load<Texture2D>("Mining/Ore");

        // collisions should be centered on drawn sprites; drawing uses top-left origin with scale
        float rockRadius = (rockTexture.Width * ROCK_DRAW_SCALE) / 2f;
        float oreRadius = (oreTexture.Width * ROCK_DRAW_SCALE) / 2f;
        Vector2 rockCenterOffset = new Vector2(rockTexture.Width, rockTexture.Height) * (ROCK_DRAW_SCALE / 2f);
        Vector2 oreCenterOffset = new Vector2(oreTexture.Width, oreTexture.Height) * (ROCK_DRAW_SCALE / 2f);

        for (int i = 0; i < ROCK_COUNT/2; i++)
        {
            rockCollisionsRight[i] = new BoundingCircle(rockPositionsRight[i] + rockCenterOffset, rockRadius*.8f);
            rockCollisionsLeft[i] = new BoundingCircle(rockPositionsLeft[i] + rockCenterOffset, rockRadius*.8f);
        }
        for (int i = 0; i < ORE_COUNT/2; i++)
        {
            oreCollisionsRight[i] = new BoundingCircle(orePositionsRight[i] + oreCenterOffset, oreRadius*.8f);
            oreCollisionsLeft[i] = new BoundingCircle(orePositionsLeft[i] + oreCenterOffset, oreRadius*.8f);
        }

        _scoreTimer.LoadContent(LOAF.Content, _gameScale);

    }

    public override void Update(GameTime gameTime)
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        var input = LOAF.InputManager;
        debugFlag = input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space);

        //delta time
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (dt <= 0f) dt = 1f / 60f;

        pickaxe.Update(gameTime, input);

        //Detect collisions with rocks
        lastHitTime += dt;
        if (oreMined < ORE_COUNT && lastHitTime > 0.7f)
        {
            // choose side based on anchor
            bool rightSide = pickaxe.Anchor.X >= centerX;
            var rockArray = rightSide ? rockCollisionsRight : rockCollisionsLeft;
            var rockPosArray = rightSide ? rockPositionsRight : rockPositionsLeft;
            for (int i = 0; i < rockArray.Length; i++)
            {
                if (pickaxe.leftBoundingCircle.CollidesWith(rockArray[i]) || pickaxe.rightBoundingCircle.CollidesWith(rockArray[i]))
                {
                    if( Math.Abs(pickaxe.AngularVelocity) > 5f)
                    {
                        screenShakeflag = true;
                        pickaxe.PlayHitSound();
                        pickaxe.Rebound();
                        //remove rock from scene by moving offscreen
                        rockArray[i].Center = new Vector2(-1000f, -1000f);
                        rockPosArray[i] = new Vector2(-1000f, -1000f);
                        lastHitTime = 0f;

                    }
                    else
                    {
                        pickaxe.PlayHitSound();
                        pickaxe.Rebound();
                        lastHitTime = 0f;
                    }
                }
            }
        }
        //Detect collisions with ores
        if (oreMined < ORE_COUNT && lastHitTime > 0.7f )
        {
            bool rightSide = pickaxe.Anchor.X >= centerX;
            var oreArray = rightSide ? oreCollisionsRight : oreCollisionsLeft;
            var orePosArray = rightSide ? orePositionsRight : orePositionsLeft;
            for (int i = 0; i < oreArray.Length; i++)
            {
                if (pickaxe.leftBoundingCircle.CollidesWith(oreArray[i]) || pickaxe.rightBoundingCircle.CollidesWith(oreArray[i]))
                {
                    if (Math.Abs(pickaxe.AngularVelocity) > 5f)
                    {
                        screenShakeflag = true;
                        pickaxe.PlayHitSound();
                        pickaxe.Rebound();
                        oreMined++;
                        //remove ore from scene by moving offscreen
                        oreArray[i].Center = new Vector2(-1000f, -1000f);
                        orePosArray[i] = new Vector2(-1000f, -1000f);
                        lastHitTime = 0f;

                    }
                    else
                    {
                        pickaxe.PlayHitSound();
                        pickaxe.Rebound();
                        lastHitTime = 0f;
                    }

                }
            }
            
        }


        //turn off emitter depending on speed
        if (MathF.Abs(pickaxe.AngularVelocity) < 8f)
        {
            fireballred.Emitting = false;
            fireballs.Emitting = false;
        }
        else
        {
            if (MathF.Abs(pickaxe.AngularVelocity) < 10f)
            {
                fireballred.Emitting = false;
                //fireballs.Emitting = true;
            }
            else
            {
                //fireballred.Emitting = true;
                fireballs.Emitting = false;
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
            if (_scoreTracker != null) _scoreTracker.BadlandPoints = _score;
            LOAF.ChangeScene(new TitleScene(LOAF));
            return;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.DimGray);
        // combine scale and screen-shake translation into the view matrix
        Matrix viewMatrix = Matrix.CreateScale(_gameScale) * Matrix.CreateTranslation(shakeOffset.X, shakeOffset.Y, 0f);
        _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

        pickaxe.Draw(gameTime, _spriteBatch);
        //draw rocks
        for (int i = 0; i < ROCK_COUNT/2; i++)
        {
            _spriteBatch.Draw(
                rockTexture,
                rockPositionsRight[i],
                null,
                Color.White,
                0f,
                Vector2.Zero,
                ROCK_DRAW_SCALE,
                SpriteEffects.None,
                0f
            );
        }
        for (int i = 0; i < ROCK_COUNT/2; i++)
        {
            _spriteBatch.Draw(
                rockTexture,
                rockPositionsLeft[i],
                null,
                Color.White,
                0f,
                Vector2.Zero,
                ROCK_DRAW_SCALE,
                SpriteEffects.None,
                0f
            );
        }
        //draw ores
        for (int i = 0; i < ORE_COUNT/2; i++)
        {
            _spriteBatch.Draw(
                oreTexture,
                orePositionsRight[i],
                null,
                Color.White,
                0f,
                Vector2.Zero,
                ROCK_DRAW_SCALE,
                SpriteEffects.None,
                0f
            );
        }
        for (int i = 0; i < ORE_COUNT/2; i++)
        {
            _spriteBatch.Draw(
                oreTexture,
                orePositionsLeft[i],
                null,
                Color.White,
                0f,
                Vector2.Zero,
                ROCK_DRAW_SCALE,
                SpriteEffects.None,
                0f
            );
        }
        SpriteFont font = Content.Load<SpriteFont>("vergilia");
        float fontScale = 1f;

        _spriteBatch.DrawString(font, "Mouse Buttons to Rotate, Spacebar: DEBUG", new Vector2(vw * 0.7f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        _spriteBatch.DrawString(font, "ESC to return", new Vector2(vw * 0.02f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

        if (debugFlag)
        {
            Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, pickaxe.leftBoundingCircle.Center, pickaxe.leftBoundingCircle.Radius, Color.Red);
            Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, pickaxe.rightBoundingCircle.Center, pickaxe.rightBoundingCircle.Radius, Color.Red);
            Debug.DrawPoint(_spriteBatch, Game.GraphicsDevice, pickaxe.Anchor, Color.Orange);

            _spriteBatch.DrawString(font, "CW: " + pickaxe.revolutionsCW.ToString(), new Vector2(vw * 0.22f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            _spriteBatch.DrawString(font, "CCW: " + pickaxe.revolutionsCCW.ToString(), new Vector2(vw * 0.32f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            _spriteBatch.DrawString(font, "Speed: " + ((int)pickaxe.AngularVelocity).ToString(), new Vector2(vw * 0.42f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            foreach (var rockCollision in rockCollisionsRight)
            {
                Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, rockCollision.Center, rockCollision.Radius, Color.Blue);
            }
            foreach (var oreCollision in oreCollisionsRight)
            {
                Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, oreCollision.Center, oreCollision.Radius, Color.Green);
            }
            foreach (var rockCollision in rockCollisionsLeft)
            {
                Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, rockCollision.Center, rockCollision.Radius, Color.Blue);
            }
            foreach (var oreCollision in oreCollisionsLeft)
            {
                Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, oreCollision.Center, oreCollision.Radius, Color.Green);
            }
        }

        if (oreMined == ORE_COUNT)
        {
            _spriteBatch.DrawString(font, "All ore mined! Well done!", new Vector2(50, 100), Color.Lime, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
            _score = 3;
        }

        _spriteBatch.End();
    }
}