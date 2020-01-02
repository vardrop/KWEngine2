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
            long now = GetCurrentTimeInMilliseconds();
            if(now - _timeStamp > 3000)
            {
                //Explosion ex = new Explosion(new Vector3(0, 2, 0), 256, 1, 2, 1, ExplosionType.SphereRingY, new Vector4(1, 1, 1, 1));
                //AddGameObject(ex);
                _timeStamp = now;
            }
            
        }
     
        public override void Prepare()
        {
            KWEngine.LoadModelFromFile("Robot", @".\models\roboters\roboters.fbx");
            KWEngine.LoadModelFromFile("Lab", @".\models\labyrinth\walls.obj");
            KWEngine.LoadModelFromFile("Panel", @".\models\spacepanel\scifipanel.obj");

            KWEngine.ShadowMapCoefficient = 0.0005f;
            KWEngine.PostProcessQuality = KWEngine.PostProcessingQuality.High;
            FOVShadow = 25f;
            DebugShadowCaster = false;
            SetSunPosition(250, 250, -250);
            SetSunColor(0.25f, 0.5f, 1, 0.75f);
            SunAmbientFactor = 0.2f;
            SetCameraPosition(100, 100, 100);

            Immovable floor = new Immovable();
            floor.SetModel(GetModel("KWCube"));
            floor.IsCollisionObject = true;
            floor.IsShadowCaster = true;
            floor.SetScale(100, 2, 100);
            floor.SetPosition(0, -1, 0);
            floor.SetTexture(@".\textures\pavement01.jpg");
            floor.SetTexture(@".\textures\pavement01_normal.jpg", KWEngine.TextureType.Normal);
            floor.SetTextureRepeat(10, 10);
            floor.SetSpecularOverride(true, 1, 512);
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
            p.SetModel(GetModel("Robot"));
            p.SetPosition(0, 0f, 0);
            p.SetScale(4);
            p.AnimationID = 0;
            p.AnimationPercentage = 0;
            p.IsShadowCaster = true;
            p.IsCollisionObject = true;
            AddGameObject(p);

            p._flashlight = new Flashlight();
            p._flashlight.Type = LightType.Directional;
            p._flashlight.SetDistanceMultiplier(1);
            p._flashlight.SetColor(1, 0.75f, 0, 1);
            AddLightObject(p._flashlight);

            Immovable lab = new Immovable();
            lab.SetModel(GetModel("Lab"));
            lab.IsCollisionObject = true;
            lab.IsShadowCaster = true;
            //lab.SetSpecularOverride(true, 10, 512);
            AddGameObject(lab);

            Panel panel = new Panel();
            panel.SetModel(GetModel("Panel"));
            panel.SetPosition(10, 0, 0);
            panel.SetScale(3);
            panel.SetSpecularOverride(true, 2, 1024);
            panel.SetTextureForMesh(0, @".\models\spacepanel\scifipanel2.png");
            panel.IsShadowCaster = true;
            panel.IsCollisionObject = true;
            AddGameObject(panel);

            PanelLight pLight = new PanelLight();
            pLight.Type = LightType.Point;
            pLight.SetColor(1, 1, 1, 1);
            pLight.SetPosition(10, 5, 0);
            pLight.SetDistanceMultiplier(0.5f);
            AddLightObject(pLight);
        }

    }
}
