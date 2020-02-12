using KWEngine2.GameObjects;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test.Objects
{
    class EnemyShip : GameObject
    {
        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            this.MoveOffset(0f, -0.1f, 0f);

            if (!IsInsideScreenSpace)
            {
                CurrentWorld.RemoveGameObject(this);
                return;
            }
        }
    }
}
