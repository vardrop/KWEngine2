using KWEngine2.Helper;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;
using System.Reflection;

namespace KWEngine2.Renderers
{
    internal class RendererStandard : Renderer
    {
        public override void Unload()
        {
            if (mProgramId >= 0)
            {
                GL.DeleteProgram(mProgramId);
                HelperGL.CheckGLErrors();
                GL.DeleteShader(mShaderVertexId);
                HelperGL.CheckGLErrors();
                GL.DeleteShader(mShaderFragmentId);
                HelperGL.CheckGLErrors();
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
                GL.BindAttribLocation(mProgramId, 2, "aNormalTangent");
                GL.BindAttribLocation(mProgramId, 3, "aNormalBiTangent");
                GL.BindAttribLocation(mProgramId, 4, "aTexture");
                GL.BindAttribLocation(mProgramId, 5, "aJoints");
                GL.BindAttribLocation(mProgramId, 6, "aWeights");
                GL.BindAttribLocation(mProgramId, 7, "aTexture2");
                GL.BindFragDataLocation(mProgramId, 0, "color");
                GL.LinkProgram(mProgramId);
            }
            else
            {
                throw new Exception("Creating and linking shaders failed.");
            }

            mAttribute_vpos = GL.GetAttribLocation(mProgramId, "aPosition");
            mAttribute_vtexture = GL.GetAttribLocation(mProgramId, "aTexture");
            mAttribute_vnormal = GL.GetAttribLocation(mProgramId, "aNormal");
            mAttribute_vnormaltangent = GL.GetAttribLocation(mProgramId, "aNormalTangent");
            mAttribute_vnormalbitangent = GL.GetAttribLocation(mProgramId, "aNormalBiTangent");
            mAttribute_vjoints = GL.GetAttribLocation(mProgramId, "aJoints");
            mAttribute_vweights = GL.GetAttribLocation(mProgramId, "aWeights");
            mAttribute_vtexture2 = GL.GetAttribLocation(mProgramId, "aTexture2");
            mUniform_MVP = GL.GetUniformLocation(mProgramId, "uMVP"); 
            mUniform_MVPShadowMap = GL.GetUniformLocation(mProgramId, "uMVPShadowMap");
            mUniform_NormalMatrix = GL.GetUniformLocation(mProgramId, "uNormalMatrix");
            mUniform_ModelMatrix = GL.GetUniformLocation(mProgramId, "uModelMatrix");
            mUniform_HasBones = GL.GetUniformLocation(mProgramId, "uHasBones");
            mUniform_BoneTransforms = GL.GetUniformLocation(mProgramId, "uBoneTransforms");

            mUniform_Texture = GL.GetUniformLocation(mProgramId, "uTexture"); 
            mUniform_TextureUse = GL.GetUniformLocation(mProgramId, "uTextureUse");

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
    }
}
