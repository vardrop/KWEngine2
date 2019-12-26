#version 430
 
//in		vec4 vPosition;
in		vec2 vTexture;
//in		vec4 vDepthPosition;

uniform vec4        uTintColor;
uniform sampler2D   uTextureDiffuse;
uniform sampler2D   uDepthMap;

out             vec4 color;
out             vec4 bloom;
 
void main()
{
	/*  
	if (texture(uDepthMap, vDepthPosition.xy).z > vPosition.z)
	{
	   discard;
	}
	*/
	color = ((texture(uTextureDiffuse, vTexture)) * vec4(uTintColor.xyz, 1.0)) * uTintColor.w;
	bloom = color;
}