using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KWEngine2.GameObjects;
using OpenTK.Input;

namespace KWEngine2Test.Objects.SpaceInvaders
{
    class Shot : GameObject
    {
        private float _movementSpeed = 0.2f;

        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            Move(_movementSpeed);

            if (!IsInsideScreenSpace)
                CurrentWorld.RemoveGameObject(this);
        }
    }
}
