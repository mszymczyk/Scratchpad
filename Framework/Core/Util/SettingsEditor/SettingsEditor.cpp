#include <Util_pch.h>
#include "SettingsEditor.h"
#include <mutex>
#include <memory>
#include <tinyxml2.h>

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

using namespace tinyxml2;

//namespace spad
//{
namespace SettingsEditor
{
struct _Impl
{
	_Impl()
	{	}

	~_Impl();

	DontTouchIt::LogFunc logInfo_;
	DontTouchIt::LogFunc logWarning_;
	DontTouchIt::LogFunc logError_;
	DontTouchIt::ReadFile readFile_;
	DontTouchIt::FreeFile freeFile_;

	std::mutex mutex_;
	std::vector<SettingsFile*> settingFiles_;

	SettingsFile* createSettingsFile( const char* settingsFilename, const StructDescription* rootStructDescription, const void* settingsBaseAddress[] );

	void releaseSettingsFile( SettingsFile*& settingsFile );

	void updateParam( const char* settingsFile, const char* groupName, const char* presetName, const char* paramName, const int* newValues, int nNewValues );
	void updateParam( const char* settingsFile, const char* groupName, const char* presetName, const char* paramName, const float* newValues, int nNewValues );
	void updateParam( const char* settingsFile, const char* groupName, const char* presetName, const char* paramName, const char* newVal );

} *_gImpl;

void logGeneric( DontTouchIt::LogFunc func, const char* format, ... )
{
	if ( !func )
		return;

	va_list	args;

	va_start( args, format );
	char st_buffer[512];
	int st_bufferLen = vsnprintf( st_buffer, 512, format, args );
	if ( st_bufferLen >= 512 )
	{
		st_bufferLen = 511;
		st_buffer[511] = 0;
	}
	va_end( args );

	func( st_buffer );
}

#define seLogInfo(...) logGeneric( _gImpl->logError_, __VA_ARGS__ );
#define seLogWarning(...) logGeneric( _gImpl->logWarning_, __VA_ARGS__ );
#define seLogError(...) logGeneric( _gImpl->logError_, __VA_ARGS__ );
//#define seAssert(x) PICO_ASSERT(x)
//#define seNotImplemented PICO_NOT_IMPLEMENTED
#define seAssert(x) FR_ASSERT(x)
#define seNotImplemented FR_NOT_IMPLEMENTED


static const char* _GetParamTypeName( e_ParamType ptype )
{
	switch ( ptype )
	{
	case eParamType_invalid:
		return "invalid";
	case eParamType_bool:
		return "bool";
	case eParamType_int:
		return "int";
	case eParamType_enum:
		return "enum";
	case eParamType_float:
		return "float";
	case eParamType_floatBool:
		return "floatBool";
	case eParamType_string:
		return "string";
	case eParamType_color:
		return "color";
	case eParamType_float4:
		return "float4";
	default:
		return "unknown";
	}
};



inline void* aligned_malloc( size_t size, size_t align )
{
	void *result;
#ifdef _MSC_VER 
	result = _aligned_malloc( size, align );
#else 
	result = memalign( align, size );
#endif
	return result;
}

inline void aligned_free( void *ptr )
{
#ifdef _MSC_VER 
	_aligned_free( ptr );
#else 
	free( ptr );
#endif
}

// this is structure representing user's view of given group
// there's impl filed, followed by user defined fields
struct _GroupClientSide
{
	void* impl_;
	// more data
};

struct SettingsFileImpl
{
	std::string filename_;

	struct _Struct;

	struct _Preset
	{
		~_Preset()
		{
			aligned_free( memory_ );
		}

		std::string name_;
		_Struct* parent_ = nullptr;
		u8* memory_ = nullptr;
		bool exists_ = false;
	};

	struct _Struct
	{
		const StructDescription* desc_ = nullptr;
		u8* absoluteAddress_ = nullptr;
		std::vector < std::unique_ptr<_Struct> > nestedStructures_;
		std::vector < std::unique_ptr<_Preset> > presets_;

		_Struct* getNestedGroup( const char* name ) const
		{
			for ( const auto& p : nestedStructures_ )
				if ( !strcmp( p->desc_->name_, name ) )
					return p.get();

			return nullptr;
		}

		_Preset* findPreset( const char* presetName )
		{
			for ( const auto& p : presets_ )
				if ( p->name_ == presetName )
					return p.get();

			return nullptr;
		}

		const u8* getPresetMemory( const char* presetName ) const
		{
			for ( const auto& p : presets_ )
				if ( p->name_ == presetName )
					return p->memory_;

			return nullptr;
		}
	};

	typedef std::vector < std::unique_ptr<_Struct> > _StructVector;

	std::unique_ptr<_Struct> rootStruct_;

	SettingsFileImpl()
	{}

	~SettingsFileImpl()
	{
	}

	bool findStruct( const char* structName, _Struct*& dstStruct )
	{
		std::string snLeft = structName;
		_Struct* structure = rootStruct_.get();

		while ( !snLeft.empty() )
		{
			std::string::size_type dotPos = snLeft.find_first_of( '.' );
			std::string snCur;
			if ( dotPos == std::string::npos )
			{
				snCur = std::move( snLeft );
			}
			else
			{
				snCur = snLeft.substr( 0, dotPos );
				snLeft = snLeft.substr( dotPos + 1 );
			}

			//bool nestedFound = false;
			//const u32 nNestedStructures = structure->desc_->nNestedStructures_;
			//for (u32 inested = 0; inested < nNestedStructures; ++inested)
			//{
			//	const StructDescription* nsdesc = structure->desc_->nestedStructures_[inested];
			//	if (snCur == nsdesc->name_)
			//	{
			//		structure = structure->nestedStructures_[inested].get();
			//		nestedFound = true;
			//		break;
			//	}
			//}

			//if (!nestedFound)
			//{
			//	seLogError( "Structure '%s' not found!", snCur.c_str() );
			//	return false;
			//}

			_Struct* nestedStructure = structure->getNestedGroup( snCur.c_str() );
			if ( !nestedStructure )
				return false;

			structure = nestedStructure;
		}

		dstStruct = structure;
		return true;
	}

	bool findGroupPresetAndField( const char* groupName, const char* presetName, const char* paramName, _Struct*& dstStruct, _Preset*& dstPreset, const FieldDescription*& dstField )
	{
		_Struct* structure;
		if ( findStruct( groupName, structure ) )
		{
			if ( presetName[0] )
			{
				_Preset* preset = structure->findPreset( presetName );
				if ( !preset )
				{
					seLogError( "findGroupPresetAndField: Can't find preset '%s'. (%s)", presetName, filename_.c_str() );
					return false;
				}

				dstPreset = preset;
			}

			dstStruct = structure;

			const u32 nFields = structure->desc_->nFields_;
			for ( u32 ifield = 0; ifield < nFields; ++ifield )
			{
				const FieldDescription& fd = structure->desc_->fields_[ifield];
				if ( !strcmp( fd.name_, paramName ) )
				{
					dstField = &fd;
					return true;
				}
			}
		}

		seLogError( "Field '%s,%s' not found.", groupName, paramName );
		return false;
	}

	void updateParam( const char* groupName, const char* presetName, const char* paramName, const int* newValues, int nNewValues )
	{
		// support for bool2, bool3 or bool4 and int3/4/5 respectively might be added in the future
		//
		seAssert( nNewValues == 1 );

		_Struct* structure;
		_Preset* preset = nullptr;
		const FieldDescription* field;
		if ( findGroupPresetAndField( groupName, presetName, paramName, structure, preset, field ) )
		{
			u8* absoluteAddress = preset ? preset->memory_ : structure->absoluteAddress_;

			if ( field->type_ == eParamType_int || field->type_ == eParamType_enum )
			{
				int* ival = reinterpret_cast<int*>( absoluteAddress + field->offset_ );
				*ival = newValues[0];
			}
			else if ( field->type_ == eParamType_bool )
			{
				bool* bval = reinterpret_cast<bool*>( absoluteAddress + field->offset_ );
				*bval = newValues[0] != 0;
			}
			else
			{
				seLogError( "updateParam: '%s,%s' inconsistent types. Found '%s', now 'boolX or intX'", groupName, paramName, _GetParamTypeName( field->type_ ) );
				return;
			}
		}
	}

	void updateParam( const char* groupName, const char* presetName, const char* paramName, const float* newValues, int nNewValues )
	{
		seAssert( nNewValues <= 4 );

		_Struct* structure;
		_Preset* preset = nullptr;
		const FieldDescription* field;
		if ( findGroupPresetAndField( groupName, presetName, paramName, structure, preset, field ) )
		{
			u8* absoluteAddress = preset ? preset->memory_ : structure->absoluteAddress_;

			if ( field->type_ == eParamType_float )
			{
				seAssert( nNewValues == 2 );
				// second value will be enabled flag, ignore it
				//

				float* fval = reinterpret_cast<float*>( absoluteAddress + field->offset_ );
				*fval = newValues[0];
			}
			else if ( field->type_ == eParamType_floatBool )
			{
				seAssert( nNewValues == 2 );

				FloatBool* fbval = reinterpret_cast<FloatBool*>( absoluteAddress + field->offset_ );
				fbval->value = newValues[0];
				fbval->enabled = newValues[1] > 0 ? true : false;
			}
			else if ( field->type_ == eParamType_float4 )
			{
				seAssert( nNewValues == 4 );
				float* fval = reinterpret_cast<float*>( absoluteAddress + field->offset_ );
				for ( int i = 0; i < 4; ++i )
					fval[i] = newValues[i];
			}
			else
			{
				seLogError( "updateParam: '%s,%s' inconsistent types. Found '%s', now 'float or float4'", groupName/*, presetName*/, paramName, _GetParamTypeName( field->type_ ), _GetParamTypeName( field->type_ ) );
				return;
			}
		}
	}

	void updateParam( const char* groupName, const char* presetName, const char* paramName, const char* newVal )
	{
		_Struct* structure;
		_Preset* preset = nullptr;
		const FieldDescription* field;
		if ( findGroupPresetAndField( groupName, presetName, paramName, structure, preset, field ) )
		{
			u8* absoluteAddress = preset ? preset->memory_ : structure->absoluteAddress_;

			if ( field->type_ == eParamType_string )
			{
				char** sval = reinterpret_cast<char**>( absoluteAddress + field->offset_ );
				delete[]( *sval );
				size_t newValLen = strlen( newVal );
				char* str = new char[newValLen + 1];
				strcpy( str, newVal );
				*sval = str;
			}
			else
			{
				seLogError( "updateParam: '%s,%s' inconsistent types. Found '%s', now 'string'", groupName/*, presetName*/, paramName, _GetParamTypeName( field->type_ ) );
				return;
			}
		}
	}

	void initStructures( _Struct& parent, const u8* settingsBaseAddress[] )
	{
		for ( u32 inested = 0; inested < parent.desc_->nNestedStructures_; ++inested )
		{
			const StructDescription* nDesc = parent.desc_->nestedStructures_[inested];
			std::unique_ptr<_Struct> ns( new _Struct() );
			ns->desc_ = nDesc;
			//ns->absoluteAddress_ = parent.absoluteAddress_ + nDesc->offset_;
			if ( !parent.desc_->parentStructure_ )
			{
				ns->absoluteAddress_ = const_cast<u8*>( settingsBaseAddress[inested] );
			}
			else
			{
				ns->absoluteAddress_ = parent.absoluteAddress_ + nDesc->offset_;
			}
			// store address of group for fast preset lookup
			_GroupClientSide* gcs = reinterpret_cast<_GroupClientSide*>( ns->absoluteAddress_ );
			gcs->impl_ = ns.get();

			initStructures( *ns, nullptr );

			parent.nestedStructures_.push_back( std::move( ns ) );
		}
	}

	int fillGroupOrPreset( const XMLElement* xmlGroupOrPreset, const _Struct* group, u8* groupOrPresetAddress )
	{
		const u32 nParams = group->desc_->nFields_;
		const FieldDescription* fields = group->desc_->fields_;

		const XMLElement* xmlProp = NULL;
		for ( xmlProp = xmlGroupOrPreset->FirstChildElement( "prop" ); xmlProp; xmlProp = xmlProp->NextSiblingElement( "prop" ) )
		{
			const char* name = xmlProp->Attribute( "name" );
			if ( !name )
			{
				seLogError( "fillGroupOrPreset: Group must have 'name' attribute! Skipping... (%s)", filename_.c_str() );
				continue;
			}

			bool found = false;

			for ( u32 iparam = 0; iparam < nParams; ++iparam )
			{
				const FieldDescription& field = fields[iparam];

				e_ParamType ptype = field.type_;
				const char* pname = field.name_;
				if ( strcmp( pname, name ) )
					continue;

				found = true;

				if ( ptype == eParamType_bool )
				{
					const char* attrValue = xmlProp->Attribute( "bval" );
					//if ( !attrValue )
					//	return -1;

					bool bval = false;
					if ( attrValue && !strcmp( attrValue, "true" ) )
						bval = true;

					bool* addr = reinterpret_cast<bool*>( groupOrPresetAddress + field.offset_ );
					*addr = bval;
				}
				else if ( ptype == eParamType_int )
				{
					int ival = 0;
					if ( xmlProp->QueryIntAttribute( "ival", &ival ) != XML_SUCCESS )
					{
						break;
						//return -1;
					}

					int* addr = reinterpret_cast<int*>( groupOrPresetAddress + field.offset_ );
					*addr = ival;
				}
				else if ( ptype == eParamType_enum )
				{
					int ival = 0;
					if ( xmlProp->QueryIntAttribute( "eval", &ival ) != XML_SUCCESS )
					{
						//return -1;
						break;
					}

					int* addr = reinterpret_cast<int*>( groupOrPresetAddress + field.offset_ );
					*addr = ival;
				}
				else if ( ptype == eParamType_float )
				{
					float fval = 0;
					if ( xmlProp->QueryFloatAttribute( "fval", &fval ) != XML_SUCCESS )
					{
						//return -1;
						break;
					}

					//// there will be four floats: value, soft min, soft max, step
					//// 
					//std::stringstream ss( attrValue );
					//float fval = 0;
					//ss >> fval;

					float* addr = reinterpret_cast<float*>( groupOrPresetAddress + field.offset_ );
					*addr = fval;
				}
				else if ( ptype == eParamType_floatBool )
				{
					//const char* attrValue = xmlGroupOrPreset->Attribute( pname );
					//if ( !attrValue )
					//	return -1;
					float fval = 0;
					if ( xmlProp->QueryFloatAttribute( "fval", &fval ) != XML_SUCCESS )
					{
						//return -1;
						break;
					}

					const char* attrValue = xmlProp->Attribute( "checked" );
					if ( !attrValue )
					{
						//return -1;
						break;
					}

					bool bval = false;
					if ( !strcmp( attrValue, "true" ) )
						bval = true;

					//// there will be four floats: value, soft min, soft max, step
					//// 
					//float fval, tmp, bval = 0;
					//std::stringstream ss( attrValue );
					//ss >> fval >> tmp >> tmp >> tmp >> bval;

					FloatBool* addr = reinterpret_cast<FloatBool*>( groupOrPresetAddress + field.offset_ );
					addr->value = fval;
					addr->enabled = bval;
				}
				else if ( ptype == eParamType_string )
				{
					char** strBuf = reinterpret_cast<char**>( groupOrPresetAddress + field.offset_ );

					const char* attrValue = xmlProp->Attribute( "sval" );
					if ( !attrValue )
					{
						// need to create src copy because new string will be placed exactly at the same memory location
						// and will override old contents
						//
						const char* src = reinterpret_cast<const char*>( *strBuf );
						size_t srcLen = strlen( src );
						char* srcCopy = new char[srcLen + 1];
						strcpy( srcCopy, src );
						*strBuf = srcCopy;
						//continue;
						break;
					}

					size_t attrValueLen = strlen( attrValue );
					char* attrValueCopy = new char[attrValueLen + 1];
					strcpy( attrValueCopy, attrValue );
					*strBuf = attrValueCopy;
				}
				else if ( ptype == eParamType_float4 )
				{
					const char* attrValue = xmlProp->Attribute( "f4val" );
					if ( !attrValue )
						//return -1;
						break;

					Float4* addr = reinterpret_cast<Float4*>( groupOrPresetAddress + field.offset_ );
					addr->x = 0;
					addr->y = 0;
					addr->z = 0;
					addr->w = 0;
					sscanf( attrValue, "%f %f %f %f", &addr->x, &addr->y, &addr->z, &addr->w );
				}
				else if ( ptype == eParamType_color )
				{
					int ival = 0;
					if ( xmlProp->QueryIntAttribute( pname, &ival ) != XML_SUCCESS )
						//return -1;
						break;

					Color* addr = reinterpret_cast<Color*>( groupOrPresetAddress + field.offset_ );
					addr->r = (float)( ( ival >> 16 ) & 0xff ) * ( 1.0f / 255.0f );
					addr->g = (float)( ( ival >> 8 ) & 0xff ) * ( 1.0f / 255.0f );
					addr->b = (float)( ( ival ) & 0xff ) * ( 1.0f / 255.0f );
				}
				else
				{
					seLogError( "initRecurse: unsupported setting type (%s)", filename_.c_str() );
					//return -1;
					break;
				}

				break;
			}

			if ( !found )
			{
				seLogError( "fillGroupOrPreset: Can't find desc for '%s'! Skipping... (%s)", name, filename_.c_str() );
				//continue;
			}
		}

		return 0;
	}

	int initRecurse( const XMLElement* xmlGroup, _Struct* group )
	{
		// create and read presets first, so we can initialize them with group's default values
		// after reading group, it's memory will be changed to match contents of a file
		//_Struct* structure = parent->getNestedGroup( groupName );
		//if (findStruct( groupName, structure ))
		//{
		const XMLElement* xmlPreset = NULL;
		for ( xmlPreset = xmlGroup->FirstChildElement( "preset" ); xmlPreset; xmlPreset = xmlPreset->NextSiblingElement( "preset" ) )
		{
			std::unique_ptr<_Preset> preset( new _Preset() );
			preset->name_ = xmlPreset->Attribute( "presetName" );
			preset->parent_ = group;
			preset->memory_ = reinterpret_cast<u8*>( aligned_malloc( group->desc_->sizeInBytes_, 16 ) );
			// init preset to defaults (values declared in the struct)
			memcpy( preset->memory_, group->absoluteAddress_, group->desc_->sizeInBytes_ );

			_GroupClientSide* gcs = reinterpret_cast<_GroupClientSide*>( preset->memory_ );
			gcs->impl_ = nullptr;

			int ires = fillGroupOrPreset( xmlPreset, group, preset->memory_ );
			if ( ires )
			{
				//return ires;
				seLogError( "fillGroupOrPreset: initialization failed! State of '%s' may be incosistent... (%s)", preset->name_.c_str(), filename_.c_str() );
			}

			group->presets_.push_back( std::move( preset ) );
		}
		//}
		//else
		//{
		//	seLogError( "initRecurse: Group '%s' not found! It will have it's default values! (%s)", filename_.c_str() );
		//}

		int ires = fillGroupOrPreset( xmlGroup, group, group->absoluteAddress_ );
		if ( ires )
		{
			//return ires;
			seLogError( "fillGroupOrPreset: initialization failed! State of '%s' may be incosistent... (%s)", group->desc_->name_, filename_.c_str() );
		}


		const XMLElement* xmlGroupChild = NULL;
		for ( xmlGroupChild = xmlGroup->FirstChildElement( "group" ); xmlGroupChild; xmlGroupChild = xmlGroupChild->NextSiblingElement( "group" ) )
		{
			const char* groupName = xmlGroupChild->Attribute( "name" );
			if ( !groupName )
			{
				seLogError( "initRecurse: Group must have 'name' attribute! Skipping... (%s)", filename_.c_str() );
				continue;
			}

			_Struct* groupChild = group->getNestedGroup( groupName );
			if ( groupChild != nullptr )
			{
				ires = initRecurse( xmlGroupChild, groupChild );
				if ( ires )
				{
					//return ires;
					seLogError( "initRecurse: Nested group '%s' initialization failed! State may be incosistent... (%s)", groupName, filename_.c_str() );
				}
			}
			else
			{
				seLogError( "initRecurse: Group '%s' not found! It will keep it's default values! (%s)", groupName, filename_.c_str() );
			}
		}

		return 0;
	}

	int init( const StructDescription* rootStructDescription, const void* settingsBaseAddress[], const unsigned char* fileBuf, const size_t fileBufSize )
	{
		rootStruct_.reset( new _Struct() );
		rootStruct_->desc_ = rootStructDescription;
		rootStruct_->absoluteAddress_ = nullptr;
		initStructures( *rootStruct_, reinterpret_cast<const u8**>( settingsBaseAddress ) );

		tinyxml2::XMLDocument xmlDoc;
		XMLError err = xmlDoc.Parse( reinterpret_cast<const char*>( fileBuf ), fileBufSize );

		if ( err != XML_SUCCESS )
		{
			seLogError( "XMLDocument::Parse failed! %s, Err=%d", filename_.c_str(), (int)err );
			return -1;
		}
		else
		{
			const XMLElement* root = xmlDoc.RootElement();
			int ires = initRecurse( root, rootStruct_.get() );
			if ( ires )
				goto init_error;
		}

		// ok
		//
		goto init2_ok;

	init_error:
		seLogError( "SettingsFile::init failed: '%s'", filename_.c_str() );

	init2_ok:
		return 0;
	} // init

};

_Impl::~_Impl()
{
	const size_t nFiles = settingFiles_.size();
	for ( size_t i = 0; i < nFiles; ++i )
	{
		SettingsFile* cf = settingFiles_[i];
		delete cf->impl_;
		delete cf;
	}
}

SettingsFile* _Impl::createSettingsFile( const char* pathRelativeToAppRoot, const StructDescription* rootStructDescription, const void* settingsBaseAddress[] )
{
	std::lock_guard<std::mutex> lck( mutex_ );

	const size_t nFiles = settingFiles_.size();
	for ( size_t i = 0; i < nFiles; ++i )
	{
		SettingsFile* cf = settingFiles_[i];
		SettingsFileImpl* cfi = cf->impl_;
		if ( cfi->filename_ == pathRelativeToAppRoot )
			return cf;
	}

	unsigned char* fileBuf = nullptr;
	size_t fileBufSize = 0;
	int ires = readFile_( pathRelativeToAppRoot, &fileBuf, &fileBufSize );
	if ( ires )
	{
		seLogError( "Couldn't read settings file '%s'", pathRelativeToAppRoot );
		return nullptr;
	}

	SettingsFileImpl* impl = new SettingsFileImpl();
	impl->filename_ = pathRelativeToAppRoot;

	ires = impl->init( rootStructDescription, settingsBaseAddress, fileBuf, fileBufSize );

	freeFile_( pathRelativeToAppRoot, fileBuf, fileBufSize );

	if ( ires )
	{
		delete impl;
		return NULL;
	}

	SettingsFile* cf = new SettingsFile();
	cf->impl_ = impl;
	settingFiles_.push_back( cf );
	return cf;
}

void _Impl::releaseSettingsFile( SettingsFile*& settingsFile )
{
	std::lock_guard<std::mutex> lck( mutex_ );

	const size_t nFiles = settingFiles_.size();
	for ( size_t i = 0; i < nFiles; ++i )
	{
		SettingsFile* cf = settingFiles_[i];
		if ( cf == settingsFile )
		{
			settingsFile = NULL;
			delete cf->impl_;
			delete cf;
			settingFiles_.erase( settingFiles_.begin() + i );
			return;
		}
	}
}




void _Impl::updateParam( const char* settingsFile, const char* groupName, const char* presetName, const char* paramName, const int* newValues, int nNewValues )
{
	std::lock_guard<std::mutex> lck( mutex_ );

	const size_t nFiles = settingFiles_.size();
	for ( size_t i = 0; i < nFiles; ++i )
	{
		SettingsFile* cf = settingFiles_[i];
		SettingsFileImpl* cfi = cf->impl_;
		if ( cfi->filename_ == settingsFile )
		{
			cfi->updateParam( groupName, presetName, paramName, newValues, nNewValues );
			return;
		}
	}

	seLogWarning( "Settings file '%s' not registered?", settingsFile );
}

void _Impl::updateParam( const char* settingsFile, const char* groupName, const char* presetName, const char* paramName, const float* newValues, int nNewValues )
{
	std::lock_guard<std::mutex> lck( mutex_ );

	const size_t nFiles = settingFiles_.size();
	for ( size_t i = 0; i < nFiles; ++i )
	{
		SettingsFile* cf = settingFiles_[i];
		SettingsFileImpl* cfi = cf->impl_;
		if ( cfi->filename_ == settingsFile )
		{
			cfi->updateParam( groupName, presetName, paramName, newValues, nNewValues );
			return;
		}
	}

	seLogWarning( "Settings file '%s' not registered?", settingsFile );
}

void _Impl::updateParam( const char* settingsFile, const char* groupName, const char* presetName, const char* paramName, const char* newVal )
{
	std::lock_guard<std::mutex> lck( mutex_ );

	const size_t nFiles = settingFiles_.size();
	for ( size_t i = 0; i < nFiles; ++i )
	{
		SettingsFile* cf = settingFiles_[i];
		SettingsFileImpl* cfi = cf->impl_;
		if ( cfi->filename_ == settingsFile )
		{
			cfi->updateParam( groupName, presetName, paramName, newVal );
			return;
		}
	}

	seLogWarning( "Settings file '%s' not registered?", settingsFile );
}



SettingsFile* createSettingsFile( const char* settingsFilename, const StructDescription* rootStructDescription, const void* settingsBaseAddress[] )
{
	return _gImpl->createSettingsFile( settingsFilename, rootStructDescription, settingsBaseAddress );
}

void releaseSettingsFile( SettingsFile*& settingsFile )
{
	if ( settingsFile )
	{
		seAssert( _gImpl );
		_gImpl->releaseSettingsFile( settingsFile );
	}
}

namespace DontTouchIt
{
int startUp( const Param& param )
{
	if ( !param.readFile || !param.freeFile )
		return -1;

	seAssert( !_gImpl );
	_gImpl = new _Impl();

	_gImpl->readFile_ = param.readFile;
	_gImpl->freeFile_ = param.freeFile;
	_gImpl->logInfo_ = param.logInfo;
	_gImpl->logWarning_ = param.logWarning;
	_gImpl->logError_ = param.logError;

	return 0;
}

void shutDown()
{
	delete _gImpl;
	_gImpl = NULL;
}

void update()
{
}

const void* getPreset( const char* presetName, const void* impl )
{
	const SettingsFileImpl::_Struct* group = reinterpret_cast<const SettingsFileImpl::_Struct*>( impl );
	return group->getPresetMemory( presetName );
}

void updateParam( const char* settingsFile, const char* groupName, const char* presetName, const char* paramName, const int* newValues, int nNewValues )
{
	_gImpl->updateParam( settingsFile, groupName, presetName, paramName, newValues, nNewValues );
}

void updateParam( const char* settingsFile, const char* groupName, const char* presetName, const char* paramName, const float* newValues, int nNewValues )
{
	_gImpl->updateParam( settingsFile, groupName, presetName, paramName, newValues, nNewValues );
}

void updateParam( const char* settingsFile, const char* groupName, const char* presetName, const char* paramName, const char* newVal )
{
	_gImpl->updateParam( settingsFile, groupName, presetName, paramName, newVal );
}
}

} // namespace SettingsEditor
//} // namespace spad
