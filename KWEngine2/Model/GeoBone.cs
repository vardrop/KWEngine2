using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    public class GeoBone
    {
        public int Index { get; internal set; }
        public string Name { get; internal set; }
        public Matrix4 Offset { get; internal set; }
        public Matrix4 Transform { get; internal set; }

        public List<GeoBone> Children { get; internal set; } = new List<GeoBone>();
        public GeoBone Parent { get; internal set; } = null;
    }
}
