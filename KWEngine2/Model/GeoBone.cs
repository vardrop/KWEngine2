using OpenTK;
using System.Collections.Generic;

namespace KWEngine2.Model
{
    public class GeoBone
    {
        public override string ToString()
        {
            return Name;
        }
        public int Index { get; internal set; }
        public string Name { get; internal set; }
        public Matrix4 Offset { get; internal set; }
        public Matrix4 Transform { get; internal set; }

        public List<GeoBone> Children { get; internal set; } = new List<GeoBone>();
        public GeoBone Parent { get; internal set; } = null;

        
    }
}
