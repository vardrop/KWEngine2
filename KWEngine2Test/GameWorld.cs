using KWEngine2;
using KWEngine2.GameObjects;
using OpenTK;
using OpenTK.Input;
using KWEngine2Test.Objects;
using KWEngine2.Helper;

namespace KWEngine2Test
{
    class GameWorld : World
    {
        private long _timeStamp = 0;
        private long _timeStampExp = 0;
        private long _timeStampExpDiff = 0;
        private Player p;

        public Player GetPlayer()
        {
            return p;
        }

        public override void Act(KeyboardState kb, MouseState ms, float deltaTimeFactor)
        {
            long now = GetCurrentTimeInMilliseconds();
            if(now - _timeStamp > 3000)
            {
                

                ParticleObject smoke = new ParticleObject(new Vector3(-32.5f, 1, -22.5f), new Vector3(5, 5, 5), ParticleType.LoopSmoke1);
                smoke.SetColor(0.2f, 0.2f, 0.2f, 1);
                smoke.SetDuration(3.25f);
                AddParticleObject(smoke);
                _timeStamp = now;
            }

            if (now - _timeStampExp > _timeStampExpDiff)
            {
                Explosion ex = new Explosion(new Vector3(-35, 2, -24), 64, 1, 8, 1, ExplosionType.Cube, new Vector4(1, 1, 1, 1));
                AddGameObject(ex);

                _timeStampExp = now;
                _timeStampExpDiff = HelperRandom.GetRandomNumber(4000, 10000);

            }

        }
     
        public override void Prepare()
        {
            KWEngine.LoadModelFromFile("Robot", @".\models\UBot\ubot.fbx");
            KWEngine.LoadModelFromFile("Lab", @".\models\labyrinth\walls.obj");
            KWEngine.LoadModelFromFile("Panel", @".\models\spacepanel\scifipanel.obj");
            KWEngine.LoadModelFromFile("CorridorEntrance", @".\models\corridors\corridorEntrance01.fbx");
            KWEngine.LoadModelFromFile("CorridorStraight01", @".\models\corridors\corridorStraight01_NoRoof.fbx");
            KWEngine.LoadModelFromFile("Spaceship", @".\models\spaceship\spaceship4.obj");

            KWEngine.BuildTerrainModel("Terrain", @".\textures\heightmap.png", @".\textures\sand_diffuse.png", 100, 2, 100, 5, 5);
            KWEngine.PostProcessQuality = KWEngine.PostProcessingQuality.Standard;
            KWEngine.ShadowMapCoefficient = 0.0005f;
            FOVShadow = 40f;
            SetSunPosition(250, 250, -250);
            SetSunColor(0.25f, 0.5f, 1, 0.75f);

            SunAmbientFactor = 0.2f;
            SetCameraPosition(100, 100, 100);
            WorldDistance = 1000;

            Immovable corridorEntrance = new Immovable();
            corridorEntrance.SetModel("CorridorEntrance");
            corridorEntrance.SetPosition(-45, 0, 2);
            corridorEntrance.IsCollisionObject = true;
            corridorEntrance.IsShadowCaster = true;
            corridorEntrance.SetRotation(0, 90, 0);
            corridorEntrance.SetScale(5);
            AddGameObject(corridorEntrance);

            Immovable corridorStraight1 = new Immovable();
            corridorStraight1.SetModel("CorridorStraight01");
            corridorStraight1.SetPosition(-60, 0, 0);
            corridorStraight1.IsCollisionObject = true;
            corridorStraight1.IsShadowCaster = true;
            corridorStraight1.SetRotation(0, 90, 0);
            corridorStraight1.SetScale(5);
            AddGameObject(corridorStraight1);

            Immovable corridorStraight2 = new Immovable();
            corridorStraight2.SetModel("CorridorStraight01");
            corridorStraight2.SetPosition(-80, 0, 0);
            corridorStraight2.IsCollisionObject = true;
            corridorStraight2.IsShadowCaster = true;
            corridorStraight2.SetRotation(0, 90, 0);
            corridorStraight2.SetScale(5);
            AddGameObject(corridorStraight2);

            Immovable corridorStraight3 = new Immovable();
            corridorStraight3.SetModel("CorridorStraight01");
            corridorStraight3.SetPosition(-100, 0, 0);
            corridorStraight3.IsCollisionObject = true;
            corridorStraight3.IsShadowCaster = true;
            corridorStraight3.SetRotation(0, 270, 0);
            corridorStraight3.SetScale(5);
            AddGameObject(corridorStraight3);

            Immovable ship = new Immovable();
            ship.SetModel("Spaceship");
            ship.IsCollisionObject = true;
            ship.IsShadowCaster = true;
            ship.SetPosition(-35, 3.5f, -25);
            ship.SetScale(15);
            ship.AddRotationZ(25, true);
            ship.AddRotationX(40, true);
            AddGameObject(ship);

            Immovable floor = new Immovable();
            floor.SetModel("Terrain");
            floor.IsCollisionObject = true;
            floor.IsShadowCaster = true;
            floor.SetTexture(@".\textures\sand_normal.png", KWEngine.TextureType.Normal);
            AddGameObject(floor);


            Immovable wallLeft1 = new Immovable();
            wallLeft1.SetModel("KWCube");
            wallLeft1.IsCollisionObject = true;
            wallLeft1.IsShadowCaster = true;
            wallLeft1.SetScale(2, 10, 38);
            wallLeft1.SetPosition(-49, 5, 29);
            AddGameObject(wallLeft1);

            Immovable wallLeft2 = new Immovable();
            wallLeft2.SetModel("KWCube");
            wallLeft2.IsCollisionObject = true;
            wallLeft2.IsShadowCaster = true;
            wallLeft2.SetScale(2, 10, 34);
            wallLeft2.SetPosition(-49, 5, -33);
            AddGameObject(wallLeft2);

            Immovable wallRight = new Immovable();
            wallRight.SetModel("KWCube");
            wallRight.IsCollisionObject = true;
            wallRight.IsShadowCaster = true;
            wallRight.SetScale(2, 10, 100);
            wallRight.SetPosition(49, 5, 0);
            AddGameObject(wallRight);

            Immovable wallFront = new Immovable();
            wallFront.SetModel("KWCube");
            wallFront.IsCollisionObject = true;
            wallFront.IsShadowCaster = true;
            wallFront.SetScale(100, 10, 2);
            wallFront.SetPosition(0, 5, 49);
            AddGameObject(wallFront);

            Immovable wallBack = new Immovable();
            wallBack.SetModel("KWCube");
            wallBack.IsCollisionObject = true;
            wallBack.IsShadowCaster = true;
            wallBack.SetScale(100, 10, 2);
            wallBack.SetPosition(0, 5, -49);
            AddGameObject(wallBack);

            p = new Player();
            p.SetModel("Robot");
            p.SetPosition(-25, 0f, -15);
            p.SetScale(4);
            p.AnimationID = 0;
            p.AnimationPercentage = 0;
            p.IsShadowCaster = true;
            p.IsCollisionObject = true;
            p.IsPickable = true;
            p.SetSpecularOverride(true, 8, 32);
            AddGameObject(p);
            //SetFirstPersonObject(p);

            p._flashlight = new Flashlight();
            p._flashlight.Type = LightType.DirectionalShadow;
            p._flashlight.SetDistanceMultiplier(2);
            p._flashlight.SetColor(1, 0.75f, 0, 1);
            p._flashlight.SetFOVShadow(180);
            AddLightObject(p._flashlight);

            //Flashlight lo2 = new Flashlight();
            //lo2.Type = LightType.DirectionalShadow;
            //AddLightObject(lo2);
            
            
            Immovable lab = new Immovable();
            lab.SetModel("Lab");
            lab.IsCollisionObject = true;
            lab.IsShadowCaster = true;
            lab.SetSpecularOverride(true, 0, 2048);
            AddGameObject(lab);

            Cube testCube = new Cube();
            testCube.SetModel("KWSphere");
            testCube.SetScale(10);
            testCube.SetPosition(-5, 5, 0);
            testCube.SetSpecularOverride(true, 20, 1024);
            testCube.SetColorOutline(1, 1, 0, 1);
            //AddGameObject(testCube);

            Panel panel = new Panel();
            panel.SetModel("Panel");
            panel.SetPosition(10, 0.25f, -5);
            panel.SetScale(3);
            panel.SetSpecularOverride(true, 2, 1024);
            panel.SetTextureForMesh(0, @".\models\spacepanel\scifipanel2.png");
            panel.IsShadowCaster = true;
            panel.IsPickable = true;
            panel.IsCollisionObject = true;
            AddGameObject(panel);
            
            PanelLight pLight = new PanelLight();
            pLight.Type = LightType.Directional;
            pLight.SetColor(1, 1, 1, 1);
            pLight.SetPosition(10, 5, -5);
            pLight.SetTarget(10, 0, -5);
            pLight.SetDistanceMultiplier(2f);
            AddLightObject(pLight);

            HUDObject ho = new HUDObject(HUDObjectType.Text, 24, 24);
            ho.SetText("kwengine.de");
            ho.SetGlow(1, 0, 0, 1);
            AddHUDObject(ho);

            Follower f = new Follower();
            f.SetModel("Robot");
            f.SetColor(1, 1, 0);
            f.SetScale(6);
            f.SetPosition(15, 15, 0);
            //AddGameObject(f);
        }

    }
}
