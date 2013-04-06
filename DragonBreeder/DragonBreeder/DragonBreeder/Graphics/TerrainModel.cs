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

namespace DragonBreeder.Graphics
{
    struct TerrainMaterial
    {
        public Texture2D displacementMap;
        public Texture2D normalMap;
        public Texture2D layersMap;
        public Texture2D[] Textures;
        public float tesselationFactor;
        public float LOD;
        public float scale;
        public Vector3 Color;
        public TerrainMaterial(Color color)
        {
            displacementMap = null;
            normalMap       = null;
            layersMap       = null;
            Textures        = null;
            tesselationFactor = 1;
            LOD   = 1;
            scale = 0.5f;
            Color = color.ToVector3();
        }

    }
    class TerrainModel : GrahicObject, IModelEntity
    {
        int count = 2;
        Quad quad;
        public Matrix World { get; set; }
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
        public Matrix ViewProjection { get; set; }
        static Effect effect;
        string technique;
        TerrainMaterial Material { get; set; }
        public TerrainModel(Quad quad)
        {
            this.quad = quad;
            technique = "Render";

            World = Matrix.CreateTranslation(0, 0, 0);
            effect = ContentManager.Load<Effect>("Terrain");
        }
        public TerrainModel(Quad quad, TerrainMaterial material)
        {
            this.quad = quad;
            this.Material = material;
            technique = "Render";
            if (material.Textures != null)
            {
                technique = "RenderTextured";
            }
            World = Matrix.CreateTranslation(0, 0, 0);
            effect = ContentManager.Load<Effect>("Terrain");
        }
        public void SetViewProjection(Matrix VP)
        {
            ViewProjection = VP;
        }
        public void Draw()
        {
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["ViewProjection"].SetValue(ViewProjection);
            effect.Parameters["WorldViewProjection"].SetValue(World*ViewProjection);
            effect.CurrentTechnique = effect.Techniques[technique];
            effect.Parameters["Color"].SetValue(Material.Color);
            effect.Parameters["DisplacementMap"].SetValue(Material.displacementMap);
            if (Material.Textures != null)
            {
                //  effect.Parameters["ModelTexture"].SetValue(mesh.Material.texture);
            }


            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
            quad.RenderQuad();
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
        public void Draw(Effect effect)
        {
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["ViewProjection"].SetValue(ViewProjection);
            effect.CurrentTechnique = effect.Techniques[technique];
            effect.Parameters["Color"].SetValue(Material.Color);
            if (Material.Textures != null)
            {
                //  effect.Parameters["ModelTexture"].SetValue(mesh.Material.texture);
            }


            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
            quad.RenderQuad();
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
        public ModelMesh GetMesh(string name)
        {
            throw (new Exception("Cannot get mesh from map"));
            return null;
        }
    }
}
