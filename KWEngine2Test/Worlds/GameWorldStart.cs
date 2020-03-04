using KWEngine2;
using KWEngine2.GameObjects;
using KWEngine2Test.Objects;
using OpenTK.Input;

namespace KWEngine2Test.Worlds
{
    class GameWorldStart : World
    {
        private HUDObject _button = null;
        private HUDObject _button2 = null;

        public override void Act(KeyboardState kb, MouseState ms, float deltaTimeFactor)
        {
            if (_button == null)
                return;

            if (_button.IsMouseCursorOnMe(ms))
            {
                _button.SetGlow(1, 0, 0, 1);

                if (ms.LeftButton == ButtonState.Pressed)
                {
                    CurrentWindow.SetWorld(new GameWorld());
                }
            }
            else
            {
                _button.SetGlow(1, 0, 0, 0);
            }


            if (_button2.IsMouseCursorOnMe(ms))
            {
                _button2.SetGlow(1, 0, 0, 1);

                if (ms.LeftButton == ButtonState.Pressed)
                {
                    CurrentWindow.SetWorld(new GameWorldArena());
                }
            }
            else
            {
                _button2.SetGlow(1, 0, 0, 0);
            }
        }
     
        public override void Prepare()
        {
            KWEngine.PostProcessQuality = KWEngine.PostProcessingQuality.High;

            int imageWidth = 300;
            int imageHeight = 162;
            int width = CurrentWindow.Width;
            int height = CurrentWindow.Height;

            Cube c = new Cube();
            c.SetModel("KWCube6");
            c.SetGlow(0, 1, 0, 1);
            c.SetPosition(0, 4, 0);
            AddGameObject(c);

            _button = new HUDObject(HUDObjectType.Image, width / 2, height / 2 - 100);
            _button.SetTexture(@".\textures\buttonStart.png");
            _button.SetScale(imageWidth * 1.5f, imageHeight * 1.5f);
            AddHUDObject(_button);

            _button2 = new HUDObject(HUDObjectType.Image, width / 2, height / 2 + 200);
            _button2.SetTexture(@".\textures\buttonStart.png");
            _button2.SetScale(imageWidth * 1.5f, imageHeight * 1.5f);
            AddHUDObject(_button2);

            DebugShowPerformanceInTitle = KWEngine.PerformanceUnit.FramesPerSecond;
        }

    }
}
