using KWEngine2;
using KWEngine2.GameObjects;
using KWEngine2.Helper;
using KWEngine2Test.Objects;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test.Worlds
{
    class GameWorldSpaceInvaders : World
    {
        private long _timestampPrevious = 0;

        public override void Act(KeyboardState kb, MouseState ms, float deltaTimeFactor)
        {


            long now = GetCurrentTimeInMilliseconds();
            if (now - _timestampPrevious > 1000)
            {
                _timestampPrevious = now;

                EnemyShip e = new EnemyShip();
                e.SetModel("SpaceshipEnemy");
                e.SetPosition(HelperRandom.GetRandomNumber(-15, 15), 10, 0);
                e.IsCollisionObject = true;
                e.SetScale(1.5f);
                e.SetRotation(90, 0, 0);
                AddGameObject(e);
            }
        }

    public override void Prepare()
    {
            KWEngine.LoadModelFromFile("SpaceshipEnemy", @".\models\spaceship\spaceship4.obj");
            KWEngine.PostProcessQuality = KWEngine.PostProcessingQuality.Standard;
            SetSunPosition(0, 0, 100);
            SetCameraPosition(0, 0, 50);
            //SetTextureBackground(@".\textures\spacebackground.jpg", 2, 2);

        }
}
}
