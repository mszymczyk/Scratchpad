#pragma once

#include "SettingsEditorHelpers.h"

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
	private: \
		static SettingsEditor::StructDescription __desc; \
		void* impl_ = nullptr; \
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

	unsigned int toABGR() const
	{
		u32 R = (u32)( clamp( r, 0.0f, 1.0f ) * 255 );
		u32 G = (u32)( clamp( g, 0.0f, 1.0f ) * 255 );
		u32 B = (u32)( clamp( b, 0.0f, 1.0f ) * 255 );
		return 0xff000000 | ( B << 16 ) | ( G << 8 ) | ( R );
	}

	unsigned int to_argb() const
	{
		u32 R = (u32)( clamp( r, 0.0f, 1.0f ) * 255 );
		u32 G = (u32)( clamp( g, 0.0f, 1.0f ) * 255 );
		u32 B = (u32)( clamp( b, 0.0f, 1.0f ) * 255 );
		return 0xff000000 | ( R << 16 ) | ( G << 8 ) | ( B );
	}

	float r;
	float g;
	float b;
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


float EvaluateAnimCurve( const void* curve, float time );

struct AnimCurve
{
public:

	float eval( float x ) const
	{
		if ( impl_ )
			return EvaluateAnimCurve( impl_, x );
		else
			return 0.0f;
	}

private:
	const void* impl_ = nullptr;
};

struct SettingsFileImpl;
class SettingsFile
{
public:

private:
	SettingsFile()
		: impl_( NULL )
	{	}
	~SettingsFile()
	{	}

	SettingsFileImpl* impl_;
	friend struct _Impl;
};

SettingsFile* createSettingsFile( const char* settingsFilename, const StructDescription* rootStructDescription, const void* settingsBaseAddress[] );
void releaseSettingsFile( SettingsFile*& configFile );

namespace DontTouchIt
{
typedef int( *ReadFile ) ( const char* filename, unsigned char** fileBuf, size_t* fileBufSize );
typedef void( *FreeFile ) ( const char* filename, unsigned char* fileBuf, size_t fileBufSize );
typedef void( *LogFunc ) ( const char* msg );

struct Param
{
	ReadFile readFile = nullptr;
	FreeFile freeFile = nullptr;
	LogFunc logInfo = nullptr;
	LogFunc logWarning = nullptr;
	LogFunc logError = nullptr;
};

int startUp( const Param& param );
void shutDown();
void update();

const void* getPreset( const char* presetName, const void* impl );

#ifndef SETTINGS_EDITOR_FROZEN
void updateParam( const char* configFile, const char* groupName, const char* presetName, const char* paramName, const int* newValues, int nNewValues );
void updateParam( const char* configFile, const char* groupName, const char* presetName, const char* paramName, const float* newValues, int nNewValues );
void updateParam( const char* configFile, const char* groupName, const char* presetName, const char* paramName, const char* newVal );
#endif //
} // namespace DontTouchIt

} // namespace SettingsEditor
