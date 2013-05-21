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
    class ScrollVertical : Lockable, GUIElement
    {
        bool scrolling = false;
        Texture2D scroller;
        Texture2D scroll;
        Rectangle scrollRegion;
        Rectangle scrollerRegion;
        float ScrollerPosition;
        int contentWidth=-1;
        public int ContentWidth
        {
            get
            {
                return contentWidth;
            }
            set
            {
                contentWidth = value;
                scrollerRegion.Height = contentWidth - scrollRegion.Height;
                scrollerRegion.Height = scrollRegion.Height - scrollerRegion.Height;
                if (scrollerRegion.Height < 0)
                {
                    scrollerRegion.Height = scrollRegion.Height;
                }
            }
        }
        public Vector2 Position
        {
            get
            {
                return new Vector2(scrollRegion.X, scrollRegion.Y);
            }
            set
            {
                scrollRegion.X = (int)value.X;
                scrollRegion.Y = (int)value.Y;
                scrollerRegion.X = (int)value.X;
                scrollerRegion.Y = (int)value.Y;
            }
        }
        public Vector2 Dimmensions
        {
            get
            {
                return new Vector2(scrollRegion.Height, scrollRegion.Width);
            }
            set
            {
                scrollRegion.Width = (int)value.Y;
                scrollRegion.Height = (int)value.X;
            }
        }
        public float ValueNormalized
        {
            get
            {
                float a = scrollRegion.Y;
                float b = scrollRegion.Height - scrollerRegion.Height;
                float s = scrollerRegion.Y;
                b -= a;
                s -= a;
                s /= b;
                return s;
            }
        }
        public ScrollVertical(Texture2D Scroll, Texture2D Scroller, int Length, int Width)
        {
            this.scroll = Scroll;
            this.scroller = Scroller;
            scrollRegion = new Rectangle(0, 0, Width, Length);
            scrollerRegion = new Rectangle(0, 0, Width, Length);
            contentWidth = -1;
            ScrollerPosition = 0;
        }
        int ClickPos = 0;
        public void Update()
        {
            if ((Lock == true && scrolling == true) || (Lock == false && scrolling == false))
            {
                MouseState state = Mouse.GetState();
                if (scrolling == false)
                {
                    ClickPos = state.Y;
                }
                if ((scrollerRegion.Contains(state.X, state.Y) || scrolling == true) && state.LeftButton == ButtonState.Pressed)
                {
                    Console.WriteLine("TRUE");
                    Lock = true;
                    scrolling = true;
                    int newPos = (int)MathHelper.Clamp((state.Y-ClickPos), scrollRegion.Y, (float)(scrollRegion.Height-scrollerRegion.Height));
                    scrollerRegion.Y = newPos;
                }
                else
                {
                    Lock = false;
                    scrolling = false;
                }
            }

        }
        public void Draw(JBBRXG11.SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(scroll, scrollRegion, Color.White);
            spriteBatch.Draw(scroller, scrollerRegion, Color.White);
        }
    }
}
