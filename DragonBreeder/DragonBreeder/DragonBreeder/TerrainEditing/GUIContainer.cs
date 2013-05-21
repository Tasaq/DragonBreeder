using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DragonBreeder.TerrainEditing
{
    class GUIContainer
    {
        List<GUIElement> elements;
        public GUIContainer()
        {
            elements = new List<GUIElement>();
        }
        public void add(GUIElement element)
        {
            elements.Add(element);
        }
        public void Update()
        {
            for (int i = elements.Count - 1; i != -1; i--)
            {
                elements[i].Update();
            }
        }
        public void Draw(JBBRXG11.SpriteBatch spriteBatch)
        {
            //for (int i = elements.Count-1; i !=-1; i--)
            //{
            //    elements[i].Draw(spriteBatch);
            //}
            foreach (GUIElement el in elements)
            {
                el.Draw(spriteBatch);
            }
        }
    }
}
