using KWEngine2.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    public struct GeoVertex
    {
        public int Index { get; internal set; }
        public float X { get; internal set; }
        public float Y { get; internal set; }
        public float Z { get; internal set; }
        public float[] Weights;
        public int[] BoneIDs;
        internal int WeightSet;

        public GeoVertex(int i, float x, float y, float z)
        {
            Index = i;

            X = x;
            Y = y;
            Z = z;

            Weights = new float[KWEngine.MAX_BONE_WEIGHTS];
            Weights[0] = 1f;

            WeightSet = 0;

            BoneIDs = new int[KWEngine.MAX_BONE_WEIGHTS];
        }
        
    }
}
