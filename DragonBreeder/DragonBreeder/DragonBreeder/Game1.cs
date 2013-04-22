using System;
using System.Collections.Generic;
using System.Linq;
using JBBRXG11;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using GameTime = Microsoft.Xna.Framework.GameTime;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SpriteSortMode = Microsoft.Xna.Framework.Graphics.SpriteSortMode;
using SpriteEffects = Microsoft.Xna.Framework.Graphics.SpriteEffects;
using BlendFunction = Microsoft.Xna.Framework.Graphics.BlendFunction;
using Blend = Microsoft.Xna.Framework.Graphics.Blend;
using ColorWriteChannels = Microsoft.Xna.Framework.Graphics.ColorWriteChannels;
using StencilOperation = Microsoft.Xna.Framework.Graphics.StencilOperation;
using CompareFunction = Microsoft.Xna.Framework.Graphics.CompareFunction;
using FillMode = Microsoft.Xna.Framework.Graphics.FillMode;
using CullMode = Microsoft.Xna.Framework.Graphics.CullMode;
using IndexElementSize = Microsoft.Xna.Framework.Graphics.IndexElementSize;
using DepthFormat = Microsoft.Xna.Framework.Graphics.DepthFormat;
using SurfaceFormat = Microsoft.Xna.Framework.Graphics.SurfaceFormat;
using VertexPositionColor = Microsoft.Xna.Framework.Graphics.VertexPositionColor;
using VertexPositionColorTexture = Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture;
using VertexPositionNormalTexture = Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture;
using VertexPositionTexture = Microsoft.Xna.Framework.Graphics.VertexPositionTexture;
using VertexElementFormat = Microsoft.Xna.Framework.Graphics.VertexElementFormat;
using VertexElementUsage = Microsoft.Xna.Framework.Graphics.VertexElementUsage;
using VertexElement = Microsoft.Xna.Framework.Graphics.VertexElement;
using ClearOptions = Microsoft.Xna.Framework.Graphics.ClearOptions;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using DragonBreeder.Graphics;

namespace DragonBreeder
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : JBBRXG11.Game
    {
        Dragon dragon1 = new Dragon();
        SkyCloudSystem clouds;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GraphicsProcessor proc;
        StaticModelTS testModel;
      //  AnimatedModel DragonTest;
        TerrainModel terrain;
        TerrainMaterial materialTerrain;
        DirectionalLight light = new DirectionalLight();
        PointLight pLight = new PointLight();
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
           // graphics.PreferredBackBufferFormat = SurfaceFormat.HdrBlendable;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            Window.Title = "DragonBreeder";
            
        }
        Camera camera;
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            GraphicObject.ContentManager = Content;
            GraphicObject.GraphicsDevice = GraphicsDevice;
            // Create a new SpriteBatch, which can be used to draw textures.
            proc = new GraphicsProcessor(graphics, Content, 1280, 720);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            testModel = new StaticModelTS("sphereTest");
            //DragonTest = new AnimatedModel("Dragons/Tail/testTail");
            //instancedModel = new TerrainModel("redBox");
           // DragonTest.World =  Matrix.CreateTranslation(0, 0.0f, 0);
            //testAnimation.GetMesh("miecz001").localTransform *= Matrix.CreateTranslation(0.07f, -0.12f, -0.15f);
          //  DragonTest.startAnimation("wave");
            testModel.DispalcementMap = Content.Load<Texture2D>("randomDots");
            testModel.World = Matrix.CreateScale(1)* Matrix.CreateTranslation(0, 0, 0);
            proc.LoadContent();
            light.Position = new Vector3(1, 2, 1);
            light.Color = new Vector3(1, 1, 1);
            light.Distance = 10.0f;
            proc.Add(light);
            pLight.Position = new Vector3(0, 0, 0);
            pLight.Distance = 2.0f;
            pLight.Color = Color.OrangeRed.ToVector3();
            proc.Add(pLight);
           // proc.Add(DragonTest);
            camera = proc.Camera;
            camera.Position = new Vector3(0, 0, 0);
            camera.LookAt = new Vector3(0, 0, 1);
            dirUnit = new Vector3(0, -0.1f, 0.9f);
            proc.Add(testModel);
            dragon1.LoadContent();
            dragon1.World = (Matrix.CreateTranslation(0, 0, -5));
            dragon1.AddToGraphicsProcessor(proc);

            materialTerrain = new TerrainMaterial(Color.LightBlue);
            materialTerrain.displacementMap = Content.Load<Texture2D>("zakopane");
            materialTerrain.layersMap = Content.Load<Texture2D>("blendMap");
            materialTerrain.Textures = new Texture2Drgba();
            materialTerrain.Textures.Base = Content.Load<Texture2D>("large_grass");
            materialTerrain.Textures.R = Content.Load<Texture2D>("bmpTestFile");
            materialTerrain.Textures.G = Content.Load<Texture2D>("grassRock");
            materialTerrain.Textures.B = Content.Load<Texture2D>("snowHill");
            materialTerrain.Textures.A = Content.Load<Texture2D>("sand02");


            terrain = new TerrainModel(new Quad(GraphicsDevice, 40), materialTerrain);
            terrain.Camera = camera;
            terrain.World = Matrix.CreateScale(20, 20, 2) * Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(0,-1,0);
            proc.Add(terrain);
            clouds = new SkyCloudSystem(100);
            clouds.LoadContent();
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        bool raise = true;
        Vector2 angle = new Vector2();
        Vector3 dirUnit = new Vector3(0, 0, 1);
        float disp = 0.0f;
        Vector2 MousePos = new Vector2();
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
            Vector3 CamPos = camera.Position;
            float camspeed = 0.2f;
            float lightspeed = 0.05f;
            //Vector3 LightPos = light.lightPosition;
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                this.Exit();
            this.IsMouseVisible = true;
            if (mouseState.RightButton == ButtonState.Pressed)
            {
                this.IsMouseVisible = false;
                angle.X += (MousePos.X - mouseState.X) * 0.1f;
                angle.Y -= (MousePos.Y - mouseState.Y) * 0.1f;
                if ((MousePos.Y - mouseState.Y) != 0 || (MousePos.X - mouseState.X) != 0)
                {
                    dirUnit.X = MathHelper.ToRadians(angle.X);
                    dirUnit = new Vector3(0, 0, 1);
                    dirUnit = Vector3.Transform(dirUnit, Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(angle.X), MathHelper.ToRadians(-angle.Y), 0));
                    dirUnit *= camspeed;
                }
            }
            else
            {
                MousePos = new Vector2(mouseState.X, mouseState.Y);
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                CamPos += dirUnit;
                //  CamPos.Z += dirUnit.Z;
            }
            if (keyboardState.IsKeyDown(Keys.W))
            {
                CamPos -= dirUnit;
                // CamPos.Z -= dirUnit.Z;
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                CamPos -= Vector3.Cross(Vector3.UnitY, dirUnit);
                //  CamPos.Z += dirUnit.Z;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                CamPos += Vector3.Cross(Vector3.UnitY, dirUnit);
                // CamPos.Z -= dirUnit.Z;
            }
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                PointLight p = new PointLight();
                p.Position = camera.Position;
                p.Color = Color.Blue.ToVector3();
                p.Distance = 2;
                proc.Add(p);
            }

            if (mouseState.RightButton == ButtonState.Pressed)
                Mouse.SetPosition((int)MousePos.X, (int)MousePos.Y);
            camera.LookAt = CamPos - dirUnit * 30;
            camera.Position = CamPos;
            //DragonTest.Update(gameTime, true);
            testModel.EyeDirection = Vector3.Normalize( -camera.LookAt + camera.Position);
            if ((raise == true) && (disp <= 0.1f))
            {
                disp += 0.01f;
                if (disp > 0.1f)
                {
                    raise = false;
                }
            }
            if ((raise == false && disp >= -0.0f))
            {
                disp -= 0.01f;
                if (disp < 0.0f)
                {
                    raise = true;
                }
            }
            
            testModel.DisplacementScale = disp;
            dragon1.Update(gameTime);
            base.Update(gameTime);
        }
        RenderTarget2D temp;
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            proc.G_BufferDraw();
       //    GraphicsDevice.setNormalView();
         //   GraphicsDevice.setWireframeView();
           proc.LightBufferDraw();
           temp = proc.CombineLightingAndAlbedo();
            GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.Clear(new Color(255,0,0,255)/*Wild blue yonder*/);
            
            spriteBatch.Begin();
            spriteBatch.Draw(temp, new Rectangle(0, 0, 1280, 720), Color.White);
            spriteBatch.End();
           // clouds.Draw(camera);
            base.Draw(gameTime);
        }
    }
}
