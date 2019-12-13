#version 430

in		vec3 vPosition;
in		vec3 vNormal;
in		vec2 vTexture;
in		vec2 vTexture2;
in		mat3 vTBN;
in		vec4 vShadowCoord;

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
uniform float uSunAmbient;
uniform vec3 uSunPosition;
uniform vec3 uSunDirection;
uniform vec4 uSunIntensity;
uniform vec3 uCameraPos;
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
	// Texture mapping:
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

	// Normal mapping:
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

	// Shadow mapping:
	vec3 surfaceToCamera = normalize(uCameraPos - vPosition);
	vec3 fragmentToSun = normalize(uSunPosition - vPosition);
	float dotNormalLight = max(dot(theNormal, uSunDirection), 0.0);
	float dotNormalLightShadow = max(dot(theNormal, fragmentToSun), 0.0);
	float darkeningAbsolute = calculateDarkening(dotNormalLightShadow, vShadowCoord);
	float darkening = max(darkeningAbsolute, uSunAmbient);

	color = vec4(uBaseColor * texColor, 1.0) * darkening;	
	color.w = 1.0;
}