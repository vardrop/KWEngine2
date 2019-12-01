using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine2.Model
{
    public struct GeoModel
    {
        public bool IsValid { get; internal set; }
        public string Filename { get; internal set; }
        public Matrix4 TransformGlobalInverse { get; internal set; }
        public Dictionary<string, GeoMesh> Meshes { get; internal set; }
        public Dictionary<string, GeoBone> Bones { get; internal set; }
        public Dictionary<string, int> Textures { get; internal set; }

        internal void Dispose()
        {
            IsValid = false;

            lock (Textures)
            {
                foreach(int t in Textures.Values)
                {
                    GL.DeleteTexture(t);
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
    }
}
