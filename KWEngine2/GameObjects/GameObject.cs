﻿using KWEngine2.Collision;
using KWEngine2.Helper;
using KWEngine2.Model;
using OpenTK;
using OpenTK.Input;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using static KWEngine2.KWEngine;
using KWEngine2.Audio;
using System.IO;

namespace KWEngine2.GameObjects
{
    /// <summary>
    /// Spielobjekte-Klasse
    /// </summary>
    public abstract class GameObject : IComparable
    {
        /// <summary>
        /// Ebene
        /// </summary>
        public enum Plane {
            /// <summary>
            /// X
            /// </summary>
            X, 
            /// <summary>
            /// Y
            /// </summary>
            Y, 
            /// <summary>
            /// Z
            /// </summary>
            Z, 
            /// <summary>
            /// Kamerablickebene
            /// </summary>
            Camera }
        internal uint DistanceToCamera { get; set; } = 100;
        /// <summary>
        /// Gibt an, ob das Objekt Schatten wirft und empfängt
        /// </summary>
        public bool IsShadowCaster { get; set; } = false;
        /// <summary>
        /// Höhenkorrektur für den First-Person-Modus (Standard: 0)
        /// </summary>
        public float FPSEyeOffset { get; set; } = 0;
        /// <summary>
        /// Gibt an, ob das Objekt von der Sonne beschienen wird
        /// </summary>
        public bool IsAffectedBySun { get; set; } = true;
        /// <summary>
        /// Gibt an, ob das Objekt von anderen Lichtquellen betroffen ist
        /// </summary>
        public bool IsAffectedByLight { get; set; } = true;
        /// <summary>
        /// Aktuelle Spielwelt
        /// </summary>
        public World CurrentWorld { get; internal set; } = null;
        private static Quaternion Turn180 = Quaternion.FromAxisAngle(KWEngine.WorldUp, (float)Math.PI);

        private IReadOnlyCollection<string> _meshNameList;

        private Vector3 _lookAtVector = new Vector3(0, 0, 1);

        internal enum Override { SpecularEnable, SpecularPower, SpecularArea, TextureDiffuse, TextureNormal, TextureSpecular, TextureTransform, TextureMetallic, TextureRoughness }

        internal Dictionary<string, Dictionary<Override, object>> _overrides = new Dictionary<string, Dictionary<Override, object>>();        

        /// <summary>
        /// Aktuelles Fenster
        /// </summary>
        public GLWindow CurrentWindow
        {
            get
            {
                if (CurrentWorld != null)
                    return CurrentWorld.CurrentWindow;
                else
                    throw new Exception("No window available.");
            }
        }
        internal int _largestHitboxIndex = -1;
        internal GeoModelCube _cubeModel = null;
        internal List<Hitbox> Hitboxes = new List<Hitbox>();
        internal Matrix4 ModelMatrixForRenderPass = Matrix4.Identity;
        internal Dictionary<string, Matrix4[]> BoneTranslationMatrices { get; set; }
        private int _animationId = -1;

        private Vector3 _tintColor = new Vector3(1, 1, 1);
        private Vector4 _glow = new Vector4(0, 0, 0, 0);
        private float _opacity = 1;

        /// <summary>
        /// Sichtbarkeit (0 = Unsichtbar, 1 = Sichtbar)
        /// </summary>
        public float Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                _opacity = HelperGL.Clamp(value, 0, 1);
            }
        }

        /// <summary>
        /// Färbung
        /// </summary>
        public Vector3 Color
        {
            get
            {
                return _tintColor;
            }
            set
            {
                CheckModel();

                _tintColor.X = HelperGL.Clamp(value.X, 0, 1);
                _tintColor.Y = HelperGL.Clamp(value.Y, 0, 1);
                _tintColor.Z = HelperGL.Clamp(value.Z, 0, 1);
               
            }
        }

        private Vector4 _emissiveColor = new Vector4(0,0,0,0);
        /// <summary>
        /// Strahlfarbe
        /// </summary>
        public Vector4 ColorEmissive
        {
            get
            {
                return _emissiveColor;
            }
            set
            {
                CheckModel();
                CheckIfNotTerrain();
                _emissiveColor.X = HelperGL.Clamp(value.X, 0, 1);
                _emissiveColor.Y = HelperGL.Clamp(value.Y, 0, 1);
                _emissiveColor.Z = HelperGL.Clamp(value.Z, 0, 1);
                _emissiveColor.W = HelperGL.Clamp(value.W, 0, 1);

            }
        }

        /// <summary>
        /// Glühfarbe
        /// </summary>
        public Vector4 Glow
        {
            get
            {
                return _glow;
            }
            set
            {
                _glow.X = HelperGL.Clamp(value.X, 0, 1);
                _glow.Y = HelperGL.Clamp(value.Y, 0, 1);
                _glow.Z = HelperGL.Clamp(value.Z, 0, 1);
                _glow.W = HelperGL.Clamp(value.W, 0, 1);
            }
        }
        
        /// <summary>
        /// Setzt die Glühfarbe
        /// </summary>
        /// <param name="red">Rot</param>
        /// <param name="green">Grün</param>
        /// <param name="blue">Blau</param>
        /// <param name="intensity">Helligkeit</param>
        public void SetGlow(float red, float green, float blue, float intensity)
        {
            Glow = new Vector4(red, green, blue, intensity);
        }

        /// <summary>
        /// Gibt an, ob das Objekt per Maus wählbar sein soll
        /// </summary>
        public bool IsPickable { get; set; } = false;
        /// <summary>
        /// ID der Animation
        /// </summary>
        public int AnimationID
        {
            get
            {
                return _animationId;
            }
            set
            {
                if (Model == null || !Model.IsValid || Model.Animations.Count == 0 || value >= Model.Animations.Count)
                {
                    throw new Exception("Cannot set animation id on invalid model. Model might be null, invalid, without any animations or the animation id does not exist.");
                }
                else
                {
                    _animationId = value;
                }
            }
        }

        /// <summary>
        /// Gibt an, ob das Objekt über Animationen verfügt
        /// </summary>
        public bool HasAnimations
        {
            get
            {
                return Model != null && Model.Animations != null && Model.Animations.Count > 0;
            }
        }

        private float _animationPercentage = 0;
        /// <summary>
        /// Prozent der Animation (0 bis 1)
        /// </summary>
        public float AnimationPercentage
        {
            get
            {
                return _animationPercentage;
            }
            set
            {
                _animationPercentage = HelperGL.Clamp(value, 0f, 1f);
            }
        }
        /// <summary>
        /// Gibt an ob mit dem Objekt kollidiert werde kann
        /// </summary>
        public bool IsCollisionObject { get; set; } = false;
        /// <summary>
        /// Optionales Speicherfeld
        /// </summary>
        public object Tag { get; protected set; } = null;
        private GeoModel _model;
        internal Matrix4 _modelMatrix = Matrix4.Identity;
        private Quaternion _rotation = new Quaternion(0, 0, 0, 1);
        private Vector3 _scale = new Vector3(1, 1, 1);
        /// <summary>
        /// Rotation (als Quaternion)
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                return _rotation;
            }
            internal set
            {
                _rotation = value;
                UpdateModelMatrixAndHitboxes();
                UpdateLookAtVector();
            }
        }

        /// <summary>
        /// Gibt die Rotation des First-Person-Objekts zurück
        /// </summary>
        public Quaternion RotationFirstPersonObject
        {
            get
            {
                if (CurrentWorld != null && CurrentWorld.IsFirstPersonMode && CurrentWorld.GetFirstPersonObject().Equals(this))
                {
                    return Quaternion.FromMatrix(HelperCamera.GetViewMatrixInversed());
                }
                else
                    return Rotation;
                
            }
        }

        internal Quaternion GetRotationNoFPSMode()
        {
            return _rotation;
        }

        internal Vector3 _sceneCenter = new Vector3(0, 0, 0);
        internal Vector3 _sceneDimensions = new Vector3(0, 0, 0);
        internal float _sceneDiameter = 1;

        private Vector3 _position = new Vector3(0, 0, 0);
        /// <summary>
        /// Position des Objekts
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                UpdateModelMatrixAndHitboxes();
            }
        }

        /// <summary>
        /// Setzt die Skalierung des Objekts
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        public void SetScale(float x, float y, float z)
        {
            Scale = new Vector3(x, y, z);
        }

        /// <summary>
        /// Setzt die Skalierung des Objekts
        /// </summary>
        /// <param name="scale"></param>
        public void SetScale(float scale)
        {
            Scale = new Vector3(scale, scale, scale);
        }

        /// <summary>
        /// Skalierung des Objekts
        /// </summary>
        public Vector3 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                CheckModel();
                if (value.X > 0 && value.Y > 0 && value.Z > 0)
                {
                    _scale = value;
                }
                else
                {
                    _scale = new Vector3(1, 1, 1);
                    Debug.WriteLine("Scale must be > 0 in all dimensions. Resetting to 1.");
                }
                UpdateModelMatrixAndHitboxes();
            }
        }
        private string _name = "undefined gameobject name";
        /// <summary>
        /// Name des Objekts
        /// </summary>
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



        internal Hitbox GetLargestHitbox()
        {
            return Hitboxes[_largestHitboxIndex];
        }

        internal void UpdateModelMatrixAndHitboxes()
        {
            _modelMatrix = Matrix4.CreateScale(_scale) * Matrix4.CreateFromQuaternion(_rotation) * Matrix4.CreateTranslation(_position);
            Vector3 sceneCenter = new Vector3(0, 0, 0);
            Vector3 tmpDims = new Vector3(0, 0, 0);
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;
            foreach (Hitbox h in Hitboxes)
            {
                Vector3 localCenter = h.Update(ref tmpDims);
                sceneCenter += localCenter;

                if (localCenter.X + tmpDims.X / 2 > maxX)
                    maxX = localCenter.X + tmpDims.X / 2;
                if (localCenter.X - tmpDims.X / 2 < minX)
                    minX = localCenter.X - tmpDims.X / 2;

                if (localCenter.Y + tmpDims.Y / 2 > maxY)
                    maxY = localCenter.Y + tmpDims.Y / 2;
                if (localCenter.Y - tmpDims.Y / 2 < minY)
                    minY = localCenter.Y - tmpDims.Y / 2;

                if (localCenter.Z + tmpDims.Z / 2 > maxZ)
                    maxZ = localCenter.Z + tmpDims.Z / 2;
                if (localCenter.Z - tmpDims.Z / 2 < minZ)
                    minZ = localCenter.Z - tmpDims.Z / 2;
            }
            _sceneCenter.X = sceneCenter.X / Hitboxes.Count;
            _sceneCenter.Y = sceneCenter.Y / Hitboxes.Count;
            _sceneCenter.Z = sceneCenter.Z / Hitboxes.Count;
            _sceneDimensions = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
            Vector3 downLeftFront = new Vector3(GetCenterPointForAllHitboxes().X - GetMaxDimensions().X / 2, GetCenterPointForAllHitboxes().Y - GetMaxDimensions().Y / 2, GetCenterPointForAllHitboxes().Z + GetMaxDimensions().Z / 2);
            Vector3 upRightBack = new Vector3(GetCenterPointForAllHitboxes().X + GetMaxDimensions().X / 2, GetCenterPointForAllHitboxes().Y + GetMaxDimensions().Y / 2, GetCenterPointForAllHitboxes().Z - GetMaxDimensions().Z / 2);
            _sceneDiameter = (upRightBack - downLeftFront).LengthFast;

            if(KWEngine.CurrentWorld != null)
                DistanceToCamera = (uint)((KWEngine.CurrentWorld.GetCameraPosition() - GetCenterPointForAllHitboxes()).LengthSquared * 10000);
        }

        /// <summary>
        /// Berechnet das Zentrum des Objekts (über alle Hitboxen)
        /// </summary>
        /// <returns>Zentrum</returns>
        public Vector3 GetCenterPointForAllHitboxes()
        {
            return _sceneCenter;
        }

        /// <summary>
        /// Erfragt die Maße des Objekts
        /// </summary>
        /// <returns>Maße (Breite, Höhe, Tiefe)</returns>
        public Vector3 GetMaxDimensions()
        {
            return _sceneDimensions;
        }

        /// <summary>
        /// Erfragt den maximalen Durchmesser des Objekts
        /// </summary>
        /// <returns>Durchmesser</returns>
        public float GetMaxDiameter()
        {
            return _sceneDiameter;
        }

        /// <summary>
        /// Bewegt das Blickfeld des FP-Objekts gemäß der Mauszeigerposition
        /// </summary>
        /// <param name="ms">Mausinformation</param>
        protected void MoveFPSCamera(MouseState ms)
        {
            CheckModel();

            if (CurrentWorld.IsFirstPersonMode)
            {
                int centerX = CurrentWindow.X + CurrentWindow.Width / 2;
                int centerY = CurrentWindow.Y + CurrentWindow.Height / 2;
                HelperCamera.AddRotation(-(ms.X - centerX) * KWEngine.MouseSensitivity, (centerY - ms.Y) * KWEngine.MouseSensitivity);
            }
            else
                throw new Exception("FPS mode is not active.");
        }

        /// <summary>
        /// Gibt an, ob das Objekt ein Modell hat
        /// </summary>
        public bool HasModel
        {
            get
            {
                return _model.IsValid;
            }
        }

        /// <summary>
        /// Gibt das aktuelle Modell zurück
        /// </summary>
        public GeoModel Model
        {
            get
            {
                return _model;
            }
        }

        /// <summary>
        /// Setzt das Modell des Objekts
        /// </summary>
        /// <param name="m">Modell-Instanz</param>
        public void SetModel(GeoModel m)
        {
            if (m == null)
            {
                throw new Exception("Your model is null.");
            }

            _model = m;
            if (m.Name == "kwcube.obj")
            {
                _cubeModel = new GeoModelCube1();
                _cubeModel.Owner = this;
            }
            else if (m.Name == "kwcube6.obj")
            {
                _cubeModel = new GeoModelCube6();
                _cubeModel.Owner = this;
            }
            else
            {
                _cubeModel = null;

                //Init overrides:
                _overrides.Clear();
                List<string> l = new List<string>();
                foreach (GeoMesh mesh in _model.Meshes.Values)
                {
                    l.Add(mesh.Name);
                    _overrides[mesh.Name] = new Dictionary<Override, object>();
                }
                _meshNameList = l.AsReadOnly();
                
            }

            int hIndex = 0;
            float diameter = float.MinValue;
            foreach (GeoMeshHitbox gmh in m.MeshHitboxes)
            {
                Hitbox h = new Hitbox(this, gmh);
                Hitboxes.Add(h);
                if (h.DiameterFull > diameter)
                {
                    diameter = h.DiameterFull;
                    _largestHitboxIndex = hIndex;
                }
                hIndex++;
            }

            if (m.HasBones)
            {
                BoneTranslationMatrices = new Dictionary<string, Matrix4[]>();
                foreach (GeoMesh mesh in m.Meshes.Values)
                {
                    BoneTranslationMatrices[mesh.Name] = new Matrix4[mesh.BoneIndices.Count];
                    for (int i = 0; i < mesh.BoneIndices.Count; i++)
                        BoneTranslationMatrices[mesh.Name][i] = Matrix4.Identity;
                }
            }
            
        }

        /// <summary>
        /// Act-Methode
        /// </summary>
        /// <param name="ks">Keyboardinfos</param>
        /// <param name="ms">Mausinfos</param>
        /// <param name="deltaTimeFactor">Delta-Zeit-Faktor (Standard: 1.0)</param>
        public abstract void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor);

        #region Gameplay
        /// <summary>
        /// Erfragt die aktuelle Rotation in Eulerschen Winkeln
        /// </summary>
        /// <returns>Euler-Winkel</returns>
        public Vector3 GetRotationEulerAngles()
        {
            return HelperRotation.ConvertQuaternionToEulerAngles(Rotation);
            
        }

        /// <summary>
        /// Setzt die Rotation des Objekts
        /// </summary>
        /// <param name="rotation">Rotation (als Quaternion)</param>
        public void SetRotation(Quaternion rotation)
        {
            Rotation = rotation;
        }

        /// <summary>
        /// Prüft, ob das Objekt in Richtung des gegebenen Punkts blickt
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        /// <param name="diameter">Durchmesser um den Punkt</param>
        /// <returns></returns>
        protected bool IsLookingAt(float x, float y, float z, float diameter)
        {
            return IsLookingAt(new Vector3(x, y, z), diameter);
        }

        /// <summary>
        /// Prüft, ob das Objekt in Richtung des gegebenen Punkts blickt
        /// </summary>
        /// <param name="target">Zielposition</param>
        /// <param name="diameter">Durchmesser um das Ziel</param>
        /// <returns></returns>
        protected bool IsLookingAt(Vector3 target, float diameter)
        {
            CheckModel();

            if (Model.IsTerrain)
            {
                throw new Exception("Terrains cannot 'look' at objects.");
            }

            Vector3 position = GetCenterPointForAllHitboxes();
            if(CurrentWorld.IsFirstPersonMode && CurrentWorld.GetFirstPersonObject().Equals(this))
            {
                position = new Vector3(Position.X, Position.Y + FPSEyeOffset, Position.Z);
            }
            Vector3 deltaGO = target - position;
            Vector3 rayDirection = GetLookAtVector();
            Vector3[] aabb = new Vector3[] { new Vector3(-0.5f * diameter, -0.5f * diameter, -0.5f * diameter), new Vector3(0.5f * diameter, 0.5f * diameter, 0.5f * diameter) };

            Matrix4 matrix = Matrix4.CreateTranslation(target);
            Vector3 x = new Vector3(matrix.Row0);
            x.NormalizeFast();
            Vector3 y = new Vector3(matrix.Row1);
            y.NormalizeFast();
            Vector3 z = new Vector3(matrix.Row2);
            z.NormalizeFast();
            Vector3[] axes = new Vector3[] { x, y, z };

            float tMin = 0.0f;
            float tMax = 100000.0f;
            for (int i = 0; i < axes.Length; i++)
            {
                Vector3 axis = axes[i];

                Vector3.Dot(ref axis, ref deltaGO, out float e);
                Vector3.Dot(ref rayDirection, ref axis, out float f);
                float t1 = (e + aabb[0][i]) / f; // Intersection with the "left" plane
                float t2 = (e + aabb[1][i]) / f; // Intersection with the "right" plane
                if (t1 > t2)
                {
                    float w = t1;
                    t1 = t2;
                    t2 = w;
                }
                // tMax is the nearest "far" intersection (amongst the X,Y and Z planes pairs)
                if (t2 < tMax) tMax = t2;
                // tMin is the farthest "near" intersection (amongst the X,Y and Z planes pairs)
                if (t1 > tMin) tMin = t1;


                if (tMax < tMin)
                    return false;
            }
            return true;
        }


        internal void CheckBounds()
        {
            if(_sceneCenter.X > CurrentWorld.WorldCenter.X + CurrentWorld.WorldDistance
                || _sceneCenter.X < CurrentWorld.WorldCenter.X - CurrentWorld.WorldDistance

                ||_sceneCenter.Y > CurrentWorld.WorldCenter.Y + CurrentWorld.WorldDistance
                || _sceneCenter.Y < CurrentWorld.WorldCenter.Y - CurrentWorld.WorldDistance

                || _sceneCenter.Z > CurrentWorld.WorldCenter.Z + CurrentWorld.WorldDistance
                || _sceneCenter.Z < CurrentWorld.WorldCenter.Z - CurrentWorld.WorldDistance
                )
            {
                Debug.WriteLine("Object '" + this.Name + " " + this.Model.Name  + "' position is beyond world's boundaries (currently: " + CurrentWorld.WorldDistance + " units from " + CurrentWorld.WorldCenter +  "). Removing object.");
                CurrentWorld.RemoveGameObject(this);
            }
        }

        /// <summary>
        /// Setzt die Rotation des Objekts (Reihenfolge: Z->Y->X)
        /// </summary>
        /// <param name="x">X in Grad</param>
        /// <param name="y">Y in Grad</param>
        /// <param name="z">Z in Grad</param>
        public void SetRotation(float x, float y, float z)
        {
            if (Model.IsTerrain)
            {
                Debug.WriteLine("Setting rotation on a GeoTerrain instance is not supported.");
            }
            else if (CurrentWorld != null && CurrentWorld.IsFirstPersonMode && CurrentWorld.GetFirstPersonObject().Equals(this))
            {
                Debug.WriteLine("Setting rotation on a first person instance is not supported.");
            }
            else if (Model != null)
            {
                Quaternion tmpRotateX = Quaternion.FromAxisAngle(Vector3.UnitX, HelperRotation.CalculateRadiansFromDegrees(x));
                Quaternion tmpRotateY = Quaternion.FromAxisAngle(Vector3.UnitY, HelperRotation.CalculateRadiansFromDegrees(y));
                Quaternion tmpRotateZ = Quaternion.FromAxisAngle(Vector3.UnitZ, HelperRotation.CalculateRadiansFromDegrees(z));
                Rotation = tmpRotateZ * tmpRotateY * tmpRotateX;
            }
            else
            {
                throw new Exception("Cannot set rotation on empty geometry object. Did you assign an instance of GeometryObject to your GameObject instance?");
            }
        }

        /// <summary>
        /// Addiert eine Rotation hinzu
        /// </summary>
        /// <param name="r">Rotation (als Quaternion)</param>
        public void AddRotation(Quaternion r)
        {
            CheckModel();
            if (Model.IsTerrain)
            {
                Debug.WriteLine("Adding rotation for GeoTerrain instance is not supported.");
            }
            else if (CurrentWorld != null && CurrentWorld.IsFirstPersonMode && CurrentWorld.GetFirstPersonObject().Equals(this))
            {
                Debug.WriteLine("Setting rotation on a first person instance is not supported.");
            }
            else
            {
                Rotation = Rotation * r;
            }
        }

        /// <summary>
        /// Fügt eine Rotation um die X-Achse hinzu
        /// </summary>
        /// <param name="amount">Rotation in Grad</param>
        /// <param name="absolute">false für relative Drehung, true für eine Rotation um die Weltachse</param>
        public void AddRotationX(float amount, bool absolute = false)
        {
            CheckModel();
            if (Model.IsTerrain)
            {
                Debug.WriteLine("Adding rotation for GeoTerrain instance is not supported.");
            }
            else if (CurrentWorld != null && CurrentWorld.IsFirstPersonMode && CurrentWorld.GetFirstPersonObject().Equals(this))
            {
                Debug.WriteLine("Setting rotation on a first person instance is not supported.");
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

        /// <summary>
        /// Fügt eine Rotation um die Y-Achse hinzu
        /// </summary>
        /// <param name="amount">Rotation in Grad</param>
        /// <param name="absolute">false für relative Drehung, true für eine Rotation um die Weltachse</param>
        public void AddRotationY(float amount, bool absolute = false)
        {
            CheckModel();
            if (Model.IsTerrain)
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

        /// <summary>
        /// Fügt eine Rotation um die Z-Achse hinzu
        /// </summary>
        /// <param name="amount">Rotation in Grad</param>
        /// <param name="absolute">false für relative Drehung, true für eine Rotation um die Weltachse</param>
        public void AddRotationZ(float amount, bool absolute = false)
        {
            CheckModel();
            if (Model.IsTerrain)
            {
                Debug.WriteLine("Adding rotation for GeoTerrain instance is not supported.");
            }
            else if (CurrentWorld != null && CurrentWorld.IsFirstPersonMode && CurrentWorld.GetFirstPersonObject().Equals(this))
            {
                Debug.WriteLine("Setting rotation on a first person instance is not supported.");
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

        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        public void SetPosition(float x, float y, float z)
        {
            CheckModel();
            SetPosition(new Vector3(x, y, z));
        }

        /// <summary>
        /// Setzt die Position des Objekts
        /// </summary>
        /// <param name="newPosition">neue Position</param>
        public void SetPosition(Vector3 newPosition)
        {
            if (Model != null)
            {
                Position = new Vector3(newPosition);
            }
            else
                throw new Exception("Cannot set position on empty model object. Did you assign an instance of GeoModel to your GameObject via SetModel() ?");

        }

        /// <summary>
        /// Setzt die Färbung des Objekts
        /// </summary>
        /// <param name="red">Rot</param>
        /// <param name="green">Grün</param>
        /// <param name="blue">Blau</param>
        protected void SetColor(float red, float green, float blue)
        {
            _tintColor.X = red >= 0 && red <= 1 ? red : 1;
            _tintColor.Y = green >= 0 && green <= 1 ? green : 1;
            _tintColor.Z = blue >= 0 && blue <= 1 ? blue : 1;
        }

        private void UpdateLookAtVector()
        {
            if (CurrentWorld != null && CurrentWorld.IsFirstPersonMode && CurrentWorld.GetFirstPersonObject().Equals(this))
            {
                _lookAtVector = HelperCamera.GetLookAtVector();
            }
            else
            {
                Vector3 standardOrientation = Vector3.UnitZ;
                Vector3 rotatedNormal = Vector3.TransformNormal(standardOrientation, _modelMatrix);
                rotatedNormal.NormalizeFast();
                _lookAtVector = rotatedNormal;
            }
        }

        /// <summary>
        /// Erfragt den normalisierten Blickrichtungsvektor
        /// </summary>
        /// <returns>Blickrichtungsvektor (normalisiert)</returns>
        public Vector3 GetLookAtVector()
        {
            CheckModelAndWorld();
            return _lookAtVector;
        }

        /// <summary>
        /// Bewegt das Objekt in Blickrichtung
        /// </summary>
        /// <param name="amount">Anzahl der Bewegungseinheiten</param>
        protected void Move(float amount)
        {
            Position += Vector3.Multiply(GetLookAtVector(), amount);
        }

        /// <summary>
        /// Bewegt das Objekt in Blickrichtung (ohne Höhenunterschied)
        /// </summary>
        /// <param name="amount">Anzahl der Bewegungseinheiten</param>
        protected void MoveXZ(float amount)
        {
            Vector3 tmp = GetLookAtVector();
            tmp.Y = 0;

            Position += Vector3.Multiply(tmp, amount);
        }

        /// <summary>
        /// Bewegt das Objekt relativ zur aktuellen Position entlang der gegebenen Achsen
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        protected void MoveOffset(float x, float y, float z)
        {
            Position = new Vector3(Position.X + x, Position.Y + y, Position.Z + z);
        }

        /// <summary>
        /// Führt Bewegungen für den First-Person-Modus aus
        /// </summary>
        /// <param name="forward">Vorwärtsanteil (-1 bis +1)</param>
        /// <param name="strafe">Seitwärtsanteil (-1 bis +1)</param>
        /// <param name="units">Einheitenmultiplikator (z.B. 0.5)</param>
        protected void MoveAndStrafeFirstPersonXYZ(float forward, float strafe, float units)
        {
            CheckModel();
            MoveAndStrafeFirstPersonXYZ(new Vector2(forward, strafe), units);
        }

        /// <summary>
        /// Bewegt das Objekt um die gegebenen Einheiten entlang eines Vektors
        /// </summary>
        /// <param name="v">Richtungsvektor</param>
        /// <param name="units">Bewegungseinheiten</param>
        protected void MoveAlongVector(Vector3 v, float units)
        {
            Position = new Vector3(Position.X + v.X * units, Position.Y + v.Y * units, Position.Z + v.Z * units);
        }

        private void MoveAndStrafeFirstPersonXYZ(Vector2? direction, float units)
        {
            CheckModel();
            if (direction == null || !direction.HasValue)
                return;

            if (CurrentWorld.IsFirstPersonMode && CurrentWorld.GetFirstPersonObject().Equals(this))
            {
                Vector3 moveVector = HelperCamera.MoveXYZ(direction.Value.X, direction.Value.Y, direction.Value.LengthFast);

                MoveOffset(moveVector.X * units, moveVector.Y * units, moveVector.Z * units);
            }
            else
            {
                throw new Exception("MoveAndStrafeFirstPerson() may only be called from the current FPS object.");
            }
        }

        /// <summary>
        /// Führt Bewegungen für den First-Person-Modus aus (ohne Höhenveränderung)
        /// </summary>
        /// <param name="forward">Vorwärtsanteil (-1 bis +1)</param>
        /// <param name="strafe">Seitwärtsanteil (-1 bis +1)</param>
        /// <param name="units">Einheitenmultiplikator (z.B. 0.5)</param>
        protected void MoveAndStrafeFirstPerson(float forward, float strafe, float units)
        {
            MoveAndStrafeFirstPerson(new Vector2(forward, strafe), units);
        }


        private void MoveAndStrafeFirstPerson(Vector2 direction, float units)
        {
            CheckModel();

            if (CurrentWorld.IsFirstPersonMode && CurrentWorld.GetFirstPersonObject().Equals(this))
            {
                Vector3 moveVector = HelperCamera.MoveXZ(direction.X, direction.Y, direction.LengthFast);
                MoveOffset(moveVector.X * units, moveVector.Y * units, moveVector.Z * units);
            }
            else
            {
                throw new Exception("MoveAndStrafeFirstPersonXZ() may only be called from the current FPS object.");
            }
        }

        #endregion

        internal void ProcessCurrentAnimation()
        {
            if (AnimationID >= 0 && AnimationID < Model.Animations.Count)
            {
                GeoAnimation a = Model.Animations[AnimationID];
                
                //Console.WriteLine(AnimationPercentage);
                float timestamp = a.DurationInTicks * AnimationPercentage;
                foreach (GeoMesh mesh in Model.Meshes.Values)
                {
                    Matrix4 identity = Matrix4.Identity;
                    ReadNodeHierarchy(timestamp, ref a, AnimationID, Model.Root, mesh, ref identity);
                }
            }
        }

        private void ReadNodeHierarchy(float timestamp, ref GeoAnimation animation, int animationId, GeoNode node, GeoMesh mesh, ref Matrix4 parentTransform, int debugLevel = 0)
        {
            string nodeName = node.Name;

            animation.AnimationChannels.TryGetValue(nodeName, out GeoNodeAnimationChannel channel);
            Matrix4 nodeTransformation = node.Transform;

            if (channel != null)
            {
                Matrix4 scalingMatrix = Matrix4.CreateScale(CalcInterpolatedScaling(timestamp, ref channel));
                Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(CalcInterpolatedRotation(timestamp, ref channel));
                Matrix4 translationMatrix = Matrix4.CreateTranslation(CalcInterpolatedTranslation(timestamp, ref channel));
                nodeTransformation =
                    scalingMatrix
                    * rotationMatrix
                    * translationMatrix;
            }
            Matrix4 globalTransform = nodeTransformation * parentTransform;

            int index = mesh.BoneNames.IndexOf(node.Name);
            if (index >= 0)
            {
                lock (BoneTranslationMatrices)
                {
                    BoneTranslationMatrices[mesh.Name][index] = mesh.BoneOffset[index] * globalTransform * Model.TransformGlobalInverse;
                }
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                ReadNodeHierarchy(timestamp, ref animation, animationId, node.Children[i], mesh, ref globalTransform, debugLevel + 1);
            }

        }

        private Vector3 CalcInterpolatedScaling(float timestamp, ref GeoNodeAnimationChannel channel)
        {
            if (channel.ScaleKeys.Count == 1)
            {
                return channel.ScaleKeys[0].Scale;
            }

            for (int i = 0; i < channel.ScaleKeys.Count - 1; i++)
            {
                GeoAnimationKeyframe key = channel.ScaleKeys[i];
                if (timestamp < channel.ScaleKeys[0].Time)
                {
                    return key.Scale;
                }
                else if (timestamp == key.Time)
                {
                    return key.Scale;
                }

                else
                {
                    if (timestamp >= key.Time && timestamp <= channel.ScaleKeys[i + 1].Time)
                    {
                        GeoAnimationKeyframe key2 = channel.ScaleKeys[i + 1];

                        float deltaTime = (key2.Time - key.Time);
                        float factor = (timestamp - key.Time) / deltaTime;
                        if (factor < 0 || factor > 1)
                        {
                            throw new Exception("Error mapping animation timestamps. Delta time not valid.");
                        }

                        Vector3 scalingStart = key.Scale;
                        Vector3 scalingEnd = key2.Scale;
                        Vector3.Lerp(ref scalingStart, ref scalingEnd, factor, out Vector3 scaling);
                        return scaling;
                    }
                }
            }
            Debug.WriteLine("Error finding scaling timestamp for animation cycle.");
            return new Vector3(1, 1, 1);
        }

        private Vector3 CalcInterpolatedTranslation(float timestamp, ref GeoNodeAnimationChannel channel)
        {
            if (channel.TranslationKeys.Count == 1)
            {
                return channel.TranslationKeys[0].Translation;
            }

            for (int i = 0; i < channel.TranslationKeys.Count - 1; i++)
            {
                GeoAnimationKeyframe key = channel.TranslationKeys[i];
                if (timestamp < channel.TranslationKeys[0].Time)
                {
                    return key.Translation;
                }
                else if (timestamp == key.Time)
                {
                    return key.Translation;
                }

                else
                {
                    if (timestamp >= key.Time && timestamp <= channel.TranslationKeys[i + 1].Time)
                    {
                        GeoAnimationKeyframe key2 = channel.TranslationKeys[i + 1];

                        float deltaTime = (key2.Time - key.Time);
                        float factor = (timestamp - key.Time) / deltaTime;
                        if (factor < 0 || factor > 1)
                        {
                            throw new Exception("Error mapping animation timestamps. Delta time not valid.");
                        }

                        Vector3 transStart = key.Translation;
                        Vector3 transEnd = key2.Translation;
                        Vector3.Lerp(ref transStart, ref transEnd, factor, out Vector3 trans);
                        return trans;
                    }
                }
            }
            Debug.WriteLine("Error finding translation timestamp for animation cycle.");
            return new Vector3(0, 0, 0);
        }

        private Quaternion CalcInterpolatedRotation(float timestamp, ref GeoNodeAnimationChannel channel)
        {
            if (channel.RotationKeys.Count == 1)
            {
                return channel.RotationKeys[0].Rotation;
            }
            for (int i = 0; i < channel.RotationKeys.Count - 1; i++)
            {
                if (timestamp < channel.RotationKeys[0].Time)
                {
                    return channel.RotationKeys[i].Rotation;
                }
                else if (timestamp == channel.RotationKeys[i].Time)
                {
                    return channel.RotationKeys[i].Rotation;
                }
                else
                {
                    if (timestamp >= channel.RotationKeys[i].Time && timestamp <= channel.RotationKeys[i + 1].Time)
                    {
                        GeoAnimationKeyframe key2 = channel.RotationKeys[i + 1];

                        float deltaTime = key2.Time - channel.RotationKeys[i].Time;
                        float factor = (timestamp - channel.RotationKeys[i].Time) / deltaTime;
                        if (factor < 0 || factor > 1)
                        {
                            throw new Exception("Error mapping animation timestamps. Delta time not valid.");
                        }

                        OpenTK.Quaternion rotationStart = channel.RotationKeys[i].Rotation;
                        OpenTK.Quaternion rotationEnd = key2.Rotation;
                        Quaternion rotation = OpenTK.Quaternion.Slerp(rotationStart, rotationEnd, factor);
                        rotation.Normalize();
                        return rotation;
                    }

                }
            }
            Debug.WriteLine("Error finding rotation timestamp for animation cycle.");
            return new Quaternion(0, 0, 0, 1);
        }

        private void CheckModelAndWorld(bool checkIfGameObjectIsInAWorld = false)
        {
            if (Model == null || checkIfGameObjectIsInAWorld ? this.CurrentWorld == null : GLWindow.CurrentWindow.CurrentWorld == null)
            {
                throw new Exception("Model and/or World have not been set yet!");
            }
        }
        private void CheckModel()
        {
            if (Model == null)
            {
                throw new Exception("Model has not been set yet!");
            }
        }

        /// <summary>
        /// Aktuelle Systemzeit in Millisekunden
        /// </summary>
        /// <returns>Systemzeit in ms</returns>
        public long GetCurrentTimeInMilliseconds()
        {
            return Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Erfragt eine Liste aller Objekte, die mit dem aufrufenden Objekt kollidieren
        /// </summary>
        /// <param name="offsetX">Versatz in X-Richtung (optional)</param>
        /// <param name="offsetY">Versatz in Y-Richtung (optional)</param>
        /// <param name="offsetZ">Versatz in Z-Richtung (optional)</param>
        /// <returns></returns>
        protected List<Intersection> GetIntersections(float offsetX = 0, float offsetY = 0, float offsetZ = 0)
        {
            CheckModelAndWorld(true);
            List<Intersection> intersections = new List<Intersection>();
            if (!IsCollisionObject)
            {
                throw new Exception("Error: You are calling GetIntersectingObjects() on an instance that is marked as a non-colliding object.");
            }
            //Objekte außerhalb der Reichweite ausfiltern:
            foreach (GameObject go in CurrentWorld.GetGameObjects())
            {
                if (!go.IsCollisionObject || go is Explosion || go.Equals(this))
                {
                    continue;
                }
                Vector3 offset = new Vector3(offsetX, offsetY, offsetZ);
                bool considerForMeasurement = ConsiderForMeasurement(go, this, ref offset);
                if (considerForMeasurement)
                {
                    foreach (Hitbox hbother in go.Hitboxes)
                    {
                        foreach (Hitbox hbcaller in this.Hitboxes)
                        {
                            Intersection i = null;
                            if (hbother.Owner.Model.IsTerrain)
                            {
                                i = Hitbox.TestIntersectionTerrain(hbcaller, hbother, offset);
                            }
                            else
                            {
                                i = Hitbox.TestIntersection(hbcaller, hbother, offset);
                            }

                            if (i != null)
                                intersections.Add(i);
                        }
                    }
                }
            }

            return intersections;
        }

        private static bool ConsiderForMeasurement(GameObject go, GameObject caller, ref Vector3 callerOffset)
        {
            if (go.Model.IsTerrain)
            {
                GeoTerrain terra = go.Model.Meshes["Terrain"].Terrain;
                float terraHigh = go.Position.Y + terra.GetScaleFactor();
                float terraLow = go.Position.Y;
                float left = go.Position.X - terra.GetWidth() / 2f;
                float right = go.Position.X + terra.GetWidth() / 2f;

                float back = go.Position.Z - terra.GetDepth() / 2f;
                float front = go.Position.Z + terra.GetDepth() / 2f;

                Vector3 hbCaller = caller.GetCenterPointForAllHitboxes();
                if (hbCaller.X + callerOffset.X >= left && hbCaller.X + callerOffset.X <= right
                    && hbCaller.Z + callerOffset.Z >= back && hbCaller.Z + callerOffset.Z <= front
                    && hbCaller.Y + callerOffset.Y + caller.GetMaxDiameter() / 2 >= terraLow
                    && hbCaller.Y + callerOffset.Y - caller.GetMaxDiameter() / 2 <= (terraHigh * 1.5f))
                {
                    return true;
                }
                return false;
            }
            else
            {
                float distance = ((caller.GetCenterPointForAllHitboxes() + callerOffset) - go.GetCenterPointForAllHitboxes()).LengthFast;
                float rad1 = caller.GetMaxDiameter() / 2;
                float rad2 = go.GetMaxDiameter() / 2;
                if (distance - (rad1 + rad2) > 0)
                    return false;
                else
                    return true;
            }
        }
        
        /// <summary>
        /// Vergleichsmethode für den Tiefenpuffer
        /// </summary>
        /// <param name="obj">zu vergleichendes Objekt</param>
        /// <returns>1, wenn das zu vergleichende Objekt weiter weg von der Kamera ist als das aufrufende Objekt</returns>
        public int CompareTo(object obj)
        {
            GameObject g = (GameObject)obj;
            return g.DistanceToCamera > this.DistanceToCamera ? 1 : -1;
        }
        

        /// <summary>
        /// Setzt die Textur für das Objekt (KWCube und KWCube6)
        /// </summary>
        /// <param name="texture">Texturdatei</param>
        /// <param name="type">Art der Textur (Standard: Diffuse)</param>
        /// <param name="side">Seite des Würfels (für KWCube-Modelle)</param>
        public void SetTexture(string texture, TextureType type = TextureType.Diffuse, CubeSide side = CubeSide.All)
        {
            CheckModelAndWorld();

            if (_cubeModel != null)
            {
                if (_cubeModel is GeoModelCube1 && side != CubeSide.All)
                {
                    throw new Exception("Cannot set side texture on single sided cube model. Please use KWCube6 as model.");
                }
                _cubeModel.SetTexture(texture, side, type);
            }
            else
            {
                if (Model.Name == "kwsphere.obj" || Model.Name == "kwrect.obj")
                {
                    SetTextureForMesh(0, texture, type);
                }
                else if (Model.IsTerrain)
                {
                    GeoMesh terrainMesh = Model.Meshes.Values.ElementAt(0);
                    GeoTexture newTex = new GeoTexture(texture);
                    newTex.Filename = texture;

                    if (KWEngine.CustomTextures[KWEngine.CurrentWorld].ContainsKey(texture))
                    {
                        newTex.OpenGLID = KWEngine.CustomTextures[KWEngine.CurrentWorld][texture];
                    }
                    else
                    {
                        int id = HelperTexture.LoadTextureForModelExternal(texture);
                        if (id > 0)
                        {
                            newTex.OpenGLID = id;
                            KWEngine.CustomTextures[KWEngine.CurrentWorld].Add(texture, id);
                        }
                        else
                        {
                            newTex.OpenGLID = KWEngine.TextureDefault;
                            Debug.WriteLine("Error loading " + type + " texture for terrain '" + Model.Name + "': " + texture);
                        }
                    }

                    if (type == TextureType.Diffuse)
                    {
                        newTex.Type = GeoTexture.TexType.Diffuse;
                        terrainMesh.Material.TextureDiffuse = newTex;
                    }
                    else if (type == TextureType.Normal)
                    {
                        newTex.Type = GeoTexture.TexType.Normal;
                        terrainMesh.Material.TextureNormal = newTex;
                    }
                    else
                    {
                        newTex.Type = GeoTexture.TexType.Specular;
                        terrainMesh.Material.TextureSpecular = newTex;
                    }
                }
                else
                {
                    throw new Exception("Cannot set textures for model " + Model.Name + " because it is not a KWCube, KWSphere or KWRect. Use SetTextureForMesh() instead.");
                }
                
            }
        }

        /// <summary>
        /// Setzt die Texturwiederholungen für das Objekt
        /// </summary>
        /// <param name="x">Breitenwiederholungen</param>
        /// <param name="y">Höhenwiederholungen</param>
        /// <param name="side">Seite des Würfels (für KWCube)</param>
        public void SetTextureRepeat(float x, float y, CubeSide side = CubeSide.All)
        {
            CheckModelAndWorld();
            CheckIfNotTerrain();

            if (_cubeModel != null)
            {
                if (_cubeModel is GeoModelCube1 && side != CubeSide.All)
                {
                    throw new Exception("Cannot set side texture repeat on single sided cube model. Please use KWCube6 as model.");
                }
                else if(!(x > 0 && y > 0))
                {
                    throw new Exception("Texture repeat values must be > 0.");
                }
                _cubeModel.SetTextureRepeat(x, y, side);
            }
            else
            {
                if (Model.Name == "kwsphere.obj" || Model.Name == "kwrect.obj")
                {
                    SetTextureRepeatForMesh(0, x, y);
                }
                else
                {
                    throw new Exception("Cannot set textures for model " + Model.Name + " KWCube, KWSphere or KWRect. Use SetTextureRepeatForMesh() instead.");
                }
            }
        }

        /// <summary>
        /// Erfragt den Blickrichtungsvektor der Kamera
        /// </summary>
        /// <returns>Blickrichtung</returns>
        protected Vector3 GetCameraLookAtVector()
        {
            if(CurrentWorld != null)
            {
                return CurrentWorld.GetCameraLookAtVector();
            }
            else
            {
                throw new Exception("Current world is null. Cannot compute camera vector.");
            }
        }

        /// <summary>
        /// Erfragt den Kollisionspunkt des Mauszeigers mit der 3D-Welt (auf Höhe des Aufrufers)
        /// </summary>
        /// <param name="ms">Mausinformationen</param>
        /// <param name="plane">Kollisionsebene (Standard: Camera)</param>
        /// <returns></returns>
        protected Vector3 GetMouseIntersectionPoint(MouseState ms, Plane plane = Plane.Camera)
        {
            if (CurrentWorld.IsFirstPersonMode)
                throw new Exception("Method GetMouseIntersectionPoint() may not be called from First Person Mode object.");
            
            Vector3 worldRay = Get3DMouseCoords(HelperGL.GetNormalizedMouseCoords(ms.X, ms.Y, CurrentWindow));
            Vector3 normal;
            if (plane == Plane.Y)
                normal = new Vector3(0, 1, 0.000001f);
            else if (plane == Plane.X)
                normal = new Vector3(1, 0, 0);
            else if (plane == Plane.Z)
                normal = new Vector3(0, 0.000001f, 1);
            else
            {
                normal = -GetCameraLookAtVector();
            }

            bool result = LinePlaneIntersection(out Vector3 intersection, worldRay, CurrentWorld.GetCameraPosition(), normal, GetCenterPointForAllHitboxes());
            if (result)
            {
                return intersection;
            }
            else
                return Position;
        }

        /// <summary>
        /// Erfragt, ob der Mauszeiger (näherungsweise) auf dem Objekt liegt 
        /// </summary>
        /// <param name="ms">Mausinformationen</param>
        /// <returns>true, wenn der Mauszeiger auf dem Objekt liegt</returns>
        protected bool IsMouseCursorInsideMyHitbox(MouseState ms)
        {
            Vector3 worldRay = Get3DMouseCoords(HelperGL.GetNormalizedMouseCoords(ms.X, ms.Y, CurrentWindow));
            Vector3 normal;
            bool result;
            Vector3 intersection;
            if (CurrentWorld != null && CurrentWorld.IsFirstPersonMode)
            {
                Vector3 fpPos = CurrentWorld.GetFirstPersonObject().Position;
                fpPos.Y += CurrentWorld.GetFirstPersonObject().FPSEyeOffset;
                normal = HelperCamera.GetLookAtVector();
                normal.Y += 0.000001f;
                normal.Z += 0.000001f;
                result = LinePlaneIntersection(out intersection, worldRay, fpPos, normal, GetCenterPointForAllHitboxes());
            }
            else
            {
                normal = -GetCameraLookAtVector();
                normal.Y += 0.000001f;
                normal.Z += 0.000001f;
                result = LinePlaneIntersection(out intersection, worldRay, CurrentWorld.GetCameraPosition(), normal, GetCenterPointForAllHitboxes());
            }
            
            
            if (result)
            {
                foreach(Hitbox hb in Hitboxes)
                {
                    if(IsPointInsideBox(intersection, hb.GetCenter(), hb.DiameterAveraged))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
                return false;
        }

        private bool IsPointInsideBox(Vector3 pos, Vector3 center, Vector3 dimensions)
        {
            return (
                pos.X >= center.X - dimensions.X / 2 &&
                pos.X <= center.X + dimensions.X / 2 &&
                pos.Y >= center.Y - dimensions.Y / 2 &&
                pos.Y <= center.Y + dimensions.Y / 2 &&
                pos.Z >= center.Z - dimensions.Z / 2 &&
                pos.Z <= center.Z + dimensions.Z / 2
                );
        }

        private bool IsPointInsideBox(Vector3 pos, Vector3 center, float diameter)
        {
            return (
                pos.X >= center.X - diameter/ 2 &&
                pos.X <= center.X + diameter / 2 &&
                pos.Y >= center.Y - diameter / 2 &&
                pos.Y <= center.Y + diameter / 2 &&
                pos.Z >= center.Z - diameter / 2 &&
                pos.Z <= center.Z + diameter / 2
                );
        }

        private bool LinePlaneIntersection(out Vector3 contact, Vector3 ray, Vector3 rayOrigin,
                                            Vector3 normal, Vector3 coord)
        {
            contact = Vector3.Zero;
            float d = Vector3.Dot(normal, coord);
            if (Vector3.Dot(normal, ray) == 0)
            {
                return false;
            }
            float x = (d - Vector3.Dot(normal, rayOrigin)) / Vector3.Dot(normal, ray);
            ray.NormalizeFast();
            contact = rayOrigin + ray * x;
            return true;
        }

        /// <summary>
        /// Erfragt die Rotation, die zu einem bestimmten Ziel notwendig wäre
        /// </summary>
        /// <param name="position">Ziel</param>
        /// <param name="plane">Rotationsebene (Standard: Camera)</param>
        /// <returns>Rotation (als Quaternion)</returns>
        protected Quaternion GetRotationToTarget(Vector3 position, Plane plane = Plane.Camera)
        {
            if (CurrentWindow.IsMouseInWindow)
            {
                Vector3 normal;
                if (plane == Plane.Y)
                    normal = new Vector3(0, 1, 0.000001f);
                else if (plane == Plane.X)
                    normal = new Vector3(1, 0, 0);
                else if (plane == Plane.Z)
                    normal = new Vector3(0, 0.000001f, 1);
                else
                {
                    normal = -GetCameraLookAtVector();
                }

                Matrix4 lookat = Matrix4.LookAt(GetCenterPointForAllHitboxes(), position, normal);
                lookat.Transpose();
                lookat.Invert();
                return Quaternion.FromMatrix(new Matrix3(lookat));
            }
            return Quaternion.Identity;
        }

       /// <summary>
       /// Dreht das Objekt, so dass es zur Zielkoordinate blickt
       /// </summary>
       /// <param name="target">Zielkoordinate</param>
        public void TurnTowardsXYZ(Vector3 target)
        {
            if (CurrentWindow.IsMouseInWindow)
            {
                Vector3 dir = target - GetCenterPointForAllHitboxes();
                if (dir.LengthFast < 0.1f)
                    return;
                target.Z += 0.00001f;
                Matrix4 lookat = Matrix4.LookAt(GetCenterPointForAllHitboxes(), target, KWEngine.WorldUp);
                lookat.Transpose();
                lookat.Invert();
                Rotation = Quaternion.FromMatrix(new Matrix3(lookat)) * Turn180;
            }
        }

        /// <summary>
        /// Dreht das Objekt, so dass es zur Zielkoordinate blickt
        /// </summary>
        /// <param name="targetX">X-Koordinate</param>
        /// <param name="targetY">Y-Koordinate</param>
        public void TurnTowardsXY(float targetX, float targetY)
        {
            if (CurrentWindow.IsMouseInWindow)
            {
                Vector3 target = new Vector3(targetX, targetY, 0);
                TurnTowardsXY(target);
            }
        }

        /// <summary>
        /// Verändert die Rotation der Instanz, so dass sie in Richtung der XY-Koordinaten blickt. Z-Unterschiede Unterschiede werden ignoriert.
        /// [Geeignet, wenn die Kamera entlang der z-Achse blickt (Standard)]
        /// </summary>
        /// <param name="target">Zielkoordinaten</param>
        public void TurnTowardsXY(Vector3 target)
        {
            if (CurrentWindow.IsMouseInWindow)
            {
                target.Z = GetCenterPointForAllHitboxes().Z;
                if ((target - Position).LengthFast < 0.00001f)
                    return;

                target.X += 0.000001f;
                Matrix4 lookat = Matrix4.LookAt(GetCenterPointForAllHitboxes(), target, Vector3.UnitZ);
                lookat.Transpose();
                lookat.Invert();
                Rotation = Quaternion.FromMatrix(new Matrix3(lookat)) * Turn180;
            }
        }

        /// <summary>
        /// Verändert die Rotation der Instanz, so dass sie in Richtung der XZ-Koordinaten blickt. Vertikale Unterschiede werden ignoriert.
        /// (Geeignet, wenn die Kamera entlang der y-Achse blickt)
        /// </summary>
        /// <param name="targetX">Zielkoordinate der x-Achse</param>
        /// <param name="targetZ">Zielkoordinate der z-Achse</param>
        public void TurnTowardsXZ(float targetX, float targetZ)
        {
            if (CurrentWindow.IsMouseInWindow)
            {
                Vector3 target = new Vector3(targetX, 0, targetZ);
                TurnTowardsXZ(target);
            }
        }

        /// <summary>
        /// Verändert die Rotation der Instanz, so dass sie in Richtung der XZ-Koordinaten blickt. Vertikale Unterschiede werden ignoriert.
        /// (Geeignet, wenn die Kamera entlang der y-Achse blickt)
        /// </summary>
        /// <param name="target">Zielkoordinaten</param>
        public void TurnTowardsXZ(Vector3 target)
        {
            if (CurrentWindow.IsMouseInWindow)
            {
                Vector3 currentPos = GetCenterPointForAllHitboxes();
                target.Y = currentPos.Y;
                if ((target - currentPos).LengthFast < 0.0001f)
                    return;

                target.X += 0.000001f;
                Matrix4 lookat = Matrix4.LookAt(currentPos, target, Vector3.UnitY);
                lookat.Transpose();
                lookat.Invert();
                Rotation = Quaternion.FromMatrix(new Matrix3(lookat)) * Turn180;
            }
        }

        /// <summary>
        /// Misst die Distanz zu einem Punkt
        /// </summary>
        /// <param name="position">Zielpunkt</param>
        /// <returns>Distanz</returns>
        public float GetDistanceTo(Vector3 position)
        {
            return (GetCenterPointForAllHitboxes() - position).LengthFast;
        }

        /// <summary>
        /// Misst die Distanz zu einem GameObject
        /// </summary>
        /// <param name="g">GameObject-Instanz</param>
        /// <returns>Distanz</returns>
        public float GetDistanceTo(GameObject g)
        {
            return (GetCenterPointForAllHitboxes() - g.GetCenterPointForAllHitboxes()).LengthFast;
        }

        /// <summary>
        /// Erfragt, ob das Objekt (mit kompletter Hitbox) noch von der Kamera gesehen werde kann
        /// </summary>
        public bool IsInsideScreenSpace { get; internal set; } = false;

        /// <summary>
        /// Gibt das GameObject zurück, das unter dem Mauszeiger liegt
        /// </summary>
        /// <param name="ms">Mausinformationen</param>
        /// <returns>Gewähltes GameObject</returns>
        protected GameObject PickGameObject(MouseState ms)
        {
            if (!CurrentWindow.Focused)
            {
                return null;
            }

            Vector3 ray = Get3DMouseCoords(ms.X, ms.Y);
            Vector3 pos = CurrentWorld.GetCameraPosition() + ray;

            GameObject pickedObject = null;
            float pickedDistance = float.MaxValue;

            foreach (GameObject go in CurrentWorld.GetGameObjects())
            {
                if (go.IsPickable && go.IsInsideScreenSpace)
                {
                    if (IntersectRaySphere(pos, ray, go.GetCenterPointForAllHitboxes(), GetMaxDiameter() / 2)) // GetDiameterFromDimensions(go.GetGameObjectCenterPoint(), go.GetGameObjectMaxDimensions())))
                    {
                        float distance = (go.GetCenterPointForAllHitboxes() - pos).LengthSquared;
                        if (distance < pickedDistance)
                        {
                            pickedDistance = distance;
                            pickedObject = go;
                        }
                    }
                }
            }
            
            return pickedObject;
        }

        private static bool IntersectRaySphere(Vector3 rayStart, Vector3 ray, Vector3 sphereCenter, float sphereRadius)
        {
            Vector3 p = rayStart - sphereCenter;

            float rSquared = sphereRadius * sphereRadius;
            float p_d = Vector3.Dot(p, ray);

            // The sphere is behind or surrounding the start point.
            if (p_d > 0 || Vector3.Dot(p, p) < rSquared)
                return false;

            // Flatten p into the plane passing through c perpendicular to the ray.
            // This gives the closest approach of the ray to the center.
            Vector3 a = p - p_d * ray;

            float aSquared = Vector3.Dot(a, a);

            // Closest approach is outside the sphere.
            if (aSquared > rSquared)
                return false;

            return true;
        }

        private static Vector3 Get3DMouseCoords(float x, float y)
        {
            HelperMouseRay r = new HelperMouseRay(x, y, GLWindow.CurrentWindow._viewMatrix, GLWindow.CurrentWindow._projectionMatrix);
            return Vector3.NormalizeFast(r.End - r.Start);
        }

        private static Vector3 Get3DMouseCoords(Vector2 mouseCoords)
        {
            HelperMouseRay r = new HelperMouseRay(mouseCoords.X, mouseCoords.Y, GLWindow.CurrentWindow._viewMatrix, GLWindow.CurrentWindow._projectionMatrix);
            return Vector3.NormalizeFast(r.End - r.Start);
        }


        /// <summary>
        /// Setzt die Textur für einen bestimmtem Mesh-Namen (Teil des Modells)
        /// </summary>
        /// <param name="meshName">Mesh</param>
        /// <param name="texture">Texturdatei</param>
        /// <param name="textureType">Texturtyp (Standard: Diffuse)</param>
        public void SetTextureForMesh(string meshName, string texture, TextureType textureType = TextureType.Diffuse)
        {
            CheckModel();
            CheckIfNotTerrain();
            if (_cubeModel != null)
            {
                SetTexture(texture, textureType, CubeSide.All);
                Debug.WriteLine("Method call forwarded to SetTexture() for KWCube instances. Please use SetTexture() for KWCube instances.");
                return;
            }

            if(textureType != TextureType.Diffuse && textureType != TextureType.Normal && textureType != TextureType.Specular)
            {
                throw new Exception("SetTextureForMesh() currently supports diffuse, normal and specular texture types only. Sorry.");
            }


            int id = 0;
            foreach (GeoMesh mesh in Model.Meshes.Values)
            {
                if (mesh.Name.ToLower().Contains(meshName.ToLower()))
                {
                    SetTextureForMesh(id, texture, textureType);
                    return;
                }
                id++;
            }
            throw new Exception("Mesh with name " + meshName + " not found.");
        }

        private void CheckIfNotTerrain()
        {
            if (Model != null && Model.IsTerrain)
                throw new Exception("Not a valid call for Terrain objects.");
        }

        /// <summary>
        /// Setzt die Texturwiederholungen für eine bestimmte Mesh-ID (Teil des Modells)
        /// </summary>
        /// <param name="meshId">Mesh-ID (bei 0 beginnend)</param>
        /// <param name="repeatX">Breitenwiederholungen</param>
        /// <param name="repeatY">Höhenwiederholungen</param>
        public void SetTextureRepeatForMesh(int meshId, float repeatX, float repeatY)
        {
            CheckModel();
            CheckIfNotTerrain();
            
            if (_cubeModel != null)
            {
                SetTextureRepeat(repeatX, repeatY, CubeSide.All);
                Debug.WriteLine("Method call forwarded to SetTextureRepeat() for KWCube instances. Please use SetTextureRepeat() for KWCube instances.");
                return;
            }

            GeoMesh mesh = Model.Meshes.Values.ElementAt(meshId);
            if (mesh != null)
            {
                if(_overrides[mesh.Name].ContainsKey(Override.TextureTransform))
                    _overrides[mesh.Name].Remove(Override.TextureTransform);
                _overrides[mesh.Name].Add(Override.TextureTransform, new Vector2(repeatX, repeatY));
            }
        }

        /// <summary>
        /// Setzt Blendmapping für Terrains
        /// </summary>
        /// <param name="blendTexture">Blend Map (schwarz, rot, grün, blau)</param>
        /// <param name="redTexture">Rottextur</param>
        /// <param name="greenTexture">Grüntextur</param>
        /// <param name="blueTexture">Blautextur</param>
        public void SetTextureTerrainBlendMapping(string blendTexture, string redTexture, string greenTexture = null, string blueTexture = null)
        {
            if (blendTexture != null && !File.Exists(blendTexture))
                throw new Exception("Blend texture not found.");

            if (redTexture != null && !File.Exists(redTexture))
                throw new Exception("Red texture not found.");

            if (greenTexture != null && !File.Exists(greenTexture))
                throw new Exception("Green texture not found.");

            if (blueTexture != null && !File.Exists(blueTexture))
                throw new Exception("Blue texture not found.");


            if (Model != null && Model.IsTerrain)
            {
                GeoTerrain terrain = Model.Meshes.Values.ElementAt(0).Terrain;
                if (blendTexture != null && redTexture != null)
                {
                    if (KWEngine.CustomTextures[KWEngine.CurrentWorld].ContainsKey(blendTexture))
                    {
                        terrain._texBlend = KWEngine.CustomTextures[KWEngine.CurrentWorld][blendTexture];
                    }
                    else
                    {
                        terrain._texBlend = HelperTexture.LoadTextureForModelExternal(blendTexture);
                        if (terrain._texBlend < 0)
                            terrain._texBlend = KWEngine.TextureBlack;

                        if (terrain._texBlend > 0 && terrain._texBlend != KWEngine.TextureBlack)
                        {
                            KWEngine.CustomTextures[KWEngine.CurrentWorld].Add(blendTexture, terrain._texBlend);
                        }
                    }

                    if (KWEngine.CustomTextures[KWEngine.CurrentWorld].ContainsKey(redTexture))
                    {
                        terrain._texR = KWEngine.CustomTextures[KWEngine.CurrentWorld][redTexture];
                    }
                    else
                    {
                        terrain._texR = HelperTexture.LoadTextureForModelExternal(redTexture);
                        if (terrain._texR > 0 && terrain._texR != KWEngine.TextureAlpha)
                        {
                            KWEngine.CustomTextures[KWEngine.CurrentWorld].Add(redTexture, terrain._texR);
                        }
                    }

                    if (greenTexture != null && KWEngine.CustomTextures[KWEngine.CurrentWorld].ContainsKey(greenTexture))
                    {
                        terrain._texG = KWEngine.CustomTextures[KWEngine.CurrentWorld][greenTexture];
                    }
                    else
                    {
                        terrain._texG = greenTexture == null ? KWEngine.TextureAlpha : HelperTexture.LoadTextureForModelExternal(greenTexture);
                        if (terrain._texG > 0 && terrain._texG != KWEngine.TextureAlpha)
                        {
                            KWEngine.CustomTextures[KWEngine.CurrentWorld].Add(greenTexture, terrain._texG);
                        }
                    }

                    if (blueTexture != null && KWEngine.CustomTextures[KWEngine.CurrentWorld].ContainsKey(blueTexture))
                    {
                        terrain._texB = KWEngine.CustomTextures[KWEngine.CurrentWorld][blueTexture];
                    }
                    else
                    {
                        terrain._texB = blueTexture == null ? KWEngine.TextureAlpha : HelperTexture.LoadTextureForModelExternal(blueTexture);
                        if(terrain._texB > 0 && terrain._texB != KWEngine.TextureAlpha)
                        {
                            KWEngine.CustomTextures[KWEngine.CurrentWorld].Add(blueTexture, terrain._texB);
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("No valid blend and red textures chosen. Please check your image files.");
                    terrain._texBlend = KWEngine.TextureBlack;
                    terrain._texR = KWEngine.TextureAlpha;
                    terrain._texG = KWEngine.TextureAlpha;
                    terrain._texB = KWEngine.TextureAlpha;
                }
            }
            else
            {
                throw new Exception("Method SetTextureTerrainBlendMapping() may only be called from a GameObject that has a terrain attached to it.");
            }
        }

        /// <summary>
        /// Setzt die Textur für eine bestimmte Mesh-ID (Teil des Modells)
        /// </summary>
        /// <param name="meshID">ID</param>
        /// <param name="texture">Texturdatei</param>
        /// <param name="textureType">Texturtyp</param>
        public void SetTextureForMesh(int meshID, string texture, TextureType textureType = TextureType.Diffuse)
        {
            CheckModel();
            CheckIfNotTerrain();
            if (_cubeModel != null)
            {
                SetTexture(texture, textureType, CubeSide.All);
                Debug.WriteLine("Method call forwarded to SetTexture() for KWCube instances. Please use SetTexture() for KWCube instances.");
                return;
            }

            if (textureType != TextureType.Diffuse && textureType != TextureType.Normal && textureType != TextureType.Specular)
            {
                throw new Exception("SetTextureForMesh() currently supports diffuse, normal and specular texture types only. Sorry.");
            }

            GeoMesh mesh = Model.Meshes.Values.ElementAt(meshID);
          

            GeoTexture tex = new GeoTexture();
            int texId = -1;
            string texName = "";
            foreach (string texturefilename in Model.Textures.Keys)
            {
                string nameStrippedLowered = SceneImporter.StripPathFromFile(texturefilename).ToLower();
                if (nameStrippedLowered.Contains(texture.Trim().ToLower()))
                {
                    texId = Model.Textures[texturefilename].OpenGLID;
                    texName = texturefilename;
                    break;
                }
            }
            if (texId < 0)
            {
                if (KWEngine.CustomTextures[KWEngine.CurrentWorld].ContainsKey(texture))
                {
                    texId = KWEngine.CustomTextures[KWEngine.CurrentWorld][texture];
                }
                else
                {
                    texId = HelperTexture.LoadTextureForModelExternal(texture);
                    if (texId > 0)
                        KWEngine.CustomTextures[KWEngine.CurrentWorld].Add(texture, texId);
                }
                
                if(texId < 0)
                {
                    throw new Exception("Cannot find custom texture " + texture + ". Is your path correct?");
                }
                texName = texture;
            }

            tex.UVMapIndex = 0;
            tex.UVTransform = new Vector2(1, 1);
            tex.OpenGLID = texId;
            tex.Filename = texName;
            if (textureType == TextureType.Diffuse)
            {
                if(_overrides[mesh.Name].ContainsKey(Override.TextureDiffuse))
                    _overrides[mesh.Name].Remove(Override.TextureDiffuse);
                _overrides[mesh.Name].Add(Override.TextureDiffuse, tex);
            }
            else if (textureType == TextureType.Normal)
            {
                if (_overrides[mesh.Name].ContainsKey(Override.TextureNormal))
                    _overrides[mesh.Name].Remove(Override.TextureNormal);
                _overrides[mesh.Name].Add(Override.TextureNormal, tex);
            }
            else
            {
                if (_overrides[mesh.Name].ContainsKey(Override.TextureSpecular))
                    _overrides[mesh.Name].Remove(Override.TextureSpecular);
                _overrides[mesh.Name].Add(Override.TextureSpecular, tex);
            }
        }

        /// <summary>
        /// Erfragt eine Liste aller Mesh-Namen des Objekts
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<string> GetMeshNameList()
        {
            CheckModel();

            if(_cubeModel != null)
            {
                throw new Exception("GetMeshNameList() is not available on KWCube models.");
            }

            return _meshNameList;
        }

        /// <summary>
        /// Setzt die Spiegelungsstärke des Objekts
        /// </summary>
        /// <param name="enable">an/aus</param>
        /// <param name="power">Intensität (Standard: 1)</param>
        /// <param name="area">Fläche (je größer der Wert, desto kleiner die Reflektionsfläche)</param>
        public void SetSpecularOverride(bool enable, float power = 1, float area = 1024)
        {
            CheckModel();
            if (Model.IsTerrain)
            {
                Model.Meshes.Values.ElementAt(0).Material.SpecularArea = HelperGL.Clamp(area, 2, 8192);
                Model.Meshes.Values.ElementAt(0).Material.SpecularPower = enable ? HelperGL.Clamp(power, 0, 2048) : 0;
            }
            else if(_cubeModel != null)
            {
                _cubeModel.SpecularPower = enable ? HelperGL.Clamp(power, 0, 2048) : 0;
                _cubeModel.SpecularArea = HelperGL.Clamp(area, 2, 8192);            
            }
            else
            {
                foreach (GeoMesh mesh in Model.Meshes.Values)
                {
                    if (enable)
                    {
                        if(_overrides[mesh.Name].ContainsKey(Override.SpecularPower))
                            _overrides[mesh.Name].Remove(Override.SpecularPower);
                        _overrides[mesh.Name].Add(Override.SpecularPower, HelperGL.Clamp(power, 0, 100));

                        if (_overrides[mesh.Name].ContainsKey(Override.SpecularEnable))
                            _overrides[mesh.Name].Remove(Override.SpecularEnable);
                        _overrides[mesh.Name].Add(Override.SpecularEnable, enable);

                        if (_overrides[mesh.Name].ContainsKey(Override.SpecularArea))
                            _overrides[mesh.Name].Remove(Override.SpecularArea);
                        _overrides[mesh.Name].Add(Override.SpecularArea, HelperGL.Clamp(area, 2, 8192));


                    }
                    else
                    {
                        if (_overrides[mesh.Name].ContainsKey(Override.SpecularPower))
                            _overrides[mesh.Name].Remove(Override.SpecularPower);
                        if (_overrides[mesh.Name].ContainsKey(Override.SpecularEnable))
                            _overrides[mesh.Name].Remove(Override.SpecularEnable);
                        if (_overrides[mesh.Name].ContainsKey(Override.SpecularArea))
                            _overrides[mesh.Name].Remove(Override.SpecularArea);
                    }
                }
            }
        }

        /// <summary>
        ///  Setzt die Spiegelungsstärke für einen Teil des 3D-Modells (Mesh)
        /// </summary>
        /// <param name="meshName">Name des Meshs</param>
        /// <param name="enable">an/aus</param>
        /// <param name="power">Intensität (Standard: 1)</param>
        /// <param name="area">Fläche (je größer der Wert, desto kleiner die Reflektionsfläche)</param>
        public void SetSpecularOverrideForMesh(string meshName, bool enable, float power = 1, float area = 1024)
        {
            CheckModel();
            CheckIfNotTerrain();
            if (_cubeModel != null)
            {
                SetSpecularOverride(enable, power, area);
                return;
            }

            foreach (GeoMesh mesh in Model.Meshes.Values)
            {
                if (mesh.Name.ToLower().Contains(meshName.Trim().ToLower()))
                {
                    if (enable)
                    {
                        _overrides[mesh.Name].Remove(Override.SpecularPower);
                        _overrides[mesh.Name].Add(Override.SpecularPower, HelperGL.Clamp(power, 0, 100));

                        _overrides[mesh.Name].Remove(Override.SpecularEnable);
                        _overrides[mesh.Name].Add(Override.SpecularEnable, enable);

                        _overrides[mesh.Name].Remove(Override.SpecularArea);
                        _overrides[mesh.Name].Add(Override.SpecularArea, HelperGL.Clamp(area, 2, 8192));
                    }
                    else
                    {
                        _overrides[mesh.Name].Remove(Override.SpecularPower);
                        _overrides[mesh.Name].Remove(Override.SpecularEnable);
                        _overrides[mesh.Name].Remove(Override.SpecularArea);
                    }

                    return;
                }
            }
            throw new Exception("Mesh " + meshName + " not found in Model.");
        }

        /// <summary>
        ///  Setzt die Spiegelungsstärke für einen Teil des 3D-Modells (Mesh)
        /// </summary>
        /// <param name="meshID">ID des Meshs</param>
        /// <param name="enable">an/aus</param>
        /// <param name="power">Intensität (Standard: 1)</param>
        /// <param name="area">Fläche (je größer der Wert, desto kleiner die Reflektionsfläche)</param>
        public void SetSpecularOverrideForMesh(int meshID, bool enable, float power = 1, float area = 1024)
        {
            if (_cubeModel != null)
            {
                SetSpecularOverride(enable, power, area);
                return;
            }

            int c = 0;
            foreach (GeoMesh mesh in Model.Meshes.Values)
            {
                if(c == meshID)
                {
                    SetSpecularOverrideForMesh(mesh.Name, enable, power, area);
                    return;
                }
                c++;
            }
            throw new Exception("Mesh with ID " + meshID + " not found in Model.");
        }

        /// <summary>
        /// Spielt einen Ton ab
        /// </summary>
        /// <param name="audiofile">Audiodatei</param>
        /// <param name="playLooping">looped playback?</param>
        /// <param name="volume">Lautstärke</param>
        public void SoundPlay(string audiofile, bool playLooping = false, float volume = 1.0f)
        {
            GLAudioEngine.SoundPlay(audiofile, playLooping, volume);
        }

        /// <summary>
        /// Stoppt einen Ton
        /// </summary>
        /// <param name="audiofile">Audiodatei</param>
        public void SoundStop(string audiofile)
        {
            GLAudioEngine.SoundStop(audiofile);
        }

        /// <summary>
        /// Stoppt alle Töne
        /// </summary>
        public void SoundStopAll()
        {
            GLAudioEngine.SoundStopAll();
        }
    }
}
