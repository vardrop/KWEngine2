using KWEngine2.GameObjects;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test.GameObjects
{
    public class Sphere : GameObject
    {
        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            /*Ship s = ((GameWorld)CurrentWorld).ship;
            if (s != null)
            {
                TurnTowardsXZ(s.Position);
            }*/

        }
    }
}
