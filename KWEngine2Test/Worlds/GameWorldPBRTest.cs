using KWEngine2;
using KWEngine2.GameObjects;
using OpenTK;
using OpenTK.Input;
using KWEngine2Test.Objects;

namespace KWEngine2Test.Worlds
{
    class GameWorldPBRTest : World
    {
        public override void Act(KeyboardState kb, MouseState ms, float deltaTimeFactor)
        {
        }
     
        public override void Prepare()
        {
            KWEngine.LoadModelFromFile("PBR", @".\models\pbrtest\pbrtest.fbx");

            Immovable i = new Immovable();
            i.SetModel("PBR");
            i.SetScale(5);
            AddGameObject(i);
        }

    }
}
