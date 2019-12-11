using System;
using KWEngine2.Model;
using KWEngine2.Renderers;
using System.Collections.Generic;
using OpenTK;
using KWEngine2.Helper;
using System.Diagnostics;

namespace KWEngine2
{
    public class KWEngine
    {
        public const int MAX_BONE_WEIGHTS = 3;
        public const int MAX_BONES = 36;
        internal static Matrix4 Identity = Matrix4.Identity;
        private static Vector3 _worldUp = new Vector3(0, 1, 0);

        public static Dictionary<World, Dictionary<string, int>> CubeTextures { get; internal set; } = new Dictionary<World, Dictionary<string, int>>();
        public enum CubeSide { All, Front, Back, Left, Right, Top, Bottom }
        public enum TextureType { Diffuse, Normal, Specular };
        public static Vector3 WorldUp
        {
            get
            {
                return _worldUp;
            }
            set
            {
                _worldUp = Vector3.Normalize(value);
            }
        }
        

        internal static Dictionary<string, Renderer> Renderers { get; set; } = new Dictionary<string, Renderer>();
        internal static Dictionary<string, GeoModel> Models { get; set; } = new Dictionary<string, GeoModel>();

        internal static void InitializeModels()
        {
            Models.Add("KWCube", SceneImporter.LoadModel("kwcube.obj", true));
            Models.Add("KWCube6", SceneImporter.LoadModel("kwcube6.obj", true));
            Models.Add("KWRect", SceneImporter.LoadModel("kwrect.obj", true));
            Models.Add("KWSphere", SceneImporter.LoadModel("kwsphere.obj", true));
        }

        internal static void InitializeShaders()
        {
            Renderers.Add("Standard", new RendererStandard());
            Renderers.Add("Shadow", new RendererShadow());
        }

        public static GeoModel GetModel(string name)
        {
            bool modelFound = Models.TryGetValue(name, out GeoModel m);
            if (!modelFound)
                throw new Exception("Model " + name + " not found.");
            return m;
        }

        private static int _shadowMapSize = 1024;
        public static int ShadowMapSize 
        {
            get
            {
                return _shadowMapSize;
            }
            set
            {
                if(value >= 256 && value <= 8192)
                {
                    _shadowMapSize = HelperTexture.RoundToPowerOf2(value);
                }
                else
                {
                    Debug.WriteLine("Cannot set shadow map to a size < 256 or > 8192. Resetting it to 1024.");
                    _shadowMapSize = 1024;
                }
            }
        }
    }
}
