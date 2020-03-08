﻿using KWEngine2.Collision;
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
    enum Phase { Stand, Jump, Fall }

    class Player : GameObject
    {
        private float _movementSpeed = 0.2f;
        private Phase _phase = Phase.Stand;
        private float _airTime = 0;
        private float _heightAtJumpStart = 0;
        private bool _jumpButtonPressed = false;

        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            // Debugging purposes:
            if (ks[Key.R])
            {
                SetPosition(0, 13, 0);
                _phase = Phase.Fall;
                _airTime = 0;
                _heightAtJumpStart = 13;
            }


            // Basic controls:
            if (CurrentWorld.IsFirstPersonMode && CurrentWorld.GetFirstPersonObject().Equals(this))
            {
                float forward = 0;
                float strafe = 0;
                if (ks[Key.A])
                {
                    strafe -= 1;
                }
                if (ks[Key.D])
                {
                    strafe += 1;
                }
                if (ks[Key.W])
                {
                    forward += 1;
                }
                if (ks[Key.S])
                {
                    forward -= 1;
                }
                
                MoveAndStrafeFirstPerson(forward, strafe, _movementSpeed * deltaTimeFactor);
            }
            

            // Jump controls:
            if(ms.RightButton == ButtonState.Pressed && _phase == Phase.Stand && _jumpButtonPressed == false)
            {
                _jumpButtonPressed = true;
                _phase = Phase.Jump;
                _airTime = 0;
                _heightAtJumpStart = Position.Y;
            }

            if(ms.RightButton == ButtonState.Released)
            {
                _jumpButtonPressed = false;
            }


            // Jump behaviour:
            if(_phase == Phase.Jump)
            {
                _airTime += (deltaTimeFactor * 16.666667f) / 1000f;
                float gain = -14f * (float)Math.Pow(_airTime - 0.4f, 2) + 2.25f;
                SetPositionY(_heightAtJumpStart + gain);

            }
            else if(_phase == Phase.Fall)
            {
                _airTime += (deltaTimeFactor * 16.666667f) / 1000f;
                float gain = 14f * (float)Math.Pow(_airTime, 2);
                SetPositionY(_heightAtJumpStart - gain);
            }
            else if(_phase == Phase.Stand)
            {
                MoveOffset(0, -_movementSpeed * 0.1f, 0);
            }
          

            // Collision detection:
            List<Intersection> intersections = GetIntersections();
            bool upCorrection = false;
            foreach(Intersection i in intersections)
            {
                MoveOffset(i.MTV);

                if (i.MTV.Y > 0)
                {
                    _phase = Phase.Stand;
                    _airTime = 0;
                    upCorrection = true;
                }
                if(i.MTV.Y < 0 && _phase == Phase.Jump)
                {
                    _phase = Phase.Fall;
                    _airTime = 0;
                    _heightAtJumpStart = Position.Y;
                }
            }
            if (!upCorrection && _phase == Phase.Stand)
            {
                _phase = Phase.Fall;
                _airTime = 0;
                _heightAtJumpStart = Position.Y;
            }


            // Has to happen last!
            // (otherwise the camera would be set before 
            //  the collision correction causing the cam
            //  to bob up and down rapidly)
            MoveFPSCamera(ms);
        }
    }
}
