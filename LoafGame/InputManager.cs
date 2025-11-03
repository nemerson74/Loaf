using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LoafGame
{
    public class InputManager
    {
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        MouseState currentMouseState;
        MouseState previousMouseState;

        GamePadState currentGamePadState;
        GamePadState previousGamePadState;

        /// <summary>
        /// Direction of the input
        /// </summary>
        public Vector2 Direction { get; private set; }

        /// <summary>
        /// Position of the mouse cursor
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        /// Detects if the right mouse button was clicked this frame
        /// </summary>
        public bool RightMouseClicked
        {
            get
            {
                return previousMouseState.RightButton == ButtonState.Released &&
                       currentMouseState.RightButton == ButtonState.Pressed;
            }
        }

        /// <summary>
        /// Detects if the left mouse button was clicked
        /// </summary>
        public bool LeftMouseClicked
        {
            get
            {
                return previousMouseState.LeftButton == ButtonState.Released &&
                       currentMouseState.LeftButton == ButtonState.Pressed;
            }
        }

        /// <summary>
        /// To exit the game
        /// </summary>
        public bool Exit { get; private set; } = false;

        /// <summary>
        /// Mouse wheel difference
        /// </summary>
        public int ScrollWheelDelta { get; private set; }

        /// <summary>
        /// Gets the cumulative value of the scroll wheel input.
        /// </summary>
        public int ScrollWheelValue { get; private set; }

        /// <summary>
        /// Gets the current state of the right mouse button.
        /// </summary>
        public ButtonState CurrentRightMouseState => currentMouseState.RightButton;

        /// <summary>
        /// Gets the state of the right mouse button during the previous update.
        /// </summary>
        public ButtonState PreviousRightMouseState => previousMouseState.RightButton;

        /// <summary>
        /// Returns true while the left mouse button is held down.
        /// </summary>
        public bool LeftMouseDown => currentMouseState.LeftButton == ButtonState.Pressed;

        /// <summary>
        /// Returns true while the right mouse button is held down.
        /// </summary>
        public bool RightMouseDown => currentMouseState.RightButton == ButtonState.Pressed;

        public void Update(GameTime gameTime)
        {
            #region updating states
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            previousGamePadState = currentGamePadState;
            currentGamePadState = GamePad.GetState(0);
            #endregion

            #region GamePad
            //Get position from GamePad
            Direction = currentGamePadState.ThumbSticks.Right * 100
            * (float)gameTime.ElapsedGameTime.TotalSeconds;
            #endregion

            #region Keyboard
            //Get position from Keyboard
            if ((currentKeyboardState.IsKeyDown(Keys.Left)) ||
                currentKeyboardState.IsKeyDown(Keys.A))
            {
                Direction += new Vector2(-100 * (float)gameTime.ElapsedGameTime.TotalSeconds, 0);
            }
            if ((currentKeyboardState.IsKeyDown(Keys.Right)) ||
                currentKeyboardState.IsKeyDown(Keys.D))
            {
                Direction += new Vector2(100 * (float)gameTime.ElapsedGameTime.TotalSeconds, 0);
            }
            if ((currentKeyboardState.IsKeyDown(Keys.Up)) ||
                currentKeyboardState.IsKeyDown(Keys.W))
            {
                Direction += new Vector2(0, -100 * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            if ((currentKeyboardState.IsKeyDown(Keys.Down)) ||
                currentKeyboardState.IsKeyDown(Keys.S))
            {
                Direction += new Vector2(0, 100 * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            #endregion

            #region Mouse
            //Get position from Mouse
            Position = currentMouseState.Position.ToVector2();
            ScrollWheelDelta = currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;
            ScrollWheelValue = currentMouseState.ScrollWheelValue;
            #endregion

            #region exit
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed || currentKeyboardState.IsKeyDown(Keys.Escape))
            {
                Exit = true;
            }
            #endregion
        }

        public bool IsKeyDown(Microsoft.Xna.Framework.Input.Keys key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }
    }
}
