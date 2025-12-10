using LoafGame.Collisions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpDX.DXGI;
using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;

namespace LoafGame.Scenes;

public class WheatScene : Scene, IParticleEmitter
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
    private GraphicsDevice _device;
    private float lastHitTime = 0;

    private bool screenShakeflag = false;
    private float screenShakeTimer = 0f;

    private Rotator scythe = new Rotator() { TextureName = "Wheat/scythe", CursorOrigin = new Vector2(1f, 10f), MaxVelocities = new float[] { 0f, 0f, 0f }, Scale = 4f};
    private int prevPosition = 1;

    // --- constants ---
    private const float SCREEN_SHAKE_DURATION = 0.5f;
    private const float SCYTHE_POSITION_1 = -.35f * MathF.PI + 8 * MathF.PI / 8;
    private const float SCYTHE_POSITION_2 = .35f * MathF.PI + 8 * MathF.PI / 8;

    // revolution tracking
    private FireballParticleSystem fireballred;
    private FireballParticleSystem fireballs;

    //wheat
    private Texture2D _wheat;

    // Region slicing: 8x4 grid (32 regions)
    private const int GridCols = 8;
    private const int GridRows = 4;
    private Rectangle[] _wheatSources;      // source rectangles on the texture
    private Vector2[] _wheatDestPositions;  // where each region draws in world space
    private bool[] _regionRemoved;          // track removed regions
    private bool _leftClickLatch;           // for edge trigger
    private int[] _regionOrder;             // randomized order of regions to click
    private int _currentRegionIndex;        // index into _regionOrder
    private Texture2D _overlayPixel;        // for highlight overlay
    private const int OverlayOutlineThickness = 6; // thicker outline for highlight boxes
    private bool _timerStarted;              // start score timer on first click

    public WheatScene(Game game, ScoreTracker scoreTracker = null) : base(game)
    {
        _scoreTracker = scoreTracker;
    }

    public override void Initialize()
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        _device = LOAF.GraphicsDevice;
        _gameScale = LOAF.GameScale;
        fireballs = new FireballParticleSystem(LOAF, this, "fireballnormalgabe") { Emitting = false };
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

        _scoreTimer = new ScoreTimer(Enums.GameType.Wheat);

        base.Initialize();
    }

    public override void LoadContent()
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;

        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        scythe.LoadContent(LOAF.Content, _gameScale);
        _scoreTimer.LoadContent(LOAF.Content, _gameScale);

        _wheat = LOAF.Content.Load<Texture2D>("Wheat/wheatfield");

        BuildWheatRegions();

        scythe.CursorOrigin = new Vector2(1f, 10f);
        scythe.SetAngle(SCYTHE_POSITION_1);
        scythe.SetToolFrame(3);
        scythe.Color = Color.Red;
        prevPosition = 1;

        _overlayPixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        _overlayPixel.SetData(new[] { Color.White });
    }

    private void BuildWheatRegions()
    {
        _wheatSources = new Rectangle[GridCols * GridRows];
        _wheatDestPositions = new Vector2[GridCols * GridRows];
        _regionRemoved = new bool[GridCols * GridRows];
        _regionOrder = new int[GridCols * GridRows];
        _currentRegionIndex = 0;
        _timerStarted = false;

        int cellW = _wheat.Width / GridCols;
        int cellH = _wheat.Height / GridRows;

        for (int r = 0; r < GridRows; r++)
        {
            for (int c = 0; c < GridCols; c++)
            {
                int idx = r * GridCols + c;
                var src = new Rectangle(c * cellW, r * cellH, cellW, cellH);
                _wheatSources[idx] = src;

                // Draw destination is aligned at (0,0) matching the original texture’s top-left.
                // Since you use viewMatrix scaling via _gameScale, positions are in world units.
                _wheatDestPositions[idx] = new Vector2(src.X, src.Y);
                _regionRemoved[idx] = false;
            }
        }

        // create randomized order of indices [0..N)
        _regionOrder = CreateRandomlySortedArray(0, GridCols * GridRows - 1);
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

        bool leftDown = input.LeftMouseDown;
        if (leftDown && !_leftClickLatch)
        {
            _leftClickLatch = true;
            Vector2 worldMouse = input.Position / _gameScale;
            TryRemoveRegionAt(worldMouse);
        }
        else if (!leftDown)
        {
            _leftClickLatch = false;
        }

        scythe.Update(gameTime, input);

        if (!gameEndFlag)
        {
            lastHitTime += dt;
            if (_currentRegionIndex >= _regionOrder.Length)
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
            float currentMagnitude = shakeMagnitude * (1f - (screenShakeTimer / SCREEN_SHAKE_DURATION));
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
        if (_timerStarted)
        {
            _scoreTimer.Update(gameTime);
        }
    }

    private void TryRemoveRegionAt(Vector2 worldMouse)
    {
        // Enforce clicking the next required region
        if (_currentRegionIndex < _regionOrder.Length)
        {
            int targetIdx = _regionOrder[_currentRegionIndex];
            var src = _wheatSources[targetIdx];
            var rect = new Rectangle(src.X, src.Y, src.Width, src.Height);
            if (rect.Contains(worldMouse.ToPoint()))
            {
                _regionRemoved[targetIdx] = true;
                // start timer on first successful removal
                if (!_timerStarted)
                {
                    _timerStarted = true;
                }
                _currentRegionIndex++;
                //Switch tool position
                if (prevPosition == 1)
                {
                    scythe.CursorOrigin = new Vector2(14f, 10f);
                    scythe.SetAngle(SCYTHE_POSITION_2);
                    scythe.SetToolFrame(0);
                    scythe.Color = Color.Green;
                    scythe.PlayWhooshSound(1f, 1f, .8f);
                    prevPosition = 2;
                }
                else
                {
                    scythe.CursorOrigin = new Vector2(1f, 10f);
                    scythe.SetAngle(SCYTHE_POSITION_1);
                    scythe.SetToolFrame(3);
                    scythe.Color = Color.Red;
                    scythe.PlayWhooshSound(1f, 1f, -.8f);
                    prevPosition = 1;
                }
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.SandyBrown);
        Matrix viewMatrix = Matrix.CreateScale(_gameScale) * Matrix.CreateTranslation(shakeOffset.X, shakeOffset.Y, 0f);

        _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

        // Draw only remaining regions of the wheat texture
        if (_wheat != null && _wheatSources != null)
        {
            for (int i = 0; i < _wheatSources.Length; i++)
            {
                if (_regionRemoved[i]) continue;
                var src = _wheatSources[i];
                var destPos = _wheatDestPositions[i];
                _spriteBatch.Draw(_wheat, destPos, src, Color.White);
            }

            // Highlight next region to click (alternate colors by order parity)
            if (_currentRegionIndex < _regionOrder.Length && _overlayPixel != null)
            {
                int targetIdx = _regionOrder[_currentRegionIndex];
                var src = _wheatSources[targetIdx];
                // Alternate colors
                bool even = (_currentRegionIndex % 2) == 0;
                var highlightColor = even ? new Color(255, 0, 0, 60) : new Color(0, 255, 0, 60);
                DrawOutline(new Rectangle(src.X, src.Y, src.Width, src.Height), highlightColor, OverlayOutlineThickness);

                int afterIdxPos = _currentRegionIndex + 1;
                if (afterIdxPos < _regionOrder.Length)
                {
                    int afterIdx = _regionOrder[afterIdxPos];
                    var afterSrc = _wheatSources[afterIdx];
                    var afterColor = even ? new Color(0, 255, 0, 50) : new Color(255, 0, 0, 50);
                    DrawOutline(new Rectangle(afterSrc.X, afterSrc.Y, afterSrc.Width, afterSrc.Height), afterColor, OverlayOutlineThickness);
                }
            }
        }

        scythe.Draw(gameTime, _spriteBatch);
        _scoreTimer.Draw(_spriteBatch);

        // Load font once in LoadContent; guard if null
        SpriteFont font = null;
        try { font = Content.Load<SpriteFont>("vergilia"); } catch { /* handle */ }
        if (font != null)
        {
            _spriteBatch.DrawString(font, "Click the highlighted color, alternating each step.", new Vector2(vw * 0.45f, vh * 0.02f), Color.Yellow);
            _spriteBatch.DrawString(font, "ESC to return", new Vector2(vw * 0.02f, vh * 0.02f), Color.Yellow);
            if (debugFlag)
            {
                _spriteBatch.DrawString(font, $"Remaining: {_regionOrder.Length - _currentRegionIndex}", new Vector2(vw * 0.40f, vh * 0.02f), Color.Yellow);
                // Show next and after-next indices for debugging
                if (_currentRegionIndex < _regionOrder.Length)
                {
                    int idx = _regionOrder[_currentRegionIndex];
                    _spriteBatch.DrawString(font, $"Next idx: {idx}", new Vector2(vw * 0.40f, vh * 0.06f), Color.Yellow);
                }
                if (_currentRegionIndex + 1 < _regionOrder.Length)
                {
                    int next2 = _regionOrder[_currentRegionIndex + 1];
                    _spriteBatch.DrawString(font, $"After idx: {next2}", new Vector2(vw * 0.40f, vh * 0.10f), Color.Yellow);
                }
            }
        }

        _spriteBatch.End();
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

    private void DrawOutline(Rectangle rect, Color color, int thickness)
    {
        // top
        _spriteBatch.Draw(_overlayPixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // bottom
        _spriteBatch.Draw(_overlayPixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
        // left
        _spriteBatch.Draw(_overlayPixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // right
        _spriteBatch.Draw(_overlayPixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
    }

}