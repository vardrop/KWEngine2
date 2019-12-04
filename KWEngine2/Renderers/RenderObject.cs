using KWEngine2.Model;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Renderers
{
    internal class RenderObject
    {
        public Matrix4 ModelMatrix;
        public GeoModel Model;
        public Vector3 ColorDiffuse;
        public Vector4 ColorGlow;

    }
}
