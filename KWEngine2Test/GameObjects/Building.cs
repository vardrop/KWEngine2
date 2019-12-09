using KWEngine2.Collision;
using KWEngine2.GameObjects;
using OpenTK.Input;
using System;
using System.Collections.Generic;


namespace KWEngine2Test.GameObjects
{
    public class Building : GameObject
    {
        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            if (ks[Key.D])
                this.MoveOffset(0.1f * deltaTimeFactor, 0, 0);
            if (ks[Key.S])
                this.MoveOffset(0, 0, 0.1f * deltaTimeFactor);
            if (ks[Key.A])
                this.MoveOffset(-0.1f * deltaTimeFactor, 0, 0);
            if (ks[Key.W])
                this.MoveOffset(0, 0, -0.1f * deltaTimeFactor);

            if (AnimationPercentage >= 1)
                AnimationPercentage = 0;
            else
                AnimationPercentage += (0.005f * deltaTimeFactor);

            //Console.WriteLine(AnimationPercentage);
            //AnimationPercentage = 0.25f;
            AnimationID = 0;

            List<Intersection> intersections = GetIntersections();
            foreach(Intersection i in intersections)
            {
                Position += i.MTV;
            }
        }
    }
}
