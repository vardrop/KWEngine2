using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    internal struct GeoTerrainTriangle
    {
        private Vector3 v1;
        private Vector3 v2;
        private Vector3 v3;

        internal Vector3 edge1;
        internal Vector3 edge2;
        private readonly float uu;
        private readonly float uv;
        private readonly float vv;
        private readonly bool isUpperTriangle;

        internal Vector3[] Vertices;
        internal Vector3 Center;
        internal Vector3 Normal;

        internal Vector3 crossEdges;

        internal float boundLeft;
        internal float boundRight;
        internal float boundFront;
        internal float boundBack;

        internal Vector3[] Normals;

        private Vector3 CalculateSurfaceNormal(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 x = Vector3.NormalizeFast(v2 - v1);
            Vector3 z = Vector3.NormalizeFast(v3 - v1);
            Vector3 y = Vector3.NormalizeFast(Vector3.Cross(x, z));
            y = y.Y < 0 ? -y : y;
            return y;
        }

        public GeoTerrainTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            Normals = new Vector3[3];
            Normal = Vector3.Zero;
            isUpperTriangle = a.Z > b.Z;
            v1 = a; v2 = b; v3 = c;
            if (!isUpperTriangle)
            {
                boundLeft = v1.X;
                boundRight = v3.X;
                boundBack = v1.Z;
                boundFront = v2.Z;
            }
            else
            {
                boundLeft = v3.X;
                boundRight = v2.X;
                boundBack = v2.Z;
                boundFront = v1.Z;
            }

            Vertices = new Vector3[3];
            Vertices[0] = v1; Vertices[1] = v2; Vertices[2] = v3;
            Center = new Vector3((v1.X + v2.X + v3.X) / 3f, (v1.Y + v2.Y + v3.Y) / 3f, (v1.Z + v2.Z + v3.Z) / 3f);
            //float dV1V2 = (v2 - v1).LengthFast;
            //float dV1V3 = (v3 - v1).LengthFast;

            edge1 = Vertices[1] - Vertices[0];
            edge2 = Vertices[2] - Vertices[0];
            Vector3.Cross(ref edge1, ref edge2, out crossEdges);

            uu = Vector3.Dot(edge1, edge1);
            uv = Vector3.Dot(edge1, edge2);
            vv = Vector3.Dot(edge2, edge2);

            // Find Normal that's pointing upward:
            Normal = CalculateSurfaceNormal(v1, v2, v3);

            // Find other 2 normals:
            Normals[2] = Vector3.Normalize(Vector3.Cross(Normal, Vector3.UnitZ));
            if (Normals[2].X < 0)
                Normals[2] = -Normals[2];
            Normals[1] = Normal;
            Normals[0] = Vector3.Normalize(Vector3.Cross(Normal, Vector3.UnitX));
            if (Normals[0].Z < 0)
                Normals[0] = -Normals[0];
        }

        public int Intersect3D_RayTriangle(ref Vector3 origin, ref Vector3 I, Vector3 offset, bool shootFromBelow = false)
        {
            Vector3 dir, w0, w;           // ray vectors
            float r, a, b;              // params to calc ray-plane intersect

            dir = shootFromBelow ? KWEngine.WorldUp : -KWEngine.WorldUp;              // ray direction vector
            w0 = origin - (v1 + offset); // R.P0 - T.V0;
            a = -Vector3.Dot(Normal, w0);
            b = Vector3.Dot(Normal, dir);
            if (Math.Abs(b) <= 0.00001f)
            {     // ray is  parallel to triangle plane
                if (a == 0)                 // ray lies in triangle plane
                    return 2;
                else return 0;              // ray disjoint from plane
            }

            // get intersect point of ray with triangle plane
            r = a / b;
            if (r < 0.0)                    // ray goes away from triangle
                return 0;                   // => no intersect
                                            // for a segment, also test if (r > 1.0) => no intersect
            I = origin + r * dir;
            //*I = R.P0 + r * dir;            // intersect point of ray and plane

            // is I inside T?
            float wu, wv, D;

            w = I - (v1 + offset);
            wu = Vector3.Dot(w, edge1);
            wv = Vector3.Dot(w, edge2);
            D = uv * uv - uu * vv;

            // get and test parametric coords
            float s, t;
            s = (uv * wv - vv * wu) / D;
            if (s < 0.0 || s > 1.0)         // I is outside T
                return 0;
            t = (uv * wu - uu * wv) / D;
            if (t < 0.0 || (s + t) > 1.0)  // I is outside T
                return 0;

            return 1;                       // I is in T
        }
    }
}
