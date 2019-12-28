using KWEngine2.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KWEngine2;
using OpenTK;
using OpenTK.Input;

namespace KWEngine2Test.GameObjects
{
    public class WuerfelTest : GameObject
    {
        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            if (!CurrentWorld.IsFirstPersonMode)
            {
                if (ks[Key.Right])
                    AddRotationY(1, true);
                if (ks[Key.Down])
                    AddRotationX(1, true);
                if (ks[Key.Left])
                    AddRotationY(-1, true);
                if (ks[Key.Up])
                    AddRotationX(-1, true);
            }
        }
    }
}
