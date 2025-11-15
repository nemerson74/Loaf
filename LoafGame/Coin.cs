using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoafGame
{
    /// <summary>
    /// A class representing a coin
    /// </summary>
    public class Coin
    {
        // The game this coin belongs to
        Game game;

        // The VertexBuffer of coin vertices
        VertexBuffer vertexBuffer;

        // The IndexBuffer defining the coin's triangles
        IndexBuffer indexBuffer;

        // The effect to render the coin with
        BasicEffect effect;

        // The texture to apply to the coin
        Texture2D texture;

        // Current rotation around Y axis in radians
        float rotationY = 0f;

        // Spin speed in radians per second
        public float SpinSpeed { get; set; } = MathF.PI;

        /// <summary>
        /// Initializes the vertex of the coin
        /// </summary>
        public void InitializeVertices()
        {
            var vertexData = new VertexPositionNormalTexture[] {
                // Front Face (z = -1)
                new VertexPositionNormalTexture() { Position = new Vector3(-1.0f, -1.0f, -1.0f), TextureCoordinate = new Vector2(0.0f, 1.0f), Normal = Vector3.Forward },
                new VertexPositionNormalTexture() { Position = new Vector3(-1.0f,  1.0f, -1.0f), TextureCoordinate = new Vector2(0.0f, 0.0f), Normal = Vector3.Forward },
                new VertexPositionNormalTexture() { Position = new Vector3( 1.0f,  1.0f, -1.0f), TextureCoordinate = new Vector2(1.0f, 0.0f), Normal = Vector3.Forward },
                new VertexPositionNormalTexture() { Position = new Vector3( 1.0f, -1.0f, -1.0f), TextureCoordinate = new Vector2(1.0f, 1.0f), Normal = Vector3.Forward },

                // Back Face (z = +1)
                new VertexPositionNormalTexture() { Position = new Vector3(-1.0f, -1.0f, 1.0f), TextureCoordinate = new Vector2(1.0f, 1.0f), Normal = Vector3.Backward },
                new VertexPositionNormalTexture() { Position = new Vector3( 1.0f, -1.0f, 1.0f), TextureCoordinate = new Vector2(0.0f, 1.0f), Normal = Vector3.Backward },
                new VertexPositionNormalTexture() { Position = new Vector3( 1.0f,  1.0f, 1.0f), TextureCoordinate = new Vector2(0.0f, 0.0f), Normal = Vector3.Backward },
                new VertexPositionNormalTexture() { Position = new Vector3(-1.0f,  1.0f, 1.0f), TextureCoordinate = new Vector2(1.0f, 0.0f), Normal = Vector3.Backward },
            };
            vertexBuffer = new VertexBuffer(game.GraphicsDevice, typeof(VertexPositionNormalTexture), vertexData.Length, BufferUsage.None);
            vertexBuffer.SetData<VertexPositionNormalTexture>(vertexData);
        }

        /// <summary>
        /// Initializes the Index Buffer
        /// </summary>
        public void InitializeIndices()
        {
            var indexData = new short[]
            {
                // Front face (two triangles)
                0, 2, 1,
                0, 3, 2,

                // Back face (two triangles)
                4, 6, 5,
                4, 7, 6
            };
            indexBuffer = new IndexBuffer(game.GraphicsDevice, IndexElementSize.SixteenBits, indexData.Length, BufferUsage.None);
            indexBuffer.SetData<short>(indexData);
        }

        /// <summary>
        /// Initializes the BasicEffect to render our coin
        /// </summary>
        void InitializeEffect()
        {
            effect = new BasicEffect(game.GraphicsDevice);
            effect.World = Matrix.CreateScale(2.0f);
            effect.View = Matrix.CreateLookAt(
                new Vector3(8, 9, 12), // The camera position
                new Vector3(0, 0, 0),  // The camera target,
                Vector3.Up             // The camera up vector
            );
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,                       // The field-of-view
                game.GraphicsDevice.Viewport.AspectRatio, // The aspect ratio
                0.1f,                                     // The near plane distance
                100.0f                                    // The far plane distance
            );
            effect.TextureEnabled = true;
            effect.Texture = texture;
            effect.LightingEnabled = false;
        }

        /// <summary>
        /// Advance rotation over time
        /// </summary>
        public void Update(GameTime gameTime)
        {
            rotationY += SpinSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (rotationY > MathHelper.TwoPi) rotationY -= MathHelper.TwoPi;
        }

        /// <summary>
        /// Draws the coin
        /// </summary>
        public void Draw()
        {
            // Show both faces while spinning
            game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Update world with rotation each draw
            effect.World = Matrix.CreateScale(2.0f) * Matrix.CreateRotationY(rotationY);

            // apply the effect
            effect.CurrentTechnique.Passes[0].Apply();

            // set the vertex buffer
            game.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            // set the index buffer
            game.GraphicsDevice.Indices = indexBuffer;

            // Draw the triangles
            game.GraphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList, // The type to draw
                0,                          // The first vertex to use
                0,                          // The first index to use
                4                           // The number of triangles to draw
            );
        }

        /// <summary>
        /// Creates a new coin instance
        /// </summary>
        /// <param name="game">The game this coin belongs to</param>
        public Coin(Game game)
        {
            this.game = game;
            this.texture = game.Content.Load<Texture2D>($"smile");
            InitializeVertices();
            InitializeIndices();
            InitializeEffect();
        }
    }
}
