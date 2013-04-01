using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace DragonBreeder
{
    class Camera
        {
        private Matrix projectionMatrix;
        /// <summary>
        /// Stores the Perspective Projection Matrix
        /// </summary>
        public Matrix ProjectionMatrix
        {
            get
            {
                return projectionMatrix;
            }
        }
        Matrix viewMatrix;
        /// <summary>
        /// Stores the View Matrix
        /// </summary>
        public Matrix ViewMatrix
        {
            get
            {
                viewMatrix = Matrix.CreateLookAt(position, lookAt, tiltNormal);
                return viewMatrix;
            }
        }
        /// <summary>
        /// Position of Camera
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                viewMatrix = Matrix.CreateLookAt(value, lookAt, tiltNormal);
                frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
            }
        }
        private Vector3 position;
        /// <summary>
        /// Camera's target (do not normalize!)
        /// </summary>
        public Vector3 LookAt
        {
            get
            {
                return lookAt;
            }
            set
            {
                lookAt = value;
                viewMatrix = Matrix.CreateLookAt(position, value, tiltNormal);
                frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
            }
        }
        private Vector3 lookAt;
        /// <summary>
        /// set or get Field Of View
        /// </summary>
        public float FOV
        {
            get
            {
                return fov;
            }
            set
            {
                fov = value;
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(value), width / height, nearPlane, farPlane);
                frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
            }
        }
        private float fov;
        /// <summary>
        /// Exposure time of camera, more means more blur
        /// </summary>
        public float ShutterSpeed
        {
            get;
            set;
        }
        /// <summary>
        /// Tilt the camera
        /// </summary>
        public Vector3 TiltNormal
        {
            get
            {
                return tiltNormal;
            }
            set
            {
                tiltNormal = value;
                tiltNormal.Normalize();
                viewMatrix = Matrix.CreateLookAt(position, lookAt, value);
                frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
            }
        }
        private float nearPlane;
        public float NearPlane
        {
            get
            {
                return nearPlane;
            }
            set
            {
                nearPlane = value;
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), width / height, nearPlane, farPlane);
                frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
            }
        }
        private Vector3 tiltNormal;
        private float width = 1280;
        private float height = 720;
        /// <summary>
        /// Far plane (recomended to change only in need)
        /// </summary>
        public float FarPlane
        {
            get
            {
                return farPlane;
            }
            set
            {
                farPlane = value;
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), width / height, nearPlane, value);
                frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
            }
        }
        private float farPlane;
        private BoundingFrustum frustrum;
        /// <summary>
        /// Camera's bounding frustrum
        /// </summary>
        public BoundingFrustum Frustrum
        {
            get
            {
                frustrum = new BoundingFrustum(viewMatrix * ProjectionMatrix);
                return frustrum;
            }
        }
        /// <summary>
        /// Create camera at Zero position looking forward
        /// </summary>
        public Camera()
        {
            nearPlane =1;
            position = Vector3.Zero;
            lookAt = Vector3.Forward;
            fov = 45;
            ShutterSpeed = 1;
            tiltNormal = Vector3.Up;
            farPlane = 50.0f;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), 16.0f/9.0f, .1f, farPlane);
            viewMatrix = Matrix.CreateLookAt(position, lookAt, tiltNormal);
            frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
        }
        /// <summary>
        /// Create camera at given position looking forward
        /// </summary>
        /// <param name="Name">Camera Name</param>
        /// <param name="Position">Camera Position</param>
        public Camera(Vector3 Position)
        {
            nearPlane = 0.1f;
            this.position = Position;
            lookAt = Vector3.Forward;
            fov = 45;
            ShutterSpeed = 1;
            tiltNormal = Vector3.Up;
            farPlane = 200;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), width / height, .1f, farPlane);
            viewMatrix = Matrix.CreateLookAt(Position, lookAt, tiltNormal);
            frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
        }
        /// <summary>
        /// Create's new Camera
        /// </summary>
        /// <param name="Name">Camera Name</param>
        /// <param name="Position">Camera Position</param>
        /// <param name="LookAtVector">Camera's target (do not normalize!)</param>
        public Camera(Vector3 Position, Vector3 LookAtVector)
        {
            nearPlane = 0.1f;
            this.position = Position;
            lookAt = LookAtVector;
            fov = 45;
            ShutterSpeed = 1;
            tiltNormal = Vector3.Up;
            farPlane = 200;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), width / height, nearPlane, farPlane);
            viewMatrix = Matrix.CreateLookAt(Position, lookAt, tiltNormal);
            frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
        }
        /// <summary>
        /// Create's new Camera
        /// </summary>
        /// <param name="Name">Camera Name</param>
        /// <param name="Position">Camera Position</param>
        /// <param name="LookAtVector">Camera's target (do not normalize!)</param>
        /// <param name="FieldOfView">Field Of View</param>
        public Camera(Vector3 Position, Vector3 LookAtVector, float FieldOfView)
        {
            nearPlane = 0.1f;
            this.position = Position;
            lookAt = LookAtVector;
            fov = FieldOfView;
            ShutterSpeed = 1;
            tiltNormal = Vector3.Up;
            farPlane = 100;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), width / height, nearPlane, farPlane);
            viewMatrix = Matrix.CreateLookAt(Position, lookAt, tiltNormal);
            frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
        }
        /// <summary>
        /// Create's new Camera
        /// </summary>
        /// <param name="Name">Camera Name</param>
        /// <param name="Position">Camera Position</param>
        /// <param name="LookAtVector">Camera's target (do not normalize!)</param>
        /// <param name="FieldOfView">Field Of View</param>
        /// <param name="Resolution">Screen Resolution (X-width, Y-height</param>
        public Camera(Vector3 Position, Vector3 LookAtVector, float FieldOfView, Vector2 Resolution)
        {
            nearPlane = 0.1f;
            this.position = Position;
            lookAt = LookAtVector;
            fov = FieldOfView;
            ShutterSpeed = 1;
            tiltNormal = Vector3.Up;
            farPlane = 1000;
            width = Resolution.X;
            height = Resolution.Y;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), width / height, nearPlane, farPlane);
            viewMatrix = Matrix.CreateLookAt(Position, lookAt, tiltNormal);
            frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
        }
        /// <summary>
        /// Create's new Camera
        /// </summary>
        /// <param name="Name">Camera Name</param>
        /// <param name="Position">Camera Position</param>
        /// <param name="LookAtVector">Camera's target (do not normalize!)</param>
        /// <param name="Resolution">Screen Resolution (X-width, Y-height</param>
        public Camera(Vector3 Position, Vector3 LookAtVector, Vector2 Resolution)
        {
            nearPlane = 0.1f;
            this.position = Position;
            lookAt = LookAtVector;
            fov = 45;
            ShutterSpeed = 1;
            tiltNormal = Vector3.Up;
            farPlane = 50;
            width = Resolution.X;
            height = Resolution.Y;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), width / height, nearPlane, farPlane);
            viewMatrix = Matrix.CreateLookAt(Position, lookAt, tiltNormal);
            frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
        }
        /// <summary>
        /// Create camera at Zero position looking forward
        /// </summary>
        /// <param name="Name">Camera Name</param>
        /// <param name="Resolution">Screen Resolution (X-width, Y-height</param>
        public Camera(String Name, Vector2 Resolution)
        {
            nearPlane = 0.1f;
            this.position = Vector3.Zero;
            lookAt = Vector3.Forward;
            fov = 60;
            ShutterSpeed = 1;
            tiltNormal = Vector3.Up;
            farPlane = 100;
            width = Resolution.X;
            height = Resolution.Y;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), width / height, nearPlane, farPlane);
            viewMatrix = Matrix.CreateLookAt(position, lookAt, tiltNormal);
            frustrum = new BoundingFrustum(viewMatrix * projectionMatrix);
        }

    }
}
