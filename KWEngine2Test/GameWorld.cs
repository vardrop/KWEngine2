using KWEngine2;
using KWEngine2.GameObjects;
using KWEngine2Test.GameObjects;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test
{
    class GameWorld : World
    {
        private long _timeStamp = 0;
        public Ship ship;
        private HUDObject hud1;
        private float y = 0;

        public override void Act(KeyboardState kb, MouseState ms, float deltaTimeFactor)
        {
            
            long t = GetCurrentTimeInMilliseconds();
            if (t - _timeStamp > 4000)
            {
                Explosion ex = new Explosion(new Vector3(0, 6, 0), 512, 0.25f, 20, 2, ExplosionType.SphereRingY, new Vector4(1, 0, 0, 1f), null);
                AddGameObject(ex);

                _timeStamp = t;

                ParticleObject p = new ParticleObject(new Vector3(-0, 0, 0), new Vector3(5, 5, 5), ParticleType.BurstHearts);
                //p.SetDuration(3);
                p.SetColor(1, 1, 1, 1f);
                AddParticleObject(p);
            }
            
            if (kb[Key.O])
            {
                CurrentWindow.SetWorld(new GameWorld2());

            }
        }
     
        public override void Prepare()
        {
            KWEngine.LoadModelFromFile("Lambo", @".\Models\lambo\Lamborghini_Aventador.fbx");
            //SoundPlay(".\\audio\\stage01.ogg", true, 0.3f);
            SetCameraPosition(0, 75, 75);
           

            DebugShowCoordinateSystem = true;

            Sphere sp = new Sphere();
            sp.SetModel(KWEngine.GetModel("Lambo"));
            sp.SetPosition(6, 0, 0);
            sp.IsCollisionObject = true;
            //AddGameObject(sp);
            
            ship = new Ship();
            ship.SetModel(KWEngine.GetModel("KWSphere"));
            ship.SetPosition(0, 0, 25);
            ship.SetScale(2);
            ship.IsCollisionObject = true;
            AddGameObject(ship);
            SetFirstPersonObject(ship, 180);

            Light l = new Light();
            l.SetPosition(0, 2, 0);
            l.SetDistanceMultiplier(10);
            //AddLightObject(l);

            SetTextureSkybox(".\\textures\\skybox1.jpg", 1, 1);

            /*HUDObject hud1 = new HUDObject(HUDObjectType.Text, 96, 128);
            hud1.SetText("Hello World!");
            hud1.SetScale(32, 32);
            AddHUDObject(hud1);
            */
        }

    }
}
