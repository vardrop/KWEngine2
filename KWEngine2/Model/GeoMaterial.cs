using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine2.Model
{
    public class GeoMaterial
    {
        public string Name { get; internal set; }
        public BlendingFactor BlendMode { get; internal set; }
        public Vector4 ColorEmissive { get; internal set; }
        public Vector4 ColorDiffuse { get; internal set; }

        public GeoTexture TextureDiffuse { get; internal set; }
    }
}
