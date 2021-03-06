﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JBBRXG11;
using Microsoft.Xna.Framework;

namespace DragonBreeder
{
    public interface IModelEntity
    {
        Model Model { get; }
        void SetViewProjection(Matrix VP);
        void Draw();
        void Draw(Effect effect);
         ModelMesh GetMesh(string name);
    }
}
