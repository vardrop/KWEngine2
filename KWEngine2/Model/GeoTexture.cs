using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    internal struct GeoTexture
    {
        public enum TexType { Diffuse, Specular, Normal, Skybox, Emissive, Light, AmbientOcclusion}

        public string Filename { get; internal set; }
        public int OpenGLID { get; internal set; }
        public int UVMapIndex { get; internal set; }
        public Vector2 UVTransform { get; internal set; }
        public TexType Type { get; internal set; }

        public GeoTexture(string name = null)
        {
            Type = TexType.Diffuse;
            Filename = "undefined.";
            OpenGLID = -1;
            UVMapIndex = 0;
            UVTransform = new Vector2(1, 1);
        }
    }
}
