using KWEngine2.Collision;
using KWEngine2.GameObjects;
using KWEngine2.Helper;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test.Objects.Arena
{
    class Player : GameObject
    {
        private float _movementSpeed = 0.2f;  
        private float _animationPercentage = 0;
        private float _height = 50;

        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            bool runs = false;
            if (CurrentWorld.IsFirstPersonMode && CurrentWorld.GetFirstPersonObject().Equals(this))
            {
                float forward = 0;
                float strafe = 0;
                if (ks[Key.A])
                {
                    strafe -= 1;
                    runs = true;
                }
                if (ks[Key.D])
                {
                    strafe += 1;
                    runs = true;
                }
                if (ks[Key.W])
                {
                    forward += 1;
                    runs = true;
                }
                if (ks[Key.S])
                {
                    forward -= 1;
                    runs = true;
                }
                MoveFPSCamera(ms);
                MoveAndStrafeFirstPerson(forward, strafe, _movementSpeed * deltaTimeFactor);
                
            }
          
            List<Intersection> intersections = GetIntersections();
            foreach(Intersection i in intersections)
            {
                MoveOffset(i.MTV);
            }

            

        }
    }
}
