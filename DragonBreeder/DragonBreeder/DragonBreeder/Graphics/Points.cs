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

namespace DragonBreeder.Graphics
{

    public class LightPoints
    {
        List<PointLight> points;
        private GraphicsDevice GraphicsDevice;
        JBBRXG11.VertexDeclaration vdecl;

        VertexPointLight[] vertices;
        VertexBuffer vbuf;
        public int Count { get { return points.Count; } }
        public LightPoints(GraphicsDevice device)
        {
            GraphicsDevice = device;
            vdecl = new VertexDeclaration(VertexPointLight.VertexDeclaration.GetVertexElements());
            points = new List<PointLight>();
        }
        public void Add(PointLight light)
        {
            points.Add(light);
        }
        void Update()
        {

            if (vertices == null && points.Count > 0)
            {
                vertices = new VertexPointLight[points.Count];
            }
            if (points.Count != vertices.Length)
            {
                vertices = new VertexPointLight[points.Count];
                for (int i = 0; i < points.Count; i++)
                {

                    vertices[i].position =  new Vector4(points[i].Position, points[i].Distance);
                    vertices[i].color = new Color(points[i].Color);
                    vertices[i].texCoord = new Vector2(1.0f,100.5f);
                }
            }
             
        }
        public void Draw(Effect effect)
        {
            Update();
            if (vertices.Length > 0)
            {
                GraphicsDevice.VertexDeclaration = vdecl;
                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
                GraphicsDevice.DrawUserPrimitives<VertexPointLight>(PrimitiveType.PointList, vertices, 0, vertices.Length);
                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }
    }
}
