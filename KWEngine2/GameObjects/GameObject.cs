using KWEngine2.Model;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.GameObjects
{
    public class GameObject
    {
        private GeoModel _model;
        internal Matrix4 _modelMatrix = Matrix4.Identity;
        private string _name = "undefined gameobject name";
        public string Name
        { 
            get
            {
                return _name;
            }
            set
            {
                _name = value != null && value.Length > 0 ? value : "undefined gameobject name";
            }
        }

        public bool IsValid { get; internal set; } = false;
        public bool HasModel
        {
            get
            {
                return _model.IsValid;
            }
        }

        public GeoModel Model
        {
            get
            {
                return _model;
            }
        }

        public GameObject(GeoModel m)
        {
            _model = m;
        }


    }
}
