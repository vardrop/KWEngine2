using KWEngine2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.GameObjects
{
    public class GameObject
    {
        public bool IsValid { get; internal set; } = false;
        private GeoModel _model;

        public GameObject(GeoModel m)
        {
            _model = m;
        }
    }
}
