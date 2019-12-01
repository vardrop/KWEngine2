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
        internal int WeightSet;

        public GeoVertex(int i, float x, float y, float z)
        {
            Index = i;

            X = x;
            Y = y;
            Z = z;

            Weights = new float[Engine.KWEngine.MAX_BONE_WEIGHTS];
            Weights[0] = 1f;

            WeightSet = 0;
            /*new bool[EngineState.MAX_BONE_WEIGHTS];
            for(int c = 0; c < WeightsSet.Length; c++)
            {
                WeightsSet[c] = false;
            }*/
        }
        
    }
}
