using KWEngine2.GameObjects;
using KWEngine2.Model;
using KWEngine2.Engine;
using System;
using System.Collections.Generic;
using OpenTK.Input;

namespace KWEngine2
{
    public abstract class World
    {
        private List<GameObject> GameObjects = new List<GameObject>();
        private List<HUDObject> HUDObjects = new List<HUDObject>();

        public void LoadModelFromFile(string name, string filename)
        {
            GeoModel m = SceneImporter.LoadModel(filename);
            m.Name = name;
            lock (KWEngine.Models)
            {
                name = name.ToLower();
                if (!KWEngine.Models.ContainsKey(name))
                    KWEngine.Models.Add(name, m);
                else
                    throw new Exception("A model with the name " + name + " already exists.");
            }
        }

        public abstract void Prepare();

        public abstract void Act(KeyboardState kbs, MouseState ms);

        public GeoModel GetModel(string name)
        {
            return KWEngine.GetModel(name);
        }

        public void AddGameObject(GameObject g)
        {
            GameObjects.Add(g);
        }

        public bool RemoveGameObject(GameObject g)
        {
            return GameObjects.Remove(g);
        }

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

            lock (KWEngine.Models)
            {
                foreach(GeoModel m in KWEngine.Models.Values)
                {
                    m.Dispose();
                }
                KWEngine.Models.Clear();
            }
        }
    }
}
