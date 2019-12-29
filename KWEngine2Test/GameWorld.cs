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
            /*
            long t = GetCurrentTimeInMilliseconds();
            if (t - _timeStamp > 4000)
            {
                Explosion ex = new Explosion(new Vector3(0, 6, 0), 512, 0.25f, 20, 2, ExplosionType.SphereRingY, new Vector4(1, 0, 0, 1f), null);
                AddGameObject(ex);

                _timeStamp = t;

                ParticleObject p = new ParticleObject(new Vector3(-3, 1, 0), new Vector3(5, 5, 5), ParticleType.BurstHearts);
                //p.SetDuration(3);
                p.SetColor(1, 1, 1, 1f);
                AddParticleObject(p);
                

                
            }
            */
            if (kb[Key.O])
            {
                CurrentWindow.SetWorld(new GameWorld2());

            }
        }
        /*
        public override void Prepare()
        {
            //SoundPlay(".\\audio\\stage01.ogg", true, 0.3f);

            hud1 = new HUDObject(HUDObjectType.Text,96, 128);
            hud1.SetText("Hello World!");
            hud1.SetScale(32, 32);
            AddHUDObject(hud1);

            FOV = 90;
            SetCameraPosition(0, 50, 50);
            SetCameraTarget(0, 0, 0);

            KWEngine.LoadModelFromFile("rect", @".\Models\lambo\Lamborghini_Aventador.fbx");
            KWEngine.LoadModelFromFile("ship", @".\Models\roboters\roboters.fbx");
            KWEngine.BuildTerrainModel("terraX", ".\\textures\\heightmap.png", ".\\textures\\asphalt.jpg", 50, 5, 50, 10, 10);
            
            
            Building go = new Building();
            go.SetModel(GetModel("rect"));
            go.SetPosition(0, 0, 0);
            //go.SetTextureForMesh(0, ".\\models\\spaceship\\spaceship5.jpg");
            go.SetScale(1f);
            //go.AddRotationY(0);
            //go.SetGlow(1, 0, 0, 1);
            go.IsCollisionObject = true;
            go.IsShadowCaster = true;
            //go.IsAffectedBySun = false;
            //go.SetTexture(".\\textures\\holland.jpg", KWEngine.CubeSide.All, KWEngine.TextureType.Diffuse);
            //go.SetTextureRepeat(2, 2, KWEngine.CubeSide.All);
            AddGameObject(go);
            
            
            
            ship = new Ship();
            ship.SetModel(GetModel("ship"));
            ship.IsCollisionObject = true;
            ship.IsShadowCaster = true;
            ship.SetScale(2f);
            ship.SetPosition(0, 3, 2);
            //ship.SetSpecularOverride(true, 1, 8192);
            //block.SetTexture(".\\textures\\holland.jpg", KWEngine.CubeSide.All, KWEngine.TextureType.Diffuse);
            //block.SetTextureRepeat(2, 2, KWEngine.CubeSide.All);
            AddGameObject(ship);
            //SetFirstPersonObject(ship, 0);
            
            
            Terrain t = new Terrain();
            t.SetModel(KWEngine.GetModel("terraX"));
            t.IsShadowCaster = true;
            t.SetPosition(-10, -5, 0);
            t.SetTextureTerrainBlendMapping(".\\textures\\blendmap.jpg", ".\\textures\\holland.jpg", ".\\textures\\world.jpg", ".\\textures\\skybox1.jpg");
            t.IsCollisionObject = true;
            AddGameObject(t);


            Sphere sp = new Sphere();
            sp.SetModel(KWEngine.GetModel("KWSphere"));
            sp.SetPosition(5, 1, 2);
            sp.SetTextureForMesh(0, ".\\textures\\world.jpg");
            AddGameObject(sp);


            Light l = new Light();
            l.SetPosition(0, 3, 0);
            l.SetDistanceMultiplier(2);
            AddLightObject(l);

            SetTextureSkybox(".\\textures\\skybox1.jpg", 1, 1);

            //DebugShadowCaster = true;
        }
        */

        public override void Prepare()
        {
            KWEngine.LoadModelFromFile("Lambo", @".\Models\lambo\Lamborghini_Aventador.fbx");
            SoundPlay(".\\audio\\stage01.ogg", true, 0.3f);
            SetCameraPosition(0, 25, 25);
            /*
            WuerfelTest wt1 = new WuerfelTest();
            wt1.SetModel(KWEngine.GetModel("KWCube"));
            wt1.SetScale(10, 1, 10);
            //wt1.SetScale(1, 1, 1);
            //wt1.SetSpecularOverride(true, 5, 256);
            wt1.SetTextureRepeat(4, 4);
            wt1.SetTexture(".\\textures\\pavement01.jpg", KWEngine.TextureType.Diffuse);
            wt1.SetTexture(".\\textures\\pavement01_normal.jpg", KWEngine.TextureType.Normal);
            //wt1.SetTexture(".\\textures\\mpanel_specular.jpg", KWEngine.TextureType.Specular);
            wt1.IsAffectedBySun = true;
            AddGameObject(wt1);
            */

            /*
            WuerfelTest wt6 = new WuerfelTest();
            wt6.SetModel(KWEngine.GetModel("KWCube6"));
            wt6.SetTexture(".\\textures\\pavement01.jpg", KWEngine.TextureType.Diffuse, KWEngine.CubeSide.All);
            wt6.SetTexture(".\\textures\\pavement01_normal.jpg", KWEngine.TextureType.Normal, KWEngine.CubeSide.All);

            wt6.SetTexture(".\\textures\\asphalt.jpg", KWEngine.TextureType.Diffuse, KWEngine.CubeSide.Top);
            AddGameObject(wt6);
            */

            /*
            KWEngine.BuildTerrainModel("terraX", ".\\textures\\heightmap.png", ".\\textures\\pavement01.jpg", 50, 5, 50, 10, 10);
            TerrainTest t = new TerrainTest();
            t.SetModel(KWEngine.GetModel("terraX"));
            t.SetTexture(".\\textures\\pavement01_normal.jpg", KWEngine.TextureType.Normal);
            t.SetTexture(".\\textures\\asphalt_specular.jpg", KWEngine.TextureType.Specular);
            t.SetSpecularOverride(true, 5, 256);
            AddGameObject(t);
            */

            DebugShowCoordinateSystem = true;

            Sphere sp = new Sphere();
            sp.SetModel(KWEngine.GetModel("Lambo"));
            /*sp.SetTexture(".\\textures\\mpanel_diffuse.jpg");
            sp.SetTexture(".\\textures\\mpanel_normal.jpg", KWEngine.TextureType.Normal);
            sp.SetTexture(".\\textures\\asphalt_specular.jpg", KWEngine.TextureType.Specular);
            sp.SetTextureRepeat(20, 20);*/
            sp.SetPosition(5, 0, -5);
            AddGameObject(sp);
            
            ship = new Ship();
            ship.SetModel(KWEngine.GetModel("KWSphere"));
            ship.SetPosition(0, 0, 25);
            AddGameObject(ship);
            SetFirstPersonObject(ship, 180);

            Light l = new Light();
            l.SetPosition(0, 2, 0);
            l.SetDistanceMultiplier(10);
            //AddLightObject(l);

            SetTextureSkybox(".\\textures\\skybox1.jpg", 1, 1);
        }

    }
}
