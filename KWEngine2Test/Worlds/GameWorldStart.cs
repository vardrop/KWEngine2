using KWEngine2;
using KWEngine2.GameObjects;
using KWEngine2Test.Objects;
using OpenTK.Input;

namespace KWEngine2Test.Worlds
{
    class GameWorldStart : World
    {
        private HUDObject _button = null;

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

            _button = new HUDObject(HUDObjectType.Image, width / 2, height / 2);
            _button.SetTexture(@".\textures\buttonStart.png");
            _button.SetScale(imageWidth * 2, imageHeight * 2);

            AddHUDObject(_button);

            DebugShowPerformanceInTitle = KWEngine.PerformanceUnit.FrameTimeInMilliseconds;
        }

    }
}
