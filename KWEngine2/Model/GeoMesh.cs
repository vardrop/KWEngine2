using Assimp;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine2.Model
{
    public struct GeoMesh
    {
        public override string ToString()
        {
            return Name;
        }

        public List<GeoBone> Bones { get; internal set; }
        internal List<Matrix4> BoneTranslationMatrices;

        internal List<string> AssimpBoneOrder { get;  set; }
        public int VAO { get; internal set; }
        public int VBOPosition { get; internal set; }
        public int VBONormal { get; internal set; }
        public int VBOTexture1 { get; internal set; }
        public int VBOTexture2 { get; internal set; }
        public int VBOBoneIDs { get; internal set; }
        public int VBOBoneWeights { get; internal set; }
        public int VBOTangent { get; internal set; }
        public int VBOBiTangent { get; internal set; }
        public int VBOIndex { get; internal set; }
        public string Name { get; internal set; }
        internal Matrix4 Transform;
        public GeoVertex[] Vertices { get; internal set; }
        public OpenTK.Graphics.OpenGL4.PrimitiveType Primitive;
        public int IndexCount 
        { 
            get
            {
                return Indices.Length;
            }
        }
        public int[] Indices { get; internal set; }
        public GeoMaterial Material { get; internal set; }

        internal void VAOGenerateAndBind()
        {
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
        }

        internal void VAOUnbind()
        {
            GL.BindVertexArray(0);
        }

        internal void VBOGenerateVerticesAndBones(bool hasBones)
        {
            float[] verticesF = new float[Vertices.Length * 3];
            int[] boneIds = new int[Vertices.Length * 3];
            float[] boneWeights = new float[Vertices.Length * 3];

            for (int i = 0, arrayIndex = 0; i < Vertices.Length; i++, arrayIndex += 3)
            {
                verticesF[arrayIndex] = Vertices[i].X;
                verticesF[arrayIndex+1] = Vertices[i].Y;
                verticesF[arrayIndex+2] = Vertices[i].Z;

                if (hasBones)
                {
                    boneIds[arrayIndex] = Vertices[i].BoneIDs[0];
                    boneIds[arrayIndex + 1] = Vertices[i].BoneIDs[1];
                    boneIds[arrayIndex + 2] = Vertices[i].BoneIDs[2];

                    boneWeights[arrayIndex] = Vertices[i].Weights[0];
                    boneWeights[arrayIndex + 1] = Vertices[i].Weights[1];
                    boneWeights[arrayIndex + 2] = Vertices[i].Weights[2];
                }
                /*
                Console.Write("V" + i.ToString().PadLeft(5, '0') + ": ");
                Console.Write(boneIds[arrayIndex] + " (" + Math.Round(boneWeights[arrayIndex], 2) + "), ");
                Console.Write(boneIds[arrayIndex+1] + " (" + Math.Round(boneWeights[arrayIndex+1], 2) + "), ");
                Console.WriteLine(boneIds[arrayIndex+2] + " (" + Math.Round(boneWeights[arrayIndex+2], 2) + "), ");
                */
            }
            VBOPosition = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOPosition);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * 3 * 4, verticesF, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            if (hasBones)
            {
                VBOBoneIDs = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOBoneIDs);
                GL.BufferData(BufferTarget.ArrayBuffer, boneIds.Length * 4, boneIds, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(6, 3, VertexAttribPointerType.Int, false, 0, 0);
                GL.EnableVertexAttribArray(6);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                VBOBoneWeights = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOBoneWeights);
                GL.BufferData(BufferTarget.ArrayBuffer, boneWeights.Length * 4, boneWeights, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(7, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(7);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

        internal void VBOGenerateNormals(Mesh mesh)
        {
            if (mesh.HasNormals)
            {
                float[] values = new float[mesh.Normals.Count * 3];

                for (int i = 0, arrayIndex = 0; i < mesh.Normals.Count; i++, arrayIndex += 3)
                {
                    values[arrayIndex] = mesh.Normals[i].X;
                    values[arrayIndex + 1] = mesh.Normals[i].Y;
                    values[arrayIndex + 2] = mesh.Normals[i].Z;
                }
                VBONormal = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBONormal);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.Normals.Count * 3 * 4, values, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(1);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

        internal void VBOGenerateTextureCoords1(Mesh mesh)
        {
            if (mesh.HasTextureCoords(0))
            {
                float[] values = new float[mesh.TextureCoordinateChannels[0].Count * 2];

                for (int i = 0, arrayIndex = 0; i < mesh.TextureCoordinateChannels[0].Count; i++, arrayIndex += 2)
                {
                    values[arrayIndex] = mesh.TextureCoordinateChannels[0][i].X;
                    values[arrayIndex + 1] = mesh.TextureCoordinateChannels[0][i].Y;
                }
                VBOTexture1 = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTexture1);
                GL.BufferData(BufferTarget.ArrayBuffer, values.Length * 4, values, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(2);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

        internal void VBOGenerateTextureCoords2(Mesh mesh)
        {
            if (mesh.HasTextureCoords(1))
            {
                float[] values = new float[mesh.TextureCoordinateChannels[1].Count * 2];

                for (int i = 0, arrayIndex = 0; i < mesh.TextureCoordinateChannels[1].Count; i++, arrayIndex += 2)
                {
                    values[arrayIndex] = mesh.TextureCoordinateChannels[1][i].X;
                    values[arrayIndex + 1] = mesh.TextureCoordinateChannels[1][i].Y;
                }
                VBOTexture2 = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTexture2);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.TextureCoordinateChannels[1].Count * 2 * 4, values, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(3);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

        internal void VBOGenerateTangents(Mesh mesh)
        {
            if (mesh.HasTangentBasis)
            {
                //Tangents
                float[] values = new float[mesh.Tangents.Count * 3];

                for (int i = 0, arrayIndex = 0; i < mesh.Tangents.Count; i++, arrayIndex += 3)
                {
                    values[arrayIndex] = mesh.Tangents[i].X;
                    values[arrayIndex + 1] = mesh.Tangents[i].Y;
                    values[arrayIndex + 2] = mesh.Tangents[i].Z;
                }
                VBOTangent = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTangent);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.Tangents.Count * 3 * 4, values, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(4);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                //BiTangents
                values = new float[mesh.BiTangents.Count * 3];

                for (int i = 0, arrayIndex = 0; i < mesh.BiTangents.Count; i++, arrayIndex += 3)
                {
                    values[arrayIndex] = mesh.BiTangents[i].X;
                    values[arrayIndex + 1] = mesh.BiTangents[i].Y;
                    values[arrayIndex + 2] = mesh.BiTangents[i].Z;
                }
                VBOBiTangent = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOBiTangent);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.BiTangents.Count * 3 * 4, values, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(5, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(5);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

        internal void VBOGenerateIndices()
        {
            VBOIndex = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIndex);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * 4, Indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        internal void Dispose()
        {
            // Dispose VAO and VBOS:
            if (VBOIndex >= 0)
                GL.DeleteBuffer(VBOIndex);
            if (VBOPosition >= 0)
                GL.DeleteBuffer(VBOPosition);
            if (VBONormal>= 0)
                GL.DeleteBuffer(VBONormal);
            if (VBOTangent >= 0)
                GL.DeleteBuffer(VBOTangent);
            if (VBOBiTangent >= 0)
                GL.DeleteBuffer(VBOBiTangent);
            if (VBOTexture1 >= 0)
                GL.DeleteBuffer(VBOTexture1);
            if (VBOTexture2 >= 0)
                GL.DeleteBuffer(VBOTexture2);
            if (VBOBoneIDs >= 0)
                GL.DeleteBuffer(VBOBoneIDs);
            if (VBOBoneWeights >= 0)
                GL.DeleteBuffer(VBOBoneWeights);

            if (VAO >= 0)
                GL.DeleteVertexArray(VAO);
        }
    }
}
