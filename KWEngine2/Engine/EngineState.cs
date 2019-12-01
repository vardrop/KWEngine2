using KWEngine2.Model;
using KWEngine2.Renderers;
using System.Collections.Generic;

namespace KWEngine2.Engine
{
    public class EngineState
    {
        public const int MAX_BONE_WEIGHTS = 3;
        public const int MAX_BONES = 36;

        internal static Dictionary<string, Renderer> Renderers { get; set; } = new Dictionary<string, Renderer>();
        internal static Dictionary<string, GeoModel> Models { get; set; } = new Dictionary<string, GeoModel>();

        internal static void InitializeModels()
        {
            Models.Add("KWCube", SceneImporter.LoadModel("littlegirl.fbx", true));
        }

        internal static void InitializeShaders()
        {
            Renderers.Add("Standard", new RendererStandard());
        }
    }
}
