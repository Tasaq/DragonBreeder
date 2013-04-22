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
using Microsoft.Xna.Framework.Content;
using DragonBreeder.Graphics;


namespace DragonBreeder
{
    class GraphicsProcessor
    {
        SkyCloudSystem clouds;

        RenderTarget2D g_depth;
        RenderTarget2D g_normal;
        RenderTarget2D g_color;

        RenderTarget2D lightBuffer;

        RenderTarget2D resultingBuffer;

        public Camera Camera { get; set; }

        ContentManager ContentManager;
        GraphicsDevice GraphicsDevice;
        Box3D box;
        List<IModelEntity> models = new List<IModelEntity>();
        List<ILight> lights = new List<ILight>();
        LightPoints PointLights;
        QuadRender quad;
        Effect DirectionalLight;
        Effect PointLight;
        Effect combine;

        public RenderTarget2D SunOcclTest;
        public RenderTarget2D Shafts;
        public GraphicsProcessor(GraphicsDeviceManager device, ContentManager manager,int width, int height)
        {
            Camera = new Camera();
            Camera.FarPlane = 100.0f;
            Camera.NearPlane = 0.1f;
            this.GraphicsDevice = device.GraphicsDevice;
            this.ContentManager = manager;
            g_depth = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Single, DepthFormat.Depth24);
            g_normal = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Rgba1010102, DepthFormat.Depth24);
            g_color = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            lightBuffer = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            resultingBuffer = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            SunOcclTest = new RenderTarget2D(GraphicsDevice, width, height, true, SurfaceFormat.Color, DepthFormat.Depth24);
            Shafts = new RenderTarget2D(GraphicsDevice, width, height, true, SurfaceFormat.Color, DepthFormat.Depth24);
            quad = new QuadRender(GraphicsDevice);
            PointLights = new LightPoints(GraphicsDevice);
            clouds = new SkyCloudSystem(10000);
        }
        public void LoadContent()
        {
            DirectionalLight = ContentManager.Load<Effect>("DirectionalLight");
            PointLight = ContentManager.Load<Effect>("PointLight");
            combine = ContentManager.Load<Effect>("combine");
            box = new Box3D();
            clouds.LoadContent();
        }
        public void Add(IModelEntity model)
        {
            models.Add(model);
        }
        public void Add(ILight light)
        {
            if (!(light is PointLight))
                lights.Add(light);
            else
                PointLights.Add(light as PointLight);
        }
        public RenderTarget2D G_BufferDraw()
        {
            
            Matrix VP = Camera.ViewMatrix * Camera.ProjectionMatrix;
            GraphicsDevice.SetRenderTarget(0, g_depth);
            GraphicsDevice.SetRenderTarget(1, g_normal);
            GraphicsDevice.SetRenderTarget(2, g_color);
            GraphicsDevice.Clear(0, Color.Transparent);
            GraphicsDevice.Clear(1, Color.Black);
            GraphicsDevice.Clear(2, Color.Black);
            foreach (var Model in models)
            {
                Model.SetViewProjection(VP);
                if (Model is AnimatedModel)
                {
                    AnimatedModel m = Model as AnimatedModel;
                    if(m.SwapCullMode == true)
                        GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
                    else
                        GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                }
                else
                {
                    GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                }
                Model.Draw();
            }
      //      clouds.Draw(Camera);
            GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.SetRenderTarget(1, null);
            GraphicsDevice.SetRenderTarget(2, null);
            return g_color;
        }
        private Vector2 toScreenSpace(Vector3 vec, Matrix VP)
        {
            Vector4 position = new Vector4(vec, 1);
            Vector4 result = Vector4.Transform(position, VP);
            result /= result.W;
            return new Vector2(result.X, result.Y);
        }
        public RenderTarget2D LightBufferDraw()
        {
            GraphicsDevice.SetRenderTarget(0, lightBuffer);
            GraphicsDevice.Clear(0, Color.Gray);
            Matrix VP = Camera.ViewMatrix * Camera.ProjectionMatrix;
            DirectionalLight.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(VP));
            //Directional
            DirectionalLight.CurrentTechnique = DirectionalLight.Techniques["PointLight"];
            foreach (ILight light in lights)
            {
                DirectionalLight.Parameters["DepthMap"].SetValue(g_depth);
                DirectionalLight.Parameters["NormalMap"].SetValue(g_normal);
                DirectionalLight.Parameters["LightPosition"].SetValue(light.Position);
                DirectionalLight.Parameters["Color"].SetValue(light.Color);
                DirectionalLight.Parameters["LightDistance"].SetValue(light.Distance);
                DirectionalLight.Parameters["Camera"].SetValue(Camera.Position);
                quad.RenderQuad(DirectionalLight);
             //   quad.RenderQuad(light.Position, light.Distance, Camera, lighting);
            }
            //TODO: off for a while to test dragons
            if(false)
            {
                PointLight.CurrentTechnique = PointLight.Techniques["PointLightGS"];
                PointLight.Parameters["DepthMap"].SetValue(g_depth);
                PointLight.Parameters["NormalMap"].SetValue(g_normal);
                PointLight.Parameters["Camera"].SetValue(Camera.Position);
                PointLight.Parameters["CameraDirection"].SetValue(Vector3.Normalize(Camera.LookAt - Camera.Position) );
                PointLight.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(VP));
                PointLight.Parameters["ViewProjection"].SetValue(VP);
                PointLight.Parameters["View"].SetValue(Camera.ViewMatrix);
                PointLight.Parameters["Projection"].SetValue(Camera.ProjectionMatrix);
                PointLight.Begin();
                PointLight.CurrentTechnique.Passes[0].Begin();
                PointLights.Draw();
                PointLight.CurrentTechnique.Passes[0].End();
                PointLight.End();
            }
            return lightBuffer;
        }
        public RenderTarget2D CombineLightingAndAlbedo()
        {
            CreateSkybox();
            GraphicsDevice.SetRenderTarget(0, resultingBuffer);
            GraphicsDevice.Clear(Color.Black);
            combine.CurrentTechnique = combine.Techniques["PointLight"];
            combine.Parameters["LightMap"].SetValue(lightBuffer);
            combine.Parameters["ColorMap"].SetValue(g_color);
            quad.RenderQuad(combine);
            // clouds.Draw(Camera);
            clouds.DrawClouds(g_depth, Camera);
            clouds.DrawClouds(g_depth, Camera);
            clouds.PrepareParamsShafts(SunOcclTest, Camera);
            quad.RenderQuad(clouds.Effect);
            GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.SetRenderTarget(0, null);
            return resultingBuffer;
        }

        public void CreateSkybox()
        {
            GraphicsDevice.SetRenderTarget(0, g_color);
            clouds.World = Matrix.CreateScale(20) * Matrix.CreateTranslation(Camera.Position);
            clouds.PrepareParamsSkyBox(g_depth, Camera);
            box.draw(clouds.Effect);
            GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.SetRenderTarget(0, SunOcclTest);
            GraphicsDevice.Clear(0, Color.Black);
            clouds.PrepareParamsSun(g_depth, Camera);
            quad.RenderQuad(clouds.Effect);
            GraphicsDevice.SetRenderTarget(0, null);
            //GraphicsDevice.SetRenderTarget(0, Shafts);
            //GraphicsDevice.Clear(0, Color.Black);

            //

        }

    }
}
