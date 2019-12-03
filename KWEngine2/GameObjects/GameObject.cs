using KWEngine2.Helper;
using KWEngine2.Model;
using OpenTK;
using OpenTK.Input;
using System;
using System.Diagnostics;

namespace KWEngine2.GameObjects
{
    public abstract class GameObject
    {
        public object Tag { get; protected set; } = null;
        private GeoModel _model;
        private Vector3 _color = new Vector3(1, 1, 1);
        public Vector3 Color {
            get
            {
                return _color;
            }
        }
        internal Matrix4 _modelMatrix = Matrix4.Identity;
        private Quaternion _rotation = new Quaternion(0, 0, 0, 1);
        private Vector3 _scale = new Vector3(1, 1, 1);
        public Quaternion Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
                UpdateModelMatrix();
            }
        }

        private Vector3 _position = new Vector3(0, 0, 0);
        public Vector3 Position 
        {
            get { return _position; }
            set
            {
                _position = value;
                UpdateModelMatrix();
            }
        }
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
                    Debug.WriteLine("Scale must be > 0 in all dimensions. Resetting to 1.");
                }
                UpdateModelMatrix();
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

        private void UpdateModelMatrix()
        {
            _modelMatrix = Matrix4.CreateScale(_scale) * Matrix4.CreateFromQuaternion(_rotation) * Matrix4.CreateTranslation(_position);
            //TODO: Update Hitboxes
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

        #region Gameplay
        public Vector3 GetRotationEulerAngles()
        {
            return HelperRotation.ConvertQuaternionToEulerAngles(Rotation);
        }

        public void SetRotation(float x, float y, float z)
        {
            if (Model is GeoTerrain)
            {
                Debug.WriteLine("Setting rotation on a GeoTerrain instance is not supported.");
            }
            else if (Model != null)
            {
                Quaternion tmpRotateX = Quaternion.FromAxisAngle(Vector3.UnitX, HelperRotation.CalculateRadiansFromDegrees(x));
                Quaternion tmpRotateY = Quaternion.FromAxisAngle(Vector3.UnitY, HelperRotation.CalculateRadiansFromDegrees(y));
                Quaternion tmpRotateZ = Quaternion.FromAxisAngle(Vector3.UnitZ, HelperRotation.CalculateRadiansFromDegrees(z));
                Rotation = new Quaternion(0, 0, 0, 1);
                Rotation = Rotation * tmpRotateZ * tmpRotateY * tmpRotateX;
            }
            else
            {
                throw new Exception("Cannot set rotation on empty geometry object. Did you assign an instance of GeometryObject to your GameObject instance?");
            }
        }

        public void AddRotation(Quaternion r)
        {
            if (Model is GeoTerrain)
            {
                Debug.WriteLine("Adding rotation for GeoTerrain instance is not supported.");
            }
            else
            {
                Rotation = Rotation * r;
            }
        }

        public void AddRotationX(float amount, bool absolute = false)
        {
            if (Model is GeoTerrain)
            {
                Debug.WriteLine("Adding rotation for GeoTerrain instance is not supported.");
            }
            else
            {
                if (absolute)
                {
                    Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitX, HelperRotation.CalculateRadiansFromDegrees(amount));
                    Rotation = tmpRotate * Rotation;
                }
                else
                {
                    Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitX, HelperRotation.CalculateRadiansFromDegrees(amount));
                    Rotation = Rotation * tmpRotate;
                }
            }
        }

        public void AddRotationY(float amount, bool absolute = false)
        {
            if (Model is GeoTerrain)
            {
                Debug.WriteLine("Adding rotation for GeoTerrain instance is not supported.");
            }
            else
            {

                if (absolute)
                {
                    Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitY, HelperRotation.CalculateRadiansFromDegrees(amount));
                    Rotation = tmpRotate * Rotation;
                }
                else
                {
                    Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitY, HelperRotation.CalculateRadiansFromDegrees(amount));
                    Rotation = Rotation * tmpRotate;
                }
            }
        }

        public void AddRotationZ(float amount, bool absolute = false)
        {
            if (Model is GeoTerrain)
            {
                Debug.WriteLine("Adding rotation for GeoTerrain instance is not supported.");
            }
            else
            {
                if (absolute)
                {
                    Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitZ, HelperRotation.CalculateRadiansFromDegrees(amount));
                    Rotation = tmpRotate * Rotation;
                }
                else
                {
                    Quaternion tmpRotate = Quaternion.FromAxisAngle(Vector3.UnitZ, HelperRotation.CalculateRadiansFromDegrees(amount));
                    Rotation = Rotation * tmpRotate;
                }
            }
        }

        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        public void SetPosition(Vector3 newPosition)
        {
            if (Model is GeoTerrain)
            {
                throw new Exception("GeoTerrain may not be repositioned after creation.");
            }
            else if (Model != null)
            {
                Position = new Vector3(newPosition);
            }
            else
                throw new Exception("Cannot set position on empty geometry object. Did you assign an instance of GeometryObject to your GameObject?");

        }

        protected void SetColor(float red, float green, float blue)
        {
            _color.X = red >= 0 && red <= 1 ? red : 1;
            _color.X = green >= 0 && green <= 1 ? green : 1;
            _color.X = blue >= 0 && blue <= 1 ? blue : 1;
        }

        protected Vector3 GetLookAtVector()
        {
            /*
            if (World.GetCurrentWindow().IsFirstPersonMode && World.GetCurrentWindow().GetFirstPersonObject().Equals(this))
            {
                return World.GetCurrentWindow().FirstPersonCamera.GetLookAtVector();
            }
            else
            {*/

            // TODO: FP-View!
                Vector3 standardOrientation = Vector3.UnitZ;
                Vector3 rotatedNormal = Vector3.TransformNormal(standardOrientation, _modelMatrix);
                return rotatedNormal;
            //}
        }

        protected void Move(float amount)
        {
            Position += Vector3.Multiply(GetLookAtVector(), amount);
        }

        protected void MoveXZ(float amount)
        {
            Vector3 tmp = GetLookAtVector();
            tmp.Y = 0;

            Position += Vector3.Multiply(tmp, amount);
        }

        protected void MoveOffset(float x, float y, float z)
        {
            Position = new Vector3(Position.X + x, Position.Y + y, Position.Z + z);
        }

        #endregion


    }
}
