using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;

namespace KWEngine2.GameObjects
{
    /// <summary>
    /// Lichttyp
    /// </summary>
    public enum LightType {
        /// <summary>
        /// Punktlicht
        /// </summary>
        Point, 
        /// <summary>
        /// Gerichtetes Licht
        /// </summary>
        Directional };

    /// <summary>
    /// Lichtklasse
    /// </summary>
    public abstract class LightObject
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = "undefined light object.";
        /// <summary>
        /// Art des Lichts
        /// </summary>
        public LightType Type { get; set; }
        private Vector4 Color { get; set; }
        internal World World { get; set; }

        /// <summary>
        /// Aktuelle Welt
        /// </summary>
        /// <returns>Instanz</returns>
        public World GetCurrentWorld()
        {
            return World;
        }

        /// <summary>
        /// Aktuelle Welt
        /// </summary>
        public World CurrentWorld
        {
            get; internal set;
        }

        /// <summary>
        /// Position des Lichts
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Ziel des Lichts
        /// </summary>
        public Vector3 Target { get; set; }
        /// <summary>
        /// Distanzmultiplikator (Standard: 10)
        /// </summary>
        public float DistanceMultiplier { get; private set; } = 10;

        /// <summary>
        /// Act
        /// </summary>
        /// <param name="ks">Keyboardinfos</param>
        /// <param name="ms">Mausinfos</param>
        /// <param name="deltaTimeFactor">Delta-Time-Faktor (Standard: 1.0)</param>
        public abstract void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor);

        /// <summary>
        /// Konstruktormethode
        /// </summary>
        public LightObject()
        {
            Position = new Vector3(0, 0, 0);
            Target = new Vector3(0, 0, 0);
            Color = new Vector4(1, 1, 1, 1);
            Type = LightType.Point;
            DistanceMultiplier = 10;
        }

        /// <summary>
        /// Ändert den Distanzmultiplikator des Lichts
        /// </summary>
        /// <param name="multiplier">Multiplikator (Standard: 10)</param>
        public void SetDistanceMultiplier(float multiplier)
        {
            if (multiplier > 0)
            {
                DistanceMultiplier = multiplier;
            }
            else
                DistanceMultiplier = 1;
        }

        /// <summary>
        /// Setzt die Lichtfarbe
        /// </summary>
        /// <param name="red">Rot</param>
        /// <param name="green">Grün</param>
        /// <param name="blue">Blau</param>
        /// <param name="intensity">Helligkeit</param>
        public void SetColor(float red, float green, float blue, float intensity)
        {
            Color = new Vector4(
                    Helper.HelperGL.Clamp(red, 0, 1),
                    Helper.HelperGL.Clamp(green, 0, 1),
                    Helper.HelperGL.Clamp(blue, 0, 1),
                    Helper.HelperGL.Clamp(intensity, 0, 1024)
                );
        }

        /// <summary>
        /// Setzt die Position des Lichts
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        public void SetPosition(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }

        /// <summary>
        /// Setzt die Position des Lichts
        /// </summary>
        /// <param name="position">Positionsdaten</param>
        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

        /// <summary>
        /// Setzt das Ziel des gerichteten Lichts
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        public void SetTarget(float x, float y, float z)
        {
            if (Type == LightType.Point)
                throw new Exception("Light instance is not of type 'Directional'.");
            Target = new Vector3(x, y, z);
        }

        /// <summary>
        /// Setzt das Ziel des gerichteten Lichts
        /// </summary>
        /// <param name="target">Zielkoordinaten</param>
        public void SetTarget(Vector3 target)
        {
            if (Type == LightType.Point)
                throw new Exception("Light instance is not of type 'Directional'.");
            Target = target;
        }

        internal static void PrepareLightsForRenderPass(IReadOnlyCollection<LightObject> lights, ref float[] colors, ref float[] targets, ref float[] positions, ref int count)
        {
            count = lights.Count;
            IEnumerator<LightObject> enumerator = lights.GetEnumerator();
            enumerator.Reset();
            for (int i = 0, arraycounter = 0; i < lights.Count; i++, arraycounter += 4)
            {
                enumerator.MoveNext();
                LightObject l = enumerator.Current;
                colors[arraycounter + 0] = l.Color.X;
                colors[arraycounter + 1] = l.Color.Y;
                colors[arraycounter + 2] = l.Color.Z;
                colors[arraycounter + 3] = l.Color.W; // Intensity of color

                targets[arraycounter + 0] = l.Target.X;
                targets[arraycounter + 1] = l.Target.Y;
                targets[arraycounter + 2] = l.Target.Z;
                targets[arraycounter + 3] = l.Type == LightType.Directional ? 1 : -1;

                positions[arraycounter + 0] = l.Position.X;
                positions[arraycounter + 1] = l.Position.Y;
                positions[arraycounter + 2] = l.Position.Z;
                positions[arraycounter + 3] = l.DistanceMultiplier;
            }

        }

    }
}
