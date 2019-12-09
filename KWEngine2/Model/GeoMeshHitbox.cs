using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    public class GeoMeshHitbox
    {
        public override string ToString()
        {
            return Name;
        }
        public string Name { get; internal set; }
        internal float maxX, maxY, maxZ;
        internal float minX, minY, minZ;

        internal Vector3[] Vertices = new Vector3[8];
        internal Vector3[] Normals = new Vector3[3];
        internal Vector3 Center = new Vector3(0, 0, 0);

        internal Vector3[] VerticesPCA = new Vector3[8];
        internal Vector3[] NormalsPCA = new Vector3[3];
        internal Vector3 CenterPCA = new Vector3(0, 0, 0);

        public bool HasPCA { get; internal set; } = false;

        internal Matrix4 Transform = Matrix4.Identity;

        public GeoModel Model { get; internal set; } = null;

        public GeoMeshHitbox(float maxX, float maxY, float maxZ, float minX, float minY, float minZ)
        {
            this.maxX = maxX;
            this.maxY = maxY;
            this.maxZ = maxZ;
            this.minX = minX;
            this.minY = minY;
            this.minZ = minZ;

            Center.X = minX + ((maxX - minX) / 2f);
            Center.Y = minY + ((maxY - minY) / 2f);
            Center.Z = minZ + ((maxZ - minZ) / 2f);

            Vertices[0] = new Vector3(minX, minY, maxZ); // frontleftdown
            Vertices[1] = new Vector3(maxX, minY, maxZ); // frontrightdown
            Vertices[2] = new Vector3(maxX, minY, minZ); // backrightdown
            Vertices[3] = new Vector3(minX, minY, minZ); // backleftdown

            Vertices[4] = new Vector3(minX, maxY, maxZ); // frontleftup
            Vertices[5] = new Vector3(maxX, maxY, maxZ); // frontrightup
            Vertices[6] = new Vector3(maxX, maxY, minZ); // backrightup
            Vertices[7] = new Vector3(minX, maxY, minZ); // backleftup

            Normals[0] = new Vector3(1, 0, 0);
            Normals[1] = new Vector3(0, 1, 0);
            Normals[2] = new Vector3(0, 0, 1);
        }
    }
}
