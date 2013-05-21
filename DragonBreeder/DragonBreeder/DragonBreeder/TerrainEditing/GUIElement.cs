using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DragonBreeder.TerrainEditing
{
    interface GUIElement
    {
        void Update();
        void Draw(JBBRXG11.SpriteBatch spriteBatch);
    }
    public class Lockable
    {
        protected static bool Lock;
        public static bool isLocked
        {
            get
            {
                return Lock;
            }
        }
    }
}
