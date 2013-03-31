﻿using System;
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

namespace DragonBreeder
{
    class GraphicsProcessor
    {
        RenderTarget2D g_depth;
        RenderTarget2D g_normal;
        RenderTarget2D g_color;

        RenderTarget2D lightBuffer;

        RenderTarget2D resultingBuffer;

        public Camera Camera { get; set; }

        ContentManager ContentManager;
        GraphicsDevice GraphicsDevice;

        List<IModelEntity> models = new List<IModelEntity>();
        List<ILight> lights = new List<ILight>();


        public GraphicsProcessor(GraphicsDeviceManager device, ContentManager manager,int width, int height)
        {
            Camera = new Camera();

            this.GraphicsDevice = device.GraphicsDevice;
            this.ContentManager = manager;
            g_depth = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Single, DepthFormat.Depth24);
            g_normal = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Rg32, DepthFormat.Depth24);
            g_color = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            lightBuffer = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);
            resultingBuffer = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);

            
        }
        public void LoadContent()
        {

        }
        public void Add(IModelEntity model)
        {
            models.Add(model);
        }
        public RenderTarget2D G_BufferDraw()
        {
            Matrix VP = Camera.ViewMatrix * Camera.ProjectionMatrix;
            GraphicsDevice.SetRenderTarget(0, g_depth);
            GraphicsDevice.SetRenderTarget(1, g_normal);
            GraphicsDevice.SetRenderTarget(2, g_color);
            GraphicsDevice.Clear(0, Color.Black);
            GraphicsDevice.Clear(1, Color.Black);
            GraphicsDevice.Clear(2, Color.Black);
            foreach (var Model in models)
            {
                Model.SetViewProjection(VP);
                Model.Draw();
            }
            GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.SetRenderTarget(1, null);
            GraphicsDevice.SetRenderTarget(2, null);
            return g_color;
        }
    }
}