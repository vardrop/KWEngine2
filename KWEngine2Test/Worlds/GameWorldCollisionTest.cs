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
            SetCameraPosition(0, 1, 15);

            Box a = new Box();
            a.SetModel("KWCube");
            a.SetScale(1, 4, 1);
            a.SetPosition(0, 0.75f, 0);
            a.IsCollisionObject = true;
            AddGameObject(a);

            Immovable b = new Immovable();
            b.SetModel("KWCube");
            b.SetScale(10,0.5f,10);
            b.IsCollisionObject = true;
            AddGameObject(b);

            //DebugShowHitboxes = true;
        }

    }
}
