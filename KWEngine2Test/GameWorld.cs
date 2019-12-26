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

        public override void Act(KeyboardState kbs, MouseState ms)
        {
            long t = GetCurrentTimeInMilliseconds();
            if (t - _timeStamp > 4000)
            {
                Explosion ex = new Explosion(new Vector3(0, 6, 0), 512, 0.25f, 20, 2, ExplosionType.SphereRingY, new Vector4(1, 0, 0, 1f), null);
                AddGameObject(ex);

                _timeStamp = t;
            }
        }

        public override void Prepare()
        {
            FOV = 90;
            SetCameraPosition(0, 10, 10);
            SetCameraTarget(0, 0, 0);

            KWEngine.LoadModelFromFile("rect", @".\Models\cubemattest\cubemattest.obj");
            KWEngine.LoadModelFromFile("ship", @".\Models\roboters\roboters.fbx");
            KWEngine.BuildTerrainModel("terraX", ".\\textures\\heightmap.png", ".\\textures\\asphalt.jpg", 50, 5, 50, 1, 1);
            
            Building go = new Building();
            go.SetModel(GetModel("rect"));
            go.SetPosition(0, 0, 0);
            go.SetScale(1);
            //go.AddRotationY(0);
            //go.SetGlow(1, 0, 0, 1);
            go.IsCollisionObject = true;
            go.IsShadowCaster = true;
            //go.IsAffectedBySun = false;
            //go.SetTexture(".\\textures\\holland.jpg", KWEngine.CubeSide.All, KWEngine.TextureType.Diffuse);
            //go.SetTextureRepeat(2, 2, KWEngine.CubeSide.All);
            AddGameObject(go);
            
            
            
            Ship ship = new Ship();
            ship.SetModel(GetModel("ship"));
            ship.IsCollisionObject = true;
            ship.IsShadowCaster = true;
            ship.SetPosition(0, 6, 0);
            //ship.SetSpecularOverride(true, 1, 8192);
            //block.SetTexture(".\\textures\\holland.jpg", KWEngine.CubeSide.All, KWEngine.TextureType.Diffuse);
            //block.SetTextureRepeat(2, 2, KWEngine.CubeSide.All);
            AddGameObject(ship);
            SetFirstPersonObject(ship, 0);
            

            Terrain t = new Terrain();
            t.SetModel(KWEngine.GetModel("terraX"));
            t.IsShadowCaster = true;
            t.SetPosition(-10, -5, 0);
            t.SetTextureRepeatForMesh(0, 10, 10);
            t.IsCollisionObject = true;
            AddGameObject(t);
            

            Light l = new Light();
            l.SetPosition(0, 3, 0);
            l.SetDistanceMultiplier(2);
            //AddLightObject(l);
        }

    }
}
