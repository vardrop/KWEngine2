using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Assimp;
using Assimp.Configs;
using KWEngine2.Engine;
using KWEngine2.Helper;
using OpenTK;

namespace KWEngine2.Model
{
    internal static class SceneImporter
    {
        private enum FileType { DirectX, Filmbox, Wavefront, GLTF, Collada, Blender, Invalid }
        private static FileType CheckFileEnding(string filename)
        {
            string ending = filename.Trim().ToLower().Substring(filename.LastIndexOf('.') + 1);
            switch (ending)
            {
                case "x":
                    return FileType.DirectX;
                case "dae":
                    return FileType.Collada;
                case "glb":
                    return FileType.GLTF;
                case "gltf":
                    return FileType.GLTF;
                case "obj":
                    return FileType.Wavefront;
                case "fbx":
                    return FileType.Filmbox;
                case "blend":
                    return FileType.Blender;
                default:
                    return FileType.Invalid;
            }
        }

        internal static GeoModel LoadModel(string filename, bool isInAssembly = false)
        {
            AssimpContext importer = new AssimpContext();
            importer.SetConfig(new VertexBoneWeightLimitConfig(KWEngine.MAX_BONE_WEIGHTS));
            importer.SetConfig(new MaxBoneCountConfig(KWEngine.MAX_BONES));

            Scene scene = null;
            if (isInAssembly)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "KWEngine2.Assets.Models." + filename;

                using (Stream s = assembly.GetManifestResourceStream(resourceName))
                {
                    PostProcessSteps steps =
                          PostProcessSteps.LimitBoneWeights
                        | PostProcessSteps.Triangulate
                        | PostProcessSteps.ValidateDataStructure
                        | PostProcessSteps.GenerateUVCoords
                        | PostProcessSteps.CalculateTangentSpace
                        | PostProcessSteps.JoinIdenticalVertices;
                    scene = importer.ImportFileFromStream(s, steps);
                }
            }
            else
            {
                FileType type = CheckFileEnding(filename);
                if(type != FileType.Invalid)
                {
                    PostProcessSteps steps =
                              PostProcessSteps.LimitBoneWeights
                            | PostProcessSteps.Triangulate
                            | PostProcessSteps.FixInFacingNormals
                            | PostProcessSteps.ValidateDataStructure
                            | PostProcessSteps.GenerateUVCoords
                            | PostProcessSteps.CalculateTangentSpace
                            | PostProcessSteps.JoinIdenticalVertices;

                    if (type == FileType.DirectX)
                        steps |= PostProcessSteps.FlipWindingOrder;

                    scene = importer.ImportFile(filename, steps);
                }
            }
            GeoModel model = ProcessScene(scene, filename.ToLower().Trim(), isInAssembly);
            return model;
        }

        private static GeoModel ProcessScene(Scene scene, string filename, bool isInAssembly)
        {
            GeoModel returnModel = new GeoModel();
            
            returnModel.Filename = filename;
            if (isInAssembly)
            {
                returnModel.PathAbsolute = "";
            }
            else
            {
                
                string p = Assembly.GetExecutingAssembly().Location;
                string pA = new DirectoryInfo(StripFileNameFromPath(p)).FullName;
                if (!Path.IsPathRooted(filename))
                {
                    returnModel.PathAbsolute = Path.Combine(pA, filename);
                }
                else
                {
                    returnModel.PathAbsolute = filename;
                }
                
                bool success = File.Exists(returnModel.PathAbsolute);
            }
            

            returnModel.IsInAssembly = isInAssembly;
            returnModel.CalculatePath();
            returnModel.Bones = new Dictionary<int, GeoBone>();
            returnModel.Meshes = new Dictionary<string, GeoMesh>();
            returnModel.TransformGlobalInverse = Matrix4.Invert(HelperMatrix.ConvertAssimpToOpenTKMatrix(scene.RootNode.Transform));
            returnModel.Textures = new Dictionary<string, GeoTexture>();
            returnModel.IsValid = false;

            ProcessBones(scene, ref returnModel);
            ProcessMeshes(scene, ref returnModel);
            ProcessAnimations(scene, ref returnModel);

            returnModel.IsValid = true;
            GC.Collect(GC.MaxGeneration);
            return returnModel;
        }

        private static bool IsBoneAlreadyStored(string boneName, ref GeoModel model)
        {
            foreach(GeoBone bone in model.Bones.Values)
            {
                if(bone.Name == boneName)
                {
                    return true;
                }
            }
            return false;
        }

        private static Node FindArmature(Node node, ref GeoModel model, int level = 0)
        {
            if (level > 0 && !node.HasMeshes) // this is not the root node
            {
                bool found = FindBoneForNode(node, ref model, out GeoBone bone);
                if (!found)
                {
                    // this must be the armature:
                    if (node.Parent != null && node.Parent.Parent == null && node.ChildCount > 0)
                    {
                        return node;
                    }
                }
            }
            
            foreach(Node child in node.Children)
            {
                Node n = FindArmature(child, ref model, level + 1);
                return n;
            }
            return null;
        }

        private static void GenerateBoneHierarchy(Node node, ref GeoModel model, int level)
        {
            Node armature = FindArmature(node, ref model);
            if(armature != null)
            {
                GeoBone bone = new GeoBone();
                bone.Index = model.LastBoneIndex;
                bone.Transform = HelperMatrix.ConvertAssimpToOpenTKMatrix(armature.Transform);
                bone.Parent = null;
                bone.Name = "Armature";
                bone.Offset = Matrix4.Identity;
                model.Bones.Add(model.LastBoneIndex, bone);

                MapNodeToBone(armature, ref model);
            }
            else
            {
                if (model.Bones.Count > 0)
                    throw new Exception("Cannot find armature bone for model " + model.Name + ".");
            }
        }

        private static void MapNodeToBone(Node n, ref GeoModel model)
        {
            GeoBone nBone = null;
            GeoBone parentBone = null;
            bool nFound = FindBoneForNode(n, ref model, out nBone);
            bool parentFound = FindBoneForNode(n.Parent, ref model, out parentBone);

            if (nFound)
            {
                if (parentFound)
                {
                    nBone.Parent = parentBone;
                }

                foreach (Node child in n.Children)
                {
                    bool found = FindBoneForNode(child, ref model, out GeoBone foundBone);
                    if (found)
                    {
                        nBone.Children.Add(foundBone);
                        MapNodeToBone(child, ref model);
                    }
                }
                 
                foreach(Node child in n.Children)
                {
                    
                }
            }
        }

        private static bool FindBoneForNode(Node node, ref GeoModel model, out GeoBone bone)
        {
            bool found = false;
            foreach (GeoBone b in model.Bones.Values)
            {
                if (b.Name == node.Name) // This node is a bone!
                {
                    bone = b;
                    return true;
                }
            }
            bone = new GeoBone();
            return found;
        }

        private static bool FindBoneForNode(string nodeName, ref GeoModel model, out GeoBone bone)
        {
            bool found = false;
            foreach (GeoBone b in model.Bones.Values)
            {
                if (b.Name == nodeName) // This node is a bone!
                {
                    bone = b;
                    return true;
                }
            }
            bone = new GeoBone();
            return found;
        }

        private static void ProcessBones(Scene scene, ref GeoModel model)
        {
            
            foreach(Mesh mesh in scene.Meshes)
            {
                foreach (Bone bone in mesh.Bones)
                {
                    if (!IsBoneAlreadyStored(bone.Name, ref model))
                    {
                        GeoBone geoBone = new GeoBone();
                        geoBone.Name = bone.Name;
                        geoBone.Index = model.LastBoneIndex;
                        geoBone.Offset = HelperMatrix.ConvertAssimpToOpenTKMatrix(bone.OffsetMatrix);
                        model.Bones.Add(geoBone.Index, geoBone);
                        model.LastBoneIndex++;
                    }
                }
            }

            // Generate GeoBone hierarchy tree:
            GenerateBoneHierarchy(scene.RootNode, ref model, 0);
        }

        private static bool FindTransformForMesh(Scene scene, Node currentNode, Mesh mesh, out Matrix4 transform, out string nodeName)
        {
            for(int i = 0; i < currentNode.MeshIndices.Count; i++)
            {
                Mesh tmpMesh = scene.Meshes[currentNode.MeshIndices[i]];
                if (tmpMesh.Name == mesh.Name)
                {
                    transform = HelperMatrix.ConvertAssimpToOpenTKMatrix(currentNode.Transform);
                    nodeName = currentNode.Name;
                    return true;
                }
            }

            for(int i = 0; i < currentNode.ChildCount; i++)
            {
                Node child = currentNode.Children[i];
                bool found = FindTransformForMesh(scene, child, mesh, out Matrix4 t, out string nName);
                if (found)
                {
                    transform = t;
                    nodeName = nName;
                    return true;
                }
            }

            transform = Matrix4.Identity;
            nodeName = null;
            return false;
        }       

        private static string StripFileNameFromPath(string path)
        {
            int index = path.LastIndexOf('\\');
            if(index < 0)
            {
                return path;
            }
            else
            {
                return path.Substring(0, index + 1).ToLower();
            }
            
        }

        private static string StripPathFromFile(string fileWithPath)
        {
            int index = fileWithPath.LastIndexOf('\\');
            if (index < 0)
            {
                return fileWithPath;
            }
            else
            {
                return fileWithPath.Substring(index + 1).ToLower();
            }
        }

        private static string FindTextureInSubs(string filename, string path = null)
        {
            DirectoryInfo currentDir;
            if (path == null)
            {
                string p = Assembly.GetExecutingAssembly().Location;
                currentDir = new DirectoryInfo(StripFileNameFromPath(p));
            }
            else
            {
                currentDir = new DirectoryInfo(StripFileNameFromPath(path));
            }
            
            foreach(FileInfo fi in currentDir.GetFiles())
            {
                if (fi.Name.ToLower() == StripPathFromFile(filename))
                {
                    // file found:
                    return fi.FullName;
                }
            }
            
            if(currentDir.GetDirectories().Length == 0)
            {
                throw new Exception("File " + filename + " not found anywhere. Aborting import.");
            }
            else
            {
                foreach(DirectoryInfo di in currentDir.GetDirectories())
                {
                    return FindTextureInSubs(filename, di.FullName);
                }
            }

            return "";
        }

        private static void ProcessMaterialsForMesh(Scene scene, Mesh mesh, ref GeoModel model, ref GeoMesh geoMesh)
        {
            GeoMaterial geoMaterial = new GeoMaterial();
            Material material = null;
            if (mesh.MaterialIndex >= 0)
            {
                material = scene.Materials[mesh.MaterialIndex];
                geoMaterial.BlendMode = material.BlendMode == BlendMode.Default ? OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha : OpenTK.Graphics.OpenGL4.BlendingFactor.One; // TODO: Check if this is correct!
                geoMaterial.ColorDiffuse = material.HasColorDiffuse ? new Vector4(material.ColorDiffuse.R, material.ColorDiffuse.G, material.ColorDiffuse.B, material.ColorDiffuse.A) : new Vector4(1,1,1,1);
                geoMaterial.ColorEmissive = material.HasColorEmissive ? new Vector4(material.ColorEmissive.R, material.ColorEmissive.G, material.ColorEmissive.B, material.ColorEmissive.A) : new Vector4(0,0,0,1);
            }
            else
            {
                geoMaterial.BlendMode = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha;
                geoMaterial.ColorDiffuse = new Vector4(1, 1, 1, 1);
                geoMaterial.ColorEmissive = new Vector4(0, 0, 0, 1);
            }

            // Process Textures:
            if(material != null)
            {
                // Diffuse texture
                if (material.HasTextureDiffuse)
                {
                    GeoTexture tex = new GeoTexture();
                    tex.Filename = material.TextureDiffuse.FilePath;
                    tex.UVMapIndex = material.TextureDiffuse.UVIndex;
                    if (model.Textures.ContainsKey(tex.Filename))
                    {
                        tex.OpenGLID = model.Textures[tex.Filename].OpenGLID;
                    }
                    else
                    {
                        tex.OpenGLID = HelperTexture.LoadTextureForModelExternal(
                                FindTextureInSubs(StripPathFromFile(tex.Filename), model.PathAbsolute)
                            );
                        model.Textures.Add(tex.Filename, tex);
                    }
                    tex.Type = GeoTexture.TexType.Diffuse;
                    geoMaterial.TextureDiffuse = tex;
                }

                // Normal map texture
                if (material.HasTextureNormal)
                {
                    GeoTexture tex = new GeoTexture();
                    tex.Filename = material.TextureNormal.FilePath;
                    tex.UVMapIndex = material.TextureNormal.UVIndex;
                    if (model.Textures.ContainsKey(tex.Filename))
                    {
                        tex.OpenGLID = model.Textures[tex.Filename].OpenGLID;
                    }
                    else
                    {
                        tex.OpenGLID = HelperTexture.LoadTextureForModelExternal(
                                FindTextureInSubs(StripPathFromFile(tex.Filename), model.PathAbsolute)
                            );
                        model.Textures.Add(tex.Filename, tex);
                    }
                    tex.Type = GeoTexture.TexType.Normal;
                }

                // Specular map texture
                if (material.HasTextureSpecular)
                {
                    GeoTexture tex = new GeoTexture();
                    tex.Filename = material.TextureSpecular.FilePath;
                    tex.UVMapIndex = material.TextureSpecular.UVIndex;
                    if (model.Textures.ContainsKey(tex.Filename))
                    {
                        tex.OpenGLID = model.Textures[tex.Filename].OpenGLID;
                    }
                    else
                    {
                        tex.OpenGLID = HelperTexture.LoadTextureForModelExternal(
                                FindTextureInSubs(StripPathFromFile(tex.Filename), model.PathAbsolute)
                            );
                        model.Textures.Add(tex.Filename, tex);
                    }
                    tex.Type = GeoTexture.TexType.Specular;
                }

                // Emissive map texture
                if (material.HasTextureEmissive)
                {
                    GeoTexture tex = new GeoTexture();
                    tex.Filename = material.TextureEmissive.FilePath;
                    tex.UVMapIndex = material.TextureEmissive.UVIndex;
                    if (model.Textures.ContainsKey(tex.Filename))
                    {
                        tex.OpenGLID = model.Textures[tex.Filename].OpenGLID;
                    }
                    else
                    {
                        tex.OpenGLID = HelperTexture.LoadTextureForModelExternal(
                                FindTextureInSubs(StripPathFromFile(tex.Filename), model.PathAbsolute)
                            );
                        model.Textures.Add(tex.Filename, tex);
                    }
                    tex.Type = GeoTexture.TexType.Emissive;
                }

                // Light map texture
                if (material.HasTextureLightMap)
                {
                    GeoTexture tex = new GeoTexture();
                    tex.Filename = material.TextureLightMap.FilePath;
                    tex.UVMapIndex = material.TextureLightMap.UVIndex;
                    if (model.Textures.ContainsKey(tex.Filename))
                    {
                        tex.OpenGLID = model.Textures[tex.Filename].OpenGLID;
                    }
                    else
                    {
                        tex.OpenGLID = HelperTexture.LoadTextureForModelExternal(
                                FindTextureInSubs(StripPathFromFile(tex.Filename), model.PathAbsolute)
                            );
                        model.Textures.Add(tex.Filename, tex);
                    }
                    tex.Type = GeoTexture.TexType.Light;
                }

            }
           
            geoMesh.Material = geoMaterial;
        }

        private static int FindBoneIndexForBone(string boneName, ref GeoModel model)
        {
            foreach(GeoBone bone in model.Bones.Values)
            {
                if (bone.Name == boneName)
                    return bone.Index;
            }
            throw new Exception("Fatal error while loading animations: Bone index for Bone " + boneName + " not found.");
        }

        private static void ProcessMeshes(Scene scene, ref GeoModel model)
        {
            for(int m = 0; m < scene.MeshCount; m++)
            {
                Mesh mesh = scene.Meshes[m];
                if (mesh.PrimitiveType != PrimitiveType.Triangle)
                {
                    throw new Exception("Model's primitive type is not set to 'triangles'. Cannot import model.");
                }

                GeoMesh geoMesh = new GeoMesh();

                bool transformFound = FindTransformForMesh(scene, scene.RootNode, mesh, out Matrix4 nodeTransform, out string nodeName);
                geoMesh.Transform = nodeTransform;
                geoMesh.Name = mesh.Name + " #" + m.ToString().PadLeft(4,'0') + " (Node: " + nodeName + ")";
                geoMesh.Vertices = new GeoVertex[mesh.VertexCount];
                geoMesh.Primitive = OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles;
                geoMesh.VAOGenerateAndBind();

                for(int i = 0; i < mesh.VertexCount; i++)
                {
                    //TODO: Find max/min for x/y/z (for hitboxing)
                    Vector3D vertex = mesh.Vertices[i];
                    GeoVertex geoVertex = new GeoVertex(i, vertex.X, vertex.Y, vertex.Z);
                    geoMesh.Vertices[i] = geoVertex;
                }
                geoMesh.Indices = mesh.GetIndices();//GenerateIndices(geoMesh.Vertices, mesh);

                if (model.HasBones)
                {
                    for (int i = 0; i < mesh.BoneCount; i++)
                    {
                        Bone bone = mesh.Bones[i];
                        foreach (VertexWeight vw in bone.VertexWeights)
                        {
                            int boneIndex = FindBoneIndexForBone(bone.Name, ref model);
                            int weightIndexToBeSet = geoMesh.Vertices[vw.VertexID].WeightSet;
                            if (weightIndexToBeSet > KWEngine.MAX_BONE_WEIGHTS - 1)
                            {
                                throw new Exception("Model's bones have more than three weights per vertex. Cannot import model.");
                            }
                            geoMesh.Vertices[vw.VertexID].Weights[weightIndexToBeSet] = vw.Weight;
                            geoMesh.Vertices[vw.VertexID].BoneIDs[weightIndexToBeSet] = boneIndex;
                            geoMesh.Vertices[vw.VertexID].WeightSet++;

                        }
                    }
                }

                geoMesh.VBOGenerateIndices();
                geoMesh.VBOGenerateVerticesAndBones(model.HasBones);
                geoMesh.VBOGenerateNormals(mesh);
                geoMesh.VBOGenerateTangents(mesh);
                geoMesh.VBOGenerateTextureCoords1(mesh);
                geoMesh.VBOGenerateTextureCoords2(mesh);

                ProcessMaterialsForMesh(scene, mesh, ref model, ref geoMesh);

                

                geoMesh.VAOUnbind();

                model.Meshes.Add(geoMesh.Name, geoMesh);
            }
        }

        private static void ProcessAnimations(Scene scene, ref GeoModel model)
        {
            
            if (scene.HasAnimations)
            {
                model.Animations = new List<GeoAnimation>();
                foreach(Animation a in scene.Animations)
                {
                    GeoAnimation ga = new GeoAnimation();
                    ga.DurationInTicks = (float)a.DurationInTicks;
                    ga.TicksPerSecond = (float)a.TicksPerSecond;
                    ga.Name = a.Name;
                    ga.AnimationChannels = new Dictionary<GeoBone, GeoNodeAnimationChannel>();
                    foreach(NodeAnimationChannel nac in a.NodeAnimationChannels)
                    {
                        GeoNodeAnimationChannel ganc = new GeoNodeAnimationChannel();

                        // Rotation:
                        ganc.RotationKeys = new List<GeoAnimationKeyframe>();
                        foreach (QuaternionKey key in nac.RotationKeys) {
                            GeoAnimationKeyframe akf = new GeoAnimationKeyframe();
                            akf.Time = (float)key.Time;
                            akf.Rotation = new OpenTK.Quaternion(key.Value.X, key.Value.Y, key.Value.Z, key.Value.W);
                            akf.Translation = new Vector3(0, 0, 0);
                            akf.Scale = new Vector3(1, 1, 1);
                            akf.Type = GeoKeyframeType.Rotation;
                            ganc.RotationKeys.Add(akf);
                        }

                        // Scale:
                        ganc.ScaleKeys = new List<GeoAnimationKeyframe>();
                        foreach (VectorKey key in nac.ScalingKeys)
                        {
                            GeoAnimationKeyframe akf = new GeoAnimationKeyframe();
                            akf.Time = (float)key.Time;
                            akf.Rotation = new OpenTK.Quaternion(0,0,0,1);
                            akf.Translation = new Vector3(0, 0, 0);
                            akf.Scale = new Vector3(key.Value.X, key.Value.Y, key.Value.Z);
                            akf.Type = GeoKeyframeType.Scale;
                            ganc.ScaleKeys.Add(akf);
                        }

                        // Translation:
                        ganc.TranslationKeys = new List<GeoAnimationKeyframe>();
                        foreach (VectorKey key in nac.PositionKeys)
                        {
                            GeoAnimationKeyframe akf = new GeoAnimationKeyframe();
                            akf.Time = (float)key.Time;
                            akf.Rotation = new OpenTK.Quaternion(0, 0, 0, 1);
                            akf.Translation = new Vector3(key.Value.X, key.Value.Y, key.Value.Z);
                            akf.Scale = new Vector3(1, 1, 1);
                            akf.Type = GeoKeyframeType.Translation;
                            ganc.TranslationKeys.Add(akf);
                        }

                        bool nodeFound = FindBoneForNode(nac.NodeName, ref model, out GeoBone bone);
                        if (nodeFound)
                        {
                            ga.AnimationChannels.Add(bone, ganc);
                        }
                    }

                    model.Animations.Add(ga);

                }
            }
        }
    }
}
