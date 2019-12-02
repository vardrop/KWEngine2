using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    public struct GeoBone
    {
        public int Index { get; internal set; }
        public string Name { get; internal set; }
        public Matrix4 Offset { get; internal set; }
        //public Vector3[] VertexWeights { get; internal set; }
    }
}
