using System;
using KWEngine2.Model;
using KWEngine2.Renderers;
using System.Collections.Generic;
using OpenTK;
using KWEngine2.Helper;
using System.Diagnostics;
using KWEngine2.GameObjects;
using System.Reflection;
using System.IO;
using System.Drawing.Text;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine2
{
    /// <summary>
    /// Kernbibliothek der Engine
    /// </summary>
    public class KWEngine
    {
        /// <summary>
        /// Anzahl der Gewichte pro Knochen
        /// </summary>
        public const int MAX_BONE_WEIGHTS = 3;
        /// <summary>
        /// Anzahl der Knochen pro GameObject
        /// </summary>
        public const int MAX_BONES = 96;
        /// <summary>
        /// Anzahl der Lichter pro Welt
        /// </summary>
        public const int MAX_LIGHTS = 10;
        internal static Matrix4 Identity = Matrix4.Identity;
        private static Vector3 _worldUp = new Vector3(0, 1, 0);

        /// <summary>
        /// Qualität des Glow-Effekts
        /// </summary>
        public enum PostProcessingQuality {
            /// <summary>
            /// Hoch
            /// </summary>
            High, 
            /// <summary>
            /// Niedrig (Standard)
            /// </summary>
            Low };

        internal static int TextureDefault = -1;
        internal static int TextureBlack = -1;
        internal static int TextureAlpha = -1;
        internal static float TimeElapsed = 0;

        /// <summary>
        /// Qualität der Post-Processing-Effekte (Glühen)
        /// </summary>
        public static PostProcessingQuality PostProcessQuality { get; set; } = PostProcessingQuality.Low;

        internal static Dictionary<ParticleType, ParticleInfo> ParticleDictionary = new Dictionary<ParticleType, ParticleInfo>();

        internal static Dictionary<World, Dictionary<string, int>> CustomTextures { get; set; } = new Dictionary<World, Dictionary<string, int>>();

        /// <summary>
        /// Seite des KWCube
        /// </summary>
        public enum CubeSide {
            /// <summary>
            /// Alle Würfelseiten
            /// </summary>
            All, 
            /// <summary>
            /// Frontseite (+Z)
            /// </summary>
            Front, 
            /// <summary>
            /// Rückseite (-Z)
            /// </summary>
            Back, 
            /// <summary>
            /// Links (-X)
            /// </summary>
            Left, 
            /// <summary>
            /// Rechts (+X)
            /// </summary>
            Right, 
            /// <summary>
            /// Oben (+Y)
            /// </summary>
            Top, 
            /// <summary>
            /// Unten (-Y)
            /// </summary>
            Bottom }
        /// <summary>
        /// Art der Textur (Standard: Diffuse)
        /// </summary>
        public enum TextureType { 
            /// <summary>
            /// Standardtexture
            /// </summary>
            Diffuse, 
            /// <summary>
            /// Normal Map
            /// </summary>
            Normal, 
            /// <summary>
            /// Specular Map
            /// </summary>
            Specular
                /*,
            /// <summary>
            /// Metallic Map (PBR Workflow)
            /// </summary>
            Metallic,
            /// <summary>
            /// Roughness Map (PBR Workflow)
            /// </summary>
            Roughness
            */
        };

        /// <summary>
        /// Welt-Vektor, der angibt, wo 'oben' ist
        /// </summary>
        public static Vector3 WorldUp
        {
            get
            {
                return _worldUp;
            }
            internal set
            {
                _worldUp = Vector3.Normalize(value);
            }
        }

        internal static PrivateFontCollection Collection = new PrivateFontCollection();

        /// <summary>
        /// Schriftart der Engine
        /// </summary>
        public static string Font { get; internal set; } = null;
        

        internal static void InitializeFont(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "KWEngine2.Assets.Fonts." + filename;

            HelperFont.AddFontFromResource(Collection, assembly, resourceName);
            Font = "Anonymous";

            HelperFont.GenerateTextures();
        }

        internal static Dictionary<string, Renderer> Renderers { get; set; } = new Dictionary<string, Renderer>();
        internal static Dictionary<string, GeoModel> Models { get; set; } = new Dictionary<string, GeoModel>();

        /// <summary>
        /// Empfindlichkeit des Mauszeigers im First-Person-Modus (Standard: 0.001f)
        /// </summary>
        public static float MouseSensitivity { get; set; } = 0.001f;

        internal static GeoModel CoordinateSystem;
        internal static GeoModel KWRect;
        internal static RendererSimple RendererSimple;
        internal static RendererStandardPBR RendererPBR;
        internal static Matrix4 CoordinateSystemMatrix = Matrix4.CreateScale(10);

        internal static void DrawCoordinateSystem(ref Matrix4 viewProjection)
        {
            GL.UseProgram(RendererSimple.GetProgramId());
            GL.Disable(EnableCap.Blend);
            Matrix4.Mult(ref CoordinateSystemMatrix, ref viewProjection, out Matrix4 _modelViewProjection);
            GL.UniformMatrix4(RendererSimple.GetUniformHandleMVP(), false, ref _modelViewProjection);

            foreach (string meshName in CoordinateSystem.Meshes.Keys)
            {
                GeoMesh mesh = CoordinateSystem.Meshes[meshName];

                GL.Uniform3(RendererSimple.GetUniformBaseColor(), mesh.Material.ColorDiffuse.X, mesh.Material.ColorDiffuse.Y, mesh.Material.ColorDiffuse.Z);

                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElements(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                GL.BindVertexArray(0);
            }
            GL.UseProgram(0);
        }

        internal static void InitializeModels()
        {
            Models.Add("KWCube", SceneImporter.LoadModel("kwcube.obj", false, true));
            Models.Add("KWCube6", SceneImporter.LoadModel("kwcube6.obj", false, true));
            KWRect = SceneImporter.LoadModel("kwrect.obj", false, true);
            Models.Add("KWSphere", SceneImporter.LoadModel("kwsphere.obj", false, true));
            CoordinateSystem = SceneImporter.LoadModel("csystem.obj", false, true);

            for (int i = 0; i < Explosion.Axes.Length; i++)
            {
                Explosion.Axes[i] = Vector3.Normalize(Explosion.Axes[i]);
            }
        }

        internal static void InitializeShaders()
        {
            Renderers.Add("Standard", new RendererStandard());
            Renderers.Add("Shadow", new RendererShadow());
            Renderers.Add("Bloom", new RendererBloom());
            Renderers.Add("Explosion", new RendererExplosion());
            Renderers.Add("Background", new RendererBackground());
            Renderers.Add("Skybox", new RendererSkybox());
            Renderers.Add("Particle", new RendererParticle());
            Renderers.Add("Terrain", new RendererTerrain());
            Renderers.Add("HUD", new RendererHUD());

            RendererSimple = new RendererSimple();
            RendererPBR = new RendererStandardPBR();
        }

        internal static void InitializeParticles()
        {
            int tex;
            
            // Bursts:
            tex = HelperTexture.LoadTextureInternal("fire01.png");
            ParticleDictionary.Add(ParticleType.BurstFire1, new ParticleInfo(tex, 8, 64));

            tex = HelperTexture.LoadTextureInternal("fire02.png");
            ParticleDictionary.Add(ParticleType.BurstFire2, new ParticleInfo(tex, 7, 49));

            tex = HelperTexture.LoadTextureInternal("fire03.png");
            ParticleDictionary.Add(ParticleType.BurstFire3, new ParticleInfo(tex, 9, 81));

            tex = HelperTexture.LoadTextureInternal("fire04.png");
            ParticleDictionary.Add(ParticleType.BurstElectricity, new ParticleInfo(tex, 4, 16));

            tex = HelperTexture.LoadTextureInternal("particleburst_bubbles.png");
            ParticleDictionary.Add(ParticleType.BurstBubblesColored, new ParticleInfo(tex, 6, 36));

            tex = HelperTexture.LoadTextureInternal("particleburst_bubbles_unicolor.png");
            ParticleDictionary.Add(ParticleType.BurstBubblesMonochrome, new ParticleInfo(tex, 6, 36));

            tex = HelperTexture.LoadTextureInternal("particleburst_explosioncolored.png");
            ParticleDictionary.Add(ParticleType.BurstFirework1, new ParticleInfo(tex, 7, 49));

            tex = HelperTexture.LoadTextureInternal("particleburst_firework.png");
            ParticleDictionary.Add(ParticleType.BurstFirework2, new ParticleInfo(tex, 7, 49));

            tex = HelperTexture.LoadTextureInternal("particleburst_hearts.png");
            ParticleDictionary.Add(ParticleType.BurstHearts, new ParticleInfo(tex, 7, 49));

            tex = HelperTexture.LoadTextureInternal("particleburst_plusplusplus.png");
            ParticleDictionary.Add(ParticleType.BurstOneUps, new ParticleInfo(tex, 6, 36));

            tex = HelperTexture.LoadTextureInternal("particleburst_shield.png");
            ParticleDictionary.Add(ParticleType.BurstShield, new ParticleInfo(tex, 6, 36));

            tex = HelperTexture.LoadTextureInternal("particleburst_teleport1.png");
            ParticleDictionary.Add(ParticleType.BurstTeleport1, new ParticleInfo(tex, 4, 16));

            tex = HelperTexture.LoadTextureInternal("particleburst_teleport2.png");
            ParticleDictionary.Add(ParticleType.BurstTeleport2, new ParticleInfo(tex, 4, 16));

            tex = HelperTexture.LoadTextureInternal("particleburst_teleport3.png");
            ParticleDictionary.Add(ParticleType.BurstTeleport3, new ParticleInfo(tex, 4, 16));

            // Loops:

            tex = HelperTexture.LoadTextureInternal("smoke01.png");
            ParticleDictionary.Add(ParticleType.LoopSmoke1, new ParticleInfo(tex, 4, 16));

            tex = HelperTexture.LoadTextureInternal("smoke02.png");
            ParticleDictionary.Add(ParticleType.LoopSmoke2, new ParticleInfo(tex, 7, 46));

            tex = HelperTexture.LoadTextureInternal("smoke03.png");
            ParticleDictionary.Add(ParticleType.LoopSmoke3, new ParticleInfo(tex, 6, 32));
        }

        /// <summary>
        /// Aktuelle Systemzeit in Millisekunden
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentTimeInMilliseconds()
        {
            return Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Erfragt ein 3D-Modell aus der Engine-Datenbank
        /// </summary>
        /// <param name="name">Name des Modells</param>
        /// <returns>Modell</returns>
        public static GeoModel GetModel(string name)
        {
            bool modelFound = Models.TryGetValue(name, out GeoModel m);
            if (!modelFound)
                throw new Exception("Model " + name + " not found.");
            return m;
        }

        private static float _shadowmapbiascoefficient = 0.001f;

        /// <summary>
        /// Koeffizient der Schattenberechnung (Standard: 0.001f);
        /// </summary>
        public static float ShadowMapCoefficient
        {
            get
            {
                return _shadowmapbiascoefficient;
            }
            set
            {
                if(value > 1 || value < -1)
                {
                    Debug.WriteLine("Shadow map coefficient may range from -1 to +1. Reset to 0.001!");
                    _shadowmapbiascoefficient = 0.001f;
                }
                else
                {
                    _shadowmapbiascoefficient = value;
                }

            }
        }

        private static int _shadowMapSize = 4096;
        /// <summary>
        /// Größe der Shadow Map (Standard: 4096)
        /// </summary>
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
                GLWindow.CurrentWindow.InitializeFramebuffers();

            }
        }

        /// <summary>
        /// Aktuelle Welt
        /// </summary>
        public static World CurrentWorld 
        {
            get
            {
                return GLWindow.CurrentWindow.CurrentWorld;
            }
        }

        /// <summary>
        /// Fenster
        /// </summary>
        public static GLWindow CurrentWindow
        {
            get
            {
                return GLWindow.CurrentWindow;
            }
        }

        /// <summary>
        /// Baut ein Terrain-Modell
        /// </summary>
        /// <param name="name">Name des Modells</param>
        /// <param name="heightmap">Height Map Textur</param>
        /// <param name="texture">Textur der Oberfläche</param>
        /// <param name="width">Breite</param>
        /// <param name="height">Höhe</param>
        /// <param name="depth">Tiefe</param>
        /// <param name="texRepeatX">Texturwiederholung Breite</param>
        /// <param name="texRepeatZ">Texturwiederholung Tiefe</param>
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
            texDiffuse.Filename = texture;
            texDiffuse.Type = GeoTexture.TexType.Diffuse;
            texDiffuse.UVMapIndex = 0;
            texDiffuse.UVTransform = new Vector2(texRepeatX, texRepeatZ);

            bool dictFound = CustomTextures.TryGetValue(CurrentWorld, out Dictionary<string, int> texDict);

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
            terrainModel.Meshes.Add("Terrain", terrainMesh);
            KWEngine.Models.Add(name, terrainModel);
        }

        /// <summary>
        /// Lädt ein Modell aus einer Datei
        /// </summary>
        /// <param name="name">Name des Modells</param>
        /// <param name="filename">Datei des Modells</param>
        public static void LoadModelFromFile(string name, string filename)
        {
            LoadModelFromFile(name, filename, true);
        }

        /// <summary>
        /// Lädt ein Modell aus einer Datei
        /// </summary>
        /// <param name="name">Name des Modells</param>
        /// <param name="filename">Datei des Modells</param>
        /// <param name="flipTextureCoordinates">UV-Map umdrehen (Standard: true)</param>
        public static void LoadModelFromFile(string name, string filename, bool flipTextureCoordinates = true)
        {
            GeoModel m = SceneImporter.LoadModel(filename, flipTextureCoordinates, false);
            name = name.Trim();
            m.Name = name;
            lock (KWEngine.Models)
            {
                //name = name.ToLower();
                if (!KWEngine.Models.ContainsKey(name))
                    KWEngine.Models.Add(name, m);
                else
                    throw new Exception("A model with the name " + name + " already exists.");
            }
        }
    }
}
