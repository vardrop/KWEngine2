using KWEngine2.GameObjects;
using KWEngine2.Helper;
using KWEngine2.Model;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;

namespace KWEngine2
{
    public abstract class World
    {
        private List<GameObject> _gameObjects = new List<GameObject>();
        private List<LightObject> _lightObjects = new List<LightObject>();
        internal int _lightcount = 0;
        public int LightCount
        { 
            get
            {
                return _lightcount;
            }
        }
            
        private List<HUDObject> _hudObjects = new List<HUDObject>();
        private Vector3 _cameraPosition = new Vector3(0, 0, 25);
        private Vector3 _cameraTarget = new Vector3(0, 0, 0);
        private Vector3 _sunPosition = new Vector3(100, 100, 100);
        private Vector3 _sunTarget = new Vector3(0, 0, 0);
        private Vector4 _sunColor = new Vector4(1, 1, 1, 1);
        private float _sunAmbient = 0.25f;

        private float _fov = 45f;
        private float _zFar = 1000f;

        public IReadOnlyCollection<LightObject> GetLightObjects()
        {
            return _lightObjects.AsReadOnly();
        }

        public GLWindow CurrentWindow
        {
            get
            {
                return GLWindow.CurrentWindow;
            }
        }

        public float ZFar
        {
            get
            {
                return _zFar;
            }
            set
            {
                _zFar = value >= 50f ? value : 1000f;
            }
        }

        public float FOV
        {
            get
            {
                return _fov;
            }
            set
            {
                _fov = value > 0 && value <= 120 ? value : 45f;
            }
        }
        public Vector3 GetCameraPosition()
        {
            return _cameraPosition;
        }

        public Vector3 GetCameraTarget()
        {
            return _cameraTarget;
        }

        public void SetCameraPosition(float x, float y, float z)
        {
            _cameraPosition = new Vector3(x, y, z);
        }

        public void SetCameraPosition(Vector3 p)
        {
            _cameraPosition = p;
        }

        public void SetCameraTarget(float x, float y, float z)
        {
            _cameraTarget = new Vector3(x, y, z);
        }
        public void SetCameraTarget(Vector3 p)
        {
            _cameraTarget = p;
        }



        // Sun
        public Vector3 GetSunPosition()
        {
            return _sunPosition;
        }

        public Vector3 GetSunTarget()
        {
            return _sunTarget;
        }

        public void SetSunPosition(float x, float y, float z)
        {
            SetSunPosition(new Vector3(x, y, z));
        }

        public void SetSunPosition(Vector3 p)
        {
            _sunPosition = p;
        }

        public void SetSunTarget(float x, float y, float z)
        {
            SetSunTarget(new Vector3(x, y, z));
        }
        public void SetSunTarget(Vector3 p)
        {
            _sunTarget = p;
        }

        public float SunAmbientFactor
        {
            get
            {
                return _sunAmbient;
            }
            set
            {
                _sunAmbient = Helper.HelperGL.Clamp(value, 0f, 1f);
            }
        }

        public void SetSunColor(float red, float green, float blue, float intensity)
        {
            _sunColor.X = HelperGL.Clamp(red, 0, 1);
            _sunColor.Y = HelperGL.Clamp(green, 0, 1);
            _sunColor.Z = HelperGL.Clamp(blue, 0, 1);
            _sunColor.W = HelperGL.Clamp(intensity, 0, 1);
        }

        public Vector4 GetSunColor()
        {
            return _sunColor;
        }

        

        public abstract void Prepare();

        public abstract void Act(KeyboardState kbs, MouseState ms);

        public GeoModel GetModel(string name)
        {
            return KWEngine.GetModel(name);
        }

        public void AddLightObject(LightObject l)
        {
            lock (_lightObjects)
            {
                if(!(_lightObjects.Contains(l)))
                {
                    _lightObjects.Add(l);
                }

                l.CurrentWorld = this;
                _lightcount = _lightObjects.Count;
            }
        }

        public bool RemoveLightObject(LightObject l)
        {
            bool success = false;
            lock (_lightObjects)
            {
                l.CurrentWorld = null;
                success = _lightObjects.Remove(l);
                _lightcount = _lightObjects.Count;
            }
            return success;
        }

        public void AddGameObject(GameObject g)
        {
            lock (_gameObjects)
            {
                if (!_gameObjects.Contains(g))
                {
                    _gameObjects.Add(g);
                    g.CurrentWorld = this;
                }
                else
                    throw new Exception("GameObject instance " + g.Name + " already in current world.");
            }
            
        }

        public bool RemoveGameObject(GameObject g)
        {
            bool success = false;
            lock(_gameObjects)
            {
                g.CurrentWorld = null;
                success = _gameObjects.Remove(g);
            }
            return success;
        }

        internal void Dispose()
        {
            lock (_gameObjects)
            {
                foreach(GameObject g in _gameObjects)
                {
                    g.IsValid = false;
                }
                _gameObjects.Clear();
            }

            lock (KWEngine.Models)
            {
                foreach(GeoModel m in KWEngine.Models.Values)
                {
                    m.Dispose();
                }
                KWEngine.Models.Clear();
            }
        }

        public IReadOnlyCollection<GameObject> GetGameObjects()
        {
            IReadOnlyCollection<GameObject> returnCollection = null;
            lock (_gameObjects)
            {
                returnCollection = _gameObjects.AsReadOnly();
            }
            return returnCollection;
        }

        internal void SortByZ()
        {
            _gameObjects.Sort((a, b) => a.CompareTo(b));
        }
    }
}
