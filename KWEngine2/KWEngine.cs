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
        public const int MAX_LIGHTS = 10;
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
            Renderers.Add("Bloom", new RendererBloom());
        }

        public static GeoModel GetModel(string name)
        {
            bool modelFound = Models.TryGetValue(name, out GeoModel m);
            if (!modelFound)
                throw new Exception("Model " + name + " not found.");
            return m;
        }

        private static int _shadowMapSize = 2048;
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

        public static World CurrentWorld 
        {
            get
            {
                return GLWindow.CurrentWindow.CurrentWorld;
            }
        }

        public static void BuildTerrainModel(string name, string heightmap, string texture, float width, float height, float depth, float texRepeatX = 1, float texRepeatZ = 1)
        {
            if (Models.ContainsKey(name)){
                throw new Exception("There already is a model with that name. Please choose a different name.");
            }
            GeoModel terrainModel = new GeoModel();
            terrainModel.Name = name;
            terrainModel.Meshes = new Dictionary<string, GeoMesh>();
            terrainModel.IsValid = true;

            GeoMeshHitbox meshHitBox = new GeoMeshHitbox(0 + width / 2, 0 + height / 2, 0 + depth / 2, 0 - width / 2, 0 - height / 2, 0 - depth / 2);
            meshHitBox.Model = terrainModel;
            meshHitBox.Name = name;

            terrainModel.MeshHitboxes = new List<GeoMeshHitbox>();
            terrainModel.MeshHitboxes.Add(meshHitBox);

            GeoTerrain t = new GeoTerrain();
            GeoMesh terrainMesh = t.BuildTerrain(new Vector3(0, 0, 0), heightmap, width, height, depth, texRepeatX, texRepeatZ);
            terrainMesh.Terrain = t;
            GeoMaterial mat = new GeoMaterial();
            mat.BlendMode = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha;
            mat.ColorDiffuse = new Vector4(1, 1, 1, 1);
            mat.ColorEmissive = new Vector4(0, 0, 0, 0);
            mat.Name = name + "-Material";
            mat.SpecularArea = 512;
            mat.SpecularPower = 0;

            GeoTexture texDiffuse = new GeoTexture(name + "-TextureDiffuse");
            texDiffuse.Filename = heightmap;
            texDiffuse.Type = GeoTexture.TexType.Diffuse;
            texDiffuse.UVMapIndex = 0;
            texDiffuse.UVTransform = new Vector2(texRepeatX, texRepeatZ);

            bool dictFound = CubeTextures.TryGetValue(CurrentWorld, out Dictionary<string, int> texDict);

            if (dictFound && texDict.ContainsKey(texture))
            {
                texDiffuse.OpenGLID = texDict[texture];
            }
            else
            {
                texDiffuse.OpenGLID = HelperTexture.LoadTextureForModelExternal(texture);
                if (dictFound)
                {
                    texDict.Add(texture, texDiffuse.OpenGLID);
                }
            }
            mat.TextureDiffuse = texDiffuse;


            terrainMesh.Material = mat;


            //terrainModel._terrain = t;
            terrainModel.Meshes.Add("Terrain", terrainMesh);

            KWEngine.Models.Add(name, terrainModel);

        }

        public static void LoadModelFromFile(string name, string filename)
        {
            GeoModel m = SceneImporter.LoadModel(filename);
            m.Name = name;
            lock (KWEngine.Models)
            {
                name = name.ToLower();
                if (!KWEngine.Models.ContainsKey(name))
                    KWEngine.Models.Add(name, m);
                else
                    throw new Exception("A model with the name " + name + " already exists.");
            }
        }
    }
}
