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
// nothing declared


// textures, buffers
//
Texture2D Texture2DSample_tex;

// samplers
//
SamplerState Texture2DSample_samp;



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

	float3 baseColor = float3( 1, 1, 1 );

// graph eval
//
float2 Texture2DSample_UV;
float4 Texture2DSample_RGBA;
float ColorSwizzle_R;
float3 ColorConstant_RGB;
float ColorSwizzle_G;
float ColorSwizzle_B;
float3 ColorSwizzle_RGB;
float3 Material_BaseColor;


Texture2DSample_UV = mp.texCoord0.xy;
Texture2DSample_RGBA = Texture2DSample_tex.Sample( Texture2DSample_samp, Texture2DSample_UV );
ColorSwizzle_R = Texture2DSample_RGBA.x;
ColorConstant_RGB = float3( 0.8784314, 0.1058824, 0.1058824 );
ColorSwizzle_G = ColorConstant_RGB.y;
ColorSwizzle_B = ColorConstant_RGB.z;
ColorSwizzle_RGB = float3( ColorSwizzle_R, ColorSwizzle_G, ColorSwizzle_B );
Material_BaseColor = ColorSwizzle_RGB;

baseColor = Material_BaseColor;



	color0 = float4( baseColor, 1 );
}
