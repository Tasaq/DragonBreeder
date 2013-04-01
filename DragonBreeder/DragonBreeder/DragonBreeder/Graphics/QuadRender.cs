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

namespace DragonBreeder
{
    internal sealed class QuadRender
    {
        private GraphicsDevice GraphicsDevice;
        JBBRXG11.VertexDeclaration vdecl;
        VertexPositionTexture[] vertices;
        private short[] ib = null;
        public QuadRender(GraphicsDevice device)
        {

            GraphicsDevice = device;
            vertices = new VertexPositionTexture[] {
                                                   new VertexPositionTexture(new Vector3( 1f, -1f,  0f), new Vector2(1, 1)),
                                                   new VertexPositionTexture(new Vector3(-1f, -1f,  0f), new Vector2(0, 1)),
                                                   new VertexPositionTexture(new Vector3(-1f,  1f,  0f), new Vector2(0, 0)),
                                                   new VertexPositionTexture(new Vector3( 1f,  1f,  0f), new Vector2(1, 0)) };

            ib = new short[] { 0, 1, 2, 2, 3, 0 };
            vdecl = new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());
        }             

        public void RenderQuad(Effect effect)
        {

            GraphicsDevice.VertexDeclaration = vdecl;
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
            //GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleListWithAdjacency, vertices, 0, vertices.Length / 4);
            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, 4, ib, 0, 2);
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
    }
}
