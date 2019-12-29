using KWEngine2.GameObjects;
using KWEngine2.Helper;
using KWEngine2.Model;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using static KWEngine2.KWEngine;

namespace KWEngine2.Renderers
{
    internal class RendererExplosion : Renderer
    {
        private int mUniform_Time = -1;
        private int mUniform_VP = -1;
        private int mUniform_Number = -1;
        private int mUniform_Spread = -1;
        private int mUniform_Position = -1;
        private int mUniform_Size = -1;
        private int mUniform_Axes = -1;

        public override void Initialize()
        {
            Name = "Explosion";

            mProgramId = GL.CreateProgram();

            string resourceNameFragmentShader = "KWEngine2.Shaders.shader_fragment_explosion.glsl";
            string resourceNameVertexShader = "KWEngine2.Shaders.shader_vertex_explosion.glsl";
            Assembly assembly = Assembly.GetExecutingAssembly();
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
                GL.BindFragDataLocation(mProgramId, 1, "bloom");
                GL.LinkProgram(mProgramId);
            }
            else
            {
                throw new Exception("Creating and linking shaders failed.");
            }

            mAttribute_vpos = GL.GetAttribLocation(mProgramId, "aPosition");
            mAttribute_vtexture = GL.GetAttribLocation(mProgramId, "aTexture");
            
            mUniform_VP = GL.GetUniformLocation(mProgramId, "uVP");

            // Textures:
            mUniform_Texture = GL.GetUniformLocation(mProgramId, "uTextureDiffuse");
            mUniform_TextureUse = GL.GetUniformLocation(mProgramId, "uUseTextureDiffuse");
            mUniform_TextureTransform = GL.GetUniformLocation(mProgramId, "uTextureTransform");

            mUniform_Glow = GL.GetUniformLocation(mProgramId, "uGlow");
            //mUniform_TintColor = GL.GetUniformLocation(mProgramId, "uTintColor");
            mUniform_SunAmbient = GL.GetUniformLocation(mProgramId, "uSunAmbient");
            mUniform_Time = GL.GetUniformLocation(mProgramId, "uTime");
            mUniform_Number = GL.GetUniformLocation(mProgramId, "uNumber");
            mUniform_Spread= GL.GetUniformLocation(mProgramId, "uSpread");
            mUniform_Size = GL.GetUniformLocation(mProgramId, "uSize");
            mUniform_Position = GL.GetUniformLocation(mProgramId, "uPosition");
            mUniform_Axes = GL.GetUniformLocation(mProgramId, "uAxes");

        }

        internal override void Draw(GameObject g, ref Matrix4 viewProjection, ref Matrix4 viewProjectionShadowBiased, HelperFrustum frustum, ref float[] lightColors, ref float[] lightTargets, ref float[] lightPositions, int lightCount)
        {
            throw new NotImplementedException();
        }

        internal override void Draw(GameObject g, ref Matrix4 viewProjection)
        {
            if (g == null || !g.HasModel || g.CurrentWorld == null || !(g is Explosion))
                return;

            GL.UseProgram(mProgramId);
            Explosion e = (Explosion)g;

            lock (g)
            {
                GL.Uniform4(mUniform_Glow, g.Glow.X, g.Glow.Y, g.Glow.Z, g.Glow.W);
                GL.Uniform3(mUniform_TintColor, g.Color.X, g.Color.Y, g.Color.Z);
                GL.Uniform1(mUniform_SunAmbient, HelperGL.Clamp(g.CurrentWorld.SunAmbientFactor * 2f, 0, 1));
                GL.Uniform1(mUniform_Number, (float)e._amount);
                GL.Uniform1(mUniform_Spread, e._spread);
                GL.Uniform3(mUniform_Position, e.Position);
                GL.Uniform1(mUniform_Time, e._secondsAlive / e._duration);
                GL.Uniform1(mUniform_Size, e._particleSize);

                GL.Uniform4(mUniform_Axes, e._amount, e._directions);

                GL.UniformMatrix4(mUniform_VP, false, ref viewProjection);
                GL.Uniform2(mUniform_TextureTransform, e._textureTransform.X, e._textureTransform.Y);

                if (e._textureId > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, e._textureId);
                    GL.Uniform1(mUniform_Texture, 0);
                    GL.Uniform1(mUniform_TextureUse, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUse, 0);
                }

                GeoMesh mesh = e.Model.Meshes.ElementAt(0).Value;
                GL.BindVertexArray(mesh.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                GL.DrawElementsInstanced(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, e._amount);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                GL.BindVertexArray(0);
                
            }

            GL.UseProgram(0);
            
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
    }
}