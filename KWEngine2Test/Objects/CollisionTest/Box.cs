using KWEngine2.Collision;
using KWEngine2.GameObjects;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test.Objects.CollisionTest
{
    class Box : GameObject
    {
        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            Intersection i = GetIntersection();
            if(i != null)
            {
                Console.WriteLine(i.MTV);
                MoveOffset(i.MTV);
            }
        }
    }
}
