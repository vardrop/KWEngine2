using KWEngine2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;
using KWEngine2Test.Objects.Arena;

namespace KWEngine2Test.Worlds
{
    class GameWorldArena : World
    {
        private Player _player = new Player();

        public override void Act(KeyboardState kb, MouseState ms, float deltaTimeFactor)
        {
           
        }

        public override void Prepare()
        {
            FOV = 90;
            SetCameraPosition(50, 50, 50);
            SetSunPosition(200, 200, 50);
            SetSunColor(1, 0.75f, 0.5f, 1);

            KWEngine.LoadModelFromFile("ArenaOuter", @".\Models\ArenaOuter\ArenaOuter.fbx");

            KWEngine.BuildTerrainModel("Arena", @".\textures\heightmapArena.png", @".\textures\sand_diffuse.png", 150, 10, 150, 7.5f, 7.5f);
            Immovable terra = new Immovable();
            terra.SetModel("Arena");
            terra.SetPosition(0, -0.5f, 0);
            terra.SetTexture(@".\textures\sand_normal.png", KWEngine.TextureType.Normal);
            AddGameObject(terra);

            Immovable floor = new Immovable();
            floor.SetModel("KWCube");
            floor.SetScale(75, 5, 75);
            floor.SetPosition(0, -2.5f, 0);
            floor.SetTextureRepeat(5, 5);
            floor.IsCollisionObject = true;
            floor.SetTexture(@".\textures\sand_diffuse.png");
            floor.SetTexture(@".\textures\sand_normal.png", KWEngine.TextureType.Normal);
            AddGameObject(floor);

            Immovable arenaOuter = new Immovable();
            arenaOuter.SetModel("ArenaOuter");
            arenaOuter.IsCollisionObject = true;
            arenaOuter.IsShadowCaster = true;
            AddGameObject(arenaOuter);

            _player = new Player();
            _player.SetModel("KWCube");
            _player.SetScale(1, 2, 1);
            _player.IsShadowCaster = false;
            _player.IsCollisionObject = true;
            _player.SetPosition(0, 1, 0);
            _player.FPSEyeOffset = 0.75f;
            AddGameObject(_player);
            SetFirstPersonObject(_player);

            SetTextureSkybox(@".\textures\skybox1.jpg", 1, 0.75f, 0.5f);
            DebugShowPerformanceInTitle = KWEngine.PerformanceUnit.FramesPerSecond;
            //DebugShadowCaster = true;
            //DebugShowHitboxes = true;
        }
    }
}
