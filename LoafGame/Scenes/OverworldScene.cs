using LoafGame.Collisions;
using LoafGame.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Reflection.Metadata;

namespace LoafGame.Scenes;

public class OverworldScene : Scene
{
    private SpriteBatch _spriteBatch;
    private HexTilemap _tilemap;
    private BoundingPoint cursor;

    /// <summary>
    /// Initializes a new instance of the <see cref="OverworldScene"/> class.
    /// </summary>
    /// <param name="game">The game instance that this scene is associated with.</param>
    public OverworldScene(Game game) : base(game) { }

    public override void Initialize()
    {
        MediaPlayer.Play(LOAF.backgroundMusicTitle);
        MediaPlayer.IsRepeating = true;
        base.Initialize();
    }

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        _tilemap = Content.Load<HexTilemap>("example");
    }

    public override void Update(GameTime gameTime)
    {
        var LOAF = Game as LOAF;
        if (LOAF == null) return;
        cursor = new BoundingPoint(LOAF.InputManager.Position / 2);

        
        if (LOAF.InputManager.LeftMouseClicked)
        {

        }
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.DarkSlateGray);

        _spriteBatch.Begin(transformMatrix: Matrix.CreateScale(1));
        _tilemap.Draw(gameTime, _spriteBatch, 2f, 30f);

        _spriteBatch.End();
    }
}