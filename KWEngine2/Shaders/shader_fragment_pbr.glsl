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

uniform sampler2D uTextureRoughness;
uniform int uUseTextureRoughness;

uniform sampler2D uTextureMetallic;
uniform int uUseTextureMetallic; 

uniform float uRoughness;
uniform float uMetalness;

uniform sampler2D uTextureEmissive;
uniform int uUseTextureEmissive;

uniform sampler2D uTextureLightmap;
uniform int uUseTextureLightmap;

uniform sampler2DShadow uTextureShadowMap;

uniform float uOpacity;
uniform vec3 uBaseColor;
uniform vec4 uGlow;
uniform vec3 uTintColor;
uniform vec4 uEmissiveColor;
uniform float uSunAmbient;
uniform int uSunAffection;
uniform vec3 uSunPosition;
uniform vec3 uSunDirection; // to sun!
uniform vec4 uSunIntensity;
uniform int uLightAffection;

uniform vec3 uCameraPos;
uniform vec3 uCameraDirection;

uniform float uSpecularArea;
uniform float uSpecularPower;

uniform float uBiasCoefficient;

uniform vec4 uLightsPositions[10];
uniform vec4 uLightsTargets[10];
uniform vec4 uLightsColors[10];
uniform int uLightCount;

uniform vec4 uMaterial;

out vec4 color;
out vec4 bloom;

#define PI 3.1415926

float calculateDarkening(float cosTheta, vec4 shadowCoord)
{
	float bias = uBiasCoefficient * sqrt ( 1.0f - cosTheta * cosTheta   ) / cosTheta;
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

float saturate(in float value)
{
    return clamp(value, 0.0, 1.0);
}


// phong (lambertian) diffuse term
float phong_diffuse()
{
    return (1.0 / PI);
}


// compute fresnel specular factor for given base specular and product
// product could be NdV or VdH depending on used technique
vec3 fresnel_factor(in vec3 f0, in float product)
{
    return mix(f0, vec3(1.0), pow(1.01 - product, 5.0));
}

float D_blinn(in float roughness, in float NdH)
{
    float m = roughness * roughness;
    float m2 = m * m;
    float n = 2.0 / m2 - 2.0;
    return (n + 2.0) / (2.0 * PI) * pow(NdH, n);
}

float D_beckmann(in float roughness, in float NdH)
{
    float m = roughness * roughness;
    float m2 = m * m;
    float NdH2 = NdH * NdH;
    return exp((NdH2 - 1.0) / (m2 * NdH2)) / (PI * m2 * NdH2 * NdH2);
}

float D_GGX(in float roughness, in float NdH)
{
    float m = roughness * roughness;
    float m2 = m * m;
    float d = (NdH * m2 - NdH) * NdH + 1.0;
    return m2 / (PI * d * d);
}

float G_schlick(in float roughness, in float NdV, in float NdL)
{
    float k = roughness * roughness * 0.5;
    float V = NdV * (1.0 - k) + k;
    float L = NdL * (1.0 - k) + k;
    return 0.25 / (V * L);
}

vec3 phong_specular(in vec3 V, in vec3 L, in vec3 N, in vec3 specular, in float roughness)
{
    vec3 R = reflect(-L, N);
    float spec = max(0.0, dot(V, R));
    float k = 1.999 / (roughness * roughness);
    return min(1.0, 3.0 * 0.0398 * k) * pow(spec, min(10000.0, k)) * specular;
}

// simple blinn specular calculation with normalization
vec3 blinn_specular(in float NdH, in vec3 specular, in float roughness)
{
    float k = 1.999 / (roughness * roughness);
    return min(1.0, 3.0 * 0.0398 * k) * pow(NdH, min(10000.0, k)) * specular;
}

// cook-torrance specular calculation                      
vec3 cooktorrance_specular(in float NdL, in float NdV, in float NdH, in vec3 specular, in float roughness)
{
    float D = D_GGX(roughness, NdH);
    float G = G_schlick(roughness, NdV, NdL);
    float rim = mix(1.0 - roughness * uMaterial.w * 0.9, 1.0, NdV);
    return (1.0 / rim) * specular * G * D;
}

void main()
{
	// Texture mapping:
	vec3 texColor = uBaseColor;
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
            theNormal = normalize(texture(uTextureNormal, vTexture).xyz * 2.0 - 1.0);
            theNormal = normalize(vTBN * theNormal);
    }
    else
    {
            theNormal = vNormal;
    }

	vec3 surfaceToCamera = normalize(uCameraPos - vPosition);
	vec3 fragmentToSun = normalize(uSunPosition - vPosition);

	// shadow mapping:
	float dotNormalLight = max(dot(theNormal, uSunDirection), 0.0);								
	float dotNormalLightShadow = max(dot(theNormal, fragmentToSun), 0.0);
	float darkeningAbsolute = max(calculateDarkening(dotNormalLightShadow, vShadowCoord), 0.0);
	float darkening = max(darkeningAbsolute, uSunAmbient);
	
	
	// check if camera may see highlights:
	float dotSunNormalInverted = max(0.0, dot(-theNormal, -uSunDirection));

	// calculate ambient light:
	vec3 ambient = vec3(0.0);
	if(uSunAffection > 0)
	{
		ambient = uSunIntensity.xyz * (uSunIntensity.w * min(max(dotNormalLight, uSunAmbient), darkening));
	}
	else
	{
		ambient = vec3(uSunAmbient);
	}

	vec3 emissive = vec3(0.0);
	if(uUseTextureEmissive > 0)
	{
		emissive = texture(uTextureEmissive, vTexture).xyz;
	}
	else
	{
		emissive = uEmissiveColor.xyz * uEmissiveColor.w;
	}
	ambient += emissive;

	vec3 colorComponentTotal = vec3(0.0);
	if(uLightAffection > 0)
	{
		for(int i = 0; i < uLightCount; i++)
		{
			vec3 lightPos = uLightsPositions[i].xyz;
			vec3 lightColor = uLightsColors[i].xyz;
			vec3 lightDirection = normalize(uLightsTargets[i].xyz - lightPos); // light pos to light target

			vec3 lightVector = lightPos - vPosition; // fragment to light
			float distance = dot(lightVector, lightVector);
			lightVector = normalize(lightVector);
			float distanceFactor = (uLightsPositions[i].w * 10 / distance);

			//check if camera may see highlights:
			float dotLightNormalInverted;
			// directional light falloff:
			float differenceLightDirectionAndFragmentDirection = 1.0;
			if(uLightsTargets[i].w > 0.0){ // directional
				differenceLightDirectionAndFragmentDirection = max(dot(lightDirection, -lightVector), 0.0);
				dotLightNormalInverted = max(dot(lightDirection, -theNormal), 0.0) * clamp(8.0 * distanceFactor, 0.0, 1.0);
			}
			else
			{
				dotLightNormalInverted = clamp(8.0 * distanceFactor, 0.0, 1.0);
			}

			// Normal light affection:
			float dotProduct = max(dot(theNormal, lightVector), 0.0);
			float dotProductNormalLight = dotProduct * distanceFactor * differenceLightDirectionAndFragmentDirection;

			colorComponentTotal += lightColor * dotProductNormalLight * uLightsColors[i].w * pow(differenceLightDirectionAndFragmentDirection, 5.0);
		}
	}

	vec3 finalColor =(colorComponentTotal + ambient) * uBaseColor.xyz * uTintColor.xyz * texColor;
	
    color.x = finalColor.x;
	color.y = finalColor.y;
	color.z = finalColor.z;
	color.w = uOpacity;

	vec3 addedBloom = vec3(max(finalColor.x - 1.0, 0.0), max(finalColor.y - 1.0, 0.0), max(finalColor.z - 1.0, 0.0)) + (uEmissiveColor.xyz * 0.5);
	bloom.x = addedBloom.x + uGlow.x * uGlow.w;
	bloom.y = addedBloom.y + uGlow.y * uGlow.w;
	bloom.z = addedBloom.z + uGlow.z * uGlow.w;
	bloom.w = 1.0;
}