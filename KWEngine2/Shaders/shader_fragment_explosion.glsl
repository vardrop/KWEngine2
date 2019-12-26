#version 430

in		vec3 vPosition;
in		vec2 vTexture;

uniform sampler2D uTextureDiffuse;
uniform int uUseTextureDiffuse;
uniform vec4 uGlow;
uniform float uSunAmbient;

out vec4 color;
out vec4 bloom;

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
	}
	else
	{
		texColor = vec3(uSunAmbient);
	}
	
    color.x = texColor.x;
	color.y = texColor.y;
	color.z = texColor.z;
	color.w = 1.0;


	bloom.x = uGlow.x * uGlow.w;
	bloom.y = uGlow.y * uGlow.w;
	bloom.z = uGlow.z * uGlow.w;
	bloom.w = 1.0;
}