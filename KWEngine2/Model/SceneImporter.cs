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
            importer.SetConfig(new VertexBoneWeightLimitConfig(EngineState.MAX_BONE_WEIGHTS));
            importer.SetConfig(new MaxBoneCountConfig(EngineState.MAX_BONES));

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
            GeoModel model = ProcessScene(scene, filename.ToLower().Trim());
            return model;
        }

        private static GeoModel ProcessScene(Scene scene, string filename)
        {
            GeoModel returnModel = new GeoModel();
            returnModel.Filename = filename;
            returnModel.Bones = new Dictionary<string, GeoBone>();
            returnModel.Meshes = new Dictionary<string, GeoMesh>();
            returnModel.TransformGlobalInverse = Matrix4.Identity;
            returnModel.Textures = new Dictionary<string, int>();
            returnModel.IsValid = false;

            ProcessMeshes(scene, ref returnModel);
            //ProcessNode(scene.RootNode, ref returnModel, 0);
            ProcessAnimations(scene);

            return returnModel;
        }

        private static bool FindTransformForMesh(Scene scene, Node currentNode, Mesh mesh, out Matrix4 transform)
        {

            for(int i = 0; i < currentNode.MeshIndices.Count; i++)
            {
                Mesh tmpMesh = scene.Meshes[currentNode.MeshIndices[i]];
                if (tmpMesh.Name == mesh.Name)
                {
                    transform = HelperMatrix.ConvertAssimpToOpenTKMatrix(currentNode.Transform);
                    return true;
                }
            }

            for(int i = 0; i < currentNode.ChildCount; i++)
            {
                Node child = currentNode.Children[i];
                bool found = FindTransformForMesh(scene, child, mesh, out Matrix4 t);
                if (found)
                {
                    transform = t;
                    return true;
                }
            }

            transform = Matrix4.Identity;
            return false;
        }

        private static void ProcessMaterialsForMesh(Scene scene, Mesh mesh, ref GeoModel model, ref GeoMesh geoMesh)
        {
            GeoMaterial geoMaterial = new GeoMaterial();
            Material material = null;
            if (mesh.MaterialIndex >= 0)
            {
                material = scene.Materials[mesh.MaterialIndex];
                geoMaterial.BlendMode = material.BlendMode == BlendMode.Default ? OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha : OpenTK.Graphics.OpenGL4.BlendingFactor.One; // TODO: Check if this is correct!
                geoMaterial.ColorDiffuse = new Vector4(material.ColorDiffuse.R, material.ColorDiffuse.G, material.ColorDiffuse.B, material.ColorDiffuse.A);
                geoMaterial.ColorEmissive = new Vector4(material.ColorEmissive.R, material.ColorEmissive.G, material.ColorEmissive.B, material.ColorEmissive.A);
            }
            else
            {
                geoMaterial.BlendMode = OpenTK.Graphics.OpenGL4.BlendingFactor.OneMinusSrcAlpha;
                geoMaterial.ColorDiffuse = new Vector4(1, 1, 1, 1);
                geoMaterial.ColorEmissive = new Vector4(0, 0, 0, 1);
            }

            if(material != null)
            {
                //TODO: Process Textures...
            }
           
            geoMesh.Material = geoMaterial;
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

                bool transformFound = FindTransformForMesh(scene, scene.RootNode, mesh, out Matrix4 nodeTransform);
                geoMesh.Transform = nodeTransform;
                geoMesh.Name = mesh.Name;
                geoMesh.Indices = mesh.GetIndices();
                geoMesh.Vertices = new GeoVertex[mesh.VertexCount];
                geoMesh.Primitive = OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles;

                ProcessMaterialsForMesh(scene, mesh, ref model, ref geoMesh);

                for(int i = 0; i < mesh.VertexCount; i++)
                {
                    Vector3D vertex = mesh.Vertices[i];
                    GeoVertex geoVertex = new GeoVertex(i, vertex.X, vertex.Y, vertex.Z);
                    geoMesh.Vertices[i] = geoVertex;
                }


                foreach(Bone bone in mesh.Bones)
                {
                    if (!model.Bones.ContainsKey(bone.Name))
                    {
                        GeoBone geoBone = new GeoBone();
                        geoBone.Name = bone.Name;
                        geoBone.Offset = HelperMatrix.ConvertAssimpToOpenTKMatrix(bone.OffsetMatrix);

                        foreach(VertexWeight vw in bone.VertexWeights)
                        {
                            int weightIndexToBeSet = geoMesh.Vertices[vw.VertexID].WeightSet;
                            if(weightIndexToBeSet > EngineState.MAX_BONE_WEIGHTS - 1)
                            {
                                throw new Exception("Model's bones have more than three weights per vertex. Cannot import model.");
                            }
                            geoMesh.Vertices[vw.VertexID].Weights[weightIndexToBeSet] = vw.Weight;
                            geoMesh.Vertices[vw.VertexID].WeightSet++;
                            
                        }
                        model.Bones.Add(bone.Name, geoBone);
                    }
                }
            }
        }

        private static void ProcessAnimations(Scene scene)
        {
            
        }

        private static void ProcessNode(Node node, ref GeoModel model, int level = 0)
        {
            if(node.Parent == null)
            {
                HelperMatrix.ConvertAssimpToOpenTKMatrix(node.Transform, out Matrix4 globalTransform);
                globalTransform.Invert();
                model.TransformGlobalInverse = globalTransform;
            }

            

            foreach(Node child in node.Children)
            {

            }
        }

    }
}
