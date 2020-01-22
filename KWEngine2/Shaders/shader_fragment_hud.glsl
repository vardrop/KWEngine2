#version 430
 
in		vec2 vTexture;

uniform sampler2D uTextureDiffuse;
uniform vec4 uTintColor;
uniform vec4 uGlow;

out		vec4 color;
out     vec4 bloom;
 
void main()
{
    vec4 tex = texture(uTextureDiffuse, vTexture);
    
    color = tex * uTintColor * uTintColor.w;
    bloom.x = uGlow.x * uGlow.w;
    bloom.y = uGlow.y * uGlow.w;
    bloom.z = uGlow.z * uGlow.w;
    bloom.w = uGlow.w * tex.w;
    
}