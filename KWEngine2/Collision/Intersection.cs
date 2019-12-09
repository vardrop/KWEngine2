using KWEngine2.GameObjects;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Collision
{
    public class Intersection
    {
        public GameObject Object { get; private set; } = null;
        public string MeshName { get; private set; } = "";
        private Vector3 mMTV = Vector3.Zero;
        public Vector3 MTV
        {
            get
            {
                return mMTV;
            }
        }

        public float HeightOnTerrain { get; internal set; } = 0;
        public float HeightOnTerrainSuggested { get; internal set; } = 0;
        public bool IsTerrain { get; internal set; } = false;

        public Intersection(GameObject collider, Vector3 mtv, string mName, float suggestedHeightOnTerrain = 0, float heightOnTerrain = 0, bool isTerrain = false)
        {
            Object = collider;
            MeshName = mName;
            mMTV.X = mtv.X;
            mMTV.Y = mtv.Y;
            mMTV.Z = mtv.Z;
            HeightOnTerrainSuggested = suggestedHeightOnTerrain;
            HeightOnTerrain = heightOnTerrain;
            IsTerrain = isTerrain;
        }
    }
}
