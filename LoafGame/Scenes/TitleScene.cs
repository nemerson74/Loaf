using LoafGame.Collisions;
using LoafGame.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpDX.Direct2D1.Effects;
using System;
using System.Reflection.Metadata;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace LoafGame.Scenes;

public class TitleScene : Scene
{
    private float[] resolutions = new float[] { 1f, 2f };
    private int currentResolutionIndex = 0;

    private SpriteBatch _spriteBatch;
    private Player _player;

    /*
    private float vw;
    private float vh;
    private float leftMargin;
    private float centerX;
    */

    private Button startButton;
    private Button loadButton;
    private Button creditsButton;
    private BoundingPoint cursor;

    private SpriteFont friedolin;
    private SpriteFont exitText;

    // Layer textures
    private Texture2D _layer1;
    private Texture2D _layer2;
    private Texture2D _layer3;
    private Texture2D _layer4;
    private Texture2D _layer5;

    /// <summary>
    /// Initializes a new instance of the <see cref="TitleScene"/> class.
    /// </summary>
    /// <param name="game">The game instance that this scene is associated with.</param>
    public TitleScene(Game game) : base(game) { }

    public override void Initialize()
    {
        var LOAF = Game as LOAF;
        float vw = Game.GraphicsDevice.Viewport.Width / LOAF.GameScale;
        float vh = Game.GraphicsDevice.Viewport.Height / LOAF.GameScale;

        float leftMargin = vw * 0.02f;
        float centerX = vw / 2;

        float buttonRowY = vh * 0.5f;
        float buttonSpacing = vw * 0.15f;

        _player = new Player()
        {
            Position = new Vector2((centerX - buttonSpacing * 2) * LOAF.GameScale, (buttonRowY + buttonSpacing / 2) * LOAF.GameScale),
            Direction = Direction.Right,
            Scale = 3f * LOAF.GameScale
        };

        startButton = new Button() { Position = new Vector2(centerX - buttonSpacing, buttonRowY), Text = "Start"};
        loadButton = new Button() { Position = new Vector2(centerX, buttonRowY), Text = "Load" };
        creditsButton = new Button() { Position = new Vector2(centerX + buttonSpacing, buttonRowY), Text = "Credits" };

        MediaPlayer.Stop();
        MediaPlayer.Play(LOAF.backgroundMusicTitle);
        MediaPlayer.IsRepeating = true;
        base.Initialize();
    }

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        friedolin = Content.Load<SpriteFont>("friedolin");
        exitText = Content.Load<SpriteFont>("hamburger");
        _player.LoadContent(Content);
        startButton.LoadContent(Content);
        loadButton.LoadContent(Content);
        creditsButton.LoadContent(Content);

        _layer1 = Game.Content.Load<Texture2D>("TitleLayersScaled/background_plains-Sheet1_scaled_3x");
        _layer2 = Game.Content.Load<Texture2D>("TitleLayersScaled/background_plains-Sheet2_scaled_3x");
        _layer3 = Game.Content.Load<Texture2D>("TitleLayersScaled/background_plains-Sheet3_scaled_3x");
        _layer4 = Game.Content.Load<Texture2D>("TitleLayersScaled/background_plains-Sheet4_scaled_3x");
        _layer5 = Game.Content.Load<Texture2D>("TitleLayersScaled/background_plains-Sheet5_scaled_3x");
    }

    public override void Update(GameTime gameTime)
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        _player.Update(gameTime);
        cursor = new BoundingPoint(LOAF.InputManager.Position / LOAF.GameScale);
        startButton.Update(cursor.CollidesWith(startButton.Bounds));
        loadButton.Update(cursor.CollidesWith(loadButton.Bounds));
        creditsButton.Update(cursor.CollidesWith(creditsButton.Bounds));

        if (LOAF.InputManager.LeftMouseClicked)
        {
            if (startButton.Hover)
            {
                startButton.PlayClickSound();
                LOAF.ChangeScene(new MinigameSelectorScene(LOAF));
            }
            if (loadButton.Hover)
            {
                if (SaveGame.TryLoadOverworld(out var save))
                {
                    loadButton.PlayClickSound();
                    LOAF.ChangeScene(new OverworldScene(LOAF, save));
                }
                else
                {
                    LOAF.DeniedSound.Play();
                }
            }
            if (creditsButton.Hover)
            {
                creditsButton.PlayClickSound();
                LOAF.ChangeScene(new CreditsScene(LOAF));
            }
        }

        if (LOAF.InputManager.KeyClicked(Keys.Escape))
        {
            LOAF.Exit();
        }
        if (LOAF.InputManager.KeyClicked(Keys.F10))
        {
            _player.Scale /= LOAF.GameScale;
            _player.Position = new Vector2(_player.Position.X / LOAF.GameScale, _player.Position.Y / LOAF.GameScale);

            currentResolutionIndex = (currentResolutionIndex + 1) % resolutions.Length;
            LOAF.ChangeResolutionScale(resolutions[currentResolutionIndex]);

            _player.Scale *= LOAF.GameScale;
            _player.Position = new Vector2(_player.Position.X * LOAF.GameScale, _player.Position.Y * LOAF.GameScale);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        float vw = Game.GraphicsDevice.Viewport.Width / LOAF.GameScale;
        float vh = Game.GraphicsDevice.Viewport.Height / LOAF.GameScale;
        float leftMargin = vw * 0.02f;
        float centerX = vw / 2;
        Game.GraphicsDevice.Clear(Color.DarkSlateGray);

        //anchor player at x=300
        float playerX = MathHelper.Clamp(_player.Position.X, 300, 13600);
        float offsetX = 300 - playerX;
        float cameraX = -offsetX; // world camera position relative to anchor
        Matrix transform =
            Matrix.CreateScale(LOAF.GameScale);
        _spriteBatch.Begin(transformMatrix: transform, samplerState: SamplerState.PointClamp);
        DrawRepeatingLayer(_layer1, parallax: 0.2f, cameraX);
        DrawRepeatingLayer(_layer2, parallax: 0.4f, cameraX);
        DrawRepeatingLayer(_layer3, parallax: 0.6f, cameraX);
        DrawRepeatingLayer(_layer4, parallax: 0.8f, cameraX);
        DrawRepeatingLayer(_layer5, parallax: 1.0f, cameraX);
        _spriteBatch.End();

        _spriteBatch.Begin(transformMatrix: Matrix.CreateTranslation(offsetX, 0, 0), samplerState: SamplerState.PointClamp);
        _player.Draw(gameTime, _spriteBatch);
        _spriteBatch.End();

        _spriteBatch.Begin(transformMatrix: transform, samplerState: SamplerState.PointClamp);
        startButton.Draw(_spriteBatch);
        loadButton.Draw(_spriteBatch);
        creditsButton.Draw(_spriteBatch);

        string title = "Life of a Foreman";
        Vector2 titleSize = friedolin.MeasureString(title);
        Vector2 titlePos = new Vector2(centerX - titleSize.X / 2f, vh * 0.12f);
        _spriteBatch.DrawString(friedolin, title, titlePos, Color.Black);

        _spriteBatch.DrawString(exitText, "Press ESC to Exit, Press F10 to change screen resolution", new Vector2(leftMargin, vh * 0.0185f), Color.Black);

        _spriteBatch.End();
    }

    //draws a texture repeating for parallax scrolling
    private void DrawRepeatingLayer(Texture2D texture, float parallax, float cameraX)
    {
        int vw = Game.GraphicsDevice.Viewport.Width;

        int texW = texture.Width;
        int y = 0; // align to top

        float layerScroll = cameraX * parallax;

        float scrollMod = layerScroll % texW;
        if (scrollMod < 0) scrollMod += texW;
        float startX = -scrollMod;

        for (float x = startX; x < vw; x += texW)
        {
            _spriteBatch.Draw(texture, new Vector2(x, y), Color.White);
        }
    }
}