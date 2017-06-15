#pragma once

#include "SettingsEditorConfig.h"
#include <stddef.h>
#include <stdint.h>
#include <math.h>
#include <new>

namespace SettingsEditor
{

enum e_ParamType
{
	eParamType_invalid,
	eParamType_bool,
	eParamType_int,
	eParamType_enum,
	eParamType_float,
	eParamType_floatBool,
	eParamType_float4,
	eParamType_color,
	eParamType_string,
	eParamType_direction,
	eParamType_animCurve,
	eParamType_stringArray,
	eParamType_count
};


struct FieldDescription
{
	FieldDescription()
		: name_( nullptr )
		, offset_( 0 )
		, type_( eParamType_invalid )
	{	}

	FieldDescription( const char* name, unsigned int offset, e_ParamType type )
		: name_( name )
		, offset_( offset )
		, type_( type )
	{	}

	const char* name_;
	unsigned int offset_; // offset relative to parent
	e_ParamType type_;
};


struct StructDescription
{
	const char* name_;
	const StructDescription* parentStructure_;
	const FieldDescription* fields_;
	const StructDescription* const* nestedStructures_;
	unsigned int nFields_;
	unsigned int nNestedStructures_;
	unsigned int offset_; // offset relative to parent
	unsigned int sizeInBytes_;
};


#define SETTINGS_EDITOR_STRUCT_DESC \
	protected: \
		static SettingsEditor::StructDescription __desc; \
		void* impl_ = nullptr; \
	public: \
		static const SettingsEditor::StructDescription* GetDesc() { return &__desc; }


// shader constants must have 16-byte alignment
// assuming void* is 8 byte wide:/
// in constant buffer generated for group, there will be fake float4 field added to account for impl_ and _padding_
#define SETTINGS_EDITOR_STRUCT_DESC_SHADER_CONSTANTS \
	protected: \
		static SettingsEditor::StructDescription __desc; \
		void* impl_ = nullptr; \
		void* _padding_ = nullptr; \
	public: \
		static const SettingsEditor::StructDescription* GetDesc() { return &__desc; }



struct FloatBool
{
	FloatBool( float v, bool e )
		: value( v )
		, enabled( e )
	{
		_padding_[0] = _padding_[1] = _padding_[2];
		static_assert( sizeof( FloatBool ) == 8, "Inconsistent size" );
	}

	float value;
	bool enabled;
private:
	bool _padding_[3];
};


struct Float3
{
	Float3()
		: x( 0 ), y( 0 ), z( 0 )
	{	}

	Float3( float _x, float _y, float _z )
		: x( _x ), y( _y ), z( _z )
	{	}

	float x;
	float y;
	float z;
};


struct Float4
{
	Float4()
		: x( 0 ), y( 0 ), z( 0 ), w( 0 )
	{	}

	Float4( float _x, float _y, float _z, float _w )
		: x( _x ), y( _y ), z( _z ), w( _w )
	{	}

	void set( const float f[4] )
	{
		x = f[0];	y = f[1];	z = f[2];	w = f[3];
	}

	float x;
	float y;
	float z;
	float w;
};


struct Color
{
	Color()
		: r( 0 ), g( 0 ), b( 0 )
	{	}

	Color( float _r, float _g, float _b )
		: r( _r ), g( _g ), b( _b )
	{	}

	void set( const float f[3] )
	{
		r = f[0];	g = f[1];	b = f[2];
	}

	static float maxOfPair( float a, float b ) { return (b < a) ? a : b; }
	static float minOfPair( float a, float b ) { return (a < b) ? a : b; }
	static float clamp( float value, float minimum, float maximum ) { return maxOfPair( minimum, minOfPair( value, maximum ) ); }

	unsigned int toABGR() const
	{
		uint32_t R = (uint32_t)( clamp( r, 0.0f, 1.0f ) * 255 );
		uint32_t G = (uint32_t)( clamp( g, 0.0f, 1.0f ) * 255 );
		uint32_t B = (uint32_t)( clamp( b, 0.0f, 1.0f ) * 255 );
		return 0xff000000 | ( B << 16 ) | ( G << 8 ) | ( R );
	}

	unsigned int to_argb() const
	{
		uint32_t R = (uint32_t)( clamp( r, 0.0f, 1.0f ) * 255 );
		uint32_t G = (uint32_t)( clamp( g, 0.0f, 1.0f ) * 255 );
		uint32_t B = (uint32_t)( clamp( b, 0.0f, 1.0f ) * 255 );
		return 0xff000000 | ( R << 16 ) | ( G << 8 ) | ( B );
	}

	Float3 toFloat3() const
	{
		return Float3( r, g, b );
	}

	Float3 toFloat3Gamma() const
	{
		return Float3( powf( r, 2.2f ), powf( g, 2.2f ), powf( b, 2.2f ) );
	}

	float r;
	float g;
	float b;
};


struct Direction
{
	Direction()
		: x( 0 ), y( 0 ), z( 1 )
	{	}

	Direction( float _x, float _y, float _z )
		: x( _x ), y( _y ), z( _z )
	{	}

	void set( const float f[3] )
	{
		x = f[0];	y = f[1];	z = f[2];
	}

	float x;
	float y;
	float z;
};


struct AnimCurve
{
public:

	float eval( float x ) const;

private:
	const void* impl_ = nullptr;
};

struct StringArray
{
public:
	size_t size() const;
	const char* c_str( size_t index ) const;
	size_t length( size_t index ) const;

private:
	const void* impl_ = nullptr;
};


class SettingsFile
{
public:
	bool isValid() const { return impl_ != nullptr; }

private:
	const void* impl_ = nullptr;
};

SettingsFile createSettingsFile( const char* settingsFilename, const StructDescription* rootStructDescription, const void* settingsBaseAddress[] );
void releaseSettingsFile( SettingsFile& configFile );




namespace _internal
{
typedef void*(*AllocFunc)	(size_t size, size_t align);
typedef void*(*FreeFunc	)	(void * p);
typedef int  (*ReadFile	)	( const char* filename, unsigned char** fileBuf, size_t* fileBufSize );
typedef void (*FreeFile	)	( const char* filename, unsigned char* fileBuf, size_t fileBufSize );
typedef void (*LogFunc	)	( const char* msg );

struct StartUpParam
{
	AllocFunc	allocFunc = nullptr;
	FreeFunc	freeFunc = nullptr;
	ReadFile	readFile = nullptr;
	FreeFile	freeFile = nullptr;
	LogFunc		logInfo = nullptr;
	LogFunc		logWarning = nullptr;
	LogFunc		logError = nullptr;
};

int startUp( const StartUpParam& param );
void shutDown();
void update();

const void* getPreset( const char* presetName, const void* impl );
float evaluateAnimCurve( const void* curve, float time );

size_t stringArraySize( const void* impl );
const char* stringArrayString( const void* impl, size_t index );
size_t stringArrayStringLength( const void* impl, size_t index );


void* allocGroup( size_t size, size_t alignment ); // clears memory to all zeros
void freeGroup( void* p );

#ifndef SETTINGS_EDITOR_FROZEN
void updateParam( const char* configFile, const char* groupName, const char* presetName, const char* paramName, const int* newValues, int nNewValues );
void updateParam( const char* configFile, const char* groupName, const char* presetName, const char* paramName, const float* newValues, int nNewValues );
void updateParam( const char* configFile, const char* groupName, const char* presetName, const char* paramName, const char* newVal );
#endif //
} // namespace _internal



inline float AnimCurve::eval( float x ) const
{
	if (impl_)
		return _internal::evaluateAnimCurve( impl_, x );
	else
		return 0.0f;
}

inline size_t StringArray::size() const
{
	return _internal::stringArraySize( impl_ );
}

inline const char* StringArray::c_str( size_t index ) const
{
	return _internal::stringArrayString( impl_, index );
}

inline size_t StringArray::length( size_t index ) const
{
	return _internal::stringArrayStringLength( impl_, index );
}

} // namespace SettingsEditor
