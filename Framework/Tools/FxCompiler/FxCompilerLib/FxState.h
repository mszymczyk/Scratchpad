#pragma once

namespace spad
{
namespace fxlib
{

typedef unsigned int u32;


namespace FillMode
{
enum Type : unsigned char
{
	solid,
	wireframe,
	count
};
}


namespace CullMode
{
enum Type : unsigned char
{
	back,
	front,
	count
};
}


namespace FrontFace
{
enum Type : unsigned char
{
	counterClockwise,
	clockwise,
	count
};
}


namespace CompareFunc
{
enum Type : unsigned char
{
	greater,
	gEqual,
	less,
	lEqual,
	equal,
	always,
	never,
	notEqual,
	count
};
}


namespace StencilOp
{
enum Type : unsigned char
{
	keep,
	replace,
	incr,
	decr,
	invert,
	count
};
}


namespace BlendFactor
{
enum Type : unsigned char
{
	zero,
	one,
	srcAlpha,
	oneMinusSrcAlpha,
	srcColor,
	oneMinusSrcColor,
	srcAlphaSaturate,
	dstAlpha,
	oneMinusDstAlpha,
	dstColor,
	oneMinusDstColor,
	count
};
}


namespace BlendEquation
{
enum Type : unsigned char
{
	add,
	subtract,
	reverseSubtract,
	min,
	max,
	count
};
}


class BlendState
{

public:
	BlendState()
		: srcFactor( BlendFactor::one )
		, srcFactorAlpha( BlendFactor::one )
		, dstFactor( BlendFactor::zero )
		, dstFactorAlpha( BlendFactor::zero )
		, equation( BlendEquation::add )
		, blendEnabled( 0 )
		, writeMaskColorRGBA( 0xf )
	{}

	BlendState( BlendFactor::Type src, BlendFactor::Type dst )
		: srcFactor( src )
		, srcFactorAlpha( src )
		, dstFactor( dst )
		, dstFactorAlpha( dst )
		, equation( BlendEquation::add )
		, blendEnabled( 1 )
		, writeMaskColorRGBA( 0xf )
	{}

	BlendState( BlendFactor::Type src, BlendFactor::Type dst, BlendEquation::Type eq )
		: srcFactor( src )
		, srcFactorAlpha( src )
		, dstFactor( dst )
		, dstFactorAlpha( dst )
		, equation( eq )
		, blendEnabled( 1 )
		, writeMaskColorRGBA( 0xf )
	{}

	void setEquation( BlendEquation::Type eq ) { equation = eq; }

	unsigned char srcFactor : 4;
	unsigned char srcFactorAlpha : 4;

	unsigned char dstFactor : 4;
	unsigned char dstFactorAlpha : 4;

	unsigned char equation : 3;
	unsigned char blendEnabled : 1;
	unsigned char writeMaskColorRGBA : 4;

	void setSrcFactor( BlendFactor::Type src ) { srcFactor = src; srcFactorAlpha = src; }
	void setDstFactor( BlendFactor::Type dst ) { dstFactor = dst; dstFactorAlpha = dst; }
	void setSrcFactor( BlendFactor::Type src, BlendFactor::Type srcAlpha ) { srcFactor = src; srcFactorAlpha = srcAlpha; }
	void setDstFactor( BlendFactor::Type dst, BlendFactor::Type dstAlpha ) { dstFactor = dst; dstFactorAlpha = dstAlpha; }

	bool factorsEqual( const BlendState& other )
	{
		return ( srcFactor == other.srcFactor && srcFactorAlpha == other.srcFactorAlpha && dstFactor == other.dstFactor && dstFactorAlpha == other.dstFactorAlpha );
	}

};


// when modifying this class make sure there is no padding anywhere
// equals compares whole chunk of data instead of individual fields
class RenderState
{
public:

	RenderState()
	{
		fillMode = FillMode::solid;
		cullMode = CullMode::back;
		frontFace = FrontFace::counterClockwise;
		// blendState constructor called automatically
		// write mask constructor called automatically
		depthFunc = CompareFunc::lEqual;
		stencilOp = StencilOp::keep;
		stencilFunc = CompareFunc::lEqual;
		cullFaceEnabled = 1;
		depthTestEnabled = 1;
		writeDepth = 1;
		stencilTestEnabled = 0;
		blendIndependentEnabled = 0;
		alphaToCoverage = 0;
		polygonOffsetFillEnabled = 0;
		depthClipEnable = 1;
		stencilRef = 0;
		polygonOffset[0] = polygonOffset[1] = 0;

		static_assert( sizeof( RenderState ) == 28, "Size mismatch" );
	}

	u32 equals( const RenderState& other ) const
	{
		const size_t siz = sizeof( RenderState );
		static_assert( ( siz % 4 == 0 ), "RenderState size must be multiple of 4" );
		const u32* curRS = reinterpret_cast<const u32*>( this );
		const u32* newRS = reinterpret_cast<const u32*>( &other );

		const u32 nWords = sizeof( RenderState ) / 4;
		u32 cond = 0;
		for ( u32 i = 0; i < nWords; ++i )
		{
			cond |= newRS[i] != curRS[i];
		}

		return ( 1 - cond );
	}

	FillMode::Type			fillMode;
	CullMode::Type			cullMode;
	FrontFace::Type			frontFace;
	// 3
	BlendState				blendState; // blend state of all buffers or buffer 1 if blendIndependentEnabled==1
	// 6
	BlendState				blendStateIndependent[3]; // blend state of buffer 1, 2, 3 if blendIndependentEnabled==1
	// 15
	CompareFunc::Type		depthFunc;
	StencilOp::Type			stencilOp;
	CompareFunc::Type		stencilFunc;
	// 18
	unsigned char			cullFaceEnabled : 1;
	unsigned char			depthTestEnabled : 1;
	unsigned char			writeDepth : 1;
	unsigned char			stencilTestEnabled : 1;
	unsigned char			blendIndependentEnabled : 1;
	unsigned char			alphaToCoverage : 1;
	unsigned char			polygonOffsetFillEnabled : 1;
	unsigned char			depthClipEnable : 1;
	// 19
	unsigned char			stencilRef;
	// 20

	float					polygonOffset[2];
	// 28
};


const char* FillModeToString( FillMode::Type fillMode );
FillMode::Type FillModeFromString( const char* fillMode );

const char* CullModeToString( CullMode::Type cullMode );
CullMode::Type CullModeFromString( const char* cullMode );

const char* FrontFaceToString( FrontFace::Type fronFace );
FrontFace::Type FrontFaceFromString( const char* frontFace );

const char* BlendFactorToString( BlendFactor::Type blendFactor );
BlendFactor::Type BlendFactorFromString( const char* blendFactor );

const char* BlendEquationToString( BlendEquation::Type blendEquation );
BlendEquation::Type BlendEquationFromString( const char* blendEquation );

const char* CompareFuncToString( CompareFunc::Type compareFunc );
CompareFunc::Type CompareFuncFromString( const char* compareFunc );

const char* StencilOpToString( StencilOp::Type stencilOp );
StencilOp::Type StencilOpFromString( const char* stencilOp );



} // namespace fxlib
} // namespace spad
