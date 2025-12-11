using LoafGame.Collisions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpDX.Direct2D1.Effects;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;

namespace LoafGame.Scenes;

public class CactusScene : Scene, IParticleEmitter
{
    /// <summary>
    /// Position of the particle emitter in world space.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Velocity of the emitted particles.
    /// </summary>
    public Vector2 Velocity { get; set; }

    struct SlidingItem
    {
        public Texture2D Texture;
        public BoundingCircle HitCircle;
        public Vector2 Position;
        public float Speed;
        public float Scale;
        public float Rotation;
        public bool IsFruit;
    }

    private List<SlidingItem> slidingItems = new();
    private float _spawnTimer = 0f;
    private const float SpawnIntervalMin = 0.6f;
    private const float SpawnIntervalMax = 1.6f;
    private float _nextSpawnIn = 1.0f;
    private int _score = 0;
    private ScoreTracker _scoreTracker;
    private ScoreTimer _scoreTimer;
    private bool debugFlag = false;
    Random random = new Random();
    Vector2 shakeOffset = Vector2.Zero;
    private float _gameScale;
    private bool gameEndFlag = false;
    private int _fruitsGathered = 0;

    private float vw;
    private float vh;
    private float leftMargin;
    private float centerX;

    private SpriteBatch _spriteBatch;
    private Texture2D _smallCactusTexture, _mediumCactusTexture, _bigCactusTexture, _bush1Texture, _bush2Texture, _fruitTexture;


    private bool screenShakeflag = false;
    private float screenShakeTimer = 0f;

    private Rotator hand = new Rotator() 
    { 
        TextureName = "Cactus/Hand",
        MultiFrame = false,
        CursorOrigin = new Vector2(7f, 47f),
        Gravity = 0,
        ClickTorque = 200000f,
        Whoosh = false,
        HasTwoBoundingCircles = false,
        CollisionRadius = 8f * 5f,
        LeftCollisionOrigin = new Vector2(7.5f, 10f),
        Fixed = true
    };

    // --- constants ---
    private const float SCREEN_SHAKE_DURATION = 0.5f;
    private const int FRUIT_NEEDED_COUNT = 15;

    // revolution tracking
    private FireballParticleSystem fireballred;
    private FireballParticleSystem fireballs;

    public CactusScene(Game game, ScoreTracker scoreTracker = null) : base(game) 
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

        hand.Anchor = new Vector2(centerX * 0.5f, vh / 2f);

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
        hand.LoadContent(LOAF.Content, _gameScale);
        //rockTexture = LOAF.Content.Load<Texture2D>("Mining/Rock");
        _smallCactusTexture = LOAF.Content.Load<Texture2D>("Cactus/CactusSmall");
        _mediumCactusTexture = LOAF.Content.Load<Texture2D>("Cactus/CactusMedium");
        _bigCactusTexture = LOAF.Content.Load<Texture2D>("Cactus/CactusBigRight");
        _bush1Texture = LOAF.Content.Load<Texture2D>("Cactus/DeadBush1");
        _bush2Texture = LOAF.Content.Load<Texture2D>("Cactus/DeadBush2");
        _fruitTexture = LOAF.Content.Load<Texture2D>("Cactus/CactusFruit");

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

        hand.Update(gameTime, input);

        // spawn sliding items
        _spawnTimer += dt;
        if (_spawnTimer >= _nextSpawnIn)
        {
            _spawnTimer = 0f;
            _nextSpawnIn = MathHelper.Lerp(SpawnIntervalMin, SpawnIntervalMax, (float)random.NextDouble());
            SpawnSlidingItem();
        }

        // move items left and handle off-screen removal
        var handCircle = hand.leftBoundingCircle;
        for (int i = slidingItems.Count - 1; i >= 0; i--)
        {
            var item = slidingItems[i];
            item.Position.X -= item.Speed * dt;
            item.HitCircle.Center = new Vector2(item.HitCircle.Center.X - item.Speed * dt, item.HitCircle.Center.Y);

            // collision with hand circle
            if (CollisionHelper.Collides(item.HitCircle, handCircle))
            {
                if (item.IsFruit)
                {
                    _fruitsGathered = Math.Min(_fruitsGathered + 1, FRUIT_NEEDED_COUNT);
                    LOAF.ButtonClickSound.Play();
                }
                else
                {
                    _fruitsGathered = Math.Max(0, _fruitsGathered - 5);
                    screenShakeflag = true;
                    screenShakeTimer = 0f;
                }
                slidingItems.RemoveAt(i);
                continue;
            }

            // remove if off-screen to the left
            if (item.Position.X + item.Texture.Width * item.Scale < 0)
            {
                slidingItems.RemoveAt(i);
                continue;
            }

            slidingItems[i] = item; // write back
        }

        if (!gameEndFlag)
        {
            if (_fruitsGathered >= FRUIT_NEEDED_COUNT)
            {
                _score = _scoreTimer.ReturnScore();
                gameEndFlag = true;
            }
        }

        //turn off emitter depending on speed
        if (MathF.Abs(hand.AngularVelocity) < 8f)
        {
            fireballred.Emitting = false;
            fireballs.Emitting = false;
        }
        else
        {
            if (MathF.Abs(hand.AngularVelocity) < 10f)
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
            float shakeMagnitude = 3f;
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
            if (_scoreTracker != null) _scoreTracker.DesertPoints = _score;
            LOAF.ChangeScene(new TitleScene(LOAF));
            return;
        }
        _scoreTimer.Update(gameTime);
    }

    private void SpawnSlidingItem()
    {
        // choose item type: more fruits than cacti
        bool spawnFruit = random.NextDouble() < 0.8; // 80% fruit
        Texture2D tex;
        bool isFruit;
        float scale = 3f;
        float boundingRadius;
        Vector2 offset = Vector2.Zero;
        if (spawnFruit)
        {
            tex = _fruitTexture;
            isFruit = true;
            scale = 2f;
            boundingRadius = Math.Max(tex.Width, tex.Height) * 0.5f * scale;
            
        }
        else
        {
            // choose a random cactus/bush
            int pick = random.Next(4);
            switch (pick)
            {
                case 0: tex = _smallCactusTexture; boundingRadius = Math.Max(tex.Width, tex.Height) * 0.4f * scale; offset.Y = 5f * scale;
                    break;
                case 1: tex = _mediumCactusTexture; boundingRadius = Math.Max(tex.Width, tex.Height) * 0.5f * scale;
                    break;
                //case 2: tex = _bigCactusTexture; break;
                //default: tex = (random.Next(2) == 0) ? _bush1Texture : _bush2Texture; break;
                default: tex = _smallCactusTexture; boundingRadius = Math.Max(tex.Width, tex.Height) * 0.4f * scale; offset.Y = 5f * scale; break;
            }
            isFruit = false;
            scale = 5f;
        }
        float speed = MathHelper.Lerp(180f, 220f, (float)random.NextDouble());
        float y = MathHelper.Lerp(vh * 0.2f, vh * 0.85f, (float)random.NextDouble());
        var position = new Vector2(vw + 40f, y);
        var circle = new BoundingCircle(new Vector2(position.X + tex.Width * 0.5f * scale, position.Y + tex.Height * 0.5f * scale) + offset, boundingRadius);

        slidingItems.Add(new SlidingItem
        {
            Texture = tex,
            HitCircle = circle,
            Position = position,
            Speed = speed,
            Scale = scale,
            Rotation = 0f,
            IsFruit = isFruit
        });
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.Tan);
        // combine scale and screen-shake translation into the view matrix
        Matrix viewMatrix = Matrix.CreateScale(_gameScale) * Matrix.CreateTranslation(shakeOffset.X, shakeOffset.Y, 0f);
        _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

        // draw sliding items
        foreach (var item in slidingItems)
        {
            _spriteBatch.Draw(item.Texture, item.Position, null, Color.White, item.Rotation, Vector2.Zero, item.Scale, SpriteEffects.None, 0f);
        }

        hand.Draw(gameTime, _spriteBatch);
        _scoreTimer.Draw(_spriteBatch);
        //draw rocks
        SpriteFont font = Content.Load<SpriteFont>("vergilia");
        float fontScale = 1f;

        _spriteBatch.DrawString(font, "Mouse Buttons to Rotate, Spacebar: DEBUG", new Vector2(vw * 0.7f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        _spriteBatch.DrawString(font, "ESC to return", new Vector2(vw * 0.02f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        _spriteBatch.DrawString(font, "Fruits: " + _fruitsGathered.ToString() + "/" + FRUIT_NEEDED_COUNT, new Vector2(vw * 0.22f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale * 2, SpriteEffects.None, 0f);

        if (debugFlag)
        {
            Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, hand.leftBoundingCircle.Center, hand.leftBoundingCircle.Radius, Color.Red);
            if (hand.HasTwoBoundingCircles)
                Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, hand.rightBoundingCircle.Center, hand.rightBoundingCircle.Radius, Color.Red);
            Debug.DrawPoint(_spriteBatch, Game.GraphicsDevice, hand.Anchor, Color.Orange);

            _spriteBatch.DrawString(font, "CCW: " + hand.revolutionsCCW.ToString(), new Vector2(vw * 0.32f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            _spriteBatch.DrawString(font, "Speed: " + ((int)hand.AngularVelocity).ToString(), new Vector2(vw * 0.42f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            foreach (var item in slidingItems)
            {
                Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, item.HitCircle.Center, item.HitCircle.Radius, item.IsFruit ? Color.Green : Color.Red);
                Debug.DrawPoint(_spriteBatch, Game.GraphicsDevice, item.Position, Color.Cyan);
            }
        }

        if (gameEndFlag)
        {
            string doneString = "All fruit gathered! Well done!";
            Vector2 lineSize = font.MeasureString(doneString);
            Vector2 linePos = new Vector2(centerX - lineSize.X / 2f, vh / 2f);
            _spriteBatch.DrawString(font, doneString, linePos, Color.Yellow, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
        }

        _spriteBatch.End();
    }
}