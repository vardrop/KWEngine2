using KWEngine2.Collision;
using KWEngine2.Helper;
using KWEngine2.Model;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static KWEngine2.KWEngine;

namespace KWEngine2.GameObjects
{
    public abstract class GameObject : IComparable
    {
        public enum Plane { X, Y, Z, Camera }
        internal uint DistanceToCamera { get; set; } = 100;
        public bool IsShadowCaster { get; set; } = false;
        public bool IsAffectedBySun { get; set; } = true;
        public World CurrentWorld { get; internal set; } = null;
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
        
        public Vector3 Color
        {
            get
            {
                return _tintColor;
            }
            set
            {
                _tintColor.X = HelperGL.Clamp(value.X, 0, 1);
                _tintColor.Y = HelperGL.Clamp(value.Y, 0, 1);
                _tintColor.Z = HelperGL.Clamp(value.Z, 0, 1);
               
            }
        }

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
        

        public void SetGlow(float red, float green, float blue, float intensity)
        {
            Glow = new Vector4(red, green, blue, intensity);
        }

        public bool IsPickable { get; set; } = false;
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

        private float _animationPercentage = 0f;
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
        public bool IsCollisionObject { get; set; } = false;
        public object Tag { get; protected set; } = null;
        private GeoModel _model;
        internal Matrix4 _modelMatrix = Matrix4.Identity;
        private Quaternion _rotation = new Quaternion(0, 0, 0, 1);
        private Vector3 _scale = new Vector3(1, 1, 1);
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
            }
        }

        internal Vector3 _sceneCenter = new Vector3(0, 0, 0);
        internal Vector3 _sceneDimensions = new Vector3(0, 0, 0);
        internal float _sceneDiameter = 1;

        private Vector3 _position = new Vector3(0, 0, 0);
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                UpdateModelMatrixAndHitboxes();
            }
        }

        public void SetScale(float x, float y, float z)
        {
            Scale = new Vector3(x, y, z);
        }

        public void SetScale(float scale)
        {
            Scale = new Vector3(scale, scale, scale);
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
                UpdateModelMatrixAndHitboxes();
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

        internal Hitbox GetLargestHitbox()
        {
            return Hitboxes[_largestHitboxIndex];
        }

        internal void UpdateModelMatrixAndHitboxes()
        {
            //_modelMatrix = Matrix4.CreateScale(_scale) * Matrix4.CreateFromQuaternion(Quaternion.FromAxisAngle(KWEngine.WorldUp, _baseRotation)) * Matrix4.CreateFromQuaternion(_rotation) * Matrix4.CreateTranslation(_position);
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
            Vector3 downLeftFront = new Vector3(GetGameObjectCenterPoint().X - GetGameObjectMaxDimensions().X / 2, GetGameObjectCenterPoint().Y - GetGameObjectMaxDimensions().Y / 2, GetGameObjectCenterPoint().Z + GetGameObjectMaxDimensions().Z / 2);
            Vector3 upRightBack = new Vector3(GetGameObjectCenterPoint().X + GetGameObjectMaxDimensions().X / 2, GetGameObjectCenterPoint().Y + GetGameObjectMaxDimensions().Y / 2, GetGameObjectCenterPoint().Z - GetGameObjectMaxDimensions().Z / 2);
            _sceneDiameter = (upRightBack - downLeftFront).LengthFast;

            if(CurrentWorld != null)
                DistanceToCamera = (uint)((CurrentWorld.GetCameraPosition() - GetGameObjectCenterPoint()).LengthSquared * 10000);
        }

        public Vector3 GetGameObjectCenterPoint()
        {
            return _sceneCenter;
        }

        public Vector3 GetGameObjectMaxDimensions()
        {
            return _sceneDimensions;
        }

        public float GetGameObjectMaxDiameter()
        {
            return _sceneDiameter;
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
                _cubeModel = null;

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
                    BoneTranslationMatrices[mesh.Name] = new Matrix4[mesh.Bones.Count];
                    for (int i = 0; i < mesh.Bones.Count; i++)
                        BoneTranslationMatrices[mesh.Name][i] = Matrix4.Identity;
                }
            }
            
        }

        public abstract void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor);

        #region Gameplay
        public Vector3 GetRotationEulerAngles()
        {
            return HelperRotation.ConvertQuaternionToEulerAngles(Rotation);
        }

        public void SetRotation(Quaternion rotation)
        {
            Rotation = rotation;
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

        public void SetRotation(float x, float y, float z)
        {
            if (Model.IsTerrain)
            {
                Debug.WriteLine("Setting rotation on a GeoTerrain instance is not supported.");
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

        public void AddRotation(Quaternion r)
        {
            if (Model.IsTerrain)
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
            if (Model.IsTerrain)
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

        public void AddRotationZ(float amount, bool absolute = false)
        {
            if (Model.IsTerrain)
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
            if (Model != null)
            {
                Position = new Vector3(newPosition);
            }
            else
                throw new Exception("Cannot set position on empty geometry object. Did you assign an instance of GeometryObject to your GameObject?");

        }

        protected void SetColor(float red, float green, float blue)
        {
            _tintColor.X = red >= 0 && red <= 1 ? red : 1;
            _tintColor.Y = green >= 0 && green <= 1 ? green : 1;
            _tintColor.Z = blue >= 0 && blue <= 1 ? blue : 1;
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

        internal void ProcessCurrentAnimation()
        {
            if (AnimationID >= 0 && AnimationID < Model.Animations.Count)
            {
                GeoAnimation a = Model.Animations[AnimationID];
                Matrix4 identity = Matrix4.Identity * Model.PreRotation;

                float timestamp = a.DurationInTicks * AnimationPercentage;
                ReadNodeHierarchy(timestamp, ref a, AnimationID, Model.Root, ref identity);
            }
        }



        private void ReadNodeHierarchy(float timestamp, ref GeoAnimation animation, int animationId, GeoNode node, ref Matrix4 parentTransform, int debugLevel = 0)
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

            lock (BoneTranslationMatrices)
            {
                foreach (GeoMesh m in Model.Meshes.Values)
                {
                    bool found = m.Bones.TryGetValue(node.Name, out GeoBone gBone);
                    if (found)
                    {
                        BoneTranslationMatrices[m.Name][gBone.Index] = gBone.Offset * globalTransform * Model.TransformGlobalInverse;
                    }
                }
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                ReadNodeHierarchy(timestamp, ref animation, animationId, node.Children[i], ref globalTransform, debugLevel + 1);
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

        protected List<Intersection> GetIntersections()
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
                if (ConsiderForMeasurement(go, this))
                {
                    foreach (Hitbox hbother in go.Hitboxes)
                    {
                        foreach (Hitbox hbcaller in this.Hitboxes)
                        {
                            Intersection i = null;
                            if (hbother.Owner.Model.IsTerrain)
                            {
                                i = Hitbox.TestIntersectionTerrain(hbcaller, hbother);
                            }
                            else
                            {
                                i = Hitbox.TestIntersection(hbcaller, hbother);
                            }

                            if (i != null)
                                intersections.Add(i);
                        }
                    }
                }
            }

            return intersections;
        }

        private static bool ConsiderForMeasurement(GameObject go, GameObject caller)
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

                Hitbox hbCaller = caller.Hitboxes[caller._largestHitboxIndex];
                if (hbCaller.GetCenter().X >= left && hbCaller.GetCenter().X <= right
                    && hbCaller.GetCenter().Z >= back && hbCaller.GetCenter().Z <= front
                    && hbCaller.GetCenter().Y >= terraLow
                    && hbCaller.GetLowestVertexHeight() <= (terraHigh * 1.01f))
                {
                    return true;
                }
                return false;
            }
            else
            {
                float distance = (caller.GetGameObjectCenterPoint() - go.GetGameObjectCenterPoint()).LengthFast;
                float rad1 = caller._sceneDiameter / 2;
                float rad2 = go._sceneDiameter / 2;
                if (distance - (rad1 + rad2) > 0)
                    return false;
                else
                    return true;
            }
        }
        
        public int CompareTo(object obj)
        {
            GameObject g = (GameObject)obj;
            return g.DistanceToCamera > this.DistanceToCamera ? 1 : -1;
        }
        

        public void SetTexture(string texture, CubeSide side, TextureType type = TextureType.Diffuse)
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
                throw new Exception("Cannot set textures for model " + Model.Name + " because it is not a KWCube.");
            }
        }

        public void SetTextureRepeat(float x, float y, CubeSide side)
        {
            CheckModelAndWorld();

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
                throw new Exception("Cannot set textures for model " + Model.Name + " because it is not a KWCube.");
            }
        }

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

            bool result = LinePlaneIntersection(out Vector3 intersection, worldRay, CurrentWorld.GetCameraPosition(), normal, GetGameObjectCenterPoint());
            if (result)
            {
                return intersection;
            }
            else
                return Position;
        }

        protected bool IsMouseCursorInsideMyHitbox(MouseState ms)
        {
            if (CurrentWorld.IsFirstPersonMode)
                throw new Exception("Method GetMouseIntersectionPoint2() may not be called from First Person Mode object.");

            Vector3 worldRay = Get3DMouseCoords(HelperGL.GetNormalizedMouseCoords(ms.X, ms.Y, CurrentWindow));
            Vector3 normal = -GetCameraLookAtVector();
            normal.Y += 0.000001f;
            normal.Z += 0.000001f;
            bool result = LinePlaneIntersection(out Vector3 intersection, worldRay, CurrentWorld.GetCameraPosition(), normal, GetGameObjectCenterPoint());
            if (result)
                return IsPointInsideBox(intersection, GetGameObjectCenterPoint(), GetGameObjectMaxDimensions());
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

        protected Quaternion GetRotationToTarget(Vector3 position, Plane plane = Plane.Camera)
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

            Matrix4 lookat = Matrix4.LookAt(GetGameObjectCenterPoint(), position, normal);
            lookat.Transpose();
            lookat.Invert();
            return Quaternion.FromMatrix(new Matrix3(lookat));
        }

       
        public void TurnTowardsXYZ(Vector3 target)
        {
            Vector3 dir = target - GetGameObjectCenterPoint();
            if (dir.LengthFast < 0.1f)
                return;
          
            Matrix4 lookat = Matrix4.LookAt(GetGameObjectCenterPoint(), target, -GetCameraLookAtVector());
            lookat.Transpose();
            lookat.Invert();
            Rotation = Quaternion.FromAxisAngle(KWEngine.WorldUp, (float)Math.PI) * Quaternion.FromMatrix(new Matrix3(lookat));
        }

        public void TurnTowardsXY(float targetX, float targetY)
        {
            Vector3 target = new Vector3(targetX, targetY, GetGameObjectCenterPoint().Z);
            TurnTowardsXY(target);
        }

        /// <summary>
        /// Verändert die Rotation der Instanz, so dass sie in Richtung der XY-Koordinaten blickt. Z-Unterschiede Unterschiede werden ignoriert.
        /// [Geeignet, wenn die Kamera entlang der z-Achse blickt (Standard)]
        /// </summary>
        /// <param name="target">Zielkoordinaten</param>
        public void TurnTowardsXY(Vector3 target)
        {
            target.Z = GetGameObjectCenterPoint().Z;
            if ((target - Position).LengthFast < 0.1f)
                return;

            Matrix4 lookat = Matrix4.LookAt(GetGameObjectCenterPoint(), target, Vector3.UnitZ);
            lookat.Transpose();
            lookat.Invert();
            Quaternion newRotation = Quaternion.FromMatrix(new Matrix3(lookat));
            newRotation *= Quaternion.FromAxisAngle(Vector3.UnitY, (float)Math.PI);
            Rotation = newRotation;
        }

        /// <summary>
        /// Verändert die Rotation der Instanz, so dass sie in Richtung der XZ-Koordinaten blickt. Vertikale Unterschiede werden ignoriert.
        /// (Geeignet, wenn die Kamera entlang der y-Achse blickt)
        /// </summary>
        /// <param name="targetX">Zielkoordinate der x-Achse</param>
        /// <param name="targetZ">Zielkoordinate der z-Achse</param>
        public void TurnTowardsXZ(float targetX, float targetZ)
        {
            Vector3 target = new Vector3(targetX, GetGameObjectCenterPoint().Y, targetZ);
            TurnTowardsXZ(target);
        }

        /// <summary>
        /// Verändert die Rotation der Instanz, so dass sie in Richtung der XZ-Koordinaten blickt. Vertikale Unterschiede werden ignoriert.
        /// (Geeignet, wenn die Kamera entlang der y-Achse blickt)
        /// </summary>
        /// <param name="target">Zielkoordinaten</param>
        public void TurnTowardsXZ(Vector3 target)
        {
            Vector3 currentPos = GetGameObjectCenterPoint();
            target.Y = currentPos.Y;
            if ((target - currentPos).LengthFast < 0.1f)
                return;

            Matrix4 lookat = Matrix4.LookAt(GetGameObjectCenterPoint(), target, Vector3.UnitY);
            lookat.Transpose();
            lookat.Invert();
            Quaternion newRotation = Quaternion.FromMatrix(new Matrix3(lookat));
            newRotation *= Quaternion.FromAxisAngle(Vector3.UnitY, (float)Math.PI);
            Rotation = newRotation;
        }

        public bool IsInsideScreenSpace
        {
            get
            {
                if(CurrentWorld != null)
                {
                    return CurrentWindow.Frustum.SphereVsFrustum(this.GetGameObjectCenterPoint(), this.GetGameObjectMaxDiameter() / 2);
                }
                else
                {
                    return false;
                }
            }
        }

        protected GameObject PickGameObject(float x, float y)
        {
            Vector3 ray = Get3DMouseCoords(x, y);
            Vector3 pos = CurrentWorld.GetCameraPosition() + ray;

            GameObject pickedObject = null;
            float pickedDistance = float.MaxValue;

            foreach (GameObject go in CurrentWorld.GetGameObjects())
            {
                if (go.IsPickable && go.IsInsideScreenSpace)
                {
                    if (IntersectRaySphere(pos, ray, go.GetGameObjectCenterPoint(), GetGameObjectMaxDiameter() / 2)) // GetDiameterFromDimensions(go.GetGameObjectCenterPoint(), go.GetGameObjectMaxDimensions())))
                    {
                        float distance = (go.GetGameObjectCenterPoint() - pos).LengthSquared;
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
    }
}
