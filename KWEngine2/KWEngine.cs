﻿using System;
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
using System.Runtime.CompilerServices;

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
            /// Standard
            /// </summary>
            Standard,
            /// <summary>
            /// Niedrig
            /// </summary>
            Low,
            /// <summary>
            /// Ausgeschaltet
            /// </summary>
            Disabled
        };

        internal static int TextureDefault = -1;
        internal static int TextureBlack = -1;
        internal static int TextureAlpha = -1;
        internal static float TimeElapsed = 0;

        internal static float _broadPhaseToleranceWidth = 1f;

        /// <summary>
        /// Toleranzbereich für die Kollisionssuche (Standard: 1 Längeneinheit)
        /// Wenn nach Überschneidungen von Objekten gesucht wird, wird jede Hitbox
        /// automatisch um die angegebene Länge vergrößert, um die im aktuellen Frame
        /// passierenden Bewegungen mit zu berücksichtigen.
        /// </summary>
        public static float SweepAndPruneToleranceWidth
        {
            get { return _broadPhaseToleranceWidth; }
            set { _broadPhaseToleranceWidth = Math.Max(0f, Math.Abs(value)); }
        }


        /// <summary>
        /// Einheit zur Anzeige der Frame-Performance
        /// </summary>
        public enum PerformanceUnit
        {
            /// <summary>
            /// Deaktiviert
            /// </summary>
            Disabled,
            /// <summary>
            /// Millisekunden
            /// </summary>
            FrameTimeInMilliseconds,
            /// <summary>
            /// Bilder pro Sekunde
            /// </summary>
            FramesPerSecond
        }

        /// <summary>
        /// Zeigt die Performance im Titelbereich des Fensters an
        /// </summary>
        public static PerformanceUnit DebugShowPerformanceInTitle { get; set; } = PerformanceUnit.Disabled;

        /// <summary>
        /// Qualität der Post-Processing-Effekte (Glühen)
        /// </summary>
        public static PostProcessingQuality PostProcessQuality { get; set; } = PostProcessingQuality.Standard;

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

        /// <summary>
        /// Setzt die Schriftart der Engine (Schriftart sollte vom Typ 'monospace' sein)
        /// </summary>
        /// <param name="filename">Dateiname mit relativem Pfad</param>
        public static void SetFont(string filename)
        {
            Collection.Dispose();
            Collection = new PrivateFontCollection();
            HelperFont.AddFontFromFile(Collection, filename);
            Font = filename;

            HelperFont.GenerateTextures();
        }


        internal static Dictionary<string, Renderer> Renderers { get; set; } = new Dictionary<string, Renderer>();
        internal static Dictionary<string, GeoModel> Models { get; set; } = new Dictionary<string, GeoModel>();

        /// <summary>
        /// Empfindlichkeit des Mauszeigers im First-Person-Modus (Standard: 0.001f)
        /// </summary>
        public static float MouseSensitivity { get; set; } = 0.001f;

        internal static GeoModel CoordinateSystem;
        internal static GeoModel CoordinateSystemX;
        internal static GeoModel CoordinateSystemY;
        internal static GeoModel CoordinateSystemZ;
        internal static GeoModel GHitbox;
        internal static GeoModel KWRect;
        internal static GeoModel KWStar;
        internal static GeoModel KWHeart;
        internal static GeoModel KWSkull;
        internal static GeoModel KWDollar;
        internal static RendererSimple RendererSimple;
        internal static RendererStandardPBR RendererPBR;
        internal static float CSScale = 4.5f;
        internal static Matrix4 CoordinateSystemMatrix = Matrix4.CreateScale(CSScale);
        internal static Matrix4 CoordinateSystemMatrixX = Matrix4.CreateScale(CSScale);
        internal static Matrix4 CoordinateSystemMatrixY = Matrix4.CreateScale(CSScale);
        internal static Matrix4 CoordinateSystemMatrixZ = Matrix4.CreateScale(CSScale);
        internal static Vector3 CoordinateSystemXOffset = new Vector3(1, 0, 0);
        internal static Vector3 CoordinateSystemYOffset = new Vector3(0, 1, 0);
        internal static Vector3 CoordinateSystemZOffset = new Vector3(0, 0, 1);
        internal static Vector3 CoordinateSystemCenter = new Vector3(0, 0, 0);

        internal static void DrawCoordinateSystem(ref Matrix4 viewProjection)
        {
            if (CurrentWorld == null)
                return;

            CSScale = CurrentWorld.IsFirstPersonMode ? Vector3.Distance(CurrentWorld.GetFirstPersonObject().GetCenterPointForAllHitboxes(), CoordinateSystemCenter) : Vector3.Distance(CurrentWorld.GetCameraPosition(), CoordinateSystemCenter) / 6f;

            GL.UseProgram(RendererSimple.GetProgramId());
            Matrix4 _modelViewProjection;
            // Simple lines:
            CoordinateSystemMatrix = Matrix4.CreateScale(CSScale);
            Matrix4.Mult(ref CoordinateSystemMatrix, ref viewProjection, out _modelViewProjection);
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

            // X
            CoordinateSystemMatrixX = Matrix4.CreateScale(CSScale * 3f) * Matrix4.CreateFromQuaternion(HelperRotation.GetRotationForPoint(CoordinateSystemXOffset * CSScale, CurrentWorld.IsFirstPersonMode ? CurrentWorld.GetFirstPersonObject().GetCenterPointForAllHitboxes() : CurrentWorld.GetCameraPosition()));
            CoordinateSystemMatrixX.M41 = CoordinateSystemXOffset.X * CSScale * 1.05f;
            CoordinateSystemMatrixX.M42 = CoordinateSystemXOffset.Y;
            CoordinateSystemMatrixX.M43 = CoordinateSystemXOffset.Z;
            Matrix4.Mult(ref CoordinateSystemMatrixX, ref viewProjection, out _modelViewProjection);
            GL.UniformMatrix4(RendererSimple.GetUniformHandleMVP(), false, ref _modelViewProjection);
            foreach (string meshName in CoordinateSystemX.Meshes.Keys)
            {
                GeoMesh mesh = CoordinateSystemX.Meshes[meshName];

                GL.Uniform3(RendererSimple.GetUniformBaseColor(), mesh.Material.ColorDiffuse.X, mesh.Material.ColorDiffuse.Y, mesh.Material.ColorDiffuse.Z);

                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElements(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                GL.BindVertexArray(0);
            }

            // Y
            CoordinateSystemMatrixY = Matrix4.CreateScale(CSScale * 3f) * Matrix4.CreateFromQuaternion(HelperRotation.GetRotationForPoint(CoordinateSystemYOffset * CSScale, CurrentWorld.IsFirstPersonMode ? CurrentWorld.GetFirstPersonObject().GetCenterPointForAllHitboxes() : CurrentWorld.GetCameraPosition()));
            CoordinateSystemMatrixY.M41 = CoordinateSystemYOffset.X;
            CoordinateSystemMatrixY.M42 = CoordinateSystemYOffset.Y * CSScale * 1.05f;
            CoordinateSystemMatrixY.M43 = CoordinateSystemYOffset.Z;
            Matrix4.Mult(ref CoordinateSystemMatrixY, ref viewProjection, out _modelViewProjection);
            GL.UniformMatrix4(RendererSimple.GetUniformHandleMVP(), false, ref _modelViewProjection);
            foreach (string meshName in CoordinateSystemY.Meshes.Keys)
            {
                GeoMesh mesh = CoordinateSystemY.Meshes[meshName];

                GL.Uniform3(RendererSimple.GetUniformBaseColor(), mesh.Material.ColorDiffuse.X, mesh.Material.ColorDiffuse.Y, mesh.Material.ColorDiffuse.Z);

                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElements(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                GL.BindVertexArray(0);
            }

            // Z
            CoordinateSystemMatrixZ = Matrix4.CreateScale(CSScale * 3f) * Matrix4.CreateFromQuaternion(HelperRotation.GetRotationForPoint(CoordinateSystemZOffset * CSScale, CurrentWorld.IsFirstPersonMode ? CurrentWorld.GetFirstPersonObject().GetCenterPointForAllHitboxes() : CurrentWorld.GetCameraPosition()));
            CoordinateSystemMatrixZ.M41 = CoordinateSystemZOffset.X;
            CoordinateSystemMatrixZ.M42 = CoordinateSystemZOffset.Y;
            CoordinateSystemMatrixZ.M43 = CoordinateSystemZOffset.Z * CSScale * 1.05f;
            Matrix4.Mult(ref CoordinateSystemMatrixZ, ref viewProjection, out _modelViewProjection);
            GL.UniformMatrix4(RendererSimple.GetUniformHandleMVP(), false, ref _modelViewProjection);
            foreach (string meshName in CoordinateSystemZ.Meshes.Keys)
            {
                GeoMesh mesh = CoordinateSystemZ.Meshes[meshName];

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
            Models.Add("KWCube", SceneImporter.LoadModel("kwcube.obj", false, SceneImporter.AssemblyMode.Internal));
            Models.Add("KWCube6", SceneImporter.LoadModel("kwcube6.obj", false, SceneImporter.AssemblyMode.Internal));
            KWRect = SceneImporter.LoadModel("kwrect.obj", false, SceneImporter.AssemblyMode.Internal);
            Models.Add("KWSphere", SceneImporter.LoadModel("kwsphere.obj", false, SceneImporter.AssemblyMode.Internal));
            KWStar = SceneImporter.LoadModel("star.obj", false, SceneImporter.AssemblyMode.Internal);
            Models.Add("KWStar", KWStar);
            KWHeart = SceneImporter.LoadModel("heart.obj", false, SceneImporter.AssemblyMode.Internal);
            Models.Add("KWHeart", KWHeart);
            KWSkull = SceneImporter.LoadModel("skull.obj", false, SceneImporter.AssemblyMode.Internal);
            Models.Add("KWSkull", KWSkull);
            KWDollar = SceneImporter.LoadModel("dollar.obj", false, SceneImporter.AssemblyMode.Internal);
            Models.Add("KWDollar", KWDollar);
            CoordinateSystem = SceneImporter.LoadModel("csystem.obj", false, SceneImporter.AssemblyMode.Internal);
            CoordinateSystemX = SceneImporter.LoadModel("csystemX.obj", false, SceneImporter.AssemblyMode.Internal);
            CoordinateSystemY = SceneImporter.LoadModel("csystemY.obj", false, SceneImporter.AssemblyMode.Internal);
            CoordinateSystemZ = SceneImporter.LoadModel("csystemZ.obj", false, SceneImporter.AssemblyMode.Internal);
            GHitbox = SceneImporter.LoadModel("Hitbox.obj", false, SceneImporter.AssemblyMode.Internal);

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
            Renderers.Add("Merge", new RendererMerge());

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
            return DeltaTime.Watch.ElapsedMilliseconds;
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
        /// Koeffizient der Schattenberechnung (Standard: 0.001f)
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
                if(value >= 512 && value <= 8192)
                {
                    _shadowMapSize = HelperTexture.RoundToPowerOf2(value);
                }
                else
                {
                    Debug.WriteLine("Cannot set shadow map to a size < 512 or > 8192. Resetting it to 4096.");
                    _shadowMapSize = 4096;
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
        /// <param name="isFile">false, wenn die Texturen Teil der EXE sind (Eingebettete Ressource)</param>
        public static void BuildTerrainModel(string name, string heightmap, string texture, float width, float height, float depth, float texRepeatX = 1, float texRepeatZ = 1, bool isFile = true)
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
            GeoMesh terrainMesh = t.BuildTerrain(new Vector3(0, 0, 0), heightmap, width, height, depth, texRepeatX, texRepeatZ, isFile);
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
                texDiffuse.OpenGLID = isFile ? HelperTexture.LoadTextureForModelExternal(texture) : HelperTexture.LoadTextureForModelInternal(texture);
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
        /// Lädt ein Modell aus den eingebetteten Ressourcen
        /// </summary>
        /// <param name="name">Name des Modells</param>
        /// <param name="path">Pfad zum Modell inkl. Dateiname</param>
        /// <param name="flipTextureCoordinates">UV-Map umdrehen (Standard: true)</param>
        /// <param name="callerName"></param>
        public static void LoadModelFromAssembly(string name, string path, bool flipTextureCoordinates = true, [CallerMemberName] string callerName = "")
        {
            if(callerName != "Prepare")
            {
                throw new Exception("Models may only be loaded in the world's Prepare() method.");
            }

            if (KWEngine.Models.ContainsKey(name.Trim()))
            {
                throw new Exception("A model with the name " + name + " already exists.");
            }
            
            GeoModel m = SceneImporter.LoadModel(path, flipTextureCoordinates, SceneImporter.AssemblyMode.User);
            name = name.Trim();
            m.Name = name;
            lock (KWEngine.Models)
            {
                KWEngine.Models.Add(name, m);
            }
        }

        /// <summary>
        /// Lädt ein Modell aus einer Datei
        /// </summary>
        /// <param name="name">Name des Modells</param>
        /// <param name="filename">Datei des Modells</param>
        /// <param name="callerName"></param>
        public static void LoadModelFromFile(string name, string filename, [CallerMemberName] string callerName = "")
        {
            if (callerName != "Prepare")
            {
                throw new Exception("Models may only be loaded in the world's Prepare() method.");
            }

            if (KWEngine.Models.ContainsKey(name.Trim()))
            {
                throw new Exception("A model with the name " + name + " already exists.");
            }
            GeoModel m = SceneImporter.LoadModel(filename, true, SceneImporter.AssemblyMode.File);
            name = name.Trim();
            m.Name = name;
            lock (KWEngine.Models)
            {
                KWEngine.Models.Add(name, m);
            }
        }

    }
}
