﻿using KWEngine2.GameObjects;
using KWEngine2.Helper;
using KWEngine2.Model;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace KWEngine2.Renderers
{
    internal sealed class RendererMerge : Renderer
    {
        private int mUniform_TextureBloom = -1;
        private int mUniform_TextureScene = -1;

        internal void DrawMerge(GeoModel quad, ref Matrix4 mvp, int sceneTexture, int bloomTexture)
        {
            GL.UniformMatrix4(mUniform_MVP, false, ref mvp);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, bloomTexture);
            GL.Uniform1(mUniform_TextureBloom, 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, sceneTexture);
            GL.Uniform1(mUniform_TextureScene, 1);

            GL.BindVertexArray(quad.Meshes.Values.ElementAt(0).VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, quad.Meshes.Values.ElementAt(0).VBOIndex);
            GL.DrawElements(quad.Meshes.Values.ElementAt(0).Primitive, quad.Meshes.Values.ElementAt(0).IndexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void Initialize()
        {
            Name = "Merge";

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceNameVertexShader = "KWEngine2.Shaders.shader_vertex_merge.glsl";
            string resourceNameFragmentShader = "KWEngine2.Shaders.shader_fragment_merge.glsl";

            mProgramId = GL.CreateProgram();
            using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
            {
                mShaderVertexId = LoadShader(s, ShaderType.VertexShader, mProgramId);
                //Console.WriteLine(GL.GetShaderInfoLog(mShaderVertexId));
            }
            using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
            {
                mShaderFragmentId = LoadShader(s, ShaderType.FragmentShader, mProgramId);
                //Console.WriteLine(GL.GetShaderInfoLog(mShaderFragmentId));
            }



            if (mShaderFragmentId >= 0 && mShaderVertexId >= 0)
            {
                GL.BindAttribLocation(mProgramId, 0, "aPosition");
                GL.BindAttribLocation(mProgramId, 2, "aTexture");

                GL.BindFragDataLocation(mProgramId, 0, "color");

                GL.LinkProgram(mProgramId);
            }
            else
            {
                throw new Exception("Creating and linking shaders failed.");
            }

            mAttribute_vpos = GL.GetAttribLocation(mProgramId, "aPosition");
            mAttribute_vtexture = GL.GetAttribLocation(mProgramId, "aTexture");

            mUniform_MVP = GL.GetUniformLocation(mProgramId, "uMVP");
            mUniform_TextureScene = GL.GetUniformLocation(mProgramId, "uTextureScene");
            mUniform_TextureBloom = GL.GetUniformLocation(mProgramId, "uTextureBloom");
        }

        internal override void Draw(GameObject g, ref Matrix4 viewProjection)
        {
            throw new NotImplementedException();
        }

        internal override void Draw(GameObject g, ref Matrix4 viewProjection, HelperFrustum frustum)
        {
            throw new NotImplementedException();
        }

        internal override void Draw(ParticleObject po, ref Matrix4 viewProjection)
        {
            throw new NotImplementedException();
        }

        internal override void Draw(HUDObject ho, ref Matrix4 viewProjection)
        {
            throw new NotImplementedException();
        }

        internal override void Draw(GameObject g, ref Matrix4 viewProjection, ref Matrix4 viewProjectionShadow, ref Matrix4 viewProjectionShadow2, HelperFrustum frustum, ref float[] lightColors, ref float[] lightTargets, ref float[] lightPositions, int lightCount, ref int lightShadow)
        {
            throw new NotImplementedException();
        }

        internal override void Draw(GameObject g, ref Matrix4 viewProjection, HelperFrustum frustum, bool isSun)
        {
            throw new NotImplementedException();
        }
    }
}
