#version 430

in		vec3 vPosition;
in		vec3 vNormal;
in		vec2 vTexture;
in		vec2 vTexture2;
in		mat3 vTBN;

uniform sampler2D uTextureDiffuse;
uniform int uUseTextureDiffuse;
uniform sampler2D uTextureNormal;
uniform int uUseTextureNormal;
uniform sampler2D uTextureSpecular;
uniform int uUseTextureSpecular;
uniform sampler2D uTextureLightmap;
uniform int uUseTextureLightmap;
uniform sampler2DShadow uTextureShadowMap;

uniform vec3 uBaseColor;
uniform vec4 uGlow;
uniform vec3 uTintColor;
uniform vec4 uEmissiveColor;
uniform vec3 uSunPosition;
uniform vec3 uSunDirection;
uniform vec4 uSunIntensity;
uniform int uLightCount;

out vec4 color;
out vec4 bloom;

float calculateDarkening(float cosTheta, vec4 shadowCoord)
{
	float bias = 0.005 * sqrt ( 1.0f - cosTheta * cosTheta   ) / cosTheta;
	bias = clamp(bias, 0.0 ,0.01);
	shadowCoord.z -= bias;
	float darkening = 0.0;
	darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(-1,-1));
	darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(-1,1));
	darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(0,0));
	darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(1,1));
	darkening += textureProjOffset(uTextureShadowMap, shadowCoord, ivec2(1,-1));
	darkening /= 5.0;
	return darkening;
}

void main()
{
	vec3 texColor = vec3(1.0, 1.0, 1.0);
	vec4 texColor4 = vec4(1.0, 1.0, 1.0, 1.0);
	if(uUseTextureDiffuse > 0)
	{
		texColor4 = texture(uTextureDiffuse, vTexture);
		if(texColor4.w <= 0.5)
		{
			discard;
		}
		texColor = texColor4.xyz;
		if(uUseTextureLightmap > 0)
		{
			texColor *= texture(uTextureLightmap, vTexture2).xyz;
		}
	}

	vec3 theNormal = vec3(0);
	if(uUseTextureNormal > 0)
    {
            theNormal = texture(uTextureNormal, vTexture).xyz * 2.0 - 1.0;
            theNormal = vTBN * theNormal;
    }
    else
    {
            theNormal = vNormal;
    }

	color = vec4(uBaseColor * texColor, 1.0);	
}