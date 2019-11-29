using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KWEngine2;

namespace KWEngine2Test
{
    class Program
    {
        static void Main(string[] args)
        {
            GameWindow gw = new GameWindow();
            gw.Run(60, 0);
            gw.Dispose();
        }
    }
}
