using KWEngine2.GameObjects;
using KWEngine2.Helper;
using KWEngine2.Model;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static KWEngine2.KWEngine;

namespace KWEngine2.Renderers
{
    internal class RendererSimple : Renderer
    {
        public override void Initialize()
        {
            Name = "Simple";

            mProgramId = GL.CreateProgram();

            string resourceNameFragmentShader = "KWEngine2.Shaders.shader_fragment_simple.glsl";
            string resourceNameVertexShader = "KWEngine2.Shaders.shader_vertex_simple.glsl";
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

                GL.BindFragDataLocation(mProgramId, 0, "color");
                GL.BindFragDataLocation(mProgramId, 1, "bloom");
                GL.LinkProgram(mProgramId);
            }
            else
            {
                throw new Exception("Creating and linking shaders failed.");
            }


            mUniform_MVP = GL.GetUniformLocation(mProgramId, "uMVP");
            mUniform_BaseColor = GL.GetUniformLocation(mProgramId, "uBaseColor");
        }

        internal override void Draw(GameObject g, ref Matrix4 viewProjection, ref Matrix4 viewProjectionShadowBiased, HelperFrustum frustum, ref float[] lightColors, ref float[] lightTargets, ref float[] lightPositions, int lightCount)
        {
            throw new NotImplementedException();
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

        private void UploadMaterialForKWCube(GeoModelCube cubeModel, GeoMesh mesh, GameObject g)
        {

            if (g.ColorEmissive.W > 0)
            {
                GL.Uniform4(mUniform_EmissiveColor, g.ColorEmissive);
            }
            else
            {
                GL.Uniform4(mUniform_EmissiveColor, Vector4.Zero);
            }

            if (mesh.Material.Name == "KWCube")
            {
                UploadMaterialForSide(CubeSide.Front, cubeModel, mesh);
            }
            else
            {
                if (mesh.Material.Name == "Front")
                {
                    UploadMaterialForSide(CubeSide.Front, cubeModel, mesh);
                }
                else if (mesh.Material.Name == "Back")
                {
                    UploadMaterialForSide(CubeSide.Back, cubeModel, mesh);
                }
                else if (mesh.Material.Name == "Left")
                {
                    UploadMaterialForSide(CubeSide.Left, cubeModel, mesh);
                }
                else if (mesh.Material.Name == "Right")
                {
                    UploadMaterialForSide(CubeSide.Right, cubeModel, mesh);
                }
                else if (mesh.Material.Name == "Top")
                {
                    UploadMaterialForSide(CubeSide.Top, cubeModel, mesh);
                }
                else if (mesh.Material.Name == "Bottom")
                {
                    UploadMaterialForSide(CubeSide.Bottom, cubeModel, mesh);
                }
            }


        }

        private void UploadMaterialForSide(CubeSide side, GeoModelCube cubeModel, GeoMesh mesh)
        {

            if (side == CubeSide.Front)
            {
                GL.Uniform2(mUniform_TextureTransform, cubeModel.GeoTextureFront.UVTransform.X, cubeModel.GeoTextureFront.UVTransform.Y);
                if (cubeModel.GeoTextureFront.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureFront.OpenGLID);
                    GL.Uniform1(mUniform_Texture, 0);
                    GL.Uniform1(mUniform_TextureUse, 1);
                    GL.Uniform3(mUniform_BaseColor, 1f, 1f, 1f);

                }
                else
                {
                    GL.Uniform1(mUniform_TextureUse, 0);
                    GL.Uniform3(mUniform_BaseColor, mesh.Material.ColorDiffuse.X, mesh.Material.ColorDiffuse.Y, mesh.Material.ColorDiffuse.Z);
                }

                if (cubeModel.GeoTextureFrontNormal.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureFrontNormal.OpenGLID);
                    GL.Uniform1(mUniform_TextureNormalMap, 1);
                    GL.Uniform1(mUniform_TextureUseNormalMap, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUseNormalMap, 0);
                }

                if (cubeModel.GeoTextureFrontSpecular.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureFrontSpecular.OpenGLID);
                    GL.Uniform1(mUniform_TextureSpecularMap, 2);
                    GL.Uniform1(mUniform_TextureUseSpecularMap, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUseSpecularMap, 0);
                }
            }
            else if (side == CubeSide.Back)
            {
                GL.Uniform2(mUniform_TextureTransform, cubeModel.GeoTextureBack.UVTransform.X, cubeModel.GeoTextureBack.UVTransform.Y);
                if (cubeModel.GeoTextureBack.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureBack.OpenGLID);
                    GL.Uniform1(mUniform_Texture, 0);
                    GL.Uniform1(mUniform_TextureUse, 1);
                    GL.Uniform3(mUniform_BaseColor, 1f, 1f, 1f);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUse, 0);
                    GL.Uniform3(mUniform_BaseColor, mesh.Material.ColorDiffuse.X, mesh.Material.ColorDiffuse.Y, mesh.Material.ColorDiffuse.Z);
                }

                if (cubeModel.GeoTextureBackNormal.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureBackNormal.OpenGLID);
                    GL.Uniform1(mUniform_TextureNormalMap, 1);
                    GL.Uniform1(mUniform_TextureUseNormalMap, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUseNormalMap, 0);
                }

                if (cubeModel.GeoTextureBackSpecular.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureBackSpecular.OpenGLID);
                    GL.Uniform1(mUniform_TextureSpecularMap, 2);
                    GL.Uniform1(mUniform_TextureUseSpecularMap, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUseSpecularMap, 0);
                }
            }
            else if (side == CubeSide.Left)
            {
                GL.Uniform2(mUniform_TextureTransform, cubeModel.GeoTextureLeft.UVTransform.X, cubeModel.GeoTextureLeft.UVTransform.Y);

                if (cubeModel.GeoTextureLeft.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureLeft.OpenGLID);
                    GL.Uniform1(mUniform_Texture, 0);
                    GL.Uniform1(mUniform_TextureUse, 1);
                    GL.Uniform3(mUniform_BaseColor, 1f, 1f, 1f);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUse, 0);
                    GL.Uniform3(mUniform_BaseColor, mesh.Material.ColorDiffuse.X, mesh.Material.ColorDiffuse.Y, mesh.Material.ColorDiffuse.Z);
                }

                if (cubeModel.GeoTextureLeftNormal.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureLeftNormal.OpenGLID);
                    GL.Uniform1(mUniform_TextureNormalMap, 1);
                    GL.Uniform1(mUniform_TextureUseNormalMap, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUseNormalMap, 0);
                }

                if (cubeModel.GeoTextureLeftSpecular.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureLeftSpecular.OpenGLID);
                    GL.Uniform1(mUniform_TextureSpecularMap, 2);
                    GL.Uniform1(mUniform_TextureUseSpecularMap, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUseSpecularMap, 0);
                }
            }
            else if (side == CubeSide.Right)
            {
                GL.Uniform2(mUniform_TextureTransform, cubeModel.GeoTextureRight.UVTransform.X, cubeModel.GeoTextureRight.UVTransform.Y);


                if (cubeModel.GeoTextureRight.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureRight.OpenGLID);
                    GL.Uniform1(mUniform_Texture, 0);
                    GL.Uniform1(mUniform_TextureUse, 1);
                    GL.Uniform3(mUniform_BaseColor, 1f, 1f, 1f);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUse, 0);
                    GL.Uniform3(mUniform_BaseColor, mesh.Material.ColorDiffuse.X, mesh.Material.ColorDiffuse.Y, mesh.Material.ColorDiffuse.Z);
                }

                if (cubeModel.GeoTextureRightNormal.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureRightNormal.OpenGLID);
                    GL.Uniform1(mUniform_TextureNormalMap, 1);
                    GL.Uniform1(mUniform_TextureUseNormalMap, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUseNormalMap, 0);
                }

                if (cubeModel.GeoTextureRightSpecular.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureRightSpecular.OpenGLID);
                    GL.Uniform1(mUniform_TextureSpecularMap, 2);
                    GL.Uniform1(mUniform_TextureUseSpecularMap, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUseSpecularMap, 0);
                }
            }
            else if (side == CubeSide.Top)
            {
                GL.Uniform2(mUniform_TextureTransform, cubeModel.GeoTextureTop.UVTransform.X, cubeModel.GeoTextureTop.UVTransform.Y);


                if (cubeModel.GeoTextureTop.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureTop.OpenGLID);
                    GL.Uniform1(mUniform_Texture, 0);
                    GL.Uniform1(mUniform_TextureUse, 1);
                    GL.Uniform3(mUniform_BaseColor, 1f, 1f, 1f);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUse, 0);
                    GL.Uniform3(mUniform_BaseColor, mesh.Material.ColorDiffuse.X, mesh.Material.ColorDiffuse.Y, mesh.Material.ColorDiffuse.Z);
                }

                if (cubeModel.GeoTextureTopNormal.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureTopNormal.OpenGLID);
                    GL.Uniform1(mUniform_TextureNormalMap, 1);
                    GL.Uniform1(mUniform_TextureUseNormalMap, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUseNormalMap, 0);
                }

                if (cubeModel.GeoTextureTopSpecular.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureTopSpecular.OpenGLID);
                    GL.Uniform1(mUniform_TextureSpecularMap, 2);
                    GL.Uniform1(mUniform_TextureUseSpecularMap, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUseSpecularMap, 0);
                }
            }
            else if (side == CubeSide.Bottom)
            {
                GL.Uniform2(mUniform_TextureTransform, cubeModel.GeoTextureBottom.UVTransform.X, cubeModel.GeoTextureBottom.UVTransform.Y);


                if (cubeModel.GeoTextureBottom.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureBottom.OpenGLID);
                    GL.Uniform1(mUniform_Texture, 0);
                    GL.Uniform1(mUniform_TextureUse, 1);
                    GL.Uniform3(mUniform_BaseColor, 1f, 1f, 1f);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUse, 0);
                    GL.Uniform3(mUniform_BaseColor, mesh.Material.ColorDiffuse.X, mesh.Material.ColorDiffuse.Y, mesh.Material.ColorDiffuse.Z);
                }

                if (cubeModel.GeoTextureBottomNormal.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureBottomNormal.OpenGLID);
                    GL.Uniform1(mUniform_TextureNormalMap, 1);
                    GL.Uniform1(mUniform_TextureUseNormalMap, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUseNormalMap, 0);
                }

                if (cubeModel.GeoTextureBottomSpecular.OpenGLID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, cubeModel.GeoTextureBottomSpecular.OpenGLID);
                    GL.Uniform1(mUniform_TextureSpecularMap, 2);
                    GL.Uniform1(mUniform_TextureUseSpecularMap, 1);
                }
                else
                {
                    GL.Uniform1(mUniform_TextureUseSpecularMap, 0);
                }
            }
        }
    }
}