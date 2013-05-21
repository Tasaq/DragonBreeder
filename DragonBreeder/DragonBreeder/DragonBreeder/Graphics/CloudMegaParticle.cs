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

namespace DragonBreeder.Graphics
{
    public class CloudMegaParticle
    {
        public RenderTarget2D g_depth;
        public RenderTarget2D g_normal;
        public RenderTarget2D g_color;
         
        public RenderTarget2D lightBuffer;
         
        public RenderTarget2D resultingBuffer;

        List<Vector4> Spheres;
        StaticModel sphere;
        Random rand;
        public CloudMegaParticle(StaticModel model, Vector3 Position, float size)
        {

            rand = new Random();
            sphere = model;
            Spheres = new List<Vector4>();
            for (int j = 0; j < 20; j++)
            {
                Position.X = (1-2*(float)rand.NextDouble()) * 10.0f + Position.X;
                Position.Y = (1-2*(float)rand.NextDouble()) * 2 + Position.Y;
                Position.Z = (1-2*(float)rand.NextDouble()) * 10.0f + Position.Z;

                for (int i = 0; i < 5; i++)
                {
                    float x = (float)rand.NextDouble() * size*2 + Position.X;
                    float y = (float)rand.NextDouble() * size*2 + Position.Y;
                    float z = (float)rand.NextDouble() * size*2 + Position.Z;
                    float w = (float)rand.NextDouble() * size * 3f;
                    Vector4 sph = new Vector4(x, y, z, w);
                    Spheres.Add(sph);
                }
            }
            foreach (var m in sphere.Model.Meshes)
            {
                MaterialMesh mat = m.Material;
                mat.color = Color.Wheat.ToVector3();
                m.Material = mat;
            }
        }
        public void initRT(GraphicsDevice GraphicsDevice, int width, int height)
        {
            g_depth = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Single, DepthFormat.Depth24);
            g_normal = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Rgba1010102, DepthFormat.Depth24);
            g_color = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            lightBuffer = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);
            resultingBuffer = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);
        }
        public void setGeomRT(GraphicsDevice GraphicsDevice)
        {
            GraphicsDevice.SetRenderTarget(0, g_depth);
            GraphicsDevice.SetRenderTarget(1, g_normal);
            GraphicsDevice.SetRenderTarget(2, g_color);
            GraphicsDevice.Clear(0, Color.Transparent);
            GraphicsDevice.Clear(1, Color.Black);
            GraphicsDevice.Clear(2, Color.LightBlue);
        }
        public void setLightRT(GraphicsDevice GraphicsDevice)
        {
            GraphicsDevice.SetRenderTarget(0, lightBuffer);
            GraphicsDevice.Clear(0, Color.Gray);
        }
        public void setCombineRT(GraphicsDevice GraphicsDevice)
        {
            GraphicsDevice.SetRenderTarget(0, resultingBuffer);
            GraphicsDevice.Clear(Color.Black);
        }
        public void Draw(Matrix VP)
        {
            sphere.ViewProjection = VP;
            foreach (Vector4 sp in Spheres)
            {
                sphere.World = Matrix.CreateScale(sp.W) * Matrix.CreateTranslation(sp.X, sp.Y, sp.Z);
                sphere.Draw();
            }
        }
    }
}
