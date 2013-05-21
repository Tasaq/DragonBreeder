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
    public class GraphicsProcessor
    {
        SkyCloudSystem clouds;

        public RenderTarget2D g_depth;
        public RenderTarget2D g_normal;
        public RenderTarget2D g_color;

        RenderTarget2D lightBuffer;
        public RenderTarget2D ShadowAccumulator;

        RenderTarget2D resultingBuffer;
        public RenderTarget2D []ShadowMaps;
        public RenderTarget2DArray rtArrayTest;
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
        Effect PostProcessing;
        Effect VSM;
        Vector2 Resolution;
        public RenderTarget2D SunOcclTest;
        public RenderTarget2D Shafts;
        Texture2D NoiseTexture;
        public RenderTarget2D getDepthBuffer
        {
            get
            {
                return g_depth;
            }
        }
        public CloudMegaParticle cloud;
        public int lightCount {get{
            return PointLights.Count;
        }
        }
        public GraphicsProcessor(GraphicsDeviceManager device, ContentManager manager,int width, int height)
        {
            Resolution = new Vector2(width, height);
            Camera = new Camera(width, height);
            Camera.FarPlane = 1000.0f;
            Camera.NearPlane = 0.1f;
            
            this.GraphicsDevice = device.GraphicsDevice;
            this.ContentManager = manager;
            g_depth = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Single, DepthFormat.Depth24);
            g_normal = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Rgba1010102, DepthFormat.Depth24);
            g_color = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            lightBuffer = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);
            resultingBuffer = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);
            SunOcclTest = new RenderTarget2D(GraphicsDevice, width/6, height/6, true, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);
            Shafts = new RenderTarget2D(GraphicsDevice, width, height, true, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);
            Temp = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);
            
            ShadowAccumulator = new RenderTarget2D(GraphicsDevice, 1280, 720, true, SurfaceFormat.Color, DepthFormat.Depth16);
            rtArrayTest = new RenderTarget2DArray(GraphicsDevice, 512, 512, 3, false, SurfaceFormat.Rg32, DepthFormat.Depth24);
            quad = new QuadRender(GraphicsDevice);
            PointLights = new LightPoints(GraphicsDevice);
            clouds = new SkyCloudSystem(10000);
            clouds.Resolution = Resolution;
            //cloud = new CloudMegaParticle(new StaticModel("sphere"), new Vector3(0, 3, 0), 1);
            //cloud.initRT(GraphicsDevice, width/4, height/4);
            //clouds = null;
        }
        public void LoadContent()
        {
            DirectionalLight = ContentManager.Load<Effect>("Engine/DirectionalLight");
            PointLight = ContentManager.Load<Effect>("Engine/PointLight");
            combine = ContentManager.Load<Effect>("Engine/combine");
            PostProcessing = ContentManager.Load<Effect>("Engine/PostProcess");
            VSM = ContentManager.Load<Effect>("Engine/VSM");
            NoiseTexture = ContentManager.Load<Texture2D>("Noise");
            box = new Box3D();
            if(clouds!=null)
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

            //shadowBufferDraw();
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
                //if (Model is AnimatedModel)
                //{
                //    AnimatedModel m = Model as AnimatedModel;
                //    if(m.SwapCullMode == true)
                //        GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
                //    else
                //        GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                //}
                //else
                //{
                //    GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                //}
                Model.Draw();
            }
            //if (cloud != null)
            //{
            //    cloud.setGeomRT(GraphicsDevice);
            //    cloud.Draw(VP);
            //}
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
            GraphicsDevice.Clear(0, Color.Transparent);
            Matrix VP = Camera.ViewMatrix * Camera.ProjectionMatrix;
            DirectionalLight.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(VP));
            //Directional
            DirectionalLight.CurrentTechnique = DirectionalLight.Techniques["PointLight"];
            foreach (ILight light in lights)
            {
                if ((light as DirectionalLight).Shadows == false)
                {
                    DirectionalLight.CurrentTechnique = DirectionalLight.Techniques["PointLight"];
                }
                else
                {
                    DirectionalLight.CurrentTechnique = DirectionalLight.Techniques["PointLightWithShadows"];
                    DirectionalLight.Parameters["ShadowMap"].SetValue(ShadowAccumulator);

                }
                DirectionalLight.Parameters["DepthMap"].SetValue(g_depth);
                DirectionalLight.Parameters["NormalMap"].SetValue(g_normal);
                DirectionalLight.Parameters["LightPosition"].SetValue(light.Position);
                DirectionalLight.Parameters["Color"].SetValue(light.Color);
                DirectionalLight.Parameters["LightDistance"].SetValue(light.Distance);
                quad.RenderQuad(DirectionalLight);
             //   quad.RenderQuad(light.Position, light.Distance, Camera, lighting);
            }
            //TODO: off for a while to test dragons
            if(true)
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
                PointLight.Parameters["Resolution"].SetValue(Resolution);
                PointLights.Draw(PointLight);

            }
           // GraphicsDevice.SetRenderTarget(0, null);
            // MEGA PARTICLE

            //if (cloud != null)
            //{
            //    cloud.setLightRT(GraphicsDevice);
            //    DirectionalLight.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(VP));
            //    //Directional
            //    DirectionalLight.CurrentTechnique = DirectionalLight.Techniques["PointLight"];
            //    foreach (ILight light in lights)
            //    {
            //        DirectionalLight.Parameters["DepthMap"].SetValue(cloud.g_depth);
            //        DirectionalLight.Parameters["NormalMap"].SetValue(cloud.g_normal);
            //        DirectionalLight.Parameters["LightPosition"].SetValue(light.Position);
            //        DirectionalLight.Parameters["Color"].SetValue(light.Color);
            //        DirectionalLight.Parameters["LightDistance"].SetValue(light.Distance);
            //        DirectionalLight.Parameters["Camera"].SetValue(Camera.Position);
            //        quad.RenderQuad(DirectionalLight);
            //        //   quad.RenderQuad(light.Position, light.Distance, Camera, lighting);
            //    }
            //    //TODO: off for a while to test dragons
            //    if (false)
            //    {
            //        PointLight.CurrentTechnique = PointLight.Techniques["PointLightGS"];
            //        PointLight.Parameters["DepthMap"].SetValue(cloud.g_depth);
            //        PointLight.Parameters["NormalMap"].SetValue(cloud.g_normal);
            //        PointLight.Parameters["Camera"].SetValue(Camera.Position);
            //        PointLight.Parameters["CameraDirection"].SetValue(Vector3.Normalize(Camera.LookAt - Camera.Position));
            //        PointLight.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(VP));
            //        PointLight.Parameters["ViewProjection"].SetValue(VP);
            //        PointLight.Parameters["View"].SetValue(Camera.ViewMatrix);
            //        PointLight.Parameters["Projection"].SetValue(Camera.ProjectionMatrix);
            //        PointLight.Parameters["Resolution"].SetValue(Resolution);

            //       // PointLights.Draw(PointLight);

            //    }
            //}
            return lightBuffer;
        }
        public RenderTarget2D CombineLightingAndAlbedo()
        {

            if (clouds != null)
                CreateSkybox();
            GraphicsDevice.SetRenderTarget(0, resultingBuffer);
            GraphicsDevice.Clear(0, Color.Black);
            combine.CurrentTechnique = combine.Techniques["PointLight"];
            combine.Parameters["LightMap"].SetValue(lightBuffer);
            combine.Parameters["ColorMap"].SetValue(g_color);
            combine.Parameters["DepthMap"].SetValue(g_depth);
            quad.RenderQuad(combine);
            // clouds.Draw(Camera);
            if (clouds != null)
            {
                clouds.DrawClouds(GraphicsDevice,g_depth, Camera);
                //clouds.DrawClouds(GraphicsDevice, g_depth, Camera);
                clouds.PrepareParamsShafts(SunOcclTest, Camera);
                quad.RenderQuad(clouds.Effect);
            }
            GraphicsDevice.SetRenderTarget(0, null);

            if (cloud != null)
            {
                cloud.setCombineRT(GraphicsDevice);
                combine.CurrentTechnique = combine.Techniques["PointLight"];
                combine.Parameters["LightMap"].SetValue(cloud.lightBuffer);
                combine.Parameters["ColorMap"].SetValue(cloud.g_color);
                quad.RenderQuad(combine);
                Blur(cloud.resultingBuffer);
                Noise(cloud.resultingBuffer);
                RadialBlur(cloud.resultingBuffer);
                Blur(cloud.resultingBuffer);
            }
            return resultingBuffer;
        }
        void shadowBufferDraw()
        {
            GraphicsDevice.SetRenderTargetArray(0, rtArrayTest);
            GraphicsDevice.Clear(0, Color.Transparent);
            foreach (ILight l in lights)
            {
                DirectionalLight directlight;
                if (l is DirectionalLight)
                {
                    directlight = l as DirectionalLight;
                }
                else
                {
                    continue;
                }
                if (directlight.Shadows == false)
                {
                    return;
                }
                bool[] preState = new bool[1];
                CSMdata splits;
                splits = directlight.CSMstruct(1, 21.0f, Camera);
                foreach (var Model in models)
                {
                    if (Model is StaticModelTS)
                    {
                    (Model as StaticModelTS).DrawShadows(splits);
                    }
                    if (Model is TerrainModel)
                    {
                    (Model as TerrainModel).DrawShadows(splits);
                    }
                }
            }
            GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.SetRenderTarget(1, null);
            GraphicsDevice.SetRenderTarget(2, null);
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

            //Blur(SunOcclTest);
            //GraphicsDevice.SetRenderTarget(0, Shafts);
            //GraphicsDevice.Clear(0, Color.Black);

            //

        }
        RenderTarget2D Temp;
        public void Blur(RenderTarget2D rt)
        {
            Temp.Dispose();
            Temp = new RenderTarget2D(GraphicsDevice, rt.Width, rt.Height, true, SurfaceFormat.Color, DepthFormat.None);
            GraphicsDevice.SetRenderTarget(0, Temp);
            GraphicsDevice.Clear(Color.Black);
            PostProcessing.CurrentTechnique = PostProcessing.Techniques["BlurH"];
            PostProcessing.Parameters["SourceTexture"].SetValue(rt);
            PostProcessing.Parameters["blurSize"].SetValue(new Vector2(1.0f / rt.Width, 1.0f / rt.Height));
            quad.RenderQuad(PostProcessing);

            GraphicsDevice.SetRenderTarget(0, rt);
            GraphicsDevice.Clear(Color.Black);
            PostProcessing.CurrentTechnique = PostProcessing.Techniques["BlurV"];
            PostProcessing.Parameters["SourceTexture"].SetValue(Temp);
            quad.RenderQuad(PostProcessing);

        }
        RenderTarget2DArray tempArray;
        public void Blur(RenderTarget2DArray rt)
        {
            if(tempArray == null)
            tempArray = new RenderTarget2DArray(GraphicsDevice, rt.Width, rt.Height, rt.Slices, true, rt.Surface, DepthFormat.Depth16);
            GraphicsDevice.SetRenderTargetArray(0, tempArray);
            GraphicsDevice.Clear(Color.Black);
            PostProcessing.CurrentTechnique = PostProcessing.Techniques["RTArrayBlurV"];
            PostProcessing.Parameters["SourceTextureArray"].SetValue(rt);
            PostProcessing.Parameters["blurSize"].SetValue(new Vector2(1.0f / rt.Width, 1.0f / rt.Height));
            quad.RenderQuad(PostProcessing);

            GraphicsDevice.SetRenderTargetArray(0, rt);
            GraphicsDevice.Clear(Color.Black);
            PostProcessing.CurrentTechnique = PostProcessing.Techniques["RTArrayBlurH"];
            PostProcessing.Parameters["SourceTextureArray"].SetValue(tempArray);
            quad.RenderQuad(PostProcessing);

        }
        public void Noise(RenderTarget2D rt)
        {
            GraphicsDevice.SetRenderTarget(0, Temp);
            GraphicsDevice.Clear(Color.Black);
            PostProcessing.CurrentTechnique = PostProcessing.Techniques["NoiseDistort"];
            PostProcessing.Parameters["SourceTexture"].SetValue(rt);
            PostProcessing.Parameters["NoiseTexture"].SetValue(NoiseTexture);
            quad.RenderQuad(PostProcessing);

            GraphicsDevice.SetRenderTarget(0, rt);
            GraphicsDevice.Clear(Color.Black);
            PostProcessing.CurrentTechnique = PostProcessing.Techniques["Copy"];
            PostProcessing.Parameters["SourceTexture"].SetValue(Temp);
            quad.RenderQuad(PostProcessing);

        }
        public void RadialBlur(RenderTarget2D rt)
        {
            GraphicsDevice.SetRenderTarget(0, Temp);
            GraphicsDevice.Clear(Color.Black);
            PostProcessing.CurrentTechnique = PostProcessing.Techniques["RadialBlur"];
            PostProcessing.Parameters["SourceTexture"].SetValue(rt);
            quad.RenderQuad(PostProcessing);

            GraphicsDevice.SetRenderTarget(0, rt);
            GraphicsDevice.Clear(Color.Black);
            PostProcessing.CurrentTechnique = PostProcessing.Techniques["Copy"];
            PostProcessing.Parameters["SourceTexture"].SetValue(Temp);
            quad.RenderQuad(PostProcessing);

        }
        public void RenderShadows()
        {
            Blur(rtArrayTest);
            //Blur(rtArrayTest);
            GraphicsDevice.SetRenderTarget(0, ShadowAccumulator);
            GraphicsDevice.Clear(Color.Black);
            foreach (ILight l in lights)
            {
                DirectionalLight directlight;
                if (l is DirectionalLight)
                {
                    directlight = l as DirectionalLight;
                }
                else
                {
                    continue;
                }

                VSM.CurrentTechnique = VSM.Techniques["Directional"];
                VSM.Parameters["DepthMap"].SetValue(g_depth);
                VSM.Parameters["LightPosition"].SetValue(Vector3.Normalize( directlight.Position));
                VSM.Parameters["CSM_Depth"].SetValue(rtArrayTest);
                VSM.Parameters["LightInvertViewProjection"].SetValue(directlight.csmData.matrix);
                VSM.Parameters["clipSpace1"].SetValue(directlight.csmData.dists[0]);
                VSM.Parameters["clipSpace2"].SetValue(directlight.csmData.dists[1]);
                VSM.Parameters["clipSpace3"].SetValue(directlight.csmData.dists[2]);
                VSM.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(Camera.ViewMatrix * Camera.ProjectionMatrix));

                quad.RenderQuad(VSM);
            }
        }

    }
}
