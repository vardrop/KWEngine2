using KWEngine2.Collision;
using KWEngine2.GameObjects;
using KWEngine2.Helper;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;


namespace KWEngine2Test.GameObjects
{
    public class Building : GameObject
    {
        public override void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor)
        {
            /*
            if (ks[Key.D])
                this.MoveOffset(0.1f * deltaTimeFactor, 0, 0);
            if (ks[Key.S])
                this.MoveOffset(0, 0, 0.1f * deltaTimeFactor);
            if (ks[Key.A])
                this.MoveOffset(-0.1f * deltaTimeFactor, 0, 0);
            if (ks[Key.W])
                this.MoveOffset(0, 0, -0.1f * deltaTimeFactor);

            if (ks[Key.Q])
                this.MoveOffset(0, -0.1f * deltaTimeFactor, 0);
            if (ks[Key.E])
                this.MoveOffset(0, +0.1f * deltaTimeFactor, 0);
                */
            /*
            if (AnimationPercentage >= 1)
                AnimationPercentage = 0;
            else
                AnimationPercentage += (0.005f * deltaTimeFactor);

            //Console.WriteLine(AnimationPercentage);
            //AnimationPercentage = 0.25f;
            //AnimationID = 0;
            */

            /*
            List<Intersection> intersections = GetIntersections();
            foreach(Intersection i in intersections)
            {
                if (i.IsTerrain)
                {
                    SetPosition(Position.X, i.HeightOnTerrainSuggested, Position.Z);
                }
                else
                {
                    Position += i.MTV;
                }
                
            }
            */

            /*
            Console.WriteLine(ms.X + "|" + ms.Y);
            Vector2 mouse = HelperGL.GetNormalizedMouseCoords(ms.X, ms.Y, CurrentWindow);
            Console.WriteLine(mouse.X + "|" + mouse.Y);
            Console.WriteLine("---------");
            */

            Ship s = ((GameWorld)CurrentWorld).ship;
            if(s != null)
            {
                TurnTowardsXZ(s.Position);
            }

            if (ks[Key.M])
            {
                if (IsMouseCursorInsideMyHitbox(ms))
                    SetGlow(1, 0, 0, 1);
                else
                    SetGlow(0, 0, 0, 0);
            }
            else
            {
                SetGlow(0, 0, 0, 0);
            }
        }
    }
}
