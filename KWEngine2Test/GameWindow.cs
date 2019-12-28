using KWEngine2;
using System;

namespace KWEngine2Test
{
    class GameWindow : GLWindow
    {
        public GameWindow()
            :base(1920, 1080, OpenTK.GameWindowFlags.Default, 0, true)
        {
            SetWorld(new GameWorld());
        }
    }
}
