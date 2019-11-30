using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    public class GeoNode
    {
        public List<GeoNode> Children { get; internal set; } = new List<GeoNode>();
        public GeoNode Parent { get; internal set; } = null;

        public Object Content { get; internal set; } = null;

        public bool IsMesh
        {
            get
            {
                return Content != null && Content is GeoMesh;
            }
        }

        public bool IsArmature
        {
            get
            {
                return Content != null && Content is GeoArmature;
            }
        }
    }
}
