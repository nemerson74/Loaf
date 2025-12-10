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

    private int _score = 0;
    private ScoreTracker _scoreTracker;
    private ScoreTimer _scoreTimer;
    private bool debugFlag = false;
    Random random = new Random();
    Vector2 shakeOffset = Vector2.Zero;
    private float _gameScale;
    private bool gameEndFlag = false;

    private float vw;
    private float vh;
    private float leftMargin;
    private float centerX;

    private SpriteBatch _spriteBatch;
    private float lastHitTime = 0;


    private bool screenShakeflag = false;
    private float screenShakeTimer = 0f;

    private Rotator scythe = new Rotator(){ TextureName = "Mining/pickaxe" };

    // --- constants ---
    private const float SCREEN_SHAKE_DURATION = 0.5f;

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
        scythe.LoadContent(LOAF.Content, _gameScale);
        //rockTexture = LOAF.Content.Load<Texture2D>("Mining/Rock");

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

        scythe.Update(gameTime, input);

        if (!gameEndFlag)
        {
            lastHitTime += dt;
            if (lastHitTime > 0.6f)
            {
            }
            if (lastHitTime > 0.6f)
            {
 
            }
            if (false)
            {
                _score = _scoreTimer.ReturnScore();
                gameEndFlag = true;
            }
        }

        //turn off emitter depending on speed
        if (MathF.Abs(scythe.AngularVelocity) < 8f)
        {
            fireballred.Emitting = false;
            fireballs.Emitting = false;
        }
        else
        {
            if (MathF.Abs(scythe.AngularVelocity) < 10f)
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
        _scoreTimer.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.DimGray);
        // combine scale and screen-shake translation into the view matrix
        Matrix viewMatrix = Matrix.CreateScale(_gameScale) * Matrix.CreateTranslation(shakeOffset.X, shakeOffset.Y, 0f);
        _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

        scythe.Draw(gameTime, _spriteBatch);
        _scoreTimer.Draw(_spriteBatch);
        //draw rocks
        SpriteFont font = Content.Load<SpriteFont>("vergilia");
        float fontScale = 1f;

        _spriteBatch.DrawString(font, "Mouse Buttons to Rotate, Spacebar: DEBUG", new Vector2(vw * 0.7f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        _spriteBatch.DrawString(font, "ESC to return", new Vector2(vw * 0.02f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

        if (debugFlag)
        {
            Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, scythe.leftBoundingCircle.Center, scythe.leftBoundingCircle.Radius, Color.Red);
            Debug.DrawCircleOutline(_spriteBatch, Game.GraphicsDevice, scythe.rightBoundingCircle.Center, scythe.rightBoundingCircle.Radius, Color.Red);
            Debug.DrawPoint(_spriteBatch, Game.GraphicsDevice, scythe.Anchor, Color.Orange);

            _spriteBatch.DrawString(font, "CW: " + scythe.revolutionsCW.ToString(), new Vector2(vw * 0.22f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            _spriteBatch.DrawString(font, "CCW: " + scythe.revolutionsCCW.ToString(), new Vector2(vw * 0.32f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            _spriteBatch.DrawString(font, "Speed: " + ((int)scythe.AngularVelocity).ToString(), new Vector2(vw * 0.42f, vh * 0.02f), Color.Yellow, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        }

        if (gameEndFlag)
        {
            string doneString = "All ore mined! Well done!";
            Vector2 lineSize = font.MeasureString(doneString);
            Vector2 linePos = new Vector2(centerX - lineSize.X / 2f, vh / 2f);
            _spriteBatch.DrawString(font, doneString, linePos, Color.Yellow, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
        }

        _spriteBatch.End();
    }
}