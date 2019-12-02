using KWEngine2;
using System;

namespace KWEngine2Test
{
    class GameWindow : GLWindow
    {
        public GameWindow()
            :base()
        {
            SetWorld(new GameWorld());
        }
    }
}
