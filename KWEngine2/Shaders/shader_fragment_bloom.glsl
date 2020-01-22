#version 430

in		vec2 vTexture;

uniform sampler2D uTextureScene;
uniform sampler2D uTextureBloom;
uniform int uMerge;
uniform int uHorizontal;
uniform vec2 uResolution;

out		vec4 color;

//float weight[11] = float[] (0.4, 0.2, 0.10, 0.075, 0.05, 0.03, 0.01 , 0.0075, 0.005, 0.0025, 0.001 );
float weight[8] = float[] (0.4, 0.2, 0.10, 0.075, 0.05, 0.03, 0.01 , 0.0075 );


void main()
{
	ivec2 size = textureSize(uTextureBloom, 0);
	vec3 result = texture(uTextureBloom, vTexture).rgb * weight[0];
    
	if(uHorizontal > 0)
    {
	
        for(int i = 1; i < 8; i++)
        {
            result += texture(uTextureBloom, vTexture + vec2((float(i) * (uResolution.x * float(i))), 0.0)).rgb * (weight[i]);
            result += texture(uTextureBloom, vTexture - vec2((float(i) * (uResolution.x * float(i))), 0.0)).rgb * (weight[i]);
        }
		color.x = result.x;
		color.y = result.y;
		color.z = result.z;
		color.w = 1.0;
	}
    else
    {
	
        for(int i = 1; i < 8; i++)
        {
            result += texture(uTextureBloom, vTexture + vec2(0.0, (float(i) * (uResolution.y * float(i))))).rgb * (weight[i]);
            result += texture(uTextureBloom, vTexture - vec2(0.0, (float(i) * (uResolution.y * float(i))))).rgb * (weight[i]);
        }	

		if(uMerge > 0){
			result += texture(uTextureScene, vTexture).rgb;
		}

		color.x = result.x;
		color.y = result.y;
		color.z = result.z;
		color.w = 1.0;
	}
}