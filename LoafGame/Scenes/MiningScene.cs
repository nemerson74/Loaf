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
    private bool debugFlag = false;
    Random random = new Random();
    Vector2 shakeOffset = Vector2.Zero;

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
    private Vector2[] rockPositions = new Vector2[ROCK_COUNT];
    private Vector2[] orePositions = new Vector2[ORE_COUNT];
    private BoundingCircle[] rockCollisions = new BoundingCircle[ROCK_COUNT];
    private BoundingCircle[] oreCollisions = new BoundingCircle[ORE_COUNT];

    // --- constants ---
    private const float SCREEN_SHAKE_DURATION = 0.5f;
    private const int ROCK_COUNT = 21;
    private const int ORE_COUNT = 3;
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
        fireballs = new FireballParticleSystem(LOAF, this, "fireballnormalgabe") { Emitting = false};
        LOAF.Components.Add(fireballs);

        fireballred = new FireballParticleSystem(LOAF, this, "fireballred") { Emitting = false };
        LOAF.Components.Add(fireballred);

        vw = Game.GraphicsDevice.Viewport.Width / LOAF.GameScale;
        vh = Game.GraphicsDevice.Viewport.Height / LOAF.GameScale;
        leftMargin = vw * 0.02f;
        centerX = vw / 2;

        // constrain spawn ranges to the right side and within vertical bounds
        float minX = vw * 0.6f;
        float minx2 = vw * 0.7f;
        float maxX = vw * 0.95f;
        float minY1 = vh * 0.1f;
        float maxY1 = vh * 0.3f;
        float minY2 = vh * 0.3f;
        float maxY2 = vh * 0.6f;
        float minY3 = vh * 0.6f;
        float maxY3 = vh * 0.9f;

        // rocks: fill all indices without gaps
        int rocksPerBand = ROCK_COUNT / 3;
        int rockRemainder = ROCK_COUNT - rocksPerBand * 3;
        int rIndex = 0;
        for (int i = 0; i < rocksPerBand + (rockRemainder > 0 ? 1 : 0); i++, rIndex++)
        {
            float xPos = (float)(minX + random.NextDouble() * (maxX - minX));
            float yPos = (float)(minY1 + random.NextDouble() * (maxY1 - minY1));
            rockPositions[rIndex] = new Vector2(xPos, yPos);
        }
        for (int i = 0; i < rocksPerBand + (rockRemainder > 1 ? 1 : 0); i++, rIndex++)
        {
            float xPos = (float)(minX + random.NextDouble() * (maxX - minX));
            float yPos = (float)(minY2 + random.NextDouble() * (maxY2 - minY2));
            rockPositions[rIndex] = new Vector2(xPos, yPos);
        }
        for (int i = 0; i < rocksPerBand; i++, rIndex++)
        {
            float xPos = (float)(minX + random.NextDouble() * (maxX - minX));
            float yPos = (float)(minY3 + random.NextDouble() * (maxY3 - minY3));
            rockPositions[rIndex] = new Vector2(xPos, yPos);
        }

        // ores: fill all indices without gaps
        int oresPerBand = ORE_COUNT / 3;
        int oreRemainder = ORE_COUNT - oresPerBand * 3;
        int oIndex = 0;
        for (int i = 0; i < oresPerBand + (oreRemainder > 0 ? 1 : 0); i++, oIndex++)
        {
            float xPos = (float)(minx2 + random.NextDouble() * (maxX - minx2));
            float yPos = (float)(minY1 + random.NextDouble() * (maxY1 - minY1));
            orePositions[oIndex] = new Vector2(xPos, yPos);
        }
        for (int i = 0; i < oresPerBand + (oreRemainder > 1 ? 1 : 0); i++, oIndex++)
        {
            float xPos = (float)(minx2 + random.NextDouble() * (maxX - minx2));
            float yPos = (float)(minY2 + random.NextDouble() * (maxY2 - minY2));
            orePositions[oIndex] = new Vector2(xPos, yPos);
        }
        for (int i = 0; i < oresPerBand; i++, oIndex++)
        {
            float xPos = (float)(minx2 + random.NextDouble() * (maxX - minx2));
            float yPos = (float)(minY3 + random.NextDouble() * (maxY3 - minY3));
            orePositions[oIndex] = new Vector2(xPos, yPos);
        }

        for (int i = 0; i < ROCK_COUNT; i++)
        {
            rockCollisions[i] = new BoundingCircle(rockPositions[i], 0f);
        }
        for (int i = 0; i < ORE_COUNT; i++)
        {
            oreCollisions[i] = new BoundingCircle(orePositions[i], 0f);
        }

        MediaPlayer.Stop();
        MediaPlayer.Volume = 0.5f;
        MediaPlayer.Play(LOAF.backgroundMusicMinigame);
        MediaPlayer.IsRepeating = true;

        base.Initialize();
    }

    public override void LoadContent()
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        pickaxe.LoadContent(LOAF.Content, LOAF.GameScale);
        rockTexture = LOAF.Content.Load<Texture2D>("Mining/Rock");
        oreTexture = LOAF.Content.Load<Texture2D>("Mining/Ore");

        // collisions should be centered on drawn sprites; drawing uses top-left origin with scale
        float rockRadius = (rockTexture.Width * ROCK_DRAW_SCALE) / 2f;
        float oreRadius = (oreTexture.Width * ROCK_DRAW_SCALE) / 2f;
        Vector2 rockCenterOffset = new Vector2(rockTexture.Width, rockTexture.Height) * (ROCK_DRAW_SCALE / 2f);
        Vector2 oreCenterOffset = new Vector2(oreTexture.Width, oreTexture.Height) * (ROCK_DRAW_SCALE / 2f);

        for (int i = 0; i < ROCK_COUNT; i++)
        {
            rockCollisions[i] = new BoundingCircle(rockPositions[i] + rockCenterOffset, rockRadius*.8f);
        }
        for (int i = 0; i < ORE_COUNT; i++)
        {
            oreCollisions[i] = new BoundingCircle(orePositions[i] + oreCenterOffset, oreRadius*.8f);
        }
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
        if (oreMined < 3 && lastHitTime > 0.7f && Math.Abs(pickaxe.AngularVelocity) > 3f)
        {
            for (int i = 0; i < ROCK_COUNT; i++)
            {
                if (pickaxe.leftBoundingCircle.CollidesWith(rockCollisions[i]) || pickaxe.rightBoundingCircle.CollidesWith(rockCollisions[i]))
                {
                    screenShakeflag = true;
                    pickaxe.PlayHitSound();
                    pickaxe.Rebound();
                    //remove rock from scene by moving offscreen
                    rockCollisions[i].Center = new Vector2(-1000f, -1000f);
                    rockPositions[i] = new Vector2(-1000f, -1000f);
                    lastHitTime = 0f;
                }
            }
        }
        //Detect collisions with ores
        if (oreMined < 3 && lastHitTime > 0.7f && Math.Abs(pickaxe.AngularVelocity) > 3f )
        {
            for (int i = 0; i < ORE_COUNT; i++)
            {
                if (pickaxe.leftBoundingCircle.CollidesWith(oreCollisions[i]) || pickaxe.rightBoundingCircle.CollidesWith(oreCollisions[i]))
                {
                    screenShakeflag = true;
                    pickaxe.PlayHitSound();
                    pickaxe.Rebound();
                    oreMined++;
                    //remove ore from scene by moving offscreen
                    oreCollisions[i].Center = new Vector2(-1000f, -1000f);
                    orePositions[i] = new Vector2(-1000f, -1000f);
                    lastHitTime = 0f;
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
        var LOAF = Game as LOAF;
        // combine scale and screen-shake translation into the view matrix
        Matrix viewMatrix = Matrix.CreateScale(LOAF.GameScale) * Matrix.CreateTranslation(shakeOffset.X, shakeOffset.Y, 0f);
        _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

        pickaxe.Draw(gameTime, _spriteBatch);
        //draw rocks
        for (int i = 0; i < ROCK_COUNT; i++)
        {
            _spriteBatch.Draw(
                rockTexture,
                rockPositions[i],
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
        for (int i = 0; i < ORE_COUNT; i++)
        {
            _spriteBatch.Draw(
                oreTexture,
                orePositions[i],
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

            foreach (var rockCollision in rockCollisions)
            {
                Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, rockCollision.Center, rockCollision.Radius, Color.Blue);
            }
            foreach (var oreCollision in oreCollisions)
            {
                Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, oreCollision.Center, oreCollision.Radius, Color.Green);
            }
        }

        if (oreMined == 3)
        {
            _spriteBatch.DrawString(font, "All ore mined! Well done!", new Vector2(50, 100), Color.Lime, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
            _score = 3;
        }

        _spriteBatch.End();
    }
}