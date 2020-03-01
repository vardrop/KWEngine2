using KWEngine2.GameObjects;
using KWEngine2Test.Worlds;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test.Objects
{
    class Follower : GameObject
    {
        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            Player p = (CurrentWorld as GameWorld).GetPlayer();
            SetRotation(GetRotationToTarget(p.Position));

            if(GetDistanceTo(p) > 3)
                Move(0.05f * deltaTimeFactor);

        }
    }
}
