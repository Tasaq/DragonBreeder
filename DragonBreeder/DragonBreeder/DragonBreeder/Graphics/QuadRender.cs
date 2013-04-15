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
using Microsoft.Xna.Framework;

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
        public void RenderQuad(Vector3 Position, float distance, Camera camera, Effect effect)
        {
            var billboardWorld = Matrix.Invert(camera.ViewMatrix);
            billboardWorld.Translation = Position;
            effect.Parameters["WorldViewProjection"].SetValue(billboardWorld * camera.ViewMatrix * camera.ProjectionMatrix);

            GraphicsDevice.VertexDeclaration = vdecl;
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
            //GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleListWithAdjacency, vertices, 0, vertices.Length / 4);
            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, 4, ib, 0, 2);
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }

    }
    public class Quad
    {
        private GraphicsDevice GraphicsDevice;
        JBBRXG11.VertexDeclaration vdecl;
        VertexPositionNormalTexture[] vertices;
        private int[] ib = null;
        int primitiveCount;
        public Quad(GraphicsDevice device)
        {

            GraphicsDevice = device;
            vertices = new VertexPositionNormalTexture[] {
                                                   new VertexPositionNormalTexture(new Vector3( 1f, -1f,  0f),new Vector3(0, 0,  -1), new Vector2(1, 1)),
                                                   new VertexPositionNormalTexture(new Vector3(-1f, -1f,  0f),new Vector3(0, 0,  -1), new Vector2(0, 1)),
                                                   new VertexPositionNormalTexture(new Vector3(-1f,  1f,  0f),new Vector3(0, 0,  -1), new Vector2(0, 0)),
                                                   new VertexPositionNormalTexture(new Vector3( 1f,  1f,  0f),new Vector3(0, 0,  -1), new Vector2(1, 0)) };
            primitiveCount = 2;
            ib = new int[] { 0, 1, 2, 2, 3, 0 };
            vdecl = new VertexDeclaration(VertexPositionNormalTexture.VertexDeclaration.GetVertexElements());
        }
        public Quad(GraphicsDevice device, int Count)
        {
            primitiveCount = Count * Count;
            Count += 1;
            GraphicsDevice = device;
            Vector3[] Vertices = new Vector3[Count * Count];
            Vector3[] Normals = new Vector3[Count * Count];
            Vector2[] TexCoords = new Vector2[Count * Count];
            int[,] ind = new int[Count, Count];
            vertices = new VertexPositionNormalTexture[Count * Count];
            int ctr = 0;
            for (int i = 0; i < Count; i++)
            {
                for (int j = 0; j < Count; j++)
                {
                    ind[j,i] = ctr;
                    vertices[ctr].Position = new Vector3((float)j / (float)(Count - 1), (float)i / (float)(Count - 1), 0.5f);
                    vertices[ctr].Position = Vector3.Subtract( Vector3.Multiply( vertices[ctr].Position,2.0f), new Vector3(1.0f));
                    vertices[ctr].Normal = new Vector3(0, 0, -1);
                    vertices[ctr].TextureCoordinate = new Vector2((float)j / (float)(Count - 1), (float)i / (float)(Count - 1));
                    vertices[ctr].TextureCoordinate =new Vector2(1,1);
                    ctr++;
                }
            }
            ib = new int[(Count ) * (Count) *4];
            int counter = 0;
            for (int j = 0; j < Count-1 ; j++)
            {
                for (int i = 0; i < Count-1; i++)
                {
                    ib[counter++] = ind[i, j];
                    ib[counter++] = ind[i, j+1];
                    ib[counter++] = ind[i + 1, j + 1];
                    ib[counter++] = ind[i+1, j];

                }
            }
            vdecl = new VertexDeclaration(VertexPositionNormalTexture.VertexDeclaration.GetVertexElements());
        }
        public void RenderQuad()
        {
            
            GraphicsDevice.VertexDeclaration = vdecl;
            //GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleListWithAdjacency, vertices, 0, vertices.Length / 4);
            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.PatchListWith4ControlPoints, vertices, 0, vertices.Length, ib, 0, primitiveCount*2);
        }
    }
}
