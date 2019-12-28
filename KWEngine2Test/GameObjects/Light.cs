using KWEngine2.GameObjects;
using KWEngine2.Helper;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test.GameObjects
{
    public class Light : LightObject
    {
        private float degrees = 0;
        private bool right = true;
        private float x = 0;
        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            //SetPosition(HelperRotation.CalculateRotationAroundPointOnAxis(new Vector3(0, 3, 0), 1.25f, degrees, GameObject.Plane.Y));
            //degrees = (degrees + 0.4f) % 360;
            if(right)
            {
                x += 0.025f;
                if (x > 4)
                    right = !right;
            }
            else
            {
                x -= 0.025f;
                if (x < -4)
                    right = !right;
            }
            SetPosition(x, 0, 2.5f);
        }
    }
}
