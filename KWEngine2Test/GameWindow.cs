using KWEngine2;
using KWEngine2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test
{
    class GameWindow : GLWindow
    {
        private GeoModel m;
        public GameWindow()
            :base()
        {
            SetWorld(new GameWorld());
        }
    }
}
