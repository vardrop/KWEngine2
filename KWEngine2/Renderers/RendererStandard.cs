﻿using KWEngine2.GameObjects;
using KWEngine2.Helper;
using KWEngine2.Model;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;
using System.Reflection;

namespace KWEngine2.Renderers
{
    internal class RendererStandard : Renderer
    {
        public override void Dispose()
        {
            if (mProgramId >= 0)
            {
                GL.DeleteProgram(mProgramId);
                HelperGL.CheckGLErrors();
                GL.DeleteShader(mShaderVertexId);
                HelperGL.CheckGLErrors();
                GL.DeleteShader(mShaderFragmentId);
                HelperGL.CheckGLErrors();

                mProgramId = -1;
            }
        }

        public override void Initialize()
        {
            Name = "Standard";

            mProgramId = GL.CreateProgram();

            string resourceNameFragmentShader = "KWEngine2.Shaders.shader_fragment.glsl";
            string resourceNameVertexShader = "KWEngine2.Shaders.shader_vertex.glsl";
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
            {
                mShaderVertexId = LoadShader(s, ShaderType.VertexShader, mProgramId);
                Console.WriteLine(GL.GetShaderInfoLog(mShaderVertexId));
            }

            using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
            {
                mShaderFragmentId = LoadShader(s, ShaderType.FragmentShader, mProgramId);
                Console.WriteLine(GL.GetShaderInfoLog(mShaderFragmentId));
            }

            if (mShaderFragmentId >= 0 && mShaderVertexId >= 0)
            {
                GL.BindAttribLocation(mProgramId, 0, "aPosition");
                GL.BindAttribLocation(mProgramId, 1, "aNormal");
                GL.BindAttribLocation(mProgramId, 2, "aTexture");
                GL.BindAttribLocation(mProgramId, 3, "aTexture2");
                GL.BindAttribLocation(mProgramId, 4, "aTangent");
                GL.BindAttribLocation(mProgramId, 5, "aBiTangent");
                GL.BindAttribLocation(mProgramId, 6, "aBoneIds");
                GL.BindAttribLocation(mProgramId, 7, "aBoneWeights");
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
            mAttribute_vnormal = GL.GetAttribLocation(mProgramId, "aNormal");
            mAttribute_vnormaltangent = GL.GetAttribLocation(mProgramId, "aTangent");
            mAttribute_vnormalbitangent = GL.GetAttribLocation(mProgramId, "aBiTangent");
            mAttribute_vjoints = GL.GetAttribLocation(mProgramId, "aBoneIds");
            mAttribute_vweights = GL.GetAttribLocation(mProgramId, "aBoneWeights");
            mAttribute_vtexture2 = GL.GetAttribLocation(mProgramId, "aTexture2");


            mUniform_MVP = GL.GetUniformLocation(mProgramId, "uMVP");
            mUniform_MVPShadowMap = GL.GetUniformLocation(mProgramId, "uMVPShadowMap");
            mUniform_NormalMatrix = GL.GetUniformLocation(mProgramId, "uN");
            mUniform_ModelMatrix = GL.GetUniformLocation(mProgramId, "uM");
            mUniform_UseAnimations = GL.GetUniformLocation(mProgramId, "uUseAnimations");
            mUniform_BoneTransforms = GL.GetUniformLocation(mProgramId, "uBoneTransforms");

            mUniform_Texture = GL.GetUniformLocation(mProgramId, "uTextureDiffuse");
            mUniform_TextureUse = GL.GetUniformLocation(mProgramId, "uUseTextureDiffuse");


            mUniform_Glow = GL.GetUniformLocation(mProgramId, "uGlow");
            mUniform_BaseColor = GL.GetUniformLocation(mProgramId, "uBaseColor");
            mUniform_TintColor = GL.GetUniformLocation(mProgramId, "uTintColor");
            mUniform_EmissiveColor = GL.GetUniformLocation(mProgramId, "uEmissiveColor");
            mUniform_SpecularArea = GL.GetUniformLocation(mProgramId, "uSpecularArea");
            mUniform_SpecularPower = GL.GetUniformLocation(mProgramId, "uSpecularPower");

            mUniform_TextureNormalMap = GL.GetUniformLocation(mProgramId, "uNormalMap");
            mUniform_TextureUseNormalMap = GL.GetUniformLocation(mProgramId, "uTextureUseNormalMap");

            mUniform_TextureSpecularMap = GL.GetUniformLocation(mProgramId, "uSpecularMap");
            mUniform_TextureUseSpecularMap = GL.GetUniformLocation(mProgramId, "uTextureUseSpecularMap");

            mUniform_TextureLightMap = GL.GetUniformLocation(mProgramId, "uLightMap");
            mUniform_TextureUseLightMap = GL.GetUniformLocation(mProgramId, "uTextureUseLightMap");

            mUniform_SpecularArea = GL.GetUniformLocation(mProgramId, "uSpecularArea");
            mUniform_SpecularPower = GL.GetUniformLocation(mProgramId, "uSpecularPower");
            mUniform_uCameraPos = GL.GetUniformLocation(mProgramId, "uCameraPos");

            mUniform_TextureShadowMap = GL.GetUniformLocation(mProgramId, "uTextureShadowMap");

            mUniform_SunPosition = GL.GetUniformLocation(mProgramId, "uSunPosition");
            mUniform_SunDirection = GL.GetUniformLocation(mProgramId, "uSunDirection");
            mUniform_SunIntensity = GL.GetUniformLocation(mProgramId, "uSunIntensity");
            mUniform_SunAffection = GL.GetUniformLocation(mProgramId, "uSunAffection");
            mUniform_SunAmbient = GL.GetUniformLocation(mProgramId, "uSunAmbient");

            mUniform_LightsColors = GL.GetUniformLocation(mProgramId, "uLightsColors");
            mUniform_LightsPositions = GL.GetUniformLocation(mProgramId, "uLightsPositions");
            mUniform_LightsTargets = GL.GetUniformLocation(mProgramId, "uLightsTargets");
            mUniform_LightCount = GL.GetUniformLocation(mProgramId, "uLightCount");

            mUniform_TextureTransform = GL.GetUniformLocation(mProgramId, "uTextureTransform");
        }

        internal override void Draw(GameObject g, ref Matrix4 viewProjection)
        {
            if (g == null || !g.HasModel)
                return;

            GL.UseProgram(mProgramId);

            lock (g)
            {
                foreach (string meshName in g.Model.Meshes.Keys)
                {
                    GeoMesh mesh = g.Model.Meshes[meshName];

                    bool useMeshTransform = true;

                    if (g.AnimationID >= 0 && g.Model.Animations.Count > 0)
                    {
                        if (mUniform_UseAnimations >= 0)
                        {
                            GL.Uniform1(mUniform_UseAnimations, 1);
                        }
                        if (mUniform_BoneTransforms >= 0)
                        {

                            lock (g.BoneTranslationMatrices)
                            {
                                for (int i = 0; i < g.BoneTranslationMatrices[meshName].Length; i++)
                                {
                                    Matrix4 tmp = g.BoneTranslationMatrices[meshName][i];
                                    GL.UniformMatrix4(mUniform_BoneTransforms + i, false, ref tmp);
                                }
                            }
                        }

                        useMeshTransform = false;
                    }
                    else
                    {
                        if (mUniform_UseAnimations >= 0)
                        {
                            GL.Uniform1(mUniform_UseAnimations, 0);
                        }
                    }


                    if (useMeshTransform)
                        Matrix4.Mult(ref mesh.Transform, ref g._modelMatrix, out _tmpMatrix);
                    else
                        _tmpMatrix = g._modelMatrix;
                    Matrix4.Mult(ref _tmpMatrix, ref viewProjection, out _modelViewProjection);
                    Matrix4.Transpose(ref g._modelMatrix, out _normalMatrix);
                    Matrix4.Invert(ref _normalMatrix, out _normalMatrix);

                    GL.UniformMatrix4(mUniform_MVP, false, ref _modelViewProjection);

                    //TODO: Add normal and specular maps

                    if (mUniform_Texture >= 0 && mesh.Material.TextureDiffuse.OpenGLID > 0)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, mesh.Material.TextureDiffuse.OpenGLID);
                        GL.Uniform1(mUniform_Texture, 0);
                        GL.Uniform1(mUniform_TextureUse, 1);

                        GL.Uniform3(mUniform_BaseColor, 1f, 1f, 1f);
                    }
                    else
                    {
                        GL.Uniform1(mUniform_TextureUse, 0);
                        GL.Uniform3(mUniform_BaseColor, mesh.Material.ColorDiffuse.X, mesh.Material.ColorDiffuse.Y, mesh.Material.ColorDiffuse.Z);
                    }

                    GL.BindVertexArray(mesh.VAO);

                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.VBOIndex);
                    GL.DrawElements(mesh.Primitive, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                    GL.BindVertexArray(0);
                }
            }

            GL.UseProgram(0);
        }
    }
}