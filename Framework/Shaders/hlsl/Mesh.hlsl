#ifdef FX_HEADER
#ifdef FX_PASSES
passes :
{
	Wireframe = {
		VertexProgram = "WireframeVp";
		FragmentProgram = "WireframeFp";
	}
};
#endif // FX_PASSES
#endif // FX_HEADER

#include "PassConstants.h"

Texture2D diffuseTex;
SamplerState diffuseTexSamp;

///////////////////////////////////////////////////////////////////////////////
// vertex program
///////////////////////////////////////////////////////////////////////////////
struct vs_output
{
    float4 hpos			: SV_POSITION; // vertex position in clip space
	float3 normalWS		: NORMAL;      // vertex normal in world space
	float2 texCoord0	: TEXCOORD0;   // vertex texture coords 
};

vs_output WireframeVp(
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

float4 WireframeFp( in vs_output IN ) : SV_Target
{
	//return float4( 1, 0, 1, 1 );
	return float4( diffuseTex.Sample( diffuseTexSamp, IN.texCoord0 ).xyz, 1 );
}
