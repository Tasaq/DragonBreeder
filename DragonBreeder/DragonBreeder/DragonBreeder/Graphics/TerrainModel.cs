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
    class Texture2Drgba
    {
        public Texture2D[] Textures;
        public Texture2D[] TexturesNormals;
        public Texture2D R { get { return Textures[0]; } set { Textures[0] = value; } }
        public Texture2D G { get { return Textures[1]; } set { Textures[1] = value; } }
        public Texture2D B { get { return Textures[2]; } set { Textures[2] = value; } }
        public Texture2D A { get { return Textures[3]; } set { Textures[3] = value; } }
        public Texture2D Base { get { return Textures[4]; } set { Textures[4] = value; } }

        public Texture2D R_N { get { return TexturesNormals[0]; } set { TexturesNormals[0] = value; } }
        public Texture2D G_N { get { return TexturesNormals[1]; } set { TexturesNormals[1] = value; } }
        public Texture2D B_N { get { return TexturesNormals[2]; } set { TexturesNormals[2] = value; } }
        public Texture2D A_N { get { return TexturesNormals[3]; } set { TexturesNormals[3] = value; } }
        public Texture2D Base_N { get { return TexturesNormals[4]; } set { TexturesNormals[4] = value; } }
        public Texture2Drgba()
        {
            Textures = new Texture2D[5];
            TexturesNormals = new Texture2D[5];
        }
    }
    class TerrainMaterial
    {
        public Texture2D displacementMap;
        public Texture2D normalMap;
        public Texture2D layersMap;
        public Texture2Drgba Textures;
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
    class TerrainMaterialNEW
    {
        public Texture2D displacementMap;
        public Texture2DArray textures;
        public Texture2DArray blends;
        public Texture2DArray normals;
        public float tesselationFactor;
        public float LOD;
        public float scale;
        public Vector3 Color;
        public TerrainMaterialNEW(Color color)
        {
            displacementMap = null;
            textures = null;
            blends = null;
            normals = null;
            tesselationFactor = 1;
            LOD   = 1;
            scale = 0.5f;
            Color = color.ToVector3();
        }

    }
    class TerrainModel : GraphicObject, IModelEntity
    {

        public  Model Model { get { return null; } }
        int count = 2;
        Quad quad;
        int Dimmensions = 3;
        public Camera Camera { get; set; }
        public Matrix World { get; set; }
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
        public Matrix ViewProjection { get; set; }
        static Effect effect;
        string technique;
        TerrainMaterial Material { get; set; }

        Vector4 rectangle;
        public Vector2 Rectangle
        {
            get
            {
                return new Vector2();
            }
        }

        public TerrainModel(Quad quad)
        {
            Dimmensions = 3;
            this.quad = quad;
            technique = "Render";
            World = Matrix.CreateTranslation(0, 0, 0);
            effect = ContentManager.Load<Effect>("Terrain");
            rectangle = new Vector4();
        }
        public TerrainModel(Quad quad, TerrainMaterial material)
        {
            this.quad = quad;

             Dimmensions = 3;
            this.Material = material;
            technique = "Render";
            if (material.Textures != null)
            {
                technique = "RenderTextured";
            }
            World = Matrix.CreateTranslation(0, 0, 0);
            effect = ContentManager.Load<Effect>("Engine/Terrain");
        }
        public void SetViewProjection(Matrix VP)
        {
            ViewProjection = VP;
        }
        public void Draw()
        {
            effect.Parameters["ViewProjection"].SetValue(ViewProjection);
            if (Material.Textures != null)
            {
                effect.Parameters["LayersMap"].SetValue(Material.layersMap);
                effect.Parameters["Textures"].SetValue(Material.Textures.Textures);
                effect.Parameters["TexturesNormals"].SetValue(Material.Textures.TexturesNormals);
            }

            effect.CurrentTechnique = effect.Techniques[technique];
            effect.Parameters["World"].SetValue(World);

            effect.Parameters["quadID_MAX"].SetValue(Dimmensions * Dimmensions);
            effect.Parameters["WorldViewProjection"].SetValue(World * ViewProjection);
            effect.Parameters["Color"].SetValue(Material.Color);
            effect.Parameters["CameraPosition"].SetValue(Camera.Position);
            effect.Parameters["DisplacementMap"].SetValue(Material.displacementMap);
            effect.Parameters["tfact"].SetValue(25);
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
            quad.RenderQuad();
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
        public void Draw(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques[technique];

            effect.Parameters["World"].SetValue(World);
            effect.Parameters["CameraPosition"].SetValue(Camera.Position);
            effect.Parameters["CameraPosition"].SetValue(Camera.Position - Camera.LookAt);
            effect.Parameters["ViewProjection"].SetValue(ViewProjection);

            effect.Parameters["Color"].SetValue(Material.Color);
            effect.Parameters["DisplacementMap"].SetValue(Material.displacementMap);

            if (Material.Textures != null)
            {
                effect.Parameters["LayersMap"].SetValue(Material.layersMap);
                effect.Parameters["Textures"].SetValue(Material.Textures.Textures);
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
        public void DrawShadows(CSMdata csm)
        {

            effect.Parameters["ViewProjection"].SetValue(ViewProjection);
            if (Material.Textures != null)
            {
                effect.Parameters["LayersMap"].SetValue(Material.layersMap);
                effect.Parameters["Textures"].SetValue(Material.Textures.Textures);
                effect.Parameters["TexturesNormals"].SetValue(Material.Textures.TexturesNormals);
            }
            effect.Parameters["World"].SetValue(World);

            effect.Parameters["quadID_MAX"].SetValue(Dimmensions * Dimmensions);
            effect.Parameters["WorldViewProjection"].SetValue(World * ViewProjection);
            effect.Parameters["ViewProjection"].SetValue(ViewProjection);
            effect.Parameters["Color"].SetValue(Material.Color);
            effect.Parameters["CameraPosition"].SetValue(Camera.Position);
            effect.Parameters["DisplacementMap"].SetValue(Material.displacementMap);
            effect.Parameters["tfact"].SetValue(25);


            effect.Parameters["ViewProjectionSplits"].SetValue(csm.matrix);
            effect.Parameters["clipSpace"].SetValue(csm.dists);

            effect.CurrentTechnique = effect.Techniques["RenderShadows"];
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
            quad.RenderQuad();
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
    }
}
