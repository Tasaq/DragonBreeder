using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DragonBreeder
{
    interface ILight
    {
         Vector3 Position { get; set; }
         float Distance { get; set; }
         Vector3 Color { get; set; }
    }
}
