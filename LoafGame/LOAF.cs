using LoafGame.Collisions;
using LoafGame.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace LoafGame
{
    public class LOAF : Game
    {
        private GraphicsDeviceManager _graphics;

        // The scene that is currently active.
        private static Scene s_activeScene;

        // The next scene to switch to, if there is one.
        private static Scene s_nextScene;

        /// <summary>
        /// Input Manager to pass to the scenes.
        /// </summary>
        public InputManager InputManager { get; private set; }

        /// <summary>
        /// Gets the sound effect that is played when a button is hovered over.
        /// </summary>
        public static SoundEffect ButtonHoverSound { get; private set; }

        /// <summary>
        /// Gets the sound effect that plays when a button is clicked.
        /// </summary>
        public static SoundEffect ButtonClickSound { get; private set; }

        /// <summary>
        /// Background music for title screen.
        /// </summary>
        public static Song backgroundMusicTitle { get; private set; }

        /// <summary>
        /// Background music for overworld screen.
        /// </summary>
        public static Song backgroundMusicOverworld { get; private set; }

        /// <summary>
        /// Background music for overworld screen.
        /// </summary>
        public static Song backgroundMusicMinigame { get; private set; }

        public LOAF()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.PreferredBackBufferWidth = 960; // Set your desired width
            _graphics.PreferredBackBufferHeight = 540; // Set your desired height
            //_graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {

            InputManager = new InputManager();
            base.Initialize();
            // Start the game with the title scene.
            ChangeScene(new TitleScene(this));
        }

        protected override void LoadContent()
        {
            ButtonHoverSound = Content.Load<SoundEffect>("001_Hover_01");
            ButtonClickSound = Content.Load<SoundEffect>("013_Confirm_03");
            backgroundMusicTitle = Content.Load<Song>("05 A joyfull get together in the royal chambers_[cut_41sec]");
            backgroundMusicOverworld = Content.Load<Song>("04 Heroes theme - Ouverture of Valor_[cut_84sec]");
            backgroundMusicMinigame = Content.Load<Song>("02 The Dark Lord - upbeat version_[cut_60sec]");
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update(gameTime);
            if (InputManager.Exit) Exit();

            // if there is a next scene waiting to be switch to, then transition
            // to that scene.
            if (s_nextScene != null)
            {
                TransitionScene();
            }

            // If there is an active scene, update it.
            if (s_activeScene != null)
            {
                s_activeScene.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // If there is an active scene, draw it.
            if (s_activeScene != null)
            {
                s_activeScene.Draw(gameTime);
            }
            base.Draw(gameTime);
        }

        public static void ChangeScene(Scene next)
        {
            // Only set the next scene value if it is not the same
            // instance as the currently active scene.
            if (s_activeScene != next)
            {
                s_nextScene = next;
            }
        }

        private static void TransitionScene()
        {
            // If there is an active scene, dispose of it.
            if (s_activeScene != null)
            {
                s_activeScene.Dispose();
            }

            // Force the garbage collector to collect to ensure memory is cleared.
            GC.Collect();

            // Change the currently active scene to the new scene.
            s_activeScene = s_nextScene;

            // Null out the next scene value so it does not trigger a change over and over.
            s_nextScene = null;

            // If the active scene now is not null, initialize it.
            // Remember, just like with Game, the Initialize call also calls the
            // Scene.LoadContent
            if (s_activeScene != null)
            {
                s_activeScene.Initialize();
            }
        }
    }
}
