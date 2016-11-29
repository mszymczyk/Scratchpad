#include "SettingsEditor.h"
#include "impl/SettingsEditorUtil.h"
#include "impl/pugixml/pugixmlHelpers.h"
#include "impl/MayaAnimCurve.h"

namespace SettingsEditor
{

namespace _internal
{

void logGeneric( _internal::LogFunc func, const char* format, ... )
{
	if (!func)
		return;

	va_list	args;

	va_start( args, format );
	char st_buffer[512];
	int st_bufferLen = vsnprintf( st_buffer, 512, format, args );
	if (st_bufferLen >= 512)
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

struct _Struct;


// this is structure representing user's view of given group
// there's impl_ field, followed by user defined fields
struct _GroupClientSide
{
	void* impl_;
	// more data
};

// helper structure for working with AnimCurve
struct _AnimCurveClientSide
{
	void* impl_;
};

struct _Preset
{
	DECLARE_ALIGNED_NEW( 8 );

	~_Preset();

	String name_;
	_Struct* parent_ = nullptr;
	uint8_t* memory_ = nullptr;
	bool exists_ = false;
};

typedef std::vector< std::unique_ptr<_Struct>, SimpleStdAllocator<std::unique_ptr<_Struct>> > _StructUniquePtrArray;
typedef std::vector< std::unique_ptr<_Preset>, SimpleStdAllocator<std::unique_ptr<_Preset>> > _PresetUniquePtrArray;

struct _Struct
{
	DECLARE_ALIGNED_NEW( 8 );

	const StructDescription* desc_ = nullptr;
	uint8_t* absoluteAddress_ = nullptr;
	_StructUniquePtrArray nestedStructures_;
	_PresetUniquePtrArray presets_;

	~_Struct();
	_Struct* getNestedGroup( const char* name ) const;
	_Preset* findPreset( const char* presetName );
	const uint8_t* getPresetMemory( const char* presetName ) const;
};

struct _SettingsFileImpl
{
	DECLARE_ALIGNED_NEW( 8 );

	String filename_;
	size_t refCount_ = 0;

	std::unique_ptr<_Struct> rootStruct_;

	size_t addRef() { return ++refCount_; }
	size_t release() { SETTINGS_EDITOR_ASSERT(refCount_ > 0); return --refCount_; }

	_SettingsFileImpl()
	{}

	~_SettingsFileImpl()
	{
	}

	bool findStruct( const char* structName, _Struct*& dstStruct );
	bool findGroupPresetAndField( const char* groupName, const char* presetName, const char* paramName, _Struct*& dstStruct, _Preset*& dstPreset, const FieldDescription*& dstField );
	void updateParam( const char* groupName, const char* presetName, const char* paramName, const int* newValues, int nNewValues );
	void updateParam( const char* groupName, const char* presetName, const char* paramName, const float* newValues, int nNewValues );
	void updateParam( const char* groupName, const char* presetName, const char* paramName, const char* newVal );
	void initStructures( _Struct& parent, const uint8_t* settingsBaseAddress[] );
	int fillGroupOrPreset( const pugi::xml_node& xmlGroupOrPreset, const _Struct* group, uint8_t* groupOrPresetAddress );
	int initRecurse( const pugi::xml_node& xmlGroup, _Struct* group );
	int init( const StructDescription* rootStructDescription, const void* settingsBaseAddress[], unsigned char* fileBuf, const size_t fileBufSize ); // init
};

// helper structure for working with SettingsFile
struct _SettingsFileClientSide
{
	_SettingsFileImpl* impl_;
};

typedef std::vector<_SettingsFileClientSide, SimpleStdAllocator<_SettingsFileClientSide>> _SettingsFileClientSideArray;


struct _Impl
{
	DECLARE_ALIGNED_NEW( 8 );

	_Impl()
	{	}

	~_Impl();

	_internal::LogFunc logInfo_;
	_internal::LogFunc logWarning_;
	_internal::LogFunc logError_;
	_internal::ReadFile readFile_;
	_internal::FreeFile freeFile_;

	std::mutex mutex_;
	_SettingsFileClientSideArray settingFiles_;

	_SettingsFileClientSide createSettingsFile( const char* settingsFilename, const StructDescription* rootStructDescription, const void* settingsBaseAddress[] );
	void releaseSettingsFile( _SettingsFileClientSide& settingsFile );

	void updateParam( const char* settingsFile, const char* groupName, const char* presetName, const char* paramName, const int* newValues, int nNewValues );
	void updateParam( const char* settingsFile, const char* groupName, const char* presetName, const char* paramName, const float* newValues, int nNewValues );
	void updateParam( const char* settingsFile, const char* groupName, const char* presetName, const char* paramName, const char* newVal );
};

// _DEBUG_NEW must be declared at this point to not interfere with DECLARE_ALIGNED_NEW inside structures
#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

_internal::AllocFunc gAllocFunc;
_internal::FreeFunc gFreeFunc;
_Impl* _gImpl;



void freeDynamicSettings( const StructDescription* desc, uint8_t* absoluteAddress )
{
	const FieldDescription* fields = desc->fields_;
	for (unsigned int ifield = 0; ifield < desc->nFields_; ++ifield)
	{
		const FieldDescription& f = fields[ifield];
		if (f.type_ == eParamType_string)
		{
			// string copy is always created, delete it when unloading
			char** sval = reinterpret_cast<char**>(absoluteAddress + f.offset_);
			memFree( *sval );
		}
		else if (f.type_ == eParamType_animCurve)
		{
			// anim curve is dynamically created, release it when unloading
			_AnimCurveClientSide* ac = reinterpret_cast<_AnimCurveClientSide*>(absoluteAddress + f.offset_);
			if (ac->impl_)
			{
				MayaAnimCurve* mac = reinterpret_cast<MayaAnimCurve*>(ac->impl_);
				delete mac;
				ac->impl_ = nullptr;
			}
		}
	}
}

inline _Struct::~_Struct()
{
	freeDynamicSettings( desc_, absoluteAddress_ );
}

_Struct* _Struct::getNestedGroup( const char* name ) const
{
	for (const auto& p : nestedStructures_)
		if (!strcmp( p->desc_->name_, name ))
			return p.get();

	return nullptr;
}

_Preset* _Struct::findPreset( const char* presetName )
{
	for (const auto& p : presets_)
		if (p->name_ == presetName)
			return p.get();

	return nullptr;
}

const uint8_t* _Struct::getPresetMemory( const char* presetName ) const
{
	for (const auto& p : presets_)
		if (p->name_ == presetName)
			return p->memory_;

	return nullptr;
}

inline _Preset::~_Preset()
{
	freeDynamicSettings( parent_->desc_, memory_ );
	memFree( memory_ );
}


//static EtTangentType convertTangentType( const char* str )
//{
//	if ( !strcmp( str, "Spline" ) )
//		return kTangentSmooth;
//	else if ( !strcmp( str, "Linear" ) )
//		return kTangentLinear;
//	else if ( !strcmp( str, "Clamped" ) )
//		return kTangentClamped;
//
//	else if ( !strcmp( str, "Stepped" ) )
//		return kTangentStep;
//	else if ( !strcmp( str, "SteppedNext" ) )
//		return kTangentStepNext;
//	else if ( !strcmp( str, "Flat" ) )
//		return kTangentFlat;
//
//	else if ( !strcmp( str, "Fixed" ) )
//		return kTangentFixed;
//	else if ( !strcmp( str, "Plateau" ) )
//		return kTangentPlateau;
//	else
//	{
//		FR_NOT_IMPLEMENTED;
//		return kTangentSmooth;
//	}
//};


static EtInfinityType convertPrePostInfinity( const char* str )
{
	if ( !strcmp( str, "Constant" ) )
		return kInfinityConstant;
	else if ( !strcmp( str, "Cycle" ) )
		return kInfinityCycle;
	else if ( !strcmp( str, "CycleWithOffset" ) )
		return kInfinityCycleRelative;
	else if ( !strcmp( str, "Oscillate" ) )
		return kInfinityOscillate;
	else if ( !strcmp( str, "Linear" ) )
		return kInfinityLinear;
	else
	{
		SETTINGS_EDITOR_NOT_IMPLEMENTED;
		return kInfinityConstant;
	}
};


//static int readCurveKey( const pugi::xml_node& xmlKey, EtReadKey& key )
//{
//	key.time = xmlKey.attribute( "x" ).as_float( 0.0f );
//	key.value = xmlKey.attribute( "y" ).as_float( 0.0f );
//
//	const char* sTangentInType = xmlKey.attribute( "tangentInType" ).value();
//	key.inTangentType = convertTangentType( sTangentInType );
//
//	const char* sTangentOutType = xmlKey.attribute( "tangentOutType" ).value();
//	key.outTangentType = convertTangentType( sTangentOutType );
//
//	return 0;
//}

static int readCurveKey( const pugi::xml_node& xmlKey, EtKey& key )
{
	key.time = xmlKey.attribute( "x" ).as_float( 0.0f );
	key.value = xmlKey.attribute( "y" ).as_float( 0.0f );

	float f[2];
	attribute_get_float_array( xmlKey.attribute( "tangentIn" ), f, 2 );
	key.inTanX = f[0];
	key.inTanY = f[1];

	attribute_get_float_array( xmlKey.attribute( "tangentOut" ), f, 2 );
	key.outTanX = f[0];
	key.outTanY = f[1];

	return 0;
}

MayaAnimCurve* readCurve( const pugi::xml_node& xmlCurve )
{
	const char* sPreInfinity = xmlCurve.attribute( "preInfinity" ).value();
	EtInfinityType preInfinity = convertPrePostInfinity( sPreInfinity );
	const char* sPostInfinity = xmlCurve.attribute( "postInfinity" ).value();
	EtInfinityType postInfinity = convertPrePostInfinity( sPostInfinity );

	// count control points
	//
	uint32_t nControlPoints = 0;
	for ( const pugi::xml_node& cp : xmlCurve.children( "controlPoint" ) )
	{
		(void)cp;
		nControlPoints += 1;
	}

	//std::vector<EtReadKey> keys(nControlPoints);
	//size_t iKey = 0;
	//for ( const pugi::xml_node& cp : xmlCurve.children( "controlPoint" ) )
	//{
	//	readCurveKey( cp, keys[iKey++] );
	//}

	MayaAnimCurve::EtKeyArray keys( nControlPoints );
	size_t iKey = 0;
	for ( const pugi::xml_node& cp : xmlCurve.children( "controlPoint" ) )
	{
		readCurveKey( cp, keys[iKey++] );
	}

	//return new MayaAnimCurve( &keys[0], keys.size(), preInfinity, postInfinity, false );
	return new MayaAnimCurve( std::move(keys), preInfinity, postInfinity, false );
	//SETTINGS_EDITOR_NOT_IMPLEMENTED;
	//return nullptr;
}

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
	case eParamType_direction:
		return "direction";
	case eParamType_animCurve:
		return "animCurve";
	default:
		return "unknown";
	}
};

bool _SettingsFileImpl::findStruct( const char* structName, _Struct*& dstStruct )
{
	String snLeft = structName;
	_Struct* structure = rootStruct_.get();

	while (!snLeft.empty())
	{
		String::size_type dotPos = snLeft.find_first_of( '.' );
		String snCur;
		if (dotPos == String::npos)
		{
			snCur = std::move( snLeft );
		}
		else
		{
			snCur = snLeft.substr( 0, dotPos );
			snLeft = snLeft.substr( dotPos + 1 );
		}

		_Struct* nestedStructure = structure->getNestedGroup( snCur.c_str() );
		if (!nestedStructure)
			return false;

		structure = nestedStructure;
	}

	dstStruct = structure;
	return true;
}

bool _SettingsFileImpl::findGroupPresetAndField( const char* groupName, const char* presetName, const char* paramName, _Struct*& dstStruct, _Preset*& dstPreset, const FieldDescription*& dstField )
{
	_Struct* structure;
	if (findStruct( groupName, structure ))
	{
		if (presetName[0])
		{
			_Preset* preset = structure->findPreset( presetName );
			if (!preset)
			{
				seLogError( "findGroupPresetAndField: Can't find preset '%s'. (%s)", presetName, filename_.c_str() );
				return false;
			}

			dstPreset = preset;
		}

		dstStruct = structure;

		const uint32_t nFields = structure->desc_->nFields_;
		for (uint32_t ifield = 0; ifield < nFields; ++ifield)
		{
			const FieldDescription& fd = structure->desc_->fields_[ifield];
			if (!strcmp( fd.name_, paramName ))
			{
				dstField = &fd;
				return true;
			}
		}
	}

	seLogError( "Field '%s,%s' not found.", groupName, paramName );
	return false;
}

void _SettingsFileImpl::updateParam( const char* groupName, const char* presetName, const char* paramName, const int* newValues, int nNewValues )
{
	// support for bool2, bool3 or bool4 and int3/4/5 respectively might be added in the future
	//
	SETTINGS_EDITOR_ASSERT( nNewValues == 1 );

	_Struct* structure;
	_Preset* preset = nullptr;
	const FieldDescription* field;
	if (findGroupPresetAndField( groupName, presetName, paramName, structure, preset, field ))
	{
		uint8_t* absoluteAddress = preset ? preset->memory_ : structure->absoluteAddress_;

		if (field->type_ == eParamType_bool)
		{
			bool* bval = reinterpret_cast<bool*>(absoluteAddress + field->offset_);
			*bval = newValues[0] != 0;
		}
		else if (field->type_ == eParamType_int || field->type_ == eParamType_enum)
		{
			int* ival = reinterpret_cast<int*>(absoluteAddress + field->offset_);
			*ival = newValues[0];
		}
		else
		{
			seLogError( "updateParam: '%s,%s' inconsistent types. Found '%s', now 'boolX or intX'", groupName, paramName, _GetParamTypeName( field->type_ ) );
			return;
		}
	}
}

void _SettingsFileImpl::updateParam( const char* groupName, const char* presetName, const char* paramName, const float* newValues, int nNewValues )
{
	SETTINGS_EDITOR_ASSERT( nNewValues <= 4 );

	_Struct* structure;
	_Preset* preset = nullptr;
	const FieldDescription* field;
	if (findGroupPresetAndField( groupName, presetName, paramName, structure, preset, field ))
	{
		uint8_t* absoluteAddress = preset ? preset->memory_ : structure->absoluteAddress_;

		if (field->type_ == eParamType_float)
		{
			SETTINGS_EDITOR_ASSERT( nNewValues == 1 );

			float* fval = reinterpret_cast<float*>(absoluteAddress + field->offset_);
			*fval = newValues[0];
		}
		else if (field->type_ == eParamType_floatBool)
		{
			SETTINGS_EDITOR_ASSERT( nNewValues == 2 );

			FloatBool* fbval = reinterpret_cast<FloatBool*>(absoluteAddress + field->offset_);
			fbval->value = newValues[0];
			fbval->enabled = newValues[1] > 0 ? true : false;
		}
		else if (field->type_ == eParamType_float4)
		{
			SETTINGS_EDITOR_ASSERT( nNewValues == 4 );
			Float4* f4val = reinterpret_cast<Float4*>(absoluteAddress + field->offset_);
			f4val->set( newValues );
		}
		else if (field->type_ == eParamType_color)
		{
			SETTINGS_EDITOR_ASSERT( nNewValues == 3 );
			Color* cval = reinterpret_cast<Color*>(absoluteAddress + field->offset_);
			cval->set( newValues );
		}
		else if (field->type_ == eParamType_direction)
		{
			SETTINGS_EDITOR_ASSERT( nNewValues == 3 );
			Direction* cval = reinterpret_cast<Direction*>(absoluteAddress + field->offset_);
			cval->set( newValues );
		}
		else
		{
			seLogError( "updateParam: '%s,%s' inconsistent types. Found '%s', now 'float or float4'", groupName/*, presetName*/, paramName, _GetParamTypeName( field->type_ ), _GetParamTypeName( field->type_ ) );
			return;
		}
	}
}

void _SettingsFileImpl::updateParam( const char* groupName, const char* presetName, const char* paramName, const char* newVal )
{
	_Struct* structure;
	_Preset* preset = nullptr;
	const FieldDescription* field;
	if (findGroupPresetAndField( groupName, presetName, paramName, structure, preset, field ))
	{
		uint8_t* absoluteAddress = preset ? preset->memory_ : structure->absoluteAddress_;

		if (field->type_ == eParamType_string)
		{
			char** sval = reinterpret_cast<char**>(absoluteAddress + field->offset_);
			memFree( *sval );
			size_t newValLen = strlen( newVal );
			char* str = (char*)memAlloc( newValLen + 1, 1 );
			strcpy( str, newVal );
			*sval = str;
		}
		else if (field->type_ == eParamType_animCurve)
		{
			pugi::xml_document doc;
			pugi::xml_parse_result res = doc.load_buffer( newVal, strlen( newVal ), pugi::parse_default );
			if (!res)
				seLogError( "xml_document::load_buffer failed! %s, Err=%s", filename_.c_str(), res.description() );

			pugi::xml_node curve = doc.child( "curve" );
			if (!curve.empty())
			{
				MayaAnimCurve* mac = readCurve( curve );

				_AnimCurveClientSide* ac = reinterpret_cast<_AnimCurveClientSide*>(absoluteAddress + field->offset_);
				MayaAnimCurve* oldMac = reinterpret_cast<MayaAnimCurve*>(ac->impl_);
				delete oldMac;
				ac->impl_ = mac;
			}
		}
		else
		{
			seLogError( "updateParam: '%s,%s' inconsistent types. Found '%s', now 'string'", groupName/*, presetName*/, paramName, _GetParamTypeName( field->type_ ) );
			return;
		}
	}
}

void _SettingsFileImpl::initStructures( _Struct& parent, const uint8_t* settingsBaseAddress[] )
{
	for (uint32_t inested = 0; inested < parent.desc_->nNestedStructures_; ++inested)
	{
		const StructDescription* nDesc = parent.desc_->nestedStructures_[inested];
		std::unique_ptr<_Struct> ns( new _Struct() );
		ns->desc_ = nDesc;
		//ns->absoluteAddress_ = parent.absoluteAddress_ + nDesc->offset_;
		if (!parent.desc_->parentStructure_)
		{
			ns->absoluteAddress_ = const_cast<uint8_t*>(settingsBaseAddress[inested]);
		}
		else
		{
			ns->absoluteAddress_ = parent.absoluteAddress_ + nDesc->offset_;
		}
		// store address of group for fast preset lookup
		_GroupClientSide* gcs = reinterpret_cast<_GroupClientSide*>(ns->absoluteAddress_);
		gcs->impl_ = ns.get();

		initStructures( *ns, nullptr );

		parent.nestedStructures_.push_back( std::move( ns ) );
	}
}

int _SettingsFileImpl::fillGroupOrPreset( const pugi::xml_node& xmlGroupOrPreset, const _Struct* group, uint8_t* groupOrPresetAddress )
{
	const uint32_t nParams = group->desc_->nFields_;
	const FieldDescription* fields = group->desc_->fields_;

	for (const pugi::xml_node& xmlProp : xmlGroupOrPreset.children( "prop" ))
	{
		const char* name = attribute_get_string( xmlProp, "name" );
		if (!name)
		{
			seLogError( "fillGroupOrPreset: Property must have 'name' attribute! Skipping... (%s)", filename_.c_str() );
			continue;
		}

		bool found = false;

		for (uint32_t iparam = 0; iparam < nParams; ++iparam)
		{
			const FieldDescription& field = fields[iparam];

			e_ParamType ptype = field.type_;
			const char* pname = field.name_;
			if (strcmp( pname, name ))
				continue;

			found = true;

			if (ptype == eParamType_bool)
			{
				pugi::xml_attribute attr = xmlProp.attribute( "bval" );
				bool* addr = reinterpret_cast<bool*>(groupOrPresetAddress + field.offset_);
				*addr = attr.as_bool( false );
			}
			else if (ptype == eParamType_int)
			{
				pugi::xml_attribute attr = xmlProp.attribute( "ival" );
				int* addr = reinterpret_cast<int*>(groupOrPresetAddress + field.offset_);
				*addr = attr.as_int( 0 );
			}
			else if (ptype == eParamType_enum)
			{
				pugi::xml_attribute attr = xmlProp.attribute( "eval" );
				int* addr = reinterpret_cast<int*>(groupOrPresetAddress + field.offset_);
				*addr = attr.as_int( 0 );
			}
			else if (ptype == eParamType_float)
			{
				pugi::xml_attribute attr = xmlProp.attribute( "fval" );
				float* addr = reinterpret_cast<float*>(groupOrPresetAddress + field.offset_);
				*addr = attr.as_float( 0.0f );
			}
			else if (ptype == eParamType_floatBool)
			{
				pugi::xml_attribute fattr = xmlProp.attribute( "fval" );
				pugi::xml_attribute battr = xmlProp.attribute( "checked" );
				float fval = fattr.as_float( 0.0f );
				bool bval = battr.as_bool( false );
				FloatBool* addr = reinterpret_cast<FloatBool*>(groupOrPresetAddress + field.offset_);
				addr->value = fval;
				addr->enabled = bval;
			}
			else if (ptype == eParamType_float4)
			{
				pugi::xml_attribute attr = xmlProp.attribute( "f4val" );
				float f[4] = { 0, 0, 0, 0 };
				if (!attr.empty())
					attribute_get_float_array( attr, f, 4 );
				Float4* addr = reinterpret_cast<Float4*>(groupOrPresetAddress + field.offset_);
				addr->set( f );
			}
			else if (ptype == eParamType_color)
			{
				pugi::xml_attribute attr = xmlProp.attribute( "colval" );
				float f[3] = { 0, 0, 0 };
				if (!attr.empty())
					attribute_get_float_array( attr, f, 3 );
				Color* addr = reinterpret_cast<Color*>(groupOrPresetAddress + field.offset_);
				addr->set( f );
			}
			else if (ptype == eParamType_string)
			{
				char** strBuf = reinterpret_cast<char**>(groupOrPresetAddress + field.offset_);

				const char* attrValue = attribute_get_string( xmlProp, "sval" );
				if (!attrValue)
				{
					// empty string
					char* srcCopy = (char*)memAlloc( 1, 1 );
					srcCopy[0] = '\0';
					*strBuf = srcCopy;
					break;
				}

				size_t attrValueLen = strlen( attrValue );
				char* attrValueCopy = (char*)memAlloc( attrValueLen + 1, 1 );
				strcpy( attrValueCopy, attrValue );
				*strBuf = attrValueCopy;
			}
			else if (ptype == eParamType_direction)
			{
				pugi::xml_attribute attr = xmlProp.attribute( "dirval" );
				float f[3] = { 0, 0, 1 };
				if (!attr.empty())
					attribute_get_float_array( attr, f, 3 );
				Direction* addr = reinterpret_cast<Direction*>(groupOrPresetAddress + field.offset_);
				addr->set( f );
			}
			else if (ptype == eParamType_animCurve)
			{
				pugi::xml_node curve = xmlProp.child( "curve" );
				_AnimCurveClientSide* ac = reinterpret_cast<_AnimCurveClientSide*>(groupOrPresetAddress + field.offset_);
				if (!curve.empty())
					ac->impl_ = readCurve( curve );
			}
			else
			{
				seLogError( "initRecurse: unsupported setting type (%s)", filename_.c_str() );
				break;
			}

			break;
		}

		if (!found)
		{
			seLogError( "fillGroupOrPreset: Can't find desc for '%s'! Skipping... (%s)", name, filename_.c_str() );
		}
	}

	return 0;
}

int _SettingsFileImpl::initRecurse( const pugi::xml_node& xmlGroup, _Struct* group )
{
	// create and read presets first, so we can initialize them with group's default values
	// after reading group, it's memory will be changed to match contents of a file

	for (const pugi::xml_node& xmlPreset : xmlGroup.children( "preset" ))
	{
		std::unique_ptr<_Preset> preset( new _Preset() );
		pugi::xml_attribute attr = xmlPreset.attribute( "name" );
		preset->name_ = attr.value();
		SETTINGS_EDITOR_ASSERT( !preset->name_.empty() );
		preset->parent_ = group;
		preset->memory_ = reinterpret_cast<uint8_t*>(memAlloc( group->desc_->sizeInBytes_, 16 ));
		// init preset to defaults (values declared in the struct)
		memcpy( preset->memory_, group->absoluteAddress_, group->desc_->sizeInBytes_ );

		_GroupClientSide* gcs = reinterpret_cast<_GroupClientSide*>(preset->memory_);
		gcs->impl_ = nullptr;

		int ires = fillGroupOrPreset( xmlPreset, group, preset->memory_ );
		if (ires)
		{
			//return ires;
			seLogError( "fillGroupOrPreset: initialization failed! State of '%s' may be incosistent... (%s)", preset->name_.c_str(), filename_.c_str() );
		}

		group->presets_.push_back( std::move( preset ) );
	}

	int ires = fillGroupOrPreset( xmlGroup, group, group->absoluteAddress_ );
	if (ires)
	{
		//return ires;
		seLogError( "fillGroupOrPreset: initialization failed! State of '%s' may be incosistent... (%s)", group->desc_->name_, filename_.c_str() );
	}


	for (const pugi::xml_node& xmlGroupChild : xmlGroup.children( "group" ))
	{
		const char* groupName = attribute_get_string( xmlGroupChild, "name" );
		if (!groupName)
		{
			seLogError( "initRecurse: Group must have 'name' attribute! Skipping... (%s)", filename_.c_str() );
			continue;
		}

		_Struct* groupChild = group->getNestedGroup( groupName );
		if (groupChild != nullptr)
		{
			ires = initRecurse( xmlGroupChild, groupChild );
			if (ires)
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

int _SettingsFileImpl::init( const StructDescription* rootStructDescription, const void* settingsBaseAddress[], unsigned char* fileBuf, const size_t fileBufSize )
{
	rootStruct_.reset( new _Struct() );
	rootStruct_->desc_ = rootStructDescription;
	rootStruct_->absoluteAddress_ = nullptr;
	initStructures( *rootStruct_, reinterpret_cast<const uint8_t**>(settingsBaseAddress) );

	pugi::xml_document doc;
	pugi::xml_parse_result res = doc.load_buffer_inplace( fileBuf, fileBufSize, pugi::parse_default );
	if (!res)
		seLogError( "xml_document::load_buffer_inplace failed! %s, Err=%s", filename_.c_str(), res.description() );

	pugi::xml_node settingsFile = doc.first_child();
	int ires = initRecurse( settingsFile, rootStruct_.get() );
	if (ires)
		goto init_error;

	// ok
	//
	goto init2_ok;

init_error:
	seLogError( "SettingsFile::init failed: '%s'", filename_.c_str() );

init2_ok:
	return 0;
}

_Impl::~_Impl()
{
	const size_t nFiles = settingFiles_.size();
	for ( size_t i = 0; i < nFiles; ++i )
	{
		_SettingsFileClientSide cf = settingFiles_[i];
		delete cf.impl_;
	}
}

_SettingsFileClientSide _Impl::createSettingsFile( const char* pathRelativeToAppRoot, const StructDescription* rootStructDescription, const void* settingsBaseAddress[] )
{
	std::lock_guard<std::mutex> lck( mutex_ );

	const size_t nFiles = settingFiles_.size();
	for ( size_t i = 0; i < nFiles; ++i )
	{
		_SettingsFileClientSide cf = settingFiles_[i];
		_SettingsFileImpl* cfi = cf.impl_;
		if (cfi->filename_ == pathRelativeToAppRoot)
		{
			cfi->addRef();
			return cf;
		}
	}

	unsigned char* fileBuf = nullptr;
	size_t fileBufSize = 0;
	int ires = readFile_( pathRelativeToAppRoot, &fileBuf, &fileBufSize );
	if ( ires )
	{
		seLogError( "Couldn't read settings file '%s'", pathRelativeToAppRoot );
		return _SettingsFileClientSide();
	}

	_SettingsFileImpl* impl = new _SettingsFileImpl();
	impl->filename_ = pathRelativeToAppRoot;

	ires = impl->init( rootStructDescription, settingsBaseAddress, fileBuf, fileBufSize );

	freeFile_( pathRelativeToAppRoot, fileBuf, fileBufSize );

	if ( ires )
	{
		delete impl;
		return _SettingsFileClientSide();
	}

	impl->addRef();

	_SettingsFileClientSide cf = { impl };
	settingFiles_.push_back( cf );
	return cf;
}

void _Impl::releaseSettingsFile( _SettingsFileClientSide& settingsFile )
{
	std::lock_guard<std::mutex> lck( mutex_ );

	const size_t nFiles = settingFiles_.size();
	for ( size_t i = 0; i < nFiles; ++i )
	{
		_SettingsFileClientSide cf = settingFiles_[i];
		if ( cf.impl_ == settingsFile.impl_ )
		{
			if (0 == cf.impl_->release())
			{
				delete cf.impl_;
				settingFiles_.erase( settingFiles_.begin() + i );
			}
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
		_SettingsFileClientSide cf = settingFiles_[i];
		_SettingsFileImpl* cfi = cf.impl_;
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
		_SettingsFileClientSide cf = settingFiles_[i];
		_SettingsFileImpl* cfi = cf.impl_;
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
		_SettingsFileClientSide cf = settingFiles_[i];
		_SettingsFileImpl* cfi = cf.impl_;
		if ( cfi->filename_ == settingsFile )
		{
			cfi->updateParam( groupName, presetName, paramName, newVal );
			return;
		}
	}

	seLogWarning( "Settings file '%s' not registered?", settingsFile );
}

void* pugixml_allocate( size_t size )
{
	return memAlloc( size, sizeof( void* ) );
}

void pugixml_deallocate( void* ptr )
{
	memFree( ptr );
}

int startUp( const StartUpParam& param )
{
	if ( !param.readFile || !param.freeFile )
		return -1;

	gAllocFunc = param.allocFunc;
	gFreeFunc = param.freeFunc;

	pugi::set_memory_management_functions( pugixml_allocate, pugixml_deallocate );

	SETTINGS_EDITOR_ASSERT( !_gImpl );
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
	const _Struct* group = reinterpret_cast<const _Struct*>( impl );
	return group->getPresetMemory( presetName );
}

float evaluateAnimCurve( const void* curve, float time )
{
	const MayaAnimCurve* mac = reinterpret_cast<const MayaAnimCurve*>(curve);
	EtCurveEvalCache cache;
	return mac->evaluate( time, &cache );
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



SettingsFile createSettingsFile( const char* settingsFilename, const StructDescription* rootStructDescription, const void* settingsBaseAddress[] )
{
	_internal::_SettingsFileClientSide sf = _internal::_gImpl->createSettingsFile( settingsFilename, rootStructDescription, settingsBaseAddress );
	return *reinterpret_cast<SettingsFile*>(&sf);
}

void releaseSettingsFile( SettingsFile& settingsFile )
{
	_internal::_SettingsFileClientSide& sf = reinterpret_cast<_internal::_SettingsFileClientSide&>(settingsFile);
	if (sf.impl_)
	{
		SETTINGS_EDITOR_ASSERT( _internal::_gImpl );
		_internal::_gImpl->releaseSettingsFile( sf );
	}
}


} // namespace SettingsEditor
//} // namespace spad
