using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Toolkit.Graphics;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Input;
using Matrix = SharpDX.Matrix;
using Color = SharpDX.Color;
namespace DragonBreeder
{
    class Game1 : Game
    {
            private GraphicsDeviceManager graphicsDeviceManager;
            Model model;
            /// <summary>
            /// Initializes a new instance of the <see cref="Game1" /> class.
            /// </summary>
            public Game1()
            {
                graphicsDeviceManager = new GraphicsDeviceManager(this);
                Content.RootDirectory = "Content";
            }
            protected override void LoadContent()
            {
                model = Content.Load<Model>("sphere");
                base.LoadContent();
            }

            protected override void Initialize()
            {
                Window.Title = "HelloWorld!";
                base.Initialize();
            }

            protected override void Draw(GameTime gameTime)
            {

                GraphicsDevice.Clear(Color.CornflowerBlue);
                model.Draw(GraphicsDevice, Matrix.Translation(0, 0, 0), Matrix.LookAtLH(new Vector3(0, 0, -10), new Vector3(0, 0, 0), new Vector3(0, 1, 0)), Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(45), 16.0f / 9.0f, 0.1f, 100.0f));
                base.Draw(gameTime);
            }
        }
}
