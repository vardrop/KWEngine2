using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    public struct GeoNodeAnimationChannel
    {
        public List<GeoAnimationKeyframe> ScaleKeys { get; internal set; }
        public List<GeoAnimationKeyframe> RotationKeys { get; internal set; }
        public List<GeoAnimationKeyframe> TranslationKeys { get; internal set; }
    }
}
