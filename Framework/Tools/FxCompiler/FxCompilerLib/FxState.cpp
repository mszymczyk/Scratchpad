#include "FxState.h"
#include <string.h>

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{
namespace fxlib
{



const char* FillModeToString( FillMode::Type fillMode )
{
	if (		fillMode == FillMode::solid )				return "Fill";
	else if (	fillMode == FillMode::wireframe )			return "Line";
	else
		return nullptr;
}

FillMode::Type FillModeFromString( const char* fillMode )
{
	if (		0 == strcmp( fillMode, "Fill" ) )			return FillMode::solid;
	else if (	0 == strcmp( fillMode, "Line" ) )			return FillMode::wireframe;
	else
		return FillMode::count;
}

const char* CullModeToString( CullMode::Type cullMode )
{
	if (		cullMode == CullMode::front )				return "Front";
	else if (	cullMode == CullMode::back )				return "Back";
	else
		return nullptr;
}

CullMode::Type CullModeFromString( const char* cullMode )
{
	if (		0 == strcmp( cullMode, "Front" ) )				return CullMode::front;
	else if (	0 == strcmp( cullMode, "Back" ) )				return CullMode::back;
	else
		return CullMode::count;
}

const char* FrontFaceToString( FrontFace::Type frontFace )
{
	if (		frontFace == FrontFace::counterClockwise )		return "CCW";
	else if (	frontFace == FrontFace::clockwise )				return "CW";
	else
		return nullptr;
}

FrontFace::Type FrontFaceFromString( const char* frontFace )
{
	if (		0 == strcmp( frontFace, "CCW" ) )				return FrontFace::counterClockwise;
	else if (	0 == strcmp( frontFace, "CW" ) )				return FrontFace::clockwise;
	else
		return FrontFace::count;
}

const char* BlendFactorToString( BlendFactor::Type blendFactor )
{
	switch ( blendFactor )
	{
	case spad::fxlib::BlendFactor::zero:					return "Zero";
	case spad::fxlib::BlendFactor::one:						return "One";
	case spad::fxlib::BlendFactor::srcAlpha:				return "SrcAlpha";
	case spad::fxlib::BlendFactor::oneMinusSrcAlpha:		return "OneMinusSrcAlpha";
	case spad::fxlib::BlendFactor::srcColor:				return "SrcColor";
	case spad::fxlib::BlendFactor::oneMinusSrcColor:		return "OneMinusSrcColor";
	case spad::fxlib::BlendFactor::srcAlphaSaturate:		return "SrcAlphaSaturate";
	case spad::fxlib::BlendFactor::dstAlpha:				return "DstAlpha";
	case spad::fxlib::BlendFactor::oneMinusDstAlpha:		return "OneMinusDstAlpha";
	case spad::fxlib::BlendFactor::dstColor:				return "DstColor";
	case spad::fxlib::BlendFactor::oneMinusDstColor:		return "OneMinusDstColor";
	default:
		return nullptr;
	}
}

BlendFactor::Type BlendFactorFromString( const char* blendFactor )
{
	if (		0 == strcmp( blendFactor, "Zero" ) )				return BlendFactor::zero;
	else if (	0 == strcmp( blendFactor, "One" ) )					return BlendFactor::one;
	else if (	0 == strcmp( blendFactor, "SrcAlpha" ) )			return BlendFactor::srcAlpha;
	else if (	0 == strcmp( blendFactor, "OneMinusSrcAlpha" ) )	return BlendFactor::oneMinusSrcAlpha;
	else if (	0 == strcmp( blendFactor, "SrcColor" ) )			return BlendFactor::srcColor;
	else if (	0 == strcmp( blendFactor, "OneMinusSrcColor" ) )	return BlendFactor::oneMinusSrcColor;
	else if (	0 == strcmp( blendFactor, "SrcAlphaSaturate" ) )	return BlendFactor::srcAlphaSaturate;
	else if (	0 == strcmp( blendFactor, "DstAlpha" ) )			return BlendFactor::dstAlpha;
	else if (	0 == strcmp( blendFactor, "OneMinusDstAlpha" ) )	return BlendFactor::oneMinusDstAlpha;
	else if (	0 == strcmp( blendFactor, "DstColor" ) )			return BlendFactor::dstColor;
	else if (	0 == strcmp( blendFactor, "OneMinusDstColor" ) )	return BlendFactor::oneMinusDstColor;
	else
		return BlendFactor::count;
}

const char* BlendEquationToString( BlendEquation::Type blendEquation )
{
	switch ( blendEquation )
	{
	case spad::fxlib::BlendEquation::add:					return "FuncAdd";
	case spad::fxlib::BlendEquation::subtract:				return "FuncSubtract";
	case spad::fxlib::BlendEquation::reverseSubtract:		return "FuncReverseSubtract";
	case spad::fxlib::BlendEquation::min:					return "Min";
	case spad::fxlib::BlendEquation::max:					return "Max";
	default:
		return nullptr;
	}
}

BlendEquation::Type BlendEquationFromString( const char* fillMode )
{
	if ( 0 == strcmp( fillMode, "FuncAdd" ) )					return BlendEquation::add;
	else if ( 0 == strcmp( fillMode, "FuncSubtract" ) )			return BlendEquation::subtract;
	else if ( 0 == strcmp( fillMode, "FuncReverseSubtract" ) )	return BlendEquation::reverseSubtract;
	else if ( 0 == strcmp( fillMode, "Min" ) )					return BlendEquation::min;
	else if ( 0 == strcmp( fillMode, "Max" ) )					return BlendEquation::max;
	else
		return BlendEquation::count;
}

const char* CompareFuncToString( CompareFunc::Type compareFunc )
{
	switch ( compareFunc )
	{
	case spad::fxlib::CompareFunc::greater:		return "Greater";
	case spad::fxlib::CompareFunc::gEqual:		return "GEqual";
	case spad::fxlib::CompareFunc::less:		return "Less";
	case spad::fxlib::CompareFunc::lEqual:		return "LEqual";
	case spad::fxlib::CompareFunc::equal:		return "Equal";
	case spad::fxlib::CompareFunc::always:		return "Always";
	case spad::fxlib::CompareFunc::never:		return "Never";
	case spad::fxlib::CompareFunc::notEqual:	return "NotEqual";
	default:
		return nullptr;
	}
}

CompareFunc::Type CompareFuncFromString( const char* compareFunc )
{
	if (		0 == strcmp( compareFunc, "Greater" ) )	return CompareFunc::greater;
	else if (	0 == strcmp( compareFunc, "GEqual" ) )	return CompareFunc::gEqual;
	else if (	0 == strcmp( compareFunc, "Less" ) )	return CompareFunc::less;
	else if (	0 == strcmp( compareFunc, "LEqual" ) )	return CompareFunc::lEqual;
	else if (	0 == strcmp( compareFunc, "Equal" ) )	return CompareFunc::equal;
	else if (	0 == strcmp( compareFunc, "Always" ) )	return CompareFunc::always;
	else if (	0 == strcmp( compareFunc, "Never" ) )	return CompareFunc::never;
	else
		return CompareFunc::count;
}

const char* StencilOpToString( StencilOp::Type stencilOp )
{
	switch ( stencilOp )
	{
	case spad::fxlib::StencilOp::keep:			return "Keep";
	case spad::fxlib::StencilOp::replace:		return "Replace";
	case spad::fxlib::StencilOp::incr:			return "Incr";
	case spad::fxlib::StencilOp::decr:			return "Decr";
	case spad::fxlib::StencilOp::invert:		return "Invert";
	default:
		return nullptr;
	}
}

StencilOp::Type StencilOpFromString( const char* stencilOp )
{
	if (		0 == strcmp( stencilOp, "Keep" ) )		return StencilOp::keep;
	else if (	0 == strcmp( stencilOp, "Replace" ) )	return StencilOp::replace;
	else if (	0 == strcmp( stencilOp, "Incr" ) )		return StencilOp::incr;
	else if (	0 == strcmp( stencilOp, "Decr" ) )		return StencilOp::decr;
	else if (	0 == strcmp( stencilOp, "Invert" ) )	return StencilOp::invert;
	else
		return StencilOp::count;
}



} // namespace fxlib
} // namespace spad
