using KWEngine2.Collision;
using KWEngine2.GameObjects;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test.Objects.Arena
{
    enum Direction { Up, Down, WaitUp, WaitDown }
    class PlatformUpDown : GameObject
    {
        private Direction _dir = Direction.WaitUp;
        private float _stateTime = 0;

        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            long timeStamp = GetCurrentTimeInMilliseconds();
            if((int)_dir > 1 && timeStamp - _stateTime > 2500)
            {
                ChangeState(timeStamp);
            }

            if (_dir == Direction.WaitUp)
            {
                
            }
            else if (_dir == Direction.WaitDown)
            {

            }
            else if (_dir == Direction.Up)
            {
                MoveOffset(0, 0.1f * deltaTimeFactor, 0);
                if(Position.Y > 5)
                {
                    ChangeState(timeStamp);
                }
            }
            else if (_dir == Direction.Down)
            {
                MoveOffset(0, -0.1f * deltaTimeFactor, 0);
                if (Position.Y <= 1.5f)
                {
                    ChangeState(timeStamp);
                }
            }

            Intersection i = GetIntersection();
            if(i != null && i.Object is Player)
            {
                if(i.MTV.Y > 0)
                {
                    MoveOffset(i.MTV);
                }
            }
        }

        private void ChangeState(long timeStamp)
        {
            if(_dir == Direction.WaitUp)
            {
                _dir = Direction.Up;
            }
            else if (_dir == Direction.WaitDown)
            {
                _dir = Direction.Down;
            }
            else if (_dir == Direction.Up)
            {
                _dir = Direction.WaitDown;
            }
            else
            {
                _dir = Direction.WaitUp;
            }
            _stateTime = timeStamp;
        }
    }
}
