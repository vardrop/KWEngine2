using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    public enum GeoKeyframeType { Rotation, Translation, Scale }

    public struct GeoAnimationKeyframe
    {
        public GeoKeyframeType Type { get; internal set; }

        public Quaternion Rotation { get; internal set; }
        public Vector3 Translation { get; internal set; }
        public Vector3 Scale { get; internal set; }

        public float Time { get; internal set; }

    }
}
