using KWEngine2;
using KWEngine2.Collision;
using KWEngine2.GameObjects;
using KWEngine2.Helper;
using KWEngine2.Model;
using KWEngine2Test.Objects;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test.Worlds
{
    class GameWorldVectors1 : World
    {
        public override void Act(KeyboardState kb, MouseState ms, float deltaTimeFactor)
        {
          
        }

        public override void Prepare()
        {
            KWEngine.LoadModelFromFile("Man", @".\models\man.fbx");
            SetCameraPosition(125, 150, 200);
            /*
                        Immovable system = new Immovable();
                        system.SetModel("System");
                        system.SetTextureForMesh(0, @".\textures\AchseMathe.png");
                        system.IsCollisionObject = false;
                        system.AddRotationY(-90);
                        AddGameObject(system);

                        Immovable floor = new Immovable();
                        floor.SetModel("KWCube");
                        floor.IsCollisionObject = true;
                        floor.IsShadowCaster = true;
                        floor.SetScale(50, 5, 50);
                        floor.SetPosition(25, -2.5f, 25);
                        floor.SetTexture(@".\textures\grid_black.png");
                        floor.SetTextureRepeat(10, 10);
                        AddGameObject(floor);
             */
            TestPlayer p = new TestPlayer();
            p.SetModel("Man");
            p.SetPosition(0, 0, 0);
            p.SetScale(10);
            p.IsCollisionObject = false;
            p.IsShadowCaster = true;
            AddGameObject(p);
        }
    }
}
