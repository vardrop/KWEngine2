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

namespace KWEngine2Test.Objects
{
    class TestPlayer : GameObject
    {
        private float _movementspeed = 0.1f;

        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            TurnTowardsXZ(GetMouseIntersectionPoint(ms, Plane.Y));
            Vector3 camLook = CurrentWorld.GetCameraLookAtVector();
            camLook.Y = 0;
            camLook.NormalizeFast();

            if (ks[Key.A])
            {

                MoveAlongVector(HelperRotation.RotateVector(camLook, 90, Plane.Y), _movementspeed);
            }
            if (ks[Key.D])
            {

                MoveAlongVector(HelperRotation.RotateVector(camLook, -90, Plane.Y), _movementspeed);
            }
            if (ks[Key.W])
            {

                MoveAlongVector(camLook, _movementspeed);
            }
            if (ks[Key.S])
            {
                MoveAlongVector(camLook, -_movementspeed);
            }

            Console.WriteLine(Position);
        }
    }
}
