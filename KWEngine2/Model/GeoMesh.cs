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
        public int VAO { get; internal set; }
        public int VBOPosition { get; internal set; }
        public int VBOIndex { get; internal set; }
        public string Name { get; internal set; }
        public Matrix4 Transform { get; internal set; }
        public GeoVertex[] Vertices { get; internal set; }
        public PrimitiveType Primitive;
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

        internal void VBOGenerateVertices()
        {
            float[] verticesF = new float[Vertices.Length * 3];
            
            for(int i = 0, arrayIndex = 0; i < Vertices.Length; i++, arrayIndex += 3)
            {
                verticesF[arrayIndex] = Vertices[i].X;
                verticesF[arrayIndex+1] = Vertices[i].Y;
                verticesF[arrayIndex+2] = Vertices[i].Z;
            }
            VBOPosition = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOPosition);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * 3 * 4, verticesF, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
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
            throw new NotImplementedException();
        }
    }
}
