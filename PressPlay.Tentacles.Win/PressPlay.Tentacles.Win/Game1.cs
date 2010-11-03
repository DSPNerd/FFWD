using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using PressPlay.U2X.Xna.Components;

namespace PressPlay.Tentacles.Win
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont font;
        bool rotate = false;

        MeshRenderer renderer = new MeshRenderer();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Camera.main.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), graphics.GraphicsDevice.Viewport.AspectRatio, 1, 20000);
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //model = Content.Load<Model>("Levels/Models/levelEntrance_edited_noMaterial");
            //model = Content.Load<Model>("xna_hierarchy_test_02");
            renderer.model = Content.Load<Model>("Levels/Models/level_entrance");
            renderer.texture = Content.Load<Texture2D>("Levels/Maps/block_brown_desat");
            renderer.Start();

            font = Content.Load<SpriteFont>("TestFont");

            Camera.main.transform.localPosition = new Vector3(0, 0, 300);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        KeyboardState oldState;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState key = Keyboard.GetState();
            Vector3 dir = Vector3.Zero;
            if (key.IsKeyDown(Keys.A))
            {
                dir.X += 1;
            }
            if (key.IsKeyDown(Keys.D))
            {
                dir.X -= 1;
            }
            if (key.IsKeyDown(Keys.W))
            {
                dir.Y += 1;
            }
            if (key.IsKeyDown(Keys.X))
            {
                dir.Y -= 1;
            }
            if (key.IsKeyDown(Keys.E))
            {
                dir.Z += 1;
            }
            if (key.IsKeyDown(Keys.Z))
            {
                dir.Z -= 1;
            }
            if (key.IsKeyDown(Keys.RightShift))
            {
                dir *= 10;
            }
            Camera.main.transform.localPosition += dir;

            if (oldState.IsKeyUp(Keys.R) && key.IsKeyDown(Keys.R))
            {
                rotate = !rotate;
            }
            oldState = key;

            renderer.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //GraphicsDevice.RasterizerState = new RasterizerState() { FillMode = FillMode.WireFrame };

            //// TODO: Add your drawing code here
            // Set the world matrix as the root transform of the model.
            //model.Root.Transform = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);

            renderer.Draw(spriteBatch);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Camera: " + Camera.main.transform.localPosition, new Vector2(10, 10), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }


        /// <summary>
        /// Gets spaceship view matrix
        /// </summary>
        private Matrix GetViewMatrix()
        {
            return Matrix.CreateLookAt(
                Camera.main.transform.localPosition,
                Camera.main.transform.localPosition + new Vector3(0, 0, -1000),
                Vector3.Up);
        }
    }
}
