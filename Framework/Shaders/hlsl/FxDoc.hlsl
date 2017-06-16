// This file presents all features supported by FxCompiler and serves as a kind of unit test (there are no fail cases)
// Default state will be not stored to output compiled file

#ifdef FX_HEADER
#ifdef FX_PASSES
passes :
{
	PassBasic1 = {
		VertexProgram = "vs_ScreenQuad"
		FragmentProgram = "ps_Color"
	}

	PassBasic2 = {
		VertexProgram = "vs_ScreenQuad"
		// fragment program is not required in all cases, ie shadow/depth rendering
	}

	PassAllState = {
		// each state has it's default value set, possible values are listed in comments

		BlendEnable = false;
		BlendEquation = "FuncAdd"; // FuncAdd, FuncSubtract, FuncReverseSubtract, Min, Max
		// BlendFunc or BlendFuncSeparate is a list of 2 and 4 values respectively
		// declaring both BlendFunc and BlendFuncSeparate is an error
		// src, dst or srcColor, srcAlpha, dstColor, dstAlpha
		// each setting can be one of: Zero, One, SrcAlpha, OneMinusSrcAlpha, SrcColor, OneMinusSrcColor, SrcAlphaSaturate, DstAlpha, OneMinusDstAlpha, DstColor, OneMinusDstColor
		BlendFunc = ( "One", "Zero" );
		// BlendFuncSeparate = ( "One", "One", "Zero", "Zero" );

		AlphaToCoverageEnable = false;

		DepthTestEnable = true;
		DepthMask = true; // depth write
		// compare function is used in DepthFunc and StencilFunc, it can be one of:
		// Greater, GEqual, Less, LEqual, Equal, Always, Never, NotEqual
		DepthFunc = "LEqual";
		DepthClipEnable = false;

		StencilTestEnable = false;
		StencilFunc = "LEqual";
		StencilOp = "Keep"; // Keep, Replace, Incr, Decr, Invert
		StencilRef = 0; // integer values in range <0,255>
		
		ColorMask = ( true, true, true, true ); // color write for all render targets: red, green, blue, alpha

		CullFaceEnable = true;
		CullFace = "Back"; // Front or Back
		FrontFace = "CCW"; // CCW - counterclockwise, CW - clockwise
		PolygonMode = "Fill"; // Fill, Line
		PolygonOffsetEnable = false;
		PolygonOffset = ( 0, 0 ) // two floats, (SlopeScaledDepthBias, DepthBias), see https://msdn.microsoft.com/en-us/library/windows/desktop/cc308048(v=vs.85).aspx

		VertexProgram = "vs_ScreenQuad"
		FragmentProgram = "ps_Color"
	}

	PassWithCombinations = {
		VertexProgram = {
			// entry name must come first
			EntryName = "vs_ScreenQuad2";
			cdefines = {
				SHIFT = ( "0", "1" );
			}
		}
		FragmentProgram = {
			EntryName = "ps_Color2"
			cdefines = {
				MSAA = ( "1", "2", "4" )
				CLAMP = ( "0", "1" )
			}
		}
	}

	PassWithRegularDefinesAndCombinations = {
		VertexProgram = {
			// entry name must come first
			EntryName = "vs_ScreenQuad2";
			cdefines = {
				SCALE = "1.5f" // treated as a regular define, doesn't affect number of combinations, can't be indexed when choosing pass at runtime
				SHIFT = ( "0", "1" );
			}
		}

		FragmentProgram = {
			EntryName = "ps_Color2"
			cdefines = {
				CLAMP = "0" // treated as a regular define, doesn't affect number of combinations, can't be indexed when choosing pass at runtime
				MSAA = ( "1", "2", "4" )
			}
		}
	}

	PassComplex = {
		VertexProgram = {
			EntryName = "vs_ScreenQuad2";
			cdefines = {
				SHIFT = ( "0", "1" );
			}
		}
		FragmentProgram = {
			EntryName = "ps_Color2"
			cflags_hlsl = "--O2";
			cflags_pssl = "--fastmath"
			cflags_glsl = "--fake"
			cdefines = {
				MSAA = ( "1", "2", "4" )
			}
		}
	}
};
#endif // FX_PASSES
#endif // FX_HEADER

cbuffer cbMaterialParams : register( b2 )
{
	float4 color;
};

struct vs_output_ScreenQuad
{
	float4 hpos			: SV_POSITION;
	float2 windowPos01	: TEXCOORD0;
	float2 texCoord01	: TEXCOORD1;
};

vs_output_ScreenQuad vs_ScreenQuad(
	float2 position : POSITION
	, float2 texCoord0 : TEXCOORD0
	)
{
	vs_output_ScreenQuad OUT;
	OUT.hpos = float4( position.xy, 0, 1 );
	OUT.texCoord01 = texCoord0;
	OUT.windowPos01 = float2( texCoord0.x, 1 - texCoord0.y );
	return OUT;
}

float4 ps_Color( in vs_output_ScreenQuad IN ) : SV_Target
{
	UNREFERENCED( IN );
	return color;
}


vs_output_ScreenQuad vs_ScreenQuad2(
float2 position : POSITION
, float2 texCoord0 : TEXCOORD0
)
{
	vs_output_ScreenQuad OUT;
	OUT.hpos = float4( position.xy, 0, 1 );
	OUT.texCoord01 = texCoord0;
	OUT.windowPos01 = float2( texCoord0.x, 1 - texCoord0.y );
	return OUT;
}

float4 ps_Color2( in vs_output_ScreenQuad IN ) : SV_Target
{
	UNREFERENCED( IN );
	return color;
}
