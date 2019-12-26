using KWEngine2.GameObjects;
using KWEngine2.Helper;
using KWEngine2.Model;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using System.Linq;

namespace KWEngine2
{
    public abstract class World
    {

        internal bool _prepared = false;
        private float _worldDistance = 100;
        public Vector3 WorldCenter { get; set; } = new Vector3(0, 0, 0);
        public float WorldDistance
        {
            get
            {
                return _worldDistance;
            }
            set
            {
                if (value > 0)
                {
                    _worldDistance = value;
                }
                else
                {
                    _worldDistance = 200f;
                    Debug.WriteLine("WorldDistance needs to be > 0");
                }
            }
        }
        private GameObject _firstPersonObject = null;

        public bool IsFirstPersonMode
        {
            get
            {
                return _firstPersonObject != null && _gameObjects.Contains(_firstPersonObject);
            }
        }

        public GameObject GetFirstPersonObject()
        {
            return _firstPersonObject;
        }

        public long GetCurrentTimeInMilliseconds()
        {
            return Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
        }

        public void SetFirstPersonObject(GameObject go, float startRotationInDegrees = 0)
        {
            if (go != null)
            {
                _firstPersonObject = go;
                CurrentWindow.CursorVisible = false;
                Mouse.SetPosition(CurrentWindow.X + CurrentWindow.Width / 2, CurrentWindow.Y + CurrentWindow.Height / 2);
                go.SetRotation(0, startRotationInDegrees, 0);
                HelperCamera.SetStartRotation(go);
                //HelperCamera.SetStartRotationY(Quaternion.FromAxisAngle(KWEngine.WorldUp, MathHelper.DegreesToRadians(startRotationInDegrees)), go);
            }
            else
            {
                CurrentWindow.CursorVisible = true;
                _firstPersonObject = null;
                HelperCamera.DeleteFirstPersonObject();
            }
        }


        internal List<GameObject> _gameObjects = new List<GameObject>();
        internal List<LightObject> _lightObjects = new List<LightObject>();

        internal List<GameObject> _gameObjectsTBA = new List<GameObject>();
        internal List<LightObject> _lightObjectsTBA = new List<LightObject>();

        internal List<GameObject> _gameObjectsTBR = new List<GameObject>();
        internal List<LightObject> _lightObjectsTBR = new List<LightObject>();


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
        private Vector3 _cameraLookAt = new Vector3(0, 0, 1);
        private Vector3 _sunPosition = new Vector3(50, 50, 50);
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
            UpdateCameraLookAtVector();
        }

        public void SetCameraPosition(Vector3 p)
        {
            _cameraPosition = p;
            UpdateCameraLookAtVector();
        }

        public void SetCameraTarget(float x, float y, float z)
        {
            _cameraTarget = new Vector3(x, y, z);
            UpdateCameraLookAtVector();
        }
        public void SetCameraTarget(Vector3 p)
        {
            _cameraTarget = p;
            UpdateCameraLookAtVector();
        }

        private void UpdateCameraLookAtVector()
        {
            _cameraLookAt = _cameraTarget - _cameraPosition;
            _cameraLookAt.NormalizeFast();
        }

        public Vector3 GetCameraLookAtVector()
        {
            return _cameraLookAt;
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

        internal void AddRemoveObjects()
        {
            lock (_gameObjects)
            {
                foreach (GameObject g in _gameObjectsTBA)
                {
                    if (!_gameObjects.Contains(g))
                    {
                        _gameObjects.Add(g);
                        g.CurrentWorld = this;
                        g.UpdateModelMatrixAndHitboxes();
                        if(g is Explosion)
                        {
                            ((Explosion)g)._starttime = Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
                        }
                    }

                }
                _gameObjectsTBA.Clear();

                foreach (GameObject g in _gameObjectsTBR)
                {
                    g.CurrentWorld = null;
                    _gameObjects.Remove(g);
                }
                _gameObjectsTBR.Clear();

                
            }


            lock (_lightObjects)
            {
                foreach (LightObject g in _lightObjectsTBR)
                {
                    g.CurrentWorld = null;
                    _lightObjects.Remove(g);
                }
                _lightObjectsTBR.Clear();

                foreach (LightObject g in _lightObjectsTBA)
                {
                    if (!_lightObjects.Contains(g))
                    {
                        _lightObjects.Add(g);
                        g.CurrentWorld = this;
                    }
                }
                _lightObjectsTBA.Clear();

                _lightcount = _lightObjects.Count;
            }
        }

        public void AddLightObject(LightObject l)
        {
            if (!_lightObjects.Contains(l))
            {
                _lightObjectsTBA.Add(l);
            }
            else
            {
                throw new Exception("This light already exists in this world.");
            }

        }

        public void RemoveLightObject(LightObject l)
        {
            _lightObjectsTBR.Add(l);
        }

        public void AddGameObject(GameObject g)
        {
            lock (_gameObjects)
            {
                if (!_gameObjects.Contains(g))
                {
                    _gameObjectsTBA.Add(g);
                }
                else
                    throw new Exception("GameObject instance " + g.Name + " already in current world.");
            }

        }

        public void RemoveGameObject(GameObject g)
        {
            _gameObjectsTBR.Add(g);
        }

        internal void Dispose()
        {
            lock (_gameObjects)
            {
                foreach (GameObject g in _gameObjects)
                {
                    g.IsValid = false;
                }
                _gameObjects.Clear();
            }

            lock (KWEngine.Models)
            {
                foreach (GeoModel m in KWEngine.Models.Values)
                {
                    m.Dispose();
                }
                KWEngine.Models.Clear();
            }

            if (KWEngine.CubeTextures.ContainsKey(this)) {
                Dictionary<string, int> dict = KWEngine.CubeTextures[this];
                foreach(int texId in dict.Values)
                {
                    GL.DeleteTexture(texId);
                }
                dict.Clear();
                KWEngine.CubeTextures.Remove(this);
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
            _gameObjects.Sort((x, y) => x == null ? (y == null ? 0 : -1)
                : (y == null ? 1 : y.DistanceToCamera.CompareTo(x.DistanceToCamera)));
        }
    }
}
