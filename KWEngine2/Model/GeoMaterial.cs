using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace KWEngine2.Model
{
    internal class GeoMaterial
    {
        public bool IsPBRMaterial { get; internal set; } = false;

        public string Name { get; internal set; }
        public BlendingFactor BlendMode { get; internal set; } = BlendingFactor.OneMinusSrcAlpha;
        public Vector4 ColorEmissive { get; internal set; } = new Vector4(0, 0, 0, 0);
        public Vector4 ColorDiffuse { get; internal set; } = new Vector4(1, 1, 1, 1);

        public float Opacity { get; internal set; } = 1;
        public float SpecularPower { get; internal set; } = 0;
        public float SpecularArea { get; internal set; } = 256;

        public GeoTexture TextureDiffuse { get; internal set; } = new GeoTexture(null);
        public GeoTexture TextureNormal { get; internal set; } = new GeoTexture(null);
        public GeoTexture TextureSpecular { get; internal set; } = new GeoTexture(null);

        public GeoTexture TextureMetallic { get; internal set; } = new GeoTexture(null);
        public GeoTexture TextureRoughness { get; internal set; } = new GeoTexture(null);

        public float ColorMetallic { get; internal set; } = 0;
        public float ColorRoughness { get; internal set; } = 1;

        public GeoTexture TextureEmissive { get; internal set; } = new GeoTexture(null);
        public GeoTexture TextureLight { get; internal set; } = new GeoTexture(null);

        public bool TextureSpecularIsRoughness { get; internal set; } = false;

    }
}
