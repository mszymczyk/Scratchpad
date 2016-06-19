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
float Curve;
}



// textures, buffers
//
Texture2D Texture2D_tex;Texture2D Texture2DParameter_tex;

// samplers
//
SamplerState Texture2D_samp;



// user functions
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
float Curve_R;
float Swizzle_R;
float2 Texture2D_UV;
float4 Texture2D_RGBA;
float Swizzle_G;
float2 Texture2DParameter_UV;
float4 Texture2DParameter_RGBA;
float Swizzle_B;
float Swizzle_A;
float4 Swizzle_RGBA;
float3 Material_BaseColor;


Curve_R = Curve;
Swizzle_R = Curve_R.x;
Texture2D_UV = mp.texCoord0.xy;
Texture2D_RGBA = Texture2D_tex.Sample( Texture2D_samp, Texture2D_UV );
Swizzle_G = Texture2D_RGBA.y;
Texture2DParameter_UV = mp.texCoord0.xy;
Texture2DParameter_RGBA = Texture2DParameter_tex.Sample( Texture2D_samp, Texture2DParameter_UV );
Swizzle_B = Texture2DParameter_RGBA.z;
Swizzle_A = 0;
Swizzle_RGBA = float4( Swizzle_R, Swizzle_G, Swizzle_B, Swizzle_A );
Material_BaseColor = Swizzle_RGBA.xyz;

baseColor = Material_BaseColor;



	color0 = float4( baseColor, 1 );
}
