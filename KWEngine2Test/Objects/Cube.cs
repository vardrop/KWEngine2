using KWEngine2.GameObjects;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test.Objects
{
    class Cube : GameObject
    {
        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            if (ks[Key.P])
            {
                AddRotationY(1, true);
            }
            if (ks[Key.I])
            {
                AddRotationX(1, true);
            }
            if (ks[Key.O])
            {
                AddRotationZ(1, true);
            }
        }
    }
}
