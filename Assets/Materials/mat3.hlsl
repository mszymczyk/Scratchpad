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
float FloatParameter;
}



// textures, buffers
//
Texture2D Texture2DParameter_tex;

// samplers
//
SamplerState Texture2DParameter_samp;



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
float4 Lerp_RGBA_1;
float2 Texture2DParameter_UV;
float4 Texture2DParameter_RGBA;
float4 Lerp_RGBA_2;
float FloatParameter_R;
float Lerp_T;
float4 Lerp_RGBA;
float4 Power_Base;
float4 Power_Exponent;
float4 Power_RGBA;
float3 Material_BaseColor;


ColorParameter_RGB = ColorParameter;
Lerp_RGBA_1 = float4( ColorParameter_RGB.xyz, 0 );
Texture2DParameter_UV = mp.texCoord0.xy;
Texture2DParameter_RGBA = Texture2DParameter_tex.Sample( Texture2DParameter_samp, Texture2DParameter_UV );
Lerp_RGBA_2 = Texture2DParameter_RGBA.xyzw;
FloatParameter_R = FloatParameter;
Lerp_T = FloatParameter_R.x;
Lerp_RGBA = lerp( Lerp_RGBA_1, Lerp_RGBA_2, Lerp_T );
Power_Base = Lerp_RGBA.xyzw;
Power_Exponent = float4( FloatParameter_R.xxxx );
Power_RGBA = pow( Power_Base, Power_Exponent );
Material_BaseColor = Power_RGBA.xyz;

baseColor = Material_BaseColor;



	color0 = float4( baseColor, 1 );
}
