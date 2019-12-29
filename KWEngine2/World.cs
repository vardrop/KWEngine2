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
using KWEngine2.Audio;

namespace KWEngine2
{
    public abstract class World
    {
        public bool DebugShadowCaster { get; set; } = false;
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

        internal List<HUDObject> _hudObjects = new List<HUDObject>();
        internal List<HUDObject> _hudObjectsTBA = new List<HUDObject>();
        internal List<HUDObject> _hudObjectsTBR = new List<HUDObject>();

        internal List<GameObject> _gameObjects = new List<GameObject>();
        internal List<LightObject> _lightObjects = new List<LightObject>();
        internal List<ParticleObject> _particleObjects = new List<ParticleObject>();

        internal List<GameObject> _gameObjectsTBA = new List<GameObject>();
        internal List<LightObject> _lightObjectsTBA = new List<LightObject>();
        internal List<ParticleObject> _particleObjectsTBA = new List<ParticleObject>();

        internal List<GameObject> _gameObjectsTBR = new List<GameObject>();
        internal List<LightObject> _lightObjectsTBR = new List<LightObject>();
        internal List<ParticleObject> _particleObjectsTBR = new List<ParticleObject>();


        internal int _lightcount = 0;
        public int LightCount
        {
            get
            {
                return _lightcount;
            }
        }

        internal int _textureBackground = -1;
        internal Vector4 _textureBackgroundTint = new Vector4(1, 1, 1, 1);
        internal Vector2 _textureBackgroundTransform = new Vector2(1, 1);
        internal int _textureSkybox = -1;

        public void SetTextureBackground(string filename, float repeatX = 1, float repeatY = 1, float red = 1, float green = 1, float blue = 1, float intensity = 1)
        {
            if (filename == null || filename.Length < 1)
            {
                _textureBackground = -1;
                _textureBackgroundTint = Vector4.Zero;
            }
            else
            {
                if (KWEngine.CustomTextures[this].ContainsKey(filename))
                {
                    _textureBackground = KWEngine.CustomTextures[this][filename];
                }
                else
                {
                    _textureBackground = HelperTexture.LoadTextureForBackgroundExternal(filename);
                    KWEngine.CustomTextures[this].Add(filename, _textureBackground);
                }
                _textureBackgroundTint.X = HelperGL.Clamp(red, 0, 1);
                _textureBackgroundTint.Y = HelperGL.Clamp(green, 0, 1);
                _textureBackgroundTint.Z = HelperGL.Clamp(blue, 0, 1);
                _textureBackgroundTint.W = HelperGL.Clamp(intensity, 0, 1);
                _textureBackgroundTransform.X = HelperGL.Clamp(repeatX, 0.001f, 8192);
                _textureBackgroundTransform.Y = HelperGL.Clamp(repeatY, 0.001f, 8192);
                _textureSkybox = -1;
            }
        }

        public void SetTextureSkybox(string filename, float red = 1, float green = 1, float blue = 1, float intensity = 1)
        {
            if (filename == null || filename.Length < 1)
                _textureSkybox = -1;
            else
            {
                if (KWEngine.CustomTextures[this].ContainsKey(filename))
                {
                    _textureSkybox = KWEngine.CustomTextures[this][filename];
                }
                else
                {
                    _textureSkybox = HelperTexture.LoadTextureSkybox(filename);
                }
                _textureBackgroundTint.X = HelperGL.Clamp(red, 0, 1);
                _textureBackgroundTint.Y = HelperGL.Clamp(green, 0, 1);
                _textureBackgroundTint.Z = HelperGL.Clamp(blue, 0, 1);
                _textureBackgroundTint.W = HelperGL.Clamp(intensity, 0, 1);
                _textureBackground = -1;
            }
        }

        
        private Vector3 _cameraPosition = new Vector3(0, 0, 25);
        private Vector3 _cameraTarget = new Vector3(0, 0, 0);
        private Vector3 _cameraLookAt = new Vector3(0, 0, 1);
        private Vector3 _sunPosition = new Vector3(50, 50, 50);
        private Vector3 _sunTarget = new Vector3(0, 0, 0);
        private Vector4 _sunColor = new Vector4(1, 1, 1, 1);
        private float _sunAmbient = 0.25f;

        private float _fov = 45f;
        private float _zFar = 1000f;

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

        public abstract void Act(KeyboardState kb, MouseState ms, float deltaTimeFactor);

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
                        if (g is Explosion)
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

            lock (_hudObjects)
            {
                foreach (HUDObject h in _hudObjectsTBA)
                {
                    if (!_hudObjects.Contains(h))
                    {
                        _hudObjects.Add(h);
                        h.CurrentWorld = this;
                    }
                }
                _hudObjectsTBA.Clear();

                foreach (HUDObject h in _hudObjectsTBR)
                {
                    h.CurrentWorld = null;
                    _hudObjects.Remove(h);
                }
                _hudObjectsTBR.Clear();
            }

            lock (_particleObjects)
            {
                foreach (ParticleObject g in _particleObjectsTBA)
                {
                    if (!_particleObjects.Contains(g))
                    {
                        g._starttime = Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
                        _particleObjects.Add(g);
                    }
                }
                _particleObjectsTBA.Clear();

                foreach (ParticleObject g in _particleObjectsTBR)
                {
                    _particleObjects.Remove(g);
                }
                _particleObjectsTBR.Clear();
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

        internal List<ParticleObject> GetParticleObjects()
        {
            return _particleObjects;
        }

        public IReadOnlyCollection<HUDObject> GetHUDObjects()
        {
            return _hudObjects.AsReadOnly();
        }


        public void AddHUDObject(HUDObject h)
        {
            if (!_hudObjects.Contains(h))
            {
                _hudObjectsTBA.Add(h);
            }
            else
            {
                throw new Exception("This HUD object already exists in this world.");
            }
        }

        public void RemoveHUDObject(HUDObject h)
        {
            _hudObjectsTBR.Add(h);
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

        public void AddParticleObject(ParticleObject g)
        {
            lock (_particleObjects)
            {
                if (!_particleObjects.Contains(g))
                {
                    _particleObjectsTBA.Add(g);
                }
            }
        }

        internal void RemoveParticleObject(ParticleObject g)
        {
            _particleObjectsTBR.Add(g);
        }


        public void RemoveGameObject(GameObject g)
        {
            _gameObjectsTBR.Add(g);
        }

        internal void Dispose()
        {
            GLAudioEngine.SoundStopAll();
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
                List<string> removableModels = new List<string>();
                foreach (string m in KWEngine.Models.Keys)
                {
                    if (!KWEngine.Models[m].IsInAssembly)
                    {
                        KWEngine.Models[m].Dispose();
                        removableModels.Add(m);
                    }
                }
                foreach (string m in removableModels)
                    KWEngine.Models.Remove(m);
            }

            if (KWEngine.CustomTextures.ContainsKey(this))
            {
                Dictionary<string, int> dict = KWEngine.CustomTextures[this];
                foreach (int texId in dict.Values)
                {
                    GL.DeleteTexture(texId);
                }
                dict.Clear();
                KWEngine.CustomTextures.Remove(this);
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

        public IReadOnlyCollection<LightObject> GetLightObjects()
        {
            IReadOnlyCollection<LightObject> returnCollection = null;
            lock (_lightObjects)
            {
                returnCollection = _lightObjects.AsReadOnly();
            }
            return returnCollection;
        }

        internal void SortByZ()
        {
            _gameObjects.Sort((x, y) => x == null ? (y == null ? 0 : -1)
                : (y == null ? 1 : y.DistanceToCamera.CompareTo(x.DistanceToCamera)));
        }

        public void SoundPlay(string audiofile, bool playLooping = false, float volume = 1.0f)
        {
            GLAudioEngine.SoundPlay(audiofile, playLooping, volume);
        }

        public void SoundStop(string audiofile)
        {
            GLAudioEngine.SoundStop(audiofile);
        }

        public void SoundStopAll()
        {
            GLAudioEngine.SoundStopAll();
        }
    }
}
