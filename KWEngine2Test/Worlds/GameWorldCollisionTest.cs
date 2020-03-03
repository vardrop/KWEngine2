using KWEngine2;
using KWEngine2.GameObjects;
using KWEngine2Test.Objects.CollisionTest;
using OpenTK;
using OpenTK.Input;
using KWEngine2Test.Objects;

namespace KWEngine2Test.Worlds
{
    class GameWorldCollisionTest : World
    {
        public override void Act(KeyboardState kb, MouseState ms, float deltaTimeFactor)
        {
        }
     
        public override void Prepare()
        {
            SetCameraPosition(0, 0, 25);

            Box a = new Box();
            a.SetModel("KWCube");
            a.SetScale(1, 6, 1);
            a.SetPosition(0, 1, 0);
            a.IsCollisionObject = true;
            a.IsShadowCaster = true;
            AddGameObject(a);

            BoxLower b = new BoxLower();
            b.SetModel("KWCube");
            b.SetScale(15,1f,15);
            b.SetPosition(0, 0.5f, 0);
            b.SetColor(1, 0, 0);
            b.IsCollisionObject = true;
            b.IsShadowCaster = true;
            AddGameObject(b);

            //DebugShowHitboxes = true;
        }

    }
}
