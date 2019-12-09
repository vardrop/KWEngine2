using System;
using KWEngine2.Model;
using KWEngine2.Renderers;
using System.Collections.Generic;
using OpenTK;

namespace KWEngine2
{
    public class KWEngine
    {
        public const int MAX_BONE_WEIGHTS = 3;
        public const int MAX_BONES = 36;
        internal static Matrix4 Identity = Matrix4.Identity;
        private static Vector3 _worldUp = new Vector3(0, 1, 0);

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
            Models.Add("KWRect", SceneImporter.LoadModel("kwrect.obj", true));
            Models.Add("KWSphere", SceneImporter.LoadModel("kwsphere.obj", true));
        }

        internal static void InitializeShaders()
        {
            Renderers.Add("Standard", new RendererStandard());
        }

        public static GeoModel GetModel(string name)
        {
            bool modelFound = Models.TryGetValue(name, out GeoModel m);
            if (!modelFound)
                throw new Exception("Model " + name + " not found.");
            return m;
        }        
    }
}
