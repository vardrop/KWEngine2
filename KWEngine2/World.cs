using KWEngine2.GameObjects;
using KWEngine2.Model;
using System.Collections.Generic;

namespace KWEngine2
{
    public abstract class World
    {
        private List<GameObject> GameObjects = new List<GameObject>();
        private List<GeoModel> Models = new List<GeoModel>();

        internal abstract void DrawHud();

        internal void Dispose()
        {
            lock (GameObjects)
            {
                foreach(GameObject g in GameObjects)
                {
                    g.IsValid = false;
                }
                GameObjects.Clear();
            }

            lock (Models)
            {
                foreach(GeoModel m in Models)
                {
                    m.Dispose();
                }
            }
        }
    }
}
