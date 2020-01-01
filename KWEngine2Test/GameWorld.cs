using KWEngine2;
using KWEngine2.GameObjects;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KWEngine2Test.Objects;

namespace KWEngine2Test
{
    class GameWorld : World
    {
        private long _timeStamp = 0;

        public override void Act(KeyboardState kb, MouseState ms, float deltaTimeFactor)
        {
            
            
        }
     
        public override void Prepare()
        {
            KWEngine.LoadModelFromFile("Nathan", @".\models\nathan\nathan.fbx");
            SetSunPosition(0, 100, 0);
            SetCameraPosition(100, 100, 100);

            Immovable floor = new Immovable();
            floor.SetModel(GetModel("KWCube"));
            floor.IsCollisionObject = true;
            floor.IsShadowCaster = true;
            floor.SetScale(100, 2, 100);
            floor.SetPosition(0, -1, 0);
            floor.SetTexture(@".\textures\pavement01.jpg");
            floor.SetTextureRepeat(50, 50);
            AddGameObject(floor);

            Immovable wallLeft = new Immovable();
            wallLeft.SetModel(GetModel("KWCube"));
            wallLeft.IsCollisionObject = true;
            wallLeft.IsShadowCaster = true;
            wallLeft.SetScale(2, 10, 100);
            wallLeft.SetPosition(-49, 5, 0);
            AddGameObject(wallLeft);

            Immovable wallRight = new Immovable();
            wallRight.SetModel(GetModel("KWCube"));
            wallRight.IsCollisionObject = true;
            wallRight.IsShadowCaster = true;
            wallRight.SetScale(2, 10, 100);
            wallRight.SetPosition(49, 5, 0);
            AddGameObject(wallRight);

            Immovable wallFront = new Immovable();
            wallFront.SetModel(GetModel("KWCube"));
            wallFront.IsCollisionObject = true;
            wallFront.IsShadowCaster = true;
            wallFront.SetScale(100, 10, 2);
            wallFront.SetPosition(0, 5, 49);
            AddGameObject(wallFront);

            Immovable wallBack = new Immovable();
            wallBack.SetModel(GetModel("KWCube"));
            wallBack.IsCollisionObject = true;
            wallBack.IsShadowCaster = true;
            wallBack.SetScale(100, 10, 2);
            wallBack.SetPosition(0, 5, -49);
            AddGameObject(wallBack);

            Player p = new Player();
            p.SetModel(GetModel("Nathan"));
            p.SetPosition(0, 0f, 0);
            p.SetScale(5);
            p.IsShadowCaster = true;
            p.IsCollisionObject = true;
            AddGameObject(p);


        }

    }
}
