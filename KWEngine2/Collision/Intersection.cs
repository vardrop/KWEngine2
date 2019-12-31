using KWEngine2.GameObjects;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Collision
{
    /// <summary>
    /// Kollisionsklasse
    /// </summary>
    public class Intersection
    {
        /// <summary>
        /// Das Objekt, mit dem kollidiert wurde
        /// </summary>
        public GameObject Object { get; private set; } = null;
        /// <summary>
        /// Der Name der Hitbox, mit der kollidiert wurde
        /// </summary>
        public string MeshName { get; private set; } = "";
        
        private Vector3 mMTV = Vector3.Zero;
        /// <summary>
        /// Minimal-Translation-Vector (für Kollisionskorrektur)
        /// </summary>
        public Vector3 MTV
        {
            get
            {
                return mMTV;
            }
        }

        /// <summary>
        /// Kollisionspunkt (für Terrains)
        /// </summary>
        public float HeightOnTerrain { get; internal set; } = 0;

        /// <summary>
        /// Kollisionspunkt (für Terrains), der die Höhe des aufrufenden Objekts berücksichtigt
        /// </summary>
        public float HeightOnTerrainSuggested { get; internal set; } = 0;

        /// <summary>
        /// Gibt an, ob es sich bei dem Kollisionsobjekt um Terrain handelt
        /// </summary>
        public bool IsTerrain { get; internal set; } = false;

        /// <summary>
        /// Konstruktormethode
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="mtv"></param>
        /// <param name="mName"></param>
        /// <param name="suggestedHeightOnTerrain"></param>
        /// <param name="heightOnTerrain"></param>
        /// <param name="isTerrain"></param>
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
