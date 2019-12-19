using KWEngine2;
using KWEngine2.GameObjects;
using KWEngine2Test.GameObjects;
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
        public override void Act(KeyboardState kbs, MouseState ms)
        {
            
        }

        public override void Prepare()
        {
            FOV = 90;
            SetCameraPosition(0, 25, 25);
            SetCameraTarget(0, 0, 0);

            KWEngine.LoadModelFromFile("rect", @".\Models\cubemattest\cubemattest.obj");

            KWEngine.BuildTerrainModel("terraX", ".\\textures\\heightmap.png", ".\\textures\\asphalt.jpg", 50, 5, 50, 1, 1);

            Building go = new Building();
            go.SetModel(GetModel("rect"));
            go.SetPosition(0, 7, 0);
            go.SetScale(2);
            go.AddRotationY(0);
            //go.SetGlow(1, 0, 0, 1);
            go.IsCollisionObject = true;
            go.IsShadowCaster = true;
            //go.SetTexture(".\\textures\\holland.jpg", KWEngine.CubeSide.All, KWEngine.TextureType.Diffuse);
            //go.SetTextureRepeat(2, 2, KWEngine.CubeSide.All);
            AddGameObject(go);

            /*
            Block block = new Block();
            block.SetModel(GetModel("KWCube6"));
            block.SetPosition(0, -1, 0);
            block.SetScale(50, 2, 50);
            block.IsCollisionObject = true;
            block.IsShadowCaster = true;
            //block.SetTexture(".\\textures\\holland.jpg", KWEngine.CubeSide.All, KWEngine.TextureType.Diffuse);
            //block.SetTextureRepeat(2, 2, KWEngine.CubeSide.All);
            AddGameObject(block);
            */

            Terrain t = new Terrain();
            t.SetModel(KWEngine.GetModel("terraX"));
            t.IsShadowCaster = true;
            //t.SetPosition(-10, -5, 0);
            t.IsCollisionObject = true;
            AddGameObject(t);


            Light l = new Light();
            l.SetPosition(0, 7, 0);
            l.SetDistanceMultiplier(2);
            AddLightObject(l);
        }

    }
}
