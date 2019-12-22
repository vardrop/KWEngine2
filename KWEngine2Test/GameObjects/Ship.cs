using KWEngine2;
using KWEngine2.Collision;
using KWEngine2.GameObjects;
using KWEngine2.Model;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test.GameObjects
{
    public class Ship : GameObject
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

            if (ks[Key.Q])
                this.MoveOffset(0, -0.1f * deltaTimeFactor, 0);
            if (ks[Key.E])
                this.MoveOffset(0, +0.1f * deltaTimeFactor, 0);

            List<Intersection> intersections = GetIntersections();
            foreach (Intersection i in intersections)
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

            Vector3 pos = GetMouseIntersectionPoint(ms, Plane.Y);
            TurnTowardsXYZ(pos);

            if (ms.LeftButton == ButtonState.Pressed)
            {
                GeoModel shotModel = KWEngine.GetModel("KWCube");
                Shot s = new Shot();
                s.SetModel(shotModel);
                s.SetRotation(Rotation);
                s.SetPosition(Position);
                CurrentWorld.AddGameObject(s);

            }
        }
    }
}
