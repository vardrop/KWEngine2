using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;

namespace KWEngine2.GameObjects
{

    public enum LightType { Point, Directional };

    public abstract class LightObject
    {

        public LightType Type { get; private set; }
        private Vector4 Color { get; set; }
        internal World World { get; set; }

        public World GetCurrentWorld()
        {
            return World;
        }

        public World CurrentWorld
        {
            get; internal set;
        }

        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public float DistanceMultiplier { get; private set; }

        public abstract void Act(KeyboardState ks, MouseState ms, float deltaTimeFactor);

        public LightObject()
        {
            Position = new Vector3(0, 0, 0);
            Target = new Vector3(0, 0, 0);
            Color = new Vector4(1, 1, 1, 1);
            Type = LightType.Point;
            DistanceMultiplier = 1;
        }

        public LightObject(LightType type)
        {
            Position = new Vector3(0, 0, 0);
            Target = new Vector3(0, 0, 0);
            Color = new Vector4(1, 1, 1, 1);
            Type = type;
            DistanceMultiplier = 1;
        }

        public void SetDistanceMultiplier(float multiplier)
        {
            if (multiplier > 0)
            {
                DistanceMultiplier = multiplier;
            }
            else
                DistanceMultiplier = 1;
        }


        public void SetColor(float red, float green, float blue, float intensity)
        {
            Color = new Vector4(
                    Helper.HelperGL.Clamp(red, 0, 1),
                    Helper.HelperGL.Clamp(green, 0, 1),
                    Helper.HelperGL.Clamp(blue, 0, 1),
                    Helper.HelperGL.Clamp(intensity, 0, 1024)
                );
        }

        public void SetPosition(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }

        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

        public void SetTarget(float x, float y, float z)
        {
            if (Type == LightType.Point)
                throw new Exception("Light instance is not of type 'Directional'.");
            Target = new Vector3(x, y, z);
        }

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
