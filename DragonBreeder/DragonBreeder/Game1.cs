using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
namespace DragonBreeder
{
    class Game1 : Game
    {
            private GraphicsDeviceManager graphicsDeviceManager;

            /// <summary>
            /// Initializes a new instance of the <see cref="HelloWorldGame" /> class.
            /// </summary>
            public Game1()
            {
                // Creates a graphics manager. This is mandatory.
                graphicsDeviceManager = new GraphicsDeviceManager(this);
                // Setup the relative directory to the executable directory
                // for loading contents with the ContentManager
                Content.RootDirectory = "Content";
            }

            protected override void Initialize()
            {
                Window.Title = "HelloWorld!";
                base.Initialize();
            }

            protected override void Draw(GameTime gameTime)
            {
                // Clears the screen with the Color.CornflowerBlue
                GraphicsDevice.Clear(Color.CornflowerBlue);
                base.Draw(gameTime);
            }
        }
}
