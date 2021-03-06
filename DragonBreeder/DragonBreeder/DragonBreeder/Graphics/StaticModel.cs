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
using DragonBreeder.Graphics;

namespace DragonBreeder
{
    public class StaticModel : GraphicObject, IModelEntity
    {
        Model model;

        public Model Model { get { return model; } }
        public Matrix World { get; set; }
        public Matrix ViewProjection { get; set; }
        static Effect effect;
       // static public ContentManager ContentManager { get; set; }
        string technique;
        public StaticModel(string name)
        {
            technique = "Render";
            model = ContentManager.Load<Model>(name);
            World = Matrix.CreateTranslation(0,0,0);
            effect = ContentManager.Load<Effect>("Engine/StaticModel");
            
            foreach (ModelMesh mesh in model.Meshes)
            {
                if (mesh.Material.texture == null)
                {
                    mesh.Technique = "Render";
                }
                else
                {
                    
                    if (System.IO.File.Exists(ContentManager.RootDirectory + "/Normals/" + mesh.Material.texture.Name + ".xnb"))
                    {
                        Console.WriteLine(ContentManager.RootDirectory+"/Normals/"+mesh.Material.texture.Name+".xnb");
                        mesh.Material.normal = ContentManager.Load<Texture2D>("Normals/" + mesh.Material.texture.Name);
                        mesh.Technique = "RenderTextureNoBlendNormal";
                    }
                    else
                    {
                        mesh.Technique = "RenderTextureNoBlend";
                    }
                }
                foreach (var part in mesh.MeshParts)
                {
                    
                    part.Effect = effect;
                }
            }
        }
        public void SetViewProjection(Matrix VP)
        {
            ViewProjection = VP;
        }
        public void Draw()
        {
            effect.Parameters["ViewProjection"].SetValue(ViewProjection);
            foreach (ModelMesh mesh in model.Meshes)
            {
                effect.CurrentTechnique = effect.Techniques[mesh.Technique];
                effect.Parameters["World"].SetValue(mesh.localTransform*World);
                effect.Parameters["Color"].SetValue(mesh.Material.color);
                if (mesh.Technique == "RenderTextureNoBlend")
                {
                    effect.Parameters["ModelTexture"].SetValue(mesh.Material.texture);
                }
                if (mesh.Technique == "RenderTextureNoBlendNormal")
                {
                    effect.Parameters["ModelTexture"].SetValue(mesh.Material.texture);
                    effect.Parameters["NormalTexture"].SetValue(mesh.Material.normal);
                }


                mesh.Effects[0].Begin();
                mesh.Effects[0].CurrentTechnique.Passes[0].Begin();
                mesh.Draw();
                mesh.Effects[0].CurrentTechnique.Passes[0].End();
                mesh.Effects[0].End();
            }
        }
        public void Draw(Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                    part.Effect = effect;
                mesh.Draw();
            }
        }
        public ModelMesh GetMesh(string name)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                if (name == mesh.Name)
                    return mesh;
            }
            return null;
        }
    }
}
