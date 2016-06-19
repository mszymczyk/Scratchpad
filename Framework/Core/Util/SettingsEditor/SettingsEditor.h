#pragma once


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
	eParamType_string,
	eParamType_color,
	eParamType_float4,
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
	{	}

	float value;
	bool enabled;
};

struct Color
{
	Color()
		: r( 0 ), g( 0 ), b( 0 )
	{	}

	Color( float _r, float _g, float _b )
		: r( _r ), g( _g ), b( _b )
	{	}

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

	float x;
	float y;
	float z;
	float w;
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
