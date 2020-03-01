using KWEngine2;
using KWEngine2Test.Worlds;
using System;

namespace KWEngine2Test
{
    class GameWindow : GLWindow
    {
        public GameWindow()
            :base(1280, 720, OpenTK.GameWindowFlags.Default, 0, false)
        {
            SetWorld(new GameWorldArena());
        }
    }
}
