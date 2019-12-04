﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    public struct GeoAnimation
    {
        public string Name { get; internal set; }
        public float DurationInTicks { get; internal set; }
        public float TicksPerSecond { get; internal set; }
        public Dictionary<GeoBone, GeoNodeAnimationChannel> AnimationChannels { get; internal set; }
    }
}
