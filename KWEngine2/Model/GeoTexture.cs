using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    public struct GeoTexture
    {
        public enum TexType { Diffuse, Specular, Normal, Skybox, Emissive, Light, AmbientOcclusion}

        public string Filename { get; internal set; }
        public int OpenGLID { get; internal set; }
        public int UVMapIndex { get; internal set; }
        public TexType Type { get; internal set; }
    }
}
