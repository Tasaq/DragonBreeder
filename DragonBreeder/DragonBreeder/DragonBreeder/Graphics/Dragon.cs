using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using JBBRXG11;

namespace DragonBreeder.Graphics
{
    public class Dragon
    {
        public Matrix World { get; set; }
        static readonly int bodyCount = 1;
        static readonly int tailCount = 1;
        int bodyID;
        int tailID;
        AnimatedModel body;
        AnimatedModel tail;
        AnimatedModel head;
        AnimatedModel FlegR;
        AnimatedModel FlegL;
        AnimatedModel HlegR;
        AnimatedModel HlegL;
        AnimatedModel WingR;
        AnimatedModel WingL;
        Random rand = new Random(2);
        List<AnimatedModel> fastEditList = new List<AnimatedModel>();
        public Color DragonColor { get; set; }
        public Dragon()
        {
            DragonColor = Color.Gray;
            World = Matrix.CreateTranslation(0, 0, 0);
            bodyID = rand.Next(1, bodyCount);
            tailID = rand.Next(1, tailCount);
        }
        public void LoadContent()
        {
            body = new AnimatedModel("Dragons/Body/testBody");
            tail = new AnimatedModel("Dragons/Tail/testTail");
            head = new AnimatedModel("Dragons/Head/testHead");
            head = new AnimatedModel("Dragons/Head/testHead");
            FlegL = new AnimatedModel("Dragons/FrontLeg/testLeg");
            FlegR = new AnimatedModel("Dragons/FrontLeg/testLeg");
            FlegR.SwapCullMode = true;
            HlegL = new AnimatedModel("Dragons/HindLeg/testHLeg");
            HlegR = new AnimatedModel("Dragons/HindLeg/testHLeg");
            HlegR.SwapCullMode = true;
            WingL = new AnimatedModel("Dragons/Wing/testWing");
            WingR = new AnimatedModel("Dragons/Wing/testWing");
            WingR.SwapCullMode = true;
        }
        public void AddToGraphicsProcessor(GraphicsProcessor proc)
        {
            proc.Add(body);
            proc.Add(tail);
            proc.Add(head);
            proc.Add(FlegL);
            proc.Add(FlegR);
            proc.Add(HlegL);
            proc.Add(HlegR);
            proc.Add(WingL);
            proc.Add(WingR);
            body.startAnimation("wave");
            tail.startAnimation("wave");
            head.startAnimation("wave");
            FlegR.startAnimation("wave");
            FlegL.startAnimation("wave");
            HlegR.startAnimation("wave");
            HlegL.startAnimation("wave");
            WingR.startAnimation("wave");
            WingL.startAnimation("wave");
            fastEditList.Add(body);
            fastEditList.Add(tail) ;
            fastEditList.Add(head) ;
            fastEditList.Add(FlegR);
            fastEditList.Add(FlegL);
            fastEditList.Add(HlegR);
            fastEditList.Add(HlegL);
            fastEditList.Add(WingR);
            fastEditList.Add(WingL);
        }
        public void Update(GameTime time)
        {
            tail.World  = Matrix.CreateTranslation(body.getBone("tail").Translation) * World;
            head.World  = Matrix.CreateTranslation(body.getBone("head").Translation) * World;
            FlegL.World = Matrix.CreateTranslation(body.getBone("legFL").Translation) * World;
            FlegR.World = Matrix.CreateScale(-1, 1, 1) * Matrix.CreateTranslation(body.getBone("legFR").Translation) * World;
            HlegL.World = Matrix.CreateTranslation(body.getBone("legRL").Translation) * World;
            HlegR.World = Matrix.CreateScale(-1, 1, 1) * Matrix.CreateTranslation(body.getBone("legRR").Translation) * World;
            WingL.World = Matrix.CreateTranslation(body.getBone("wingL").Translation) * World;
            WingR.World = Matrix.CreateScale(-1, 1, 1) * Matrix.CreateTranslation(body.getBone("wingR").Translation) * World;
            body.World  = World;
            foreach (AnimatedModel m in fastEditList)
            {
                foreach (ModelMesh mesh in m.Model.Meshes)
                {
                    MaterialMesh mat = mesh.Material;
                    mat.color = DragonColor.ToVector3();
                    mesh.Material = mat;
                }
                m.Update(time, true);
            }
        }
    }
}
