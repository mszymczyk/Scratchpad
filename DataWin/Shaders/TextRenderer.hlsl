#ifdef FX_HEADER
#ifdef FX_PASSES
passes :
{
	Text = {
		VertexProgram = "fontVp";
		FragmentProgram = "fontFp";
		//DepthTestEnable = false;
		//BlendEnable = true
		//BlendFunc = ("SrcAlpha", "OneMinusSrcAlpha")
	}

	//FontOutline = {
	//	VertexProgram = "fontVp";
	//	FragmentProgram = "fontFpOutline";
	//	DepthTestEnable = false;
	//	BlendEnable = true
	//	BlendFunc = ("SrcAlpha", "OneMinusSrcAlpha")
	//}

	//FontInv = {
	//	VertexProgram = "fontVp";
	//	FragmentProgram = "fontFpInv";
	//	DepthTestEnable = false;
	//	BlendEnable = true
	//	BlendFunc = ("SrcAlpha", "OneMinusSrcAlpha")
	//}

	//FontOutlineInv = {
	//	VertexProgram = "fontVp";
	//	FragmentProgram = "fontFpOutlineInv";
	//	DepthTestEnable = false;
	//	BlendEnable = true
	//	BlendFunc = ("SrcAlpha", "OneMinusSrcAlpha")
	//}

	//Icon = {
	//	VertexProgram = "fontVp";
	//	FragmentProgram = "iconFp";
	//	DepthTestEnable = false;
	//	BlendEnable = true
	//	BlendFunc = ("SrcAlpha", "OneMinusSrcAlpha")
	//}

	//Background :	{
	//	VertexProgram = "vs_Background";
	//	FragmentProgram = "ps_Background";
	//	DepthTestEnable = false
	//	BlendEnable = true
	//	BlendEquation = "FuncAdd"
	//	BlendFunc = ( "SrcAlpha", "OneMinusSrcAlpha" )
	//}

};
#endif // FX_PASSES
#endif // FX_HEADER

#include "TextRendererConstants.h"

//cbuffer cbMaterialParams : register( b2 )
//{
//	float4x4 transform;
//	//float4 bgCol;
//};

Texture2D fontTex : register(t0);
SamplerState fontTexSamp : register(s0);

//Texture2D iconTex;
//SamplerState iconTexSamp;

///////////////////////////////////////////////////////////////////////////////
// vertex program
///////////////////////////////////////////////////////////////////////////////
struct vs_output
{
    float4 hpos			: SV_POSITION; // vertex position 
    float2 texCoord0	: TEXCOORD0;   // vertex texture coords 
	//float4 color		: COLOR0;
};

vs_output fontVp(
					  float2 position : POSITION
					, float2 texCoord0 : TEXCOORD0
					//, float4 color : COLOR0
				)
{
	vs_output OUT;
	//OUT.hpos = mul( proj, float4(position, 0, 1) );
	float4 hpos = mul( Transform, float4( position, 0, 1 ) );
	//float4 hpos = float4( position, 0.5f, 1 );
	hpos.xy *= ViewportSize.zw;
	hpos.xy = hpos.xy * 2 - 1;
	hpos.y *= -1;

	OUT.hpos = hpos;

	float2 tc = texCoord0;
	tc *= TextureSize.zw;

	OUT.texCoord0 = tc;
	//OUT.color = color;
	return OUT;
}

///////////////////////////////////////////////////////////////////////////////
// fragment program
///////////////////////////////////////////////////////////////////////////////

float4 fontFp( in vs_output IN ) : SV_Target
{
	float t = fontTex.SampleLevel( fontTexSamp, float2(IN.texCoord0.x, IN.texCoord0.y), 0 ).x;
	float4 res = float4( t * Color );
	return res;
}

//float4 fontFpOutline( in vs_output IN ) : SV_Target
//{
//	float t = fontTex.SampleLevel( fontTexSamp, float2(IN.texCoord0.x, IN.texCoord0.y), 0 ).x;
//	float val = t;
//
//	float4 p;
//	p.xyz = val > 0.5 ? 2*val-1 : 0;
//	p.w = val > 0.5 ? 1 : 2 * val;
//
//	float4 res = float4( p * IN.color );
//	return res;
//}
//
//float4 fontFpInv( in vs_output IN ) : SV_Target
//{
//	float t = fontTex.SampleLevel( fontTexSamp, float2(IN.texCoord0.x, 1-IN.texCoord0.y), 0 ).x;
//	float4 res = float4( t * IN.color );
//	return res;
//}
//
//float4 fontFpOutlineInv( in vs_output IN ) : SV_Target
//{
//	float t = fontTex.SampleLevel( fontTexSamp, float2(IN.texCoord0.x, 1-IN.texCoord0.y), 0 ).x;
//	float val = t;
//
//	float4 p;
//	p.xyz = val > 0.5 ? 2*val-1 : 0;
//	p.w = val > 0.5 ? 1 : 2 * val;
//
//	float4 res = float4( p * IN.color );
//	return res;
//}
//
//
//
//float4 iconFp( in vs_output IN ) : SV_Target
//{
//	float4 t = iconTex.Sample( iconTexSamp, IN.texCoord0 );
//	float4 res = float4( t * IN.color );
//	//float4 res = float4( IN.texCoord0, 0, 1 );
//	return res;
//}
//
//
//
//
//void vs_Background(
//					  float4 position : POSITION
//					, out float4 hpos : SV_POSITION
//					)
//{
//	hpos = position;
//}
//
//
//
//
//float4 ps_Background() : SV_Target
//{
//	return bgCol;
//}
