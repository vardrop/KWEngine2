using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    public struct GeoMesh
    {
        public string Name { get; internal set; }
        public Matrix4 Transform { get; internal set; }
        public GeoVertex[] Vertices { get; internal set; }
        public PrimitiveType Primitive;
        public int IndexCount 
        { 
            get
            {
                return Indices.Length;
            }
        }
        public int[] Indices { get; internal set; }
        public GeoMaterial Material { get; internal set; }
    }
}
