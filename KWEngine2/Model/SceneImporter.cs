using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Assimp;

namespace KWEngine2.Model
{
    public static class SceneImporter
    {
        private const int MAX_BONES = 36;
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
            importer.SetConfig(new Assimp.Configs.VertexBoneWeightLimitConfig(3));
            importer.SetConfig(new Assimp.Configs.MaxBoneCountConfig(MAX_BONES));

            Scene scene = null;
            if (isInAssembly)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "KWEngine2.Assets.Models." + filename;

                using (Stream s = assembly.GetManifestResourceStream(resourceName))
                {
                    PostProcessSteps steps = 
                          PostProcessSteps.ValidateDataStructure
                        | PostProcessSteps.GenerateUVCoords
                        | PostProcessSteps.CalculateTangentSpace;
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
                            | PostProcessSteps.CalculateTangentSpace;

                    if (type == FileType.DirectX)
                        steps |= PostProcessSteps.FlipWindingOrder;

                    scene = importer.ImportFile(filename, steps);
                }
            }

            return ProcessScene(scene, filename.ToLower().Trim());
        }

        private static GeoModel ProcessScene(Scene scene, string filename)
        {
            GeoModel returnModel = new GeoModel();
            returnModel.Filename = filename;

            ProcessNode(scene.RootNode);


            return returnModel;
        }

        private static void ProcessNode(Node node)
        {

        }

    }
}
