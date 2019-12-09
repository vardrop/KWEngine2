using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    public class GeoNode
    {
        public override string ToString()
        {
            return Name;
        }
        public string Name { get; internal set; } = null;
        public Matrix4 Transform = Matrix4.Identity;

        public List<GeoNode> Children { get; internal set; } = new List<GeoNode>();
        public GeoNode Parent { get; internal set; } = null;
    }
}
