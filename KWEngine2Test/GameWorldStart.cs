using KWEngine2;
using KWEngine2.GameObjects;
using OpenTK.Input;

namespace KWEngine2Test
{
    class GameWorldStart : World
    {
        private HUDObject _button = null;

        public override void Act(KeyboardState kb, MouseState ms, float deltaTimeFactor)
        {
            if (_button.IsMouseCursorOnMe(ms))
            {
                _button.SetColor(1, 0.75f, 0.5f, 1);

                if (ms.LeftButton == ButtonState.Pressed)
                {
                    CurrentWindow.SetWorld(new GameWorld());
                }
            }
            else
            {
                _button.SetColor(1, 1, 1, 1);
            }
        }
     
        public override void Prepare()
        {
            int imageWidth = 300;
            int imageHeight = 162;
            int width = CurrentWindow.Width;
            int height = CurrentWindow.Height;


            _button = new HUDObject(HUDObjectType.Image, width / 2, height / 2);
            _button.SetTexture(@".\textures\buttonStart.png");
            _button.SetScale(imageWidth, imageHeight);
           
            
            AddHUDObject(_button);
        }

    }
}
