using KWEngine2.GameObjects;
using KWEngine2.Model;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Collision
{
    internal class Hitbox
    {
        private Vector3[] mVertices = new Vector3[8];
        private Vector3[] mNormals = new Vector3[3];
        private Vector3 mCenter = new Vector3(0, 0, 0);
        private Vector3 mDimensions = new Vector3(0, 0, 0);
        private float mLow = 0;
        private float mHigh = 0;
        private float mAverageDiameter = 0;
        private float mFullDiameter = 0;
        private static Vector3 tmp = new Vector3(0, 0, 0);
        private static Vector3 tmpMap = Vector3.Zero;
        private static List<GeoTerrainTriangle> triangles = new List<GeoTerrainTriangle>();
        private static List<GeoTerrainTriangle> trianglesMTV = new List<GeoTerrainTriangle>();

        public float DiameterAveraged
        {
            get
            {
                return mAverageDiameter;
            }
        }

        public float DiameterFull
        {
            get
            {
                return mFullDiameter;
            }

        }

        private static Vector3 MTVTemp = new Vector3(0, 0, 0);
        private Matrix4 mTempMatrix = Matrix4.Identity;
        private Matrix4 mModelMatrixFinal = Matrix4.Identity;

        public GameObject Owner { get; private set; }
        private GeoMeshHitbox mMesh;

        private Vector3 mOldPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private Quaternion mOldRotation = new Quaternion(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
        private Vector3 mOldScale = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        public Hitbox(GameObject owner, GeoMeshHitbox mesh)
        {
            Owner = owner;
            mMesh = mesh;
            Vector3 sceneCenter = Update(ref owner._sceneDimensions);
            Owner._sceneCenter = sceneCenter;
        }

        public Vector3 Update(ref Vector3 dims)
        {
            if (mOldPosition == Owner.Position && mOldRotation == Owner.Rotation && mOldScale == Owner.Scale)
            {
                dims = mDimensions;
                return mCenter;
            }
            else
            {
                mOldPosition.X = Owner.Position.X;
                mOldPosition.Y = Owner.Position.Y;
                mOldPosition.Z = Owner.Position.Z;

                mOldRotation.X = Owner.Rotation.X;
                mOldRotation.Y = Owner.Rotation.Y;
                mOldRotation.Z = Owner.Rotation.Z;
                mOldRotation.W = Owner.Rotation.W;

                mOldScale.X = Owner.Scale.X;
                mOldScale.Y = Owner.Scale.Y;
                mOldScale.Z = Owner.Scale.Z;
            }

            if (Owner.Model.HasBones && Owner.Model.Armature != null)
            {
                Matrix4.Mult(ref Owner.Model.PreRotation, ref Owner.Model.Armature.Transform, out mTempMatrix);
                //Matrix4.Mult(ref tmp, ref mMesh.Transform, out mTempMatrix);
                Matrix4.Mult(ref mTempMatrix, ref Owner._modelMatrix, out mModelMatrixFinal);
            }
            else
            {
                Matrix4.Mult(ref Owner.Model.PreRotation, ref mMesh.Transform, out Matrix4 tmp);
                Matrix4.Mult(ref tmp, ref Owner._modelMatrix, out mModelMatrixFinal);
            }

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            if (mMesh.HasPCA && (mMesh.Name.Contains("P-C-A") || mMesh.Name.Contains("P-C-A")))
            {
                for (int i = 0; i < mVertices.Length; i++)
                {
                    if (i < 3)
                    {
                        Vector3.TransformNormal(ref mMesh.NormalsPCA[i], ref mModelMatrixFinal, out mNormals[i]);
                    }
                    
                    Vector3.TransformPosition(ref mMesh.VerticesPCA[i], ref mModelMatrixFinal, out mVertices[i]);
                    if (mVertices[i].X > maxX)
                        maxX = mVertices[i].X;
                    if (mVertices[i].X < minX)
                        minX = mVertices[i].X;
                    if (mVertices[i].Y > maxY)
                        maxY = mVertices[i].Y;
                    if (mVertices[i].Y < minY)
                        minY = mVertices[i].Y;
                    if (mVertices[i].Z > maxZ)
                        maxZ = mVertices[i].Z;
                    if (mVertices[i].Z < minZ)
                        minZ = mVertices[i].Z;
                }

                Vector3.TransformPosition(ref mMesh.CenterPCA, ref mModelMatrixFinal, out mCenter);
            }
            else
            {
                for (int i = 0; i < mVertices.Length; i++)
                {
                    if (i < 3)
                    {
                        Vector3.TransformNormal(ref mMesh.Normals[i], ref mModelMatrixFinal, out mNormals[i]);
                    }

                    Vector3.TransformPosition(ref mMesh.Vertices[i], ref mModelMatrixFinal, out mVertices[i]);
                    if (mVertices[i].X > maxX)
                        maxX = mVertices[i].X;
                    if (mVertices[i].X < minX)
                        minX = mVertices[i].X;
                    if (mVertices[i].Y > maxY)
                        maxY = mVertices[i].Y;
                    if (mVertices[i].Y < minY)
                        minY = mVertices[i].Y;
                    if (mVertices[i].Z > maxZ)
                        maxZ = mVertices[i].Z;
                    if (mVertices[i].Z < minZ)
                        minZ = mVertices[i].Z;
                }

                Vector3.TransformPosition(ref mMesh.Center, ref mModelMatrixFinal, out mCenter);
            }

            float xWidth = maxX - minX;
            float yWidth = maxY - minY;
            float zWidth = maxZ - minZ;
            dims.X = xWidth;
            dims.Y = yWidth;
            dims.Z = zWidth;

            mLow = minY;
            mHigh = maxY;

            mAverageDiameter = (xWidth + yWidth + zWidth) / 3;
            mFullDiameter = -1;
            if (xWidth > mFullDiameter)
                mFullDiameter = xWidth;
            if (yWidth > mFullDiameter)
                mFullDiameter = yWidth;
            if (zWidth > mFullDiameter)
                mFullDiameter = zWidth;

            mDimensions.X = xWidth;
            mDimensions.Y = yWidth;
            mDimensions.Z = zWidth;

            return mCenter;
        }

        public Vector3 GetDimensions()
        {
            return mDimensions;
        }

        public Vector3 GetCenter()
        {
            return mCenter;
        }

        internal float GetLowestVertexHeight()
        {
            return mLow;
        }

        internal float GetHighestVertexHeight()
        {
            return mHigh;
        }

        public static Intersection TestIntersection(Hitbox caller, Hitbox collider)
        {

            float mtvDistance = float.MaxValue;
            float mtvDirection = 1;
            MTVTemp.X = 0; MTVTemp.Y = 0; MTVTemp.Z = 0;
            for (int i = 0; i < caller.mNormals.Length; i++)
            {
                bool error = false;
                float shape1Min, shape1Max, shape2Min, shape2Max;
                SatTest(ref caller.mNormals[i], ref caller.mVertices, out shape1Min, out shape1Max);
                SatTest(ref caller.mNormals[i], ref collider.mVertices, out shape2Min, out shape2Max);
                if (!Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                {
                    return null;
                }
                else
                {
                    CalculateOverlap(ref caller.mNormals[i], ref shape1Min, ref shape1Max, ref shape2Min, ref shape2Max,
                        out error, ref mtvDistance, ref MTVTemp, ref mtvDirection, ref caller.mCenter, ref collider.mCenter);
                }


                SatTest(ref collider.mNormals[i], ref caller.mVertices, out shape1Min, out shape1Max);
                SatTest(ref collider.mNormals[i], ref collider.mVertices, out shape2Min, out shape2Max);
                if (!Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                {
                    return null;
                }
                else
                {
                    CalculateOverlap(ref collider.mNormals[i], ref shape1Min, ref shape1Max, ref shape2Min, ref shape2Max,
                        out error, ref mtvDistance, ref MTVTemp, ref mtvDirection, ref caller.mCenter, ref collider.mCenter);
                }

            }

            if (MTVTemp == Vector3.Zero)
                return null;

            Intersection o = new Intersection(collider.Owner, MTVTemp, collider.mMesh.Name);
            return o;
        }

        internal static Intersection TestIntersectionTerrain(Hitbox caller, Hitbox collider)
        {
            float heightOnMap;
            triangles.Clear();
            GeoModel model = collider.Owner.Model;
            triangles.AddRange(model.Meshes.Values.ElementAt(0).Terrain.GetTrianglesForHitbox(caller, collider.Owner.Position));
            float a = (caller.mCenter.Y - caller.GetLowestVertexHeight());
            TestIntersectionSATForTerrain(ref triangles, caller, collider);
            Vector3 mobbPosition = new Vector3();
            int lowestTriangle = -1;
            float lowestIntersectionHeight = float.MaxValue;
            int c = 0;
            foreach (GeoTerrainTriangle triangle in trianglesMTV)
            {
                mobbPosition.X = caller.Owner.GetLargestHitbox().mCenter.X;
                mobbPosition.Y = caller.Owner.GetLargestHitbox().mCenter.Y + a;
                mobbPosition.Z = caller.Owner.GetLargestHitbox().mCenter.Z;

                int rayResult = triangle.Intersect3D_RayTriangle(ref mobbPosition, ref tmpMap, collider.Owner.Position);
                float lowestVertexHeight = caller.GetLowestVertexHeight();

                if (rayResult > 0)
                {
                    if (tmpMap.Y < lowestIntersectionHeight)
                    {
                        lowestIntersectionHeight = tmpMap.Y;
                        lowestTriangle = c;
                    }
                }
                else
                {
                    if (rayResult == 0)
                    {
                        if (mobbPosition.X == tmpMap.X && mobbPosition.Z == tmpMap.Z)
                        {
                            if (tmpMap.Y < lowestIntersectionHeight)
                            {
                                lowestIntersectionHeight = tmpMap.Y;
                                lowestTriangle = c;
                            }
                        }
                    }
                }
                c++;
            }
            if (lowestTriangle >= 0)
            {
                heightOnMap = lowestIntersectionHeight + a;
                return new Intersection(collider.Owner, Vector3.Zero, collider.Owner.Name, heightOnMap, lowestIntersectionHeight, true);
            }
            if (trianglesMTV.Count > 0)
            {
                return new Intersection(collider.Owner, Vector3.Zero, collider.Owner.Name, collider.mCenter.Y, collider.mCenter.Y, true);
            }
            return null;
        }

        internal static void TestIntersectionSATForTerrain(ref List<GeoTerrainTriangle> tris, Hitbox caller, Hitbox collider)
        {
            trianglesMTV.Clear();
            float shape1Min, shape1Max, shape2Min, shape2Max;
            Vector3 o = collider.Owner.Position;
            for (int i = 0; i < tris.Count; i++)
            {
                GeoTerrainTriangle triangle = tris[i];

                // Test #1
                SatTest(ref triangle.Normal, ref caller.mVertices, out shape1Min, out shape1Max);
                SatTestOffset(ref triangle.Normal, ref triangle.Vertices, out shape2Min, out shape2Max, ref o);

                if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                {
                    // Test #2:
                    SatTest(ref caller.mNormals[0], ref caller.mVertices, out shape1Min, out shape1Max);
                    SatTestOffset(ref caller.mNormals[0], ref triangle.Vertices, out shape2Min, out shape2Max, ref o);
                    if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                    {
                        // Test #3:
                        SatTest(ref caller.mNormals[1], ref caller.mVertices, out shape1Min, out shape1Max);
                        SatTestOffset(ref caller.mNormals[1], ref triangle.Vertices, out shape2Min, out shape2Max, ref o);
                        if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                        {
                            // Test #4:
                            SatTest(ref caller.mNormals[2], ref caller.mVertices, out shape1Min, out shape1Max);
                            SatTestOffset(ref caller.mNormals[2], ref triangle.Vertices, out shape2Min, out shape2Max, ref o);
                            if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                            {
                                // Test #5: B-A x Hitbox-X-Axis
                                Vector3 subVector = triangle.Vertices[1] - triangle.Vertices[0];
                                //Vector3.Subtract(ref triangle.Vertices[1], ref triangle.Vertices[0], out Vector3 subVector);
                                Vector3.Cross(ref subVector, ref caller.mNormals[0], out Vector3 axisFive);
                                axisFive.NormalizeFast();

                                SatTest(ref axisFive, ref caller.mVertices, out shape1Min, out shape1Max);
                                SatTestOffset(ref axisFive, ref triangle.Vertices, out shape2Min, out shape2Max, ref o);
                                if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                                {
                                    // Test #6: B-A x Hitbox-Y-Axis
                                    //Vector3.Subtract(ref triangle.Vertices[1], ref triangle.Vertices[0], out subVector);
                                    Vector3.Cross(ref subVector, ref caller.mNormals[1], out Vector3 axisSix);
                                    axisSix.NormalizeFast();


                                    SatTest(ref axisSix, ref caller.mVertices, out shape1Min, out shape1Max);
                                    SatTestOffset(ref axisSix, ref triangle.Vertices, out shape2Min, out shape2Max, ref o);
                                    if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                                    {
                                        // Test #7: B-A x Hitbox-Z-Axis
                                        //Vector3.Subtract(ref triangle.Vertices[1], ref triangle.Vertices[0], out subVector);
                                        Vector3.Cross(ref subVector, ref caller.mNormals[2], out Vector3 axisSeven);
                                        axisSeven.NormalizeFast();

                                        SatTest(ref axisSeven, ref caller.mVertices, out shape1Min, out shape1Max);
                                        SatTestOffset(ref axisSeven, ref triangle.Vertices, out shape2Min, out shape2Max, ref o);
                                        if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                                        {
                                            // Test #8: C-B x Hitbox-X-Axis
                                            //Vector3.Subtract(ref triangle.Vertices[2], ref triangle.Vertices[1], out subVector);
                                            subVector = triangle.Vertices[2] - triangle.Vertices[1];
                                            Vector3.Cross(ref subVector, ref caller.mNormals[0], out Vector3 axisEight);
                                            axisEight.NormalizeFast();

                                            SatTest(ref axisEight, ref caller.mVertices, out shape1Min, out shape1Max);
                                            SatTestOffset(ref axisEight, ref triangle.Vertices, out shape2Min, out shape2Max, ref o);
                                            if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                                            {
                                                // Test #9: C-B x Hitbox-Y-Axis
                                                //Vector3.Subtract(ref triangle.Vertices[2], ref triangle.Vertices[1], out subVector);
                                                Vector3.Cross(ref subVector, ref caller.mNormals[1], out Vector3 axisNine);
                                                axisNine.NormalizeFast();

                                                SatTest(ref axisNine, ref caller.mVertices, out shape1Min, out shape1Max);
                                                SatTestOffset(ref axisNine, ref triangle.Vertices, out shape2Min, out shape2Max, ref o);
                                                if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                                                {
                                                    // Test #10: C-B x Hitbox-Z-Axis
                                                    //Vector3.Subtract(ref triangle.Vertices[2], ref triangle.Vertices[1], out subVector);
                                                    Vector3.Cross(ref subVector, ref caller.mNormals[2], out Vector3 axisTen);
                                                    axisTen.NormalizeFast();

                                                    SatTest(ref axisTen, ref caller.mVertices, out shape1Min, out shape1Max);
                                                    SatTestOffset(ref axisTen, ref triangle.Vertices, out shape2Min, out shape2Max, ref o);
                                                    if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                                                    {
                                                        // Test #11: A-C x Hitbox-X-Axis
                                                        //Vector3.Subtract(ref triangle.Vertices[0], ref triangle.Vertices[2], out subVector);
                                                        subVector = triangle.Vertices[0] - triangle.Vertices[2];
                                                        Vector3.Cross(ref subVector, ref caller.mNormals[0], out Vector3 axisEleven);
                                                        axisEleven.NormalizeFast();
                                                        SatTest(ref axisEleven, ref caller.mVertices, out shape1Min, out shape1Max);
                                                        SatTestOffset(ref axisEleven, ref triangle.Vertices, out shape2Min, out shape2Max, ref o);
                                                        if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                                                        {
                                                            // Test #12: A-C x Hitbox-Y-Axis
                                                            //Vector3.Subtract(ref triangle.Vertices[0], ref triangle.Vertices[2], out subVector);
                                                            Vector3.Cross(ref subVector, ref caller.mNormals[1], out Vector3 axisTwelve);
                                                            axisTwelve.NormalizeFast();
                                                            SatTest(ref axisTwelve, ref caller.mVertices, out shape1Min, out shape1Max);
                                                            SatTestOffset(ref axisTwelve, ref triangle.Vertices, out shape2Min, out shape2Max, ref o);
                                                            if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                                                            {
                                                                // Test #13: A-C x Hitbox-Z-Axis
                                                                //Vector3.Subtract(ref triangle.Vertices[0], ref triangle.Vertices[2], out subVector);
                                                                Vector3.Cross(ref subVector, ref caller.mNormals[2], out Vector3 axisThirteen);
                                                                axisThirteen.NormalizeFast();
                                                                SatTest(ref axisThirteen, ref caller.mVertices, out shape1Min, out shape1Max);
                                                                SatTestOffset(ref axisThirteen, ref triangle.Vertices, out shape2Min, out shape2Max, ref o);
                                                                if (Overlaps(shape1Min, shape1Max, shape2Min, shape2Max))
                                                                {
                                                                    trianglesMTV.Add(triangle);
                                                                }
                                                            }
                                                            else
                                                                continue;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                    else
                                                        continue;
                                                }
                                                else
                                                    continue;
                                            }
                                            else
                                                continue;
                                        }
                                        else
                                            continue;
                                    }
                                    else
                                        continue;
                                }
                                else
                                    continue;
                            }
                            else
                                continue;
                        }
                        else
                            continue;
                    }
                    else
                        continue;
                }
                else
                    continue;
            }
        }


        private static void CalculateOverlap(ref Vector3 axis, ref float shape1Min, ref float shape1Max, ref float shape2Min, ref float shape2Max,
            out bool error, ref float mtvDistance, ref Vector3 mtv, ref float mtvDirection, ref Vector3 posA, ref Vector3 posB)
        {
            float intersectionDepthScaled = (shape1Min < shape2Min) ? (shape1Max - shape2Min) : (shape1Min - shape2Max);

            float axisLengthSquared = Vector3.Dot(axis, axis);
            if (axisLengthSquared < 1.0e-8f)
            {
                error = true;
                return;
            }
            float intersectionDepthSquared = (intersectionDepthScaled * intersectionDepthScaled) / axisLengthSquared;

            error = false;

            if (intersectionDepthSquared < mtvDistance || mtvDistance < 0)
            {
                mtvDistance = intersectionDepthSquared;
                mtv = axis * (intersectionDepthScaled / axisLengthSquared);
                float notSameDirection = Vector3.Dot(posA - posB, mtv);
                mtvDirection = notSameDirection < 0 ? -1.0f : 1.0f;
                mtv = mtv * mtvDirection;
            }

        }

        private static bool Overlaps(float min1, float max1, float min2, float max2)
        {
            return IsBetweenOrdered(min2, min1, max1) || IsBetweenOrdered(min1, min2, max2);
        }

        private static bool IsBetweenOrdered(float val, float lowerBound, float upperBound)
        {
            return lowerBound <= val && val <= upperBound;
        }

        private static void SatTest(ref Vector3 axis, ref Vector3[] ptSet, out float minAlong, out float maxAlong)
        {
            minAlong = float.MaxValue;
            maxAlong = float.MinValue;
            for (int i = 0; i < ptSet.Length; i++)
            {
                float dotVal = Vector3.Dot(ptSet[i], axis);
                if (dotVal < minAlong) minAlong = dotVal;
                if (dotVal > maxAlong) maxAlong = dotVal;
            }
        }

        private static void SatTestOffset(ref Vector3 axis, ref Vector3[] ptSet, out float minAlong, out float maxAlong, ref Vector3 offset)
        {
            minAlong = float.MaxValue;
            maxAlong = float.MinValue;
            for (int i = 0; i < ptSet.Length; i++)
            {
                float dotVal = Vector3.Dot(ptSet[i] + offset, axis);
                if (dotVal < minAlong) minAlong = dotVal;
                if (dotVal > maxAlong) maxAlong = dotVal;
            }
        }
    }
}
