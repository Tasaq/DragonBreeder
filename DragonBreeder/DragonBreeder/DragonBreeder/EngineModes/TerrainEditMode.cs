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
using Microsoft.Xna.Framework.Content;
using DragonBreeder.TerrainEditing;
using wf = System.Windows.Forms;
namespace DragonBreeder.EngineModes
{
    class TerrainEditMode : EngineMode
    {
        ContentManager Content;
        GraphicsProcessor proc;
        SpriteBatch spriteBatch;

        TerrainModel terrain;
        TerrainMaterial materialTerrain;
        DirectionalLight light = new DirectionalLight();
        GraphicsDevice GraphicsDevice;

        Camera camera;
        Game game;

        SpriteFont Arial;
        bool raise = true;
        Vector2 angle = new Vector2();
        float disp = 0.0f;
        Vector2 MousePos = new Vector2();

        private Vector3 dirUnit;
        RenderTarget2D temp;



        float tempScrollPosV = 0;
        Rectangle ScrollV = new Rectangle();

        RenderTarget2D newTerrainA;
        RenderTarget2D newTerrainB;

        ComputeBuffer cbufTexIn;
        ComputeBuffer cbufRtA;
        ComputeBuffer cbufRtB;
        ComputeBuffer cbufDepth;

        bool ActiveA = false;

        Effect painter;
        Effect CopyTexture;
        QuadRender quad;

        GUIContainer GUIcontainer;
        Button testButton;
        ScrollHorizontal scrollHZ;
        ScrollVertical scrollVT;
        public void LoadContent(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        {
            this.game = game;
            Content = content;
            GraphicsDevice = graphicsDevice;
            GUIcontainer = new GUIContainer();
            testButton = new Button(Content.Load<Texture2D>("Editor/TestButton"), Content.Load<Texture2D>("Editor/TestButtonPressed"), 100, 50);
            scrollHZ = new ScrollHorizontal(content.Load<Texture2D>("Editor/Scroll"), content.Load<Texture2D>("Editor/Scroller"), 640, 10);
            scrollHZ.ContentWidth = 1024;
            scrollVT = new ScrollVertical(content.Load<Texture2D>("Editor/Scroll"), content.Load<Texture2D>("Editor/Scroller"), 720, 10);
            scrollVT.ContentWidth = 1024;
            testButton.Position = new Vector2(640, 0);
            GUIcontainer.add(testButton);
            GUIcontainer.add(scrollHZ);
            GUIcontainer.add(scrollVT);
            // Create a new SpriteBatch, which can be used to draw textures.
            proc = new GraphicsProcessor(game.graphics, Content, 1280/2, 720);
            spriteBatch = game.spriteBatch;
            proc.LoadContent();
            light.Position = new Vector3(1, 2, 1);
            light.Color = new Vector3(1, 1, 1);
            light.Distance = 10.0f;
            proc.Add(light);
            camera = proc.Camera;
            camera.Position = new Vector3(0, 0, 0);
            camera.LookAt = new Vector3(0, 0, 1);
            dirUnit = new Vector3(0, -0.1f, 0.9f);
            quad = new QuadRender(GraphicsDevice);

            newTerrainA = new RenderTarget2D(graphicsDevice, 1024, 1024, true);
            newTerrainB = new RenderTarget2D(graphicsDevice, 1024, 1024, true);

            cbufTexIn = new ComputeBuffer(GraphicsDevice, Content.Load<Texture2D>("zakopane"), ComputeBufferUsage.None);
            cbufRtA = new ComputeBuffer(graphicsDevice, newTerrainA, ComputeBufferUsage.UnorderedAccess);
            cbufRtB = new ComputeBuffer(graphicsDevice, newTerrainB, ComputeBufferUsage.None);
            cbufDepth = new ComputeBuffer(graphicsDevice, proc.getDepthBuffer, ComputeBufferUsage.None);
            painter = content.Load<Effect>("Editor/HeightMapPainter");
            CopyTexture = content.Load<Effect>("Editor/CopyTexture");
            painter.CurrentTechnique = painter.Techniques["copyAtoB"];
            painter.Parameters["inputTex"].SetValue(cbufTexIn);
            painter.Parameters["outputTex"].SetValue(cbufRtA);
            painter.Begin();
            painter.CurrentTechnique.Passes[0].Begin();
            GraphicsDevice.Dispatch(64, 64, 1);  // Run the shader
            painter.CurrentTechnique.Passes[0].End();
            painter.End();
            EffectParameter.SetUnorderedValue(GraphicsDevice, null, 0);
            
            materialTerrain = new TerrainMaterial(Color.LightBlue);
            materialTerrain.displacementMap = newTerrainA;// Content.Load<Texture2D>("zakopane");
            materialTerrain.layersMap = Content.Load<Texture2D>("blendMap");
            materialTerrain.Textures = new Texture2Drgba();
            materialTerrain.Textures.Base = Content.Load<Texture2D>("large_grass");
            materialTerrain.Textures.R = Content.Load<Texture2D>("bmpTestFile");
            materialTerrain.Textures.G = Content.Load<Texture2D>("grassRock");
            materialTerrain.Textures.B = Content.Load<Texture2D>("snowHill");
            materialTerrain.Textures.A = Content.Load<Texture2D>("sand02");
            terrain = new TerrainModel(new Quad(GraphicsDevice, 100, QuadType.Quad), materialTerrain);
            terrain.Camera = camera;
            terrain.World = Matrix.CreateScale(100, 100, 1) * Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(0, -5, 0);
            //terrain.World = Matrix.CreateTranslation(0, 0, 0);
            proc.Add(terrain);

            int sH = 640 - (materialTerrain.displacementMap.Width - 640);
            int sV = 720 - (materialTerrain.displacementMap.Height - 720);
            ScrollV = new Rectangle(1280 / 2, 0, 10, sV + 10);
            Arial = Content.Load<SpriteFont>("Arial");
            data = new Color[materialTerrain.displacementMap.Width * materialTerrain.displacementMap.Height];
            mapRect = new Rectangle(1280 - 640, -ScrollV.Y, materialTerrain.displacementMap.Width, materialTerrain.displacementMap.Height);
           
        }
        public void saveMap()
        {
            this.materialTerrain.displacementMap.SaveAsPng(new System.IO.FileStream("test.bmp", System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite), 1024, 1024);
        }
        Color[] data;
        bool scrollingV = false;
        MouseState mouseState;
        float TempPos=0;
        public void Update(GameTime gameTime)
        {
            Vector3 CamPos = camera.Position;
            float camspeed = 0.2f;
            float lightspeed = 0.05f;
            Mouse.WindowHandle = game.Window.Handle;
            //Vector3 LightPos = light.lightPosition;
             mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
            {

                saveMap();
                this.game.Exit();
            }
            if (testButton.isClicked)
            {
                wf.OpenFileDialog dlg = new wf.OpenFileDialog();
                dlg.Filter = "jpeg files (*.jpg)|*.jpeg|All files (*.*)|*.*";
                if (dlg.ShowDialog() == wf.DialogResult.OK)
                {
                    try
                    {
                        if ((dlg.FileName) != null)
                        {
                            Console.WriteLine(dlg.FileName);
                            //File.Copy(dlg.FileName, @"C:\Users\Tasak\Documents\GitHub\XNA_Tracker\XNA_Tracker\XNA_Tracker\bin\x86\Debug\Content\inputName.wmv", true);
                            cbufTexIn = new ComputeBuffer(GraphicsDevice, Texture2D.FromStream(GraphicsDevice, new System.IO.FileStream(dlg.FileName, System.IO.FileMode.Open)), ComputeBufferUsage.None);
                            CopyTexture = Content.Load<Effect>("Editor/CopyTexture");
                            painter.CurrentTechnique = painter.Techniques["copyAtoB"];
                            painter.Parameters["inputTex"].SetValue(cbufTexIn);
                            painter.Parameters["outputTex"].SetValue(cbufRtA);
                            painter.Begin();
                            painter.CurrentTechnique.Passes[0].Begin();
                            GraphicsDevice.Dispatch(64, 64, 1);  // Run the shader
                            painter.CurrentTechnique.Passes[0].End();
                            painter.End();
                            EffectParameter.SetUnorderedValue(GraphicsDevice, null, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        wf.MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    }
                }
            }
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    this.game.IsMouseVisible = false;
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
            mapRect.X = 650 - (int)((1044-640)*(scrollHZ.ValueNormalized));
            mapRect.Y = 10 - (int)((1024 - 700) * scrollVT.ValueNormalized);
            ScreeCoord = -new Vector2(mapRect.X - mouseState.X, mapRect.Y - mouseState.Y);
            GUIcontainer.Update();
            
        }
        Vector2 ScreeCoord = new Vector2();
        Rectangle mapRect;
        public void Draw()
        {
            Paint();
             // GraphicsDevice.setWireframeView();
            proc.G_BufferDraw();
             GraphicsDevice.setNormalView();
            proc.LightBufferDraw();
            temp =  proc.CombineLightingAndAlbedo();
            GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.Clear(new Color(255, 0, 0, 255)/*Wild blue yonder*/);
            spriteBatch.Begin();
            spriteBatch.Draw(materialTerrain.displacementMap, mapRect, Color.White);
            spriteBatch.Draw(temp, new Rectangle(0, 0, 1280 / 2, 720), Color.White);
            spriteBatch.End();
            spriteBatch.Begin();
            spriteBatch.DrawString(Arial, mouseState.ToString().ToString(), new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(Arial, materialTerrain.displacementMap.Width.ToString(), new Vector2(0, 20), Color.White);
            spriteBatch.DrawString(Arial, new Vector2(mouseState.X/640.0f, mouseState.Y/720.0f).ToString(), new Vector2(0, 40), Color.White);
            GUIcontainer.Draw(spriteBatch);
            spriteBatch.End();
        }
        int it = 0;
        public void Paint()
        {
            GraphicsDevice.SetRenderTarget(0, newTerrainB);
            CopyTexture.Parameters["TextureA"].SetValue(newTerrainA);
            quad.RenderQuad(CopyTexture);
            GraphicsDevice.SetRenderTarget(0, null);
            if (mouseState.X > 640)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && ScreeCoord.X > 0 && ScreeCoord.Y > 0 && !Lockable.isLocked)
                {
                    painter.CurrentTechnique = painter.Techniques["paint2D"];
                    painter.Parameters["inputTex"].SetValue(cbufRtB);
                    painter.Parameters["outputTex"].SetValue(cbufRtA);

                    painter.Parameters["TextureDim"].SetValue(new Vector2(newTerrainA.Width, newTerrainA.Height));
                    painter.Parameters["ClickPosition"].SetValue(ScreeCoord);
                    painter.Parameters["PixelDistance"].SetValue(0.05f);
                    painter.Parameters["Strength"].SetValue(0.001f);
                    painter.Begin();
                    painter.CurrentTechnique.Passes[0].Begin();
                    GraphicsDevice.Dispatch(64, 64, 1);  // Run the shader
                    painter.CurrentTechnique.Passes[0].End();
                    painter.End();
                    EffectParameter.SetUnorderedValue(GraphicsDevice, null, 0);
                }
            }
            else
            {
                if (mouseState.LeftButton == ButtonState.Pressed && !Lockable.isLocked)
                {
                    painter.CurrentTechnique = painter.Techniques["paint3D"];
                    painter.Parameters["inputTex"].SetValue(cbufRtB);
                    painter.Parameters["outputTex"].SetValue(cbufRtA);
                    painter.Parameters["Depth"].SetValue(cbufDepth);

                    painter.Parameters["TextureDim"].SetValue(new Vector2(newTerrainA.Width, newTerrainA.Height));
                    painter.Parameters["ClickPosition"].SetValue(new Vector2(mouseState.X, mouseState.Y));
                    painter.Parameters["PixelDistance"].SetValue(0.05f);
                    painter.Parameters["Strength"].SetValue(0.01f);
                    painter.Parameters["invViewProjection"].SetValue(Matrix.Invert(camera.ViewMatrix*camera.ProjectionMatrix));
                    painter.Parameters["Rectangle"].SetValue(new Vector4(-100, -100, 100, 100));
                    painter.Begin();
                    painter.CurrentTechnique.Passes[0].Begin();
                    GraphicsDevice.Dispatch(64, 64, 1);  // Run the shader
                    painter.CurrentTechnique.Passes[0].End();
                    painter.End();
                    EffectParameter.SetUnorderedValue(GraphicsDevice, null, 0);
                }
            }
        }
    }
}
