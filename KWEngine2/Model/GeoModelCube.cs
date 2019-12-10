using KWEngine2.GameObjects;
using KWEngine2.Helper;
using System;
using static KWEngine2.GameObjects.GameObject;
using static KWEngine2.KWEngine;

namespace KWEngine2.Model
{
    internal class GeoModelCube
    {
        public int TextureFront = -1;
        public int TextureBack = -1;
        public int TextureLeft = -1;
        public int TextureRight = -1;
        public int TextureTop = -1;
        public int TextureBottom = -1;

        public int TextureFrontNormal = -1;
        public int TextureBackNormal = -1;
        public int TextureLeftNormal = -1;
        public int TextureRightNormal = -1;
        public int TextureTopNormal = -1;
        public int TextureBottomNormal = -1;

        public int TextureFrontSpecular = -1;
        public int TextureBackSpecular = -1;
        public int TextureLeftSpecular = -1;
        public int TextureRightSpecular = -1;
        public int TextureTopSpecular = -1;
        public int TextureBottomSpecular = -1;

        public GameObject Owner = null;


        public void SetTexture(string texture, CubeSide side, TextureType type)
        {
            if(Owner == null || Owner.CurrentWorld == null)
            {
                throw new Exception("Cube texture owner is not set or is not member of a world.");
            }
            if(this is GeoModelCube1)
            {
                SetTextureAll(texture, type);
            }
            else if(side == CubeSide.All)
            {
                SetTextureFront(texture, type);
                SetTextureBack(texture, type);
                SetTextureLeft(texture, type);
                SetTextureRight(texture, type);
                SetTextureTop(texture, type);
                SetTextureBottom(texture, type);
            }
            else if(side == CubeSide.Front)
            {
                SetTextureFront(texture, type);
            }
            else if (side == CubeSide.Back)
            {
                SetTextureBack(texture, type);
            }
            else if (side == CubeSide.Left)
            {
                SetTextureLeft(texture, type);
            }
            else if (side == CubeSide.Right)
            {
                SetTextureRight(texture, type);
            }
            else if (side == CubeSide.Top)
            {
                SetTextureTop(texture, type);
            }
            else if (side == CubeSide.Bottom)
            {
                SetTextureBottom(texture, type);
            }
        }

        private void SetTextureAll(string texture, TextureType type)
        {
            int texAll = -1;
            int texAllNormal = -1;
            int texAllSpecular = -1;
            if (KWEngine.CubeTextures[Owner.CurrentWorld].ContainsKey(texture))
            {
                if (type == TextureType.Diffuse)
                    texAll = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if (type == TextureType.Normal)
                    texAllNormal = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if (type == TextureType.Specular)
                    texAllSpecular = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
            }
            else
            {
                if (type == TextureType.Diffuse)
                    texAll = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Normal)
                    texAllNormal = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Specular)
                    texAllSpecular = HelperTexture.LoadTextureForModelExternal(texture);
            }

            TextureFront = texAll;
            TextureBack = texAll;
            TextureLeft = texAll;
            TextureRight = texAll;
            TextureTop = texAll;
            TextureBottom = texAll;

            TextureFrontNormal = texAllNormal;
            TextureBackNormal = texAllNormal;
            TextureLeftNormal = texAllNormal;
            TextureRightNormal = texAllNormal;
            TextureTopNormal = texAllNormal;
            TextureBottomNormal = texAllNormal;

            TextureFrontSpecular = texAllSpecular;
            TextureBackSpecular = texAllSpecular;
            TextureLeftSpecular = texAllSpecular;
            TextureRightSpecular = texAllSpecular;
            TextureTopSpecular = texAllSpecular;
            TextureBottomSpecular = texAllSpecular;

            foreach (GeoMesh m in Owner.Model.Meshes.Values)
            {
                CreateTextureObject(m, TextureFront, TextureFrontNormal, TextureFrontSpecular, type, texture);
            }
        }

        private void SetTextureFront(string texture, TextureType type)
        {
            if (KWEngine.CubeTextures[Owner.CurrentWorld].ContainsKey(texture))
            {
                if (type == TextureType.Diffuse)
                    TextureFront = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if (type == TextureType.Normal)
                    TextureFrontNormal = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if (type == TextureType.Specular)
                    TextureFrontSpecular = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
            }
            else
            {
                if (type == TextureType.Diffuse)
                    TextureFront = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Normal)
                    TextureFrontNormal = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Specular)
                    TextureFrontSpecular = HelperTexture.LoadTextureForModelExternal(texture);
            }

            foreach (GeoMesh m in Owner.Model.Meshes.Values)
            {
                if (m.Material.Name == "Front")
                {
                    CreateTextureObject(m, TextureFront, TextureFrontNormal, TextureFrontSpecular, type, texture);
                    return;
                }
            }
        }

       

        private void SetTextureBack(string texture, TextureType type)
        {
            if (KWEngine.CubeTextures[Owner.CurrentWorld].ContainsKey(texture))
            {
                if (type == TextureType.Diffuse)
                    TextureBack = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if (type == TextureType.Normal)
                    TextureBackNormal = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if (type == TextureType.Specular)
                    TextureBackSpecular = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
            }
            else
            {
                if (type == TextureType.Diffuse)
                    TextureBack = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Normal)
                    TextureBackNormal = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Specular)
                    TextureBackSpecular = HelperTexture.LoadTextureForModelExternal(texture);
            }

            foreach (GeoMesh m in Owner.Model.Meshes.Values)
            {
                if (m.Material.Name == "Back")
                {
                    CreateTextureObject(m, TextureBack, TextureBackNormal, TextureBackSpecular, type, texture);
                    return;
                }
            }
        }

        private void SetTextureLeft(string texture, TextureType type)
        {
            if (KWEngine.CubeTextures[Owner.CurrentWorld].ContainsKey(texture))
            {
                if (type == TextureType.Diffuse)
                    TextureLeft = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if (type == TextureType.Normal)
                    TextureLeftNormal = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if (type == TextureType.Specular)
                    TextureLeftSpecular = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
            }
            else
            {
                if (type == TextureType.Diffuse)
                    TextureLeft = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Normal)
                    TextureLeftNormal = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Specular)
                    TextureLeftSpecular = HelperTexture.LoadTextureForModelExternal(texture);
            }

            foreach (GeoMesh m in Owner.Model.Meshes.Values)
            {
                if (m.Material.Name == "Left")
                {
                    CreateTextureObject(m, TextureLeft, TextureLeftNormal, TextureLeftSpecular, type, texture);
                    return;
                }
            }
        }

        private void SetTextureRight(string texture, TextureType type)
        {
            if (KWEngine.CubeTextures[Owner.CurrentWorld].ContainsKey(texture))
            {
                if (type == TextureType.Diffuse)
                    TextureRight = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if (type == TextureType.Normal)
                    TextureRightNormal = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if (type == TextureType.Specular)
                    TextureRightSpecular = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
            }
            else
            {
                if (type == TextureType.Diffuse)
                    TextureRight = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Normal)
                    TextureRightNormal = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Specular)
                    TextureRightSpecular = HelperTexture.LoadTextureForModelExternal(texture);
            }

            foreach (GeoMesh m in Owner.Model.Meshes.Values)
            {
                if (m.Material.Name == "Right")
                {
                    CreateTextureObject(m, TextureRight, TextureRightNormal, TextureRightSpecular, type, texture);
                    return;
                }
            }
        }

        private void SetTextureTop(string texture, TextureType type)
        {
            if (KWEngine.CubeTextures[Owner.CurrentWorld].ContainsKey(texture))
            {
                if (type == TextureType.Diffuse)
                    TextureTop = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if (type == TextureType.Normal)
                    TextureTopNormal = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if (type == TextureType.Specular)
                    TextureTopSpecular = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
            }
            else
            {
                if (type == TextureType.Diffuse)
                    TextureTop = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Normal)
                    TextureTopNormal = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Specular)
                    TextureTopSpecular = HelperTexture.LoadTextureForModelExternal(texture);
            }

            foreach (GeoMesh m in Owner.Model.Meshes.Values)
            {
                if (m.Material.Name == "Top")
                {
                    CreateTextureObject(m, TextureTop, TextureTopNormal, TextureTopSpecular, type, texture);
                    return;
                }
            }
        }

        private void SetTextureBottom(string texture, TextureType type)
        {
            if (KWEngine.CubeTextures[Owner.CurrentWorld].ContainsKey(texture))
            {
                if(type == TextureType.Diffuse)
                    TextureBottom = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if(type == TextureType.Normal)
                    TextureBottomNormal = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
                else if(type == TextureType.Specular)
                    TextureBottomSpecular = KWEngine.CubeTextures[Owner.CurrentWorld][texture];
            }
            else
            {
                if (type == TextureType.Diffuse)
                    TextureBottom = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Normal)
                    TextureBottomNormal = HelperTexture.LoadTextureForModelExternal(texture);
                else if (type == TextureType.Specular)
                    TextureBottomSpecular= HelperTexture.LoadTextureForModelExternal(texture);
            }

            foreach (GeoMesh m in Owner.Model.Meshes.Values)
            {
                if (m.Material.Name == "Bottom")
                {
                    CreateTextureObject(m, TextureBottom, TextureBottomNormal, TextureBottomSpecular, type, texture);
                    return;
                }
            }
        }

        private void CreateTextureObject(GeoMesh mesh, int diffuse, int normal, int specular, TextureType type, string name)
        {
            if (type == TextureType.Diffuse)
            {
                GeoTexture tex = new GeoTexture();
                tex.Filename = name;
                tex.Type = GeoTexture.TexType.Diffuse;
                tex.UVMapIndex = 0;
                tex.OpenGLID = diffuse;

                mesh.Material.TextureDiffuse = tex;
            }
            else if (type == TextureType.Normal)
            {
                GeoTexture tex = new GeoTexture();
                tex.Filename = name;
                tex.Type = GeoTexture.TexType.Normal;
                tex.UVMapIndex = 0;
                tex.OpenGLID = normal;

                mesh.Material.TextureNormal = tex;
            }
            else
            {
                GeoTexture tex = new GeoTexture();
                tex.Filename = name;
                tex.Type = GeoTexture.TexType.Specular;
                tex.UVMapIndex = 0;
                tex.OpenGLID = specular;

                mesh.Material.TextureSpecular = tex;
            }
        }

    }
}
