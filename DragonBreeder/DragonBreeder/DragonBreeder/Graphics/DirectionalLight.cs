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
    public struct CSMdata
    {
        public Matrix[] matrix { get; set; }
        public float[] dists { get; set; }
        public int csmLayers { get; set; }
    }
    public class DirectionalLight : ILight
    {
        Vector3 position;
        Vector3 color;
        float distance;
        public int csmLayers { get; set; }
        public CSMdata csmData { get; set; }
        public bool Shadows { get; set; }

        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        public Vector3 Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        public float Distance
        {
            get
            {
                return distance;
            }
            set
            {
                distance = value;
            }
        }
        public DirectionalLight()
        {
            Position = new Vector3();
            Color = Microsoft.Xna.Framework.Color.White.ToVector3();
            Distance = float.MaxValue;
            Shadows = true;
            csmLayers = 3;
        }
        public CSMdata CSMstruct(float dist, float coeff, Camera camera)
        {

            float distance = 70.0f;
            CSMdata result = new CSMdata();
            //Direction.Y = -Direction.Y;
            result.matrix = new Matrix[3];
            result.dists = new float[3];
            result.csmLayers = csmLayers;
            Vector2 cameraDir = new Vector2(camera.Position.X + camera.LookAt.X, camera.Position.Z + camera.LookAt.Z);
            cameraDir.Normalize();
            float[] farPlanes = new float[3];
            float[] nearPlanes = new float[3];
            nearPlanes[0] = camera.NearPlane;
            if (csmLayers == 3)
            {
                farPlanes[0] = distance * 0.1f;
                farPlanes[1] = distance * 0.4f;
                farPlanes[2] = distance * 1;
            }
            if (csmLayers == 2)
            {
                farPlanes[0] = distance * 0.35f;
                farPlanes[1] = distance * 1;
                farPlanes[2] = distance * 2;
            }
            if (csmLayers == 1)
            {
                farPlanes[0] = distance * 1;
                farPlanes[1] = distance * 2;
                farPlanes[2] = distance * 3;
            }
            nearPlanes[1] = farPlanes[0];

            nearPlanes[2] = farPlanes[1];

            result.dists[0] = farPlanes[0];
            result.dists[1] = farPlanes[1];
            result.dists[2] = farPlanes[2];
            for (int i = 0; i < 3; i++)
            {
                Matrix split;
                Vector3 testPos = camera.Position - camera.LookAt;
                BoundingFrustum frustum = new BoundingFrustum(
                Matrix.CreateLookAt(camera.Position, camera.Position - Vector3.Normalize(testPos), Vector3.Up) *
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1, nearPlanes[i], farPlanes[i]));
                Matrix tempView;
                Vector3[] pointsWS = new Vector3[8];
                Vector3[] pointsLS = new Vector3[8];
                pointsWS = frustum.GetCorners();
                Vector3 centroid = Vector3.Zero;
                foreach (var a in pointsWS)
                {
                    centroid = Vector3.Add(centroid, a);
                }
                centroid /= 8;
                tempView = Matrix.CreateLookAt(centroid + Vector3.Normalize(position) * (coeff), centroid, Vector3.Up);
                Vector3.Transform(pointsWS, ref tempView, pointsLS);
                float Xmax = pointsLS[0].X,
                      Xmin = pointsLS[0].X,
                      Ymax = pointsLS[0].Y,
                      Ymin = pointsLS[0].Y,
                      Zmax = pointsLS[0].Z,
                      Zmin = pointsLS[0].Z;
                for (int j = 1; j < 8; j++)
                {
                    //max X
                    if (pointsLS[j].X > Xmax)
                    {
                        Xmax = pointsLS[j].X;
                    }
                    //min X
                    if (pointsLS[j].X < Xmin)
                    {
                        Xmin = pointsLS[j].X;
                    }
                    //max Y
                    if (pointsLS[j].Y > Ymax)
                    {
                        Ymax = pointsLS[j].Y;
                    }
                    //min Y
                    if (pointsLS[j].Y < Ymin)
                    {
                        Ymin = pointsLS[j].Y;
                    }
                    //max Z
                    if (pointsLS[j].Z > Zmax)
                    {
                        Zmax = pointsLS[j].Z;
                    }
                    //min Z
                    if (pointsLS[j].Z < Zmin)
                    {
                        Zmin = pointsLS[j].Z;
                    }
                }
                split = Matrix.CreateOrthographicOffCenter(Xmin, Xmax, Ymin, Ymax, -Zmax - coeff, -Zmin);
                result.matrix[i] = tempView * split;
            }
            csmData = result;
            return result;
        }
    }
}
