using KWEngine2.Model;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.GameObjects
{
    public abstract class GameObject
    {
        private GeoModel _model;
        internal Matrix4 _modelMatrix = Matrix4.Identity;
        private Vector3 _scale = new Vector3(1, 1, 1);
        public Quaternion Rotation { get; set; } = new Quaternion(0, 0, 0, 1);
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
        public Vector3 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                if (value.X > 0 && value.Y > 0 && value.Z > 0)
                {
                    _scale = value;
                }
                else
                {
                    _scale = new Vector3(1, 1, 1);
                    Debug.WriteLine("Scale must be > 0 in all dimensions.");
                }
            }
        }
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

        public void SetModel(GeoModel m)
        {
            _model = m;
        }

        public abstract void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor); 
    }
}
