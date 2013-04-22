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

     class Box3D : GraphicObject
    {
        JBBRXG11.VertexDeclaration vdecl;
        public BoundingBox boundignBox { get; set; }
        public VertexBuffer vertexBuffer { get; private set; }
        public IndexBuffer indexBuffer { get; private set; }
        GraphicsDevice device;
        BasicEffect basicEffect;
        Matrix world = Matrix.CreateTranslation(0, 0.2f, 0);
        Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);
        public Box3D()
        {
            basicEffect = new BasicEffect(GraphicsDevice);
            device = GraphicsDevice;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[8];

            vertices[0] = new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, 1.0f), new Vector3(-1.0f, -1.0f, 1.0f), new Vector2(0, 0));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, 1.0f), new Vector3(1.0f, -1.0f, 1.0f), new Vector2(0, 1));
            vertices[2] = new VertexPositionNormalTexture(new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), new Vector2(1, 1));

            vertices[3] = new VertexPositionNormalTexture(new Vector3(-1.0f, 1.0f, 1.0f), new Vector3(-1.0f, 1.0f, 1.0f), new Vector2(1, 1));
            vertices[4] = new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, -1.0f), new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(0, 0));
            vertices[5] = new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, -1.0f), new Vector3(1.0f, -1.0f, -1.0f), new Vector2(1, 0));

            vertices[6] = new VertexPositionNormalTexture(new Vector3(1.0f, 1.0f, -1.0f), new Vector3(1.0f, 1.0f, -1.0f), new Vector2(1, 0));
            vertices[7] = new VertexPositionNormalTexture(new Vector3(-1.0f, 1.0f, -1.0f), new Vector3(-1.0f, 1.0f, -1.0f), new Vector2(1, 0));
            boundignBox = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            int[] ib = new int[] {
                0, 1, 2, 2, 3, 0, 
				3, 2, 6, 6, 7, 3, 
				7, 6, 5, 5, 4, 7, 
				4, 0, 3, 3, 7, 4, 
				0, 1, 5, 5, 4, 0,
				1, 5, 6, 6, 2, 1  };
            vdecl = new VertexDeclaration(VertexPositionNormalTexture.VertexDeclaration.GetVertexElements());
            vertexBuffer = new VertexBuffer(GraphicsDevice, vdecl, vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            //indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, ib.Length, BufferUsage.WriteOnly);
            indexBuffer = new IndexBuffer(device, ib.Length * 4, BufferUsage.WriteOnly, Microsoft.Xna.Framework.Graphics.IndexElementSize.ThirtyTwoBits);
            indexBuffer.SetData<int>(ib);
        }
        public void draw(Effect effect)
        {
            GraphicsDevice.VertexDeclaration = vdecl;
            GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, vdecl.GetVertexStrideSize());
            GraphicsDevice.Indices = indexBuffer;
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 6*2);
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
    }
}
