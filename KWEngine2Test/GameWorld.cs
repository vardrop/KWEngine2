using KWEngine2;
using KWEngine2.GameObjects;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2Test
{
    class GameWorld : World
    {
        public override void Act(KeyboardState kbs, MouseState ms)
        {
            
        }

        public override void Prepare()
        {
            LoadModelFromFile("building", @".\Models\Schoolpart\building.fbx");
            GameObject go = new GameObject(GetModel("building"));
            AddGameObject(go);
        }
    }
}
