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
using DragonBreeder.Graphics;
using Microsoft.Xna.Framework.Content;

namespace DragonBreeder.TerrainEditing
{
    class Button : Lockable, GUIElement
    {
        bool pressed = false;
        bool clicked = false;
        public bool isClicked
        {
            get
            {
                return clicked;
            }
        }
        Rectangle area;
        Texture2D texture;
        Texture2D onPressTexture;
        Color pressColor = Color.Red;
        String text = null;
        public Color PressedColor
        {
            get
            {
                return pressColor;
            }
            set
            {
                pressColor = value;
            }
        }
        public Vector2 Position
        {
            get
            {
                return new Vector2(area.X, area.Y);
            }
            set
            {
                area.X = (int)value.X;
                area.Y = (int)value.Y;
            }
        }
        public Vector2 Dimmensions
        {
            get
            {
                return new Vector2(area.Width, area.Height);
            }
            set
            {
                area.Width = (int)value.X;
                area.Height = (int)value.Y;
            }
        }
        public Button(Texture2D texture, int Width, int Height)
        {
            area = new Rectangle(0, 0, Width, Height);
            this.texture = texture;
        }
        public Button(Texture2D texture, Texture2D onPressTexture, int Width, int Height)
        {
            area = new Rectangle(0, 0, Width, Height);
            this.texture = texture;
            this.onPressTexture = onPressTexture;
        }
        public void Update()
        {
            clicked = false;
            if ((Lock == true && pressed == true) || (Lock == false && pressed == false))
            {
                MouseState state = Mouse.GetState();
                if (area.Contains(state.X, state.Y) && state.LeftButton == ButtonState.Pressed && pressed == false)
                {
                    pressed = true;
                    Lock = true;
                }
                if (pressed == true && state.LeftButton == ButtonState.Released)
                {
                    pressed = false;
                    Lock = false;
                    if (area.Contains(state.X, state.Y))
                    {
                        clicked = true;
                    }
                }
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!pressed)
            {
                spriteBatch.Draw(texture, area, Color.White);
            }
            else
            {
                if (onPressTexture != null)
                {
                    spriteBatch.Draw(onPressTexture, area, Color.White);
                    if (text != null)
                    {
                        throw new NotImplementedException("Not Yet Implemented");
                    }
                }
                else if (text == null)
                {
                    spriteBatch.Draw(texture, area, pressColor);
                }
                else
                {
                    throw new NotImplementedException("Not Yet Implemented");
                }
            }
        }
    }
}
