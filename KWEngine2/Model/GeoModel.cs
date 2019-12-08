using OpenTK;
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using System.IO;

namespace KWEngine2.Model
{
    public class GeoModel
    {
        public GeoNode Root { get; internal set; } = null;
        public string Path { get; internal set; }
        public string PathAbsolute { get; internal set; }
        internal bool IsInAssembly { get; set; }
        public List<GeoAnimation> Animations { get; internal set; }
        public bool HasBones
        {
            get; internal set;

        } = false;
        
        public bool IsValid { get; internal set; }
        public string Name { get; internal set; }
        public string Filename { get; internal set; }
        public Matrix4 TransformGlobalInverse { get; internal set; }
        public Dictionary<string, GeoMesh> Meshes { get; internal set; }
        //public Dictionary<int, GeoBone> Bones { get; internal set; }
        public List<string> BoneNames { get; internal set; } = new List<string>();
        public Dictionary<string, GeoTexture> Textures { get; internal set; }

        internal void Dispose()
        {
            IsValid = false;

            lock (Textures)
            {
                foreach(GeoTexture t in Textures.Values)
                {
                    GL.DeleteTexture(t.OpenGLID);
                }
                Textures.Clear();
            }

            lock (Meshes)
            {
                foreach(GeoMesh m in Meshes.Values)
                {
                    m.Dispose();
                }
            }
        }

        internal void CalculatePath()
        {
            if (!IsInAssembly)
            {
                FileInfo fi = new FileInfo(Filename);
                if (fi.Exists)
                {
                    Path = fi.DirectoryName;
                }
                else
                {
                    throw new Exception("File " + Filename + " does not exist.");
                }
            }
        }
    }
}
