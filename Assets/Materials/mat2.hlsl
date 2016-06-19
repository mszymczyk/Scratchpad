#ifdef FX_HEADER

#ifdef FX_PASSES
passes :
{
	Pass0 = {
		VertexProgram = "MainVp";
		FragmentProgram = "MainFp";
	}
};
#endif // FX_PASSES
#endif // FX_HEADER

#include "PassConstants.h"

struct MaterialParameters
{
	float2 texCoord0;
};

// uniforms
//
cbuffer MaterialParams : register(b2)
{
float3 ColorParameter;
}



// textures, buffers
//
// samplers
//


///////////////////////////////////////////////////////////////////////////////
// vertex program
///////////////////////////////////////////////////////////////////////////////
struct vs_output
{
    float4 hpos			: SV_POSITION; // vertex position in clip space
	float3 normalWS		: NORMAL;      // vertex normal in world space
	float2 texCoord0	: TEXCOORD0;   // vertex texture coords 
};

vs_output MainVp(
					  float3 position : POSITION
					, float3 normal : NORMAL
					, float2 texCoord0 : TEXCOORD0
				)
{
	vs_output OUT;

	float4 positionWorld = mul( World, float4( position, 1 ) );

	OUT.hpos = mul( ViewProjection, positionWorld );
	OUT.normalWS = mul( ( float3x3 )WorldIT, normal );
	OUT.texCoord0 = texCoord0;

	return OUT;
}

///////////////////////////////////////////////////////////////////////////////
// fragment program
///////////////////////////////////////////////////////////////////////////////

void MainFp( in vs_output IN,
		out float4 color0 : SV_Target
	)
{
	MaterialParameters mp = (MaterialParameters)0;
	mp.texCoord0 = IN.texCoord0;

	float3 baseColor = float3( 1, 1, 1 );

// graph eval
//
float3 ColorParameter_RGB;
float ColorSwizzle_R;
float4 Lerp_RGBA_1;
float4 Lerp_RGBA_2;
float Lerp_T;
float4 Lerp_RGBA;
float ColorSwizzle_G;
float ColorSwizzle_B;
float3 ColorSwizzle_RGB;
float3 Material_BaseColor;


ColorParameter_RGB = ColorParameter;
ColorSwizzle_R = ColorParameter_RGB.x;
Lerp_RGBA_1 = float4( 0, 0, 0, 0 );
Lerp_RGBA_2 = float4( 1, 1, 1, 1 );
Lerp_T = 0.5f;
Lerp_RGBA = lerp( Lerp_RGBA_1, Lerp_RGBA_2, Lerp_T );
ColorSwizzle_G = Lerp_RGBA.y;
ColorSwizzle_B = 0;
ColorSwizzle_RGB = float3( ColorSwizzle_R, ColorSwizzle_G, ColorSwizzle_B );
Material_BaseColor = ColorSwizzle_RGB.xyz;

baseColor = Material_BaseColor;



	color0 = float4( baseColor, 1 );
}
