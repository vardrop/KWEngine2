using KWEngine2;
using System;

namespace KWEngine2Test
{
    class GameWindow : GLWindow
    {
        public GameWindow()
            :base(1280, 720, OpenTK.GameWindowFlags.Default, 0, true)
        {
            SetWorld(new GameWorld());
        }
    }
}
