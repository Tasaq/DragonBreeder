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
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;


namespace DragonBreeder.Graphics
{
    class SkyCloudSystem : GraphicObject
    {
        public Texture2DArray CloudTextures { get; set; }
        List<Vector3> clouds;
        List<int> cloudIDs;
        public Vector2 Bounds;
        public Vector2 OriginPoint;
        public float MinHeight;
        public float MaxHeight;
        public float MinSize;
        public float MaxSize;
        public int MaxCount;
        public int CloudTextureCount;
        JBBRXG11.VertexDeclaration vdecl;
        VertexPositionNormalTexture[] vertices;
        VertexBuffer vbuf;

        Color SkyColor;
        Effect SkyDraw;
        Random rand;
        RenderTarget2D mask;
        public Matrix World { get; set; }
        public SkyCloudSystem(int MaxCount)
        {
            World = new Matrix();
             rand = new Random();
            this.MaxCount = MaxCount;
            Bounds = new Vector2(50, 50);
            OriginPoint = new Vector2(-50, -50);
            MaxHeight = 10.0f;
            MinHeight = 4.0f;
            CloudTextureCount = 2;
            clouds = new List<Vector3>();
            cloudIDs = new List<int>();
            for (int i = 0; i < MaxCount; i++)
            {
                Vector3 newCloud = new Vector3();
                newCloud.X = FloatRandom(Bounds.X, OriginPoint.X);
                newCloud.Z = FloatRandom(Bounds.Y, OriginPoint.Y);
                newCloud.Y = FloatRandom(MinHeight, MaxHeight);
                cloudIDs.Add(rand.Next(0, CloudTextureCount));
                clouds.Add(newCloud);

            }
            vertices = new VertexPositionNormalTexture[clouds.Count];

            for (int i = 0; i < clouds.Count; i++)
            {
                vertices[i].Position = clouds[i];
                vertices[i].TextureCoordinate = new Vector2(cloudIDs[i], (float)rand.NextDouble());
                vertices[i].Normal = new Vector3(0.3f);
            }
            vdecl = new VertexDeclaration(VertexPositionNormalTexture.VertexDeclaration.GetVertexElements());
            vbuf = new VertexBuffer(GraphicsDevice, vdecl, vertices.Length , BufferUsage.WriteOnly);
            vbuf.SetData(vertices);
            
        }
        public void LoadContent()
        {
            SkyDraw = ContentManager.Load<Effect>("CloudySky");
            SkyDraw.CurrentTechnique = SkyDraw.Techniques["RenderCloudsPost"];
            Texture2D []textures = new Texture2D[2];
            textures[0] = ContentManager.Load<Texture2D>("cloud1");
            textures[1] = ContentManager.Load<Texture2D>("cloud2");
            CloudTextures = new Texture2DArray(GraphicsDevice, textures, 512, 512);

            mask = new RenderTarget2D(GraphicsDevice, 1280, 720, true, SurfaceFormat.Color, DepthFormat.Depth16);
        }
        public void DrawClouds(RenderTarget2D DepthBuffer, Camera Camera)
        {
            SkyDraw.CurrentTechnique = SkyDraw.Techniques["RenderCloudsPost"];
            SkyDraw.Parameters["DepthMap"].SetValue(DepthBuffer);
            SkyDraw.Parameters["invProjection"].SetValue(Matrix.Invert(Camera.ProjectionMatrix));
            SkyDraw.Parameters["Clouds"].SetValue(CloudTextures);
            SkyDraw.Parameters["Camera"].SetValue(Camera.Position);
            SkyDraw.Parameters["CameraDirection"].SetValue(Vector3.Normalize(Camera.LookAt - Camera.Position));
            SkyDraw.Parameters["Projection"].SetValue(Camera.ProjectionMatrix);
            SkyDraw.Parameters["View"].SetValue(Camera.ViewMatrix);
            SkyDraw.Parameters["World"].SetValue(Camera.ViewMatrix);
            SkyDraw.Begin();
            SkyDraw.CurrentTechnique.Passes[0].Begin();
            GraphicsDevice.VertexDeclaration = vdecl;
            GraphicsDevice.Vertices[0].SetSource(vbuf, 0, vdecl.GetVertexStrideSize());
            GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, clouds.Count);
            SkyDraw.CurrentTechnique.Passes[0].End();
            SkyDraw.End();
        }
        public void Draw(Camera Camera)
        {
            SkyDraw.CurrentTechnique = SkyDraw.Techniques["RenderClouds"];
            SkyDraw.Parameters["Clouds"].SetValue(CloudTextures);
            SkyDraw.Parameters["Camera"].SetValue(Camera.Position);
            SkyDraw.Parameters["CameraDirection"].SetValue(Vector3.Normalize(Camera.LookAt - Camera.Position));
            SkyDraw.Parameters["Projection"].SetValue(Camera.ProjectionMatrix);
            SkyDraw.Parameters["invProjection"].SetValue(Matrix.Invert(Camera.ProjectionMatrix));
            SkyDraw.Parameters["View"].SetValue(Camera.ViewMatrix);
            SkyDraw.Begin();
            SkyDraw.CurrentTechnique.Passes[0].Begin();
            GraphicsDevice.VertexDeclaration = vdecl;
            GraphicsDevice.Vertices[0].SetSource(vbuf, 0, vdecl.GetVertexStrideSize());
            GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, clouds.Count);
            SkyDraw.CurrentTechnique.Passes[0].End();
            SkyDraw.End();
        }
        public Effect Effect { get { return SkyDraw; } }
        public void PrepareParamsSkyBox(RenderTarget2D Depth, Camera Camera)
        {
            SkyDraw.CurrentTechnique = SkyDraw.Techniques["RenderSky"];
            SkyDraw.Parameters["Clouds"].SetValue(CloudTextures);
            SkyDraw.Parameters["Camera"].SetValue(Camera.Position);
            SkyDraw.Parameters["CameraDirection"].SetValue(Vector3.Normalize(Camera.LookAt - Camera.Position));
            SkyDraw.Parameters["Projection"].SetValue(Camera.ProjectionMatrix);
            SkyDraw.Parameters["invProjection"].SetValue(Matrix.Invert(Camera.ProjectionMatrix));
            SkyDraw.Parameters["ViewProjection"].SetValue(Camera.ViewMatrix * Camera.ProjectionMatrix);
            SkyDraw.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(Camera.ViewMatrix * Camera.ProjectionMatrix));
            SkyDraw.Parameters["World"].SetValue(World);
        }
        public void PrepareParamsSun(RenderTarget2D Depth, Camera Camera)
        {
            SkyDraw.CurrentTechnique = SkyDraw.Techniques["FullScreenSun"];
            SkyDraw.Parameters["Clouds"].SetValue(CloudTextures);
            SkyDraw.Parameters["Camera"].SetValue(Camera.Position);
            SkyDraw.Parameters["CameraDirection"].SetValue(Vector3.Normalize(Camera.LookAt - Camera.Position));
            SkyDraw.Parameters["Projection"].SetValue(Camera.ProjectionMatrix);
            SkyDraw.Parameters["invProjection"].SetValue(Matrix.Invert(Camera.ProjectionMatrix));
            SkyDraw.Parameters["ViewProjection"].SetValue(Camera.ViewMatrix * Camera.ProjectionMatrix);
            SkyDraw.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(Camera.ViewMatrix * Camera.ProjectionMatrix));
            SkyDraw.Parameters["World"].SetValue(World);
        }
        public void PrepareParamsShafts(RenderTarget2D OcculusionMap, Camera Camera)
        {
            SkyDraw.CurrentTechnique = SkyDraw.Techniques["FullScreenShaft"];
            SkyDraw.Parameters["Clouds"].SetValue(CloudTextures);
            SkyDraw.Parameters["Camera"].SetValue(Camera.Position);
            SkyDraw.Parameters["OcculusionMap"].SetValue(OcculusionMap);
            SkyDraw.Parameters["CameraDirection"].SetValue(Vector3.Normalize(Camera.LookAt - Camera.Position));
            SkyDraw.Parameters["Projection"].SetValue(Camera.ProjectionMatrix);
            SkyDraw.Parameters["invProjection"].SetValue(Matrix.Invert(Camera.ProjectionMatrix));
            SkyDraw.Parameters["ViewProjection"].SetValue(Camera.ViewMatrix * Camera.ProjectionMatrix);
            SkyDraw.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(Camera.ViewMatrix * Camera.ProjectionMatrix));
            SkyDraw.Parameters["World"].SetValue(World);
        }
        float FloatRandom(float one, float two)
        {
            return one + (float)rand.NextDouble() * (two - one);
        }

    }
}
