using KWEngine2;
using KWEngine2.GameObjects;
using OpenTK;
using OpenTK.Input;
using KWEngine2Test.Objects;

namespace KWEngine2Test
{
    class GameWorldPBRTest : World
    {
        public override void Act(KeyboardState kb, MouseState ms, float deltaTimeFactor)
        {
        }
     
        public override void Prepare()
        {
            Immovable i = new Immovable();
            i.SetModel(GetModel("KWCube"));
            i.SetScale(5);
            AddGameObject(i);
        }

    }
}
