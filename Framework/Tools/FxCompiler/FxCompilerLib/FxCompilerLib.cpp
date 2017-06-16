#include "FxCompilerLib_pch.h"
#include "FxCompilerLib.h"
#include <fstream>

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{
namespace fxlib
{


FxFile::FxFile()
{
	config_init( &config_ );
}

FxFile::~FxFile()
{
	config_destroy( &config_ );
}

void FxFile::_Parse()
{
	const char* sourceCode = NULL;
	size_t sourceCodeSize = 0;
	_FindHeader( sourceCode_.c_str(), sourceCode_.size(), &fxHeader_, &fxHeaderLength_, &sourceCode, &sourceCodeSize );

	_FindPasses( fxHeader_, fxHeaderLength_, &fxPasses_, &fxPassesLength_ );

	_ExtractPasses();
}

void FxFile::_FindHeader( const char* fileBuf, const size_t fileSize, const char** dstHeader, size_t* dstHeaderSize, const char** dstSourceCode, size_t* dstSourceCodeSize )
{
	const char* startTag = "#ifdef FX_HEADER";
	const char* endTag = "#endif // FX_HEADER";

	const char* head = strstr( fileBuf, startTag );
	if ( !head )
	{
		THROW_MESSAGE( "Couldn't find '%s'!", startTag );
	}

	head += strlen( startTag );

	const char* endHead = strstr( head, endTag );
	if ( !endHead )
	{
		THROW_MESSAGE( "Couldn't find closing '%s'", endTag );
	}

	endHead += strlen( endTag );

	size_t offsHead = (size_t)head;
	size_t offsEndHead = (size_t)endHead;
	size_t headLen = offsEndHead - offsHead;

	*dstHeader = head;
	*dstHeaderSize = headLen;
	*dstSourceCode = endHead;
	*dstSourceCodeSize = ( (size_t)fileBuf + fileSize ) - offsEndHead;
}

void FxFile::_FindPasses( const char* fxheader, const size_t /*fxheaderSize*/, const char** dstFxPasses, size_t* dstFxPassesSize )
{
	const char* startTag = "#ifdef FX_PASSES";
	const char* endTag = "#endif // FX_PASSES";

	const char* head = strstr( fxheader, startTag );
	if ( !head )
	{
		THROW_MESSAGE( "Couldn't find '%s'!", startTag );
	}

	head += strlen( startTag );

	const char* endHead = strstr( head, endTag );
	if ( !endHead )
	{
		THROW_MESSAGE( "Couldn't find closing '%s'", endTag );
	}

	size_t offsHead = (size_t)head;
	size_t offsEndHead = (size_t)endHead;
	size_t headLen = offsEndHead - offsHead;

	*dstFxPasses = head;
	*dstFxPassesSize = headLen;
}

void FxFile::_ExtractPasses()
{
	const size_t gapSize = (size_t)fxPasses_ - (size_t)sourceCode_.c_str();
	u32 lineNo = 0;
	for ( size_t i = 0; i < gapSize; ++i )
		lineNo += sourceCode_[i] == '\n';

	std::string tmpHeader;
	tmpHeader.assign( fxPasses_, fxPassesLength_ );

	try
	{
		int ires = config_read_string( &config_, tmpHeader.c_str() );
		if ( !ires )
		{
			THROW_MESSAGE( "config_read_string:\n%s(%d) - %s %d\n", filename_.c_str(), config_error_line( &config_ ) + lineNo, config_error_text( &config_ ), config_error_type( &config_ ) );
		}


		_ReadPasses( &config_ );
	}
	catch (Exception ex)
	{
		throw;
	}
}

void FxFile::_ReadPasses( const void* configPtr )
{
	const config_t* config = reinterpret_cast<const config_t*>( configPtr );

	const config_setting_t* passes = config_lookup( config, "passes" );
	if ( !passes )
	{
		THROW_MESSAGE( "%s: at least one pass is required!", filename_.c_str() );
	}

	int passesType = config_setting_type( passes );
	if ( passesType != CONFIG_TYPE_GROUP )
	{
		THROW_MESSAGE( "%s: 'passes' must be a group! (Use '{' and '}')", filename_.c_str() );
	}

	int nPasses = config_setting_length( passes );
	if ( !nPasses )
	{
		THROW_MESSAGE( "%s: at least one pass is required!", filename_.c_str() );
	}

	//bool defineFxHeaderForAllPrograms = false;
	//const config_setting_t* DefineFxHeader = config_lookup( config, "DefineFxHeader" );
	//if ( DefineFxHeader )
	//{
	//	defineFxHeaderForAllPrograms = true;
	//}

	// moved vector declarations here to reduce number of small allocations
	std::vector<FxProgDefine> thisCombinationDefines;
	thisCombinationDefines.reserve( 32 );

	std::vector<u32> dims;
	dims.reserve( 32 );

	std::vector<u32> index;
	index.reserve( 32 );

	for ( int ipas = 0; ipas < nPasses; ++ipas )
	{
		const config_setting_t* pas = config_setting_get_elem( passes, ipas );

		const char* passName = config_setting_name( pas );
		if ( !passName )
		{
			THROW_MESSAGE( "%s: pass must be a group and must have a name! (Use '{' and '}')", filename_.c_str() );
		}

		int pasType = config_setting_type( pas );
		if ( pasType != CONFIG_TYPE_GROUP )
		{
			THROW_MESSAGE( "%s: pass '%s' is not a group! (Use '{' and '}')", filename_.c_str(), passName );
		}

		int nSettings = config_setting_length( pas );
		if ( !nSettings )
		{
			THROW_MESSAGE( "%s: pass '%s' doesn't have any settings!", filename_.c_str(), passName );
		}

		int settingsOffset = 0;

		std::vector<FxProgDefineMultiValue> cdefinesPerStage[eProgramType_count];
		std::string entryName[eProgramType_count];
		std::string cflags[eProgramType_count];
		const config_setting_t* progSett[eProgramType_count] = { nullptr };

		bool haveAnyProgram = false;

		for ( int iset = settingsOffset; iset < nSettings; ++iset )
		{
			const config_setting_t* sett = config_setting_get_elem( pas, iset );
			const char* settName = config_setting_name( sett );

			if ( 0 == strcmp( settName, "VertexProgram" ) )
			{
				haveAnyProgram = true;
				progSett[eProgramType_vertexShader] = sett;
				_ReadProgram( sett, cflags[eProgramType_vertexShader], entryName[eProgramType_vertexShader], cdefinesPerStage[eProgramType_vertexShader] );
			}
			else if ( 0 == strcmp( settName, "FragmentProgram" ) )
			{
				haveAnyProgram = true;
				progSett[eProgramType_pixelShader] = sett;
				_ReadProgram( sett, cflags[eProgramType_pixelShader], entryName[eProgramType_pixelShader], cdefinesPerStage[eProgramType_pixelShader] );
			}
			else if ( 0 == strcmp( settName, "GeometryProgram" ) )
			{
				haveAnyProgram = true;
				progSett[eProgramType_geometryShader] = sett;
				_ReadProgram( sett, cflags[eProgramType_geometryShader], entryName[eProgramType_geometryShader], cdefinesPerStage[eProgramType_geometryShader] );
			}
			else if ( 0 == strcmp( settName, "ComputeProgram" ) )
			{
				haveAnyProgram = true;
				progSett[eProgramType_computeShader] = sett;
				_ReadProgram( sett, cflags[eProgramType_computeShader], entryName[eProgramType_computeShader], cdefinesPerStage[eProgramType_computeShader] );
			}
		}

		if ( !haveAnyProgram )
			THROW_MESSAGE( "%s error: pass '%s' doesn't define any program!", filename_.c_str(), passName );

		RenderState passRenderState;
		_ReadState( pas, passRenderState, passName );

		// generate pass combinations
		std::vector<FxProgDefineMultiValue> mvDefines; // multi-value defines, used to generate pass combinations
		u32 typeFirstMvDefine[fxlib::eProgramType_count];
		for ( u32 i = 0; i < eProgramType_count; ++i ) typeFirstMvDefine[i] = 0xffffffff;
		u32 typeNMvDefines[fxlib::eProgramType_count] = { 0 };
		for ( u32 itype = 0; itype < fxlib::eProgramType_count; ++itype )
		{
			if ( cdefinesPerStage[itype].size() )
			{
				for ( size_t i = 0; i < cdefinesPerStage[itype].size(); ++i )
				{
					// take into account only defines that have more than one value
					// treat defines with one value as regular defines (won't be included in combinations generation)
					const FxProgDefineMultiValue& mvd = cdefinesPerStage[itype][i];
					if ( mvd.values_.size() > 1 )
					{
						typeNMvDefines[itype] += 1;
						if ( typeFirstMvDefine[itype] == 0xffffffff )
							typeFirstMvDefine[itype] = (u32)mvDefines.size();

						mvDefines.push_back( mvd );
					}
				}
			}
		}

		// fake define when there are no user multi-value defined cdefines
		// required for combinations generation
		if ( mvDefines.empty() )
		{
			FxProgDefineMultiValue d;
			d.values_.emplace_back( "" );
			mvDefines.push_back( std::move( d ) );
		}

		dims.resize( mvDefines.size() );
		for ( size_t idim = 0; idim < mvDefines.size(); ++idim )
			dims[idim] = (u32)mvDefines[idim].values_.size();

		FxPassCombinationsMatrix combinations;
		combinations.grow( &dims[0], (u32)dims.size() );

		// 'generate combinations' loop
		std::vector<u32> indices( mvDefines.size() );
		bool done = false;
		while ( !done )
		{
			// assemble 'mvDefines.size()' index for accessing combination from 'mvDefines.size()' dimensional array
			index.resize( mvDefines.size() );
			for ( size_t idim = 0; idim < mvDefines.size(); ++idim )
				index[idim] = indices[idim];

			FxPassCombination& comb = combinations.element( &index[0], (u32)index.size() );

			std::stringstream fullPassName; // for debug
			fullPassName << passName;

			for ( u32 itype = 0; itype < fxlib::eProgramType_count; ++itype )
			{
				if ( entryName[itype].empty() )
					continue;

					thisCombinationDefines.clear();
					if ( typeFirstMvDefine[itype] != 0xffffffff )
					{
						for ( u32 i = typeFirstMvDefine[itype]; i < typeFirstMvDefine[itype] + typeNMvDefines[itype]; ++i )
						{
							const FxProgDefineMultiValue& mvd = mvDefines[i];

							u32 idef = indices[i];

							if ( !mvd.name_.empty() && !mvd.values_.empty() )
							{
								thisCombinationDefines.emplace_back( mvd.name_, mvd.values_[idef] );
								fullPassName << '_' << mvd.name_ << '=' << mvd.values_[idef];
							}
						}
					}

					// add regular defines (with only one value)
					for ( size_t i = 0; i < cdefinesPerStage[itype].size(); ++i )
					{
						const FxProgDefineMultiValue& mvd = cdefinesPerStage[itype][i];
						if ( mvd.values_.size() == 1 )
							thisCombinationDefines.emplace_back( mvd.name_, mvd.values_[0] );
					}

					int isp = _FindMatchingProgram( entryName[itype], static_cast<e_ProgramType>(itype), cflags[itype], thisCombinationDefines );
					if ( isp == -1 )
					{
						std::unique_ptr<FxProgram> fxProg = std::make_unique<FxProgram>(
							entryName[itype],
							static_cast<e_ProgramType>( itype ),
							(u32)uniquePrograms_.size(),
							cflags[itype],
							thisCombinationDefines,
							progSett[itype]
							);

						isp = static_cast<int>( uniquePrograms_.size() );
						uniquePrograms_.push_back( std::move( fxProg ) );
					}
					else
					{
						FxProgram& fxProg = *uniquePrograms_[isp];
						fxProg.incRefCount();
					}

					comb.setUniqueProgramIndex( static_cast<e_ProgramType>( itype ), (u32)isp );
			}

			comb.setFullName( fullPassName.str() );

			// next iteration
			for ( size_t idim = 0; idim < mvDefines.size(); ++idim )
			{
				indices[idim] += 1;
				if ( indices[idim] < mvDefines[idim].values_.size() )
				{
					// go to next combination
					//
					break;
				}
				else if ( idim == mvDefines.size() - 1 )
				{
					// last index is done
					//
					done = true;
					break;
				}
				else
				{
					indices[idim] = 0;
				}
			}
		}

		std::unique_ptr<FxPass> fxPass = std::make_unique<FxPass>( *this, passName, passRenderState, std::move( combinations ) );
		passes_.push_back( std::move(fxPass) );
	}

	if ( passes_.size() > 255 )
	{
		THROW_MESSAGE( "%s: Pass count exceeded! Max=255, found=%u", filename_.c_str(), (u32)passes_.size() );
	}
}

void FxFile::_ReadProgram( const config_setting_t* prog, std::string& cflags, std::string& entryName, std::vector<FxProgDefineMultiValue>& progCDefines )
{
	int progType = config_setting_type( prog );
	if ( progType == CONFIG_TYPE_STRING )
	{
		const char* sval = config_setting_get_string( prog );
		SPAD_ASSERT( sval );
		entryName = sval;
	}
	else if ( progType == CONFIG_TYPE_GROUP )
	{
		int nProgSettings = config_setting_length( prog );
		if ( !nProgSettings )
		{
			THROW_MESSAGE( "%s: Shading program doesn't have any settings!", filename_.c_str() );
		}

		// first element must be entry name
		//
		const config_setting_t* entryNameSett = config_setting_get_elem( prog, 0 );
		const char* entryNameSettName = config_setting_name( entryNameSett );
		if ( strcmp( entryNameSettName, "EntryName" ) )
		{
			THROW_MESSAGE( "%s: First element of shading block must be 'EntryName'! Found '%s'.", filename_.c_str(), entryNameSettName );
		}

		int settType = config_setting_type( entryNameSett );
		if ( settType != CONFIG_TYPE_STRING )
		{
			THROW_MESSAGE( "%s: Shading program 'EntryName' must be string!", filename_.c_str() );
		}

		const char* sval = config_setting_get_string( entryNameSett );
		SPAD_ASSERT( sval );
		entryName = sval;

		for ( int ipset = 1; ipset < nProgSettings; ++ipset )
		{
			const config_setting_t* psett = config_setting_get_elem( prog, ipset );
			SPAD_ASSERT( psett );
			const char* psettName = config_setting_name( psett );
			SPAD_ASSERT( psettName );

			//if ( 0 == strcmp( psettName, "RuntimeCompilation" ) )
			//{
			//	int settType2 = config_setting_type( psett );
			//	if ( settType2 != CONFIG_TYPE_INT )
			//	{
			//		THROW_MESSAGE( "%s: pass '%s', Shading program '%s', RuntimeCompilation must be an int!", filename_.c_str(), fxPass.passName.c_str(), fxProg->entryName.c_str() );
			//	}

			//	int ival = config_setting_get_int( psett );
			//	fxProg->compilerParams_.runtimeCompilation = ival;
			//}

			if ( 0 == strcmp( psettName, "cdefines" ) )
			{
				// compiler cdefines
				//
				int settType2 = config_setting_type( psett );
				if ( settType2 != CONFIG_TYPE_GROUP )
				{
					THROW_MESSAGE( "%s: Shading program '%s', cdefines must be a group!", filename_.c_str(), entryName.c_str() );
				}

				const config_setting_t* cdefines = psett;
				const int nCdefines = config_setting_length( cdefines );

				if ( !nCdefines )
				{
					THROW_MESSAGE( "%s: Shading program '%s', cdefines doesn't have any settings!", filename_.c_str(), entryName.c_str() );
				}

				progCDefines.resize( nCdefines );

				for ( int ie = 0; ie < nCdefines; ++ie )
				{
					const config_setting_t* def = config_setting_get_elem( cdefines, ie );
					SPAD_ASSERT( def );
					const char* defName = config_setting_name( def );
					SPAD_ASSERT( defName );

					FxProgDefineMultiValue& d = progCDefines[ie];
					d.name_ = defName;

					int defType = config_setting_type( def );
					if ( defType == CONFIG_TYPE_LIST )
					{
						int nValues = config_setting_length( def );
						if ( !nValues )
						{
							THROW_MESSAGE( "%s: Shading program '%s', cdefines doesn't have any settings!", filename_.c_str(), entryName.c_str() );
						}
						else if ( nValues == 1 )
						{
							const char* val = config_setting_get_string_elem( def, 0 );
							SPAD_ASSERT( val );
							d.values_.emplace_back( val );
						}
						else
						{
							for ( int iv = 0; iv < nValues; ++iv )
							{
								const char* val = config_setting_get_string_elem( def, iv );
								SPAD_ASSERT( val );
								d.values_.emplace_back( val );
							}
						}
					}
					else if ( defType == CONFIG_TYPE_STRING )
					{
						const char* val = config_setting_get_string( def );
						SPAD_ASSERT( val );
						d.values_.emplace_back( val );
					}
					else
					{
						THROW_MESSAGE( "%s: Shading program '%s', cdefines, each define must be a string or a list!", filename_.c_str(), entryName.c_str() );
					}
				}
			}
			else if ( !strncmp( psettName, "cflags_", 7 ) )
			{
				// continue, this is passed to extension
				if ( !cflags.empty() )
					cflags.append( 1, ';' );

				cflags.append( psettName );
			}
			else
			{
				THROW_MESSAGE( "%s: Shading program '%s' has unknown setting '%s'!", filename_.c_str(), entryName.c_str(), psettName );
			}
		}

	}
	else
	{
		THROW_MESSAGE( "%s: Shading program must be string or group!", filename_.c_str() );
	}
}

int FxFile::_FindMatchingProgram( std::string& entryName, e_ProgramType programProfile, const std::string& cflags, const std::vector<FxProgDefine>& cdefines )
{
	const std::string uniqueName = FxProgram::MakeUniqueName( entryName, programProfile, cflags, cdefines );

	const size_t siz = uniquePrograms_.size();
	for ( size_t isp = 0; isp < siz; ++isp )
	{
		const FxProgram& sp = *uniquePrograms_[isp];
		if ( sp.getUniqueName() == uniqueName )
			return (int)isp;
	}

	return -1;
}

bool FxFile::_ReadBoolState( const config_setting_t* sett, const char* passName )
{
	int settType = config_setting_type( sett );
	if ( settType == CONFIG_TYPE_BOOL )
	{
		int ival = config_setting_get_bool( sett );
		return ival != 0;
	}
	else if ( settType == CONFIG_TYPE_INT )
	{
		int ival = config_setting_get_int( sett );
		return ival != 0;
	}
	else
		THROW_MESSAGE( "%s: pass '%s', %s must be bool or int", getFileAbsolutePath().c_str(), passName, config_setting_name( sett ) );
}

void FxFile::_EnsureSettingIsString( const config_setting_t* sett, const char* passName )
{
	if ( config_setting_type( sett ) != CONFIG_TYPE_STRING )
		THROW_MESSAGE( "%s: pass '%s', %s must be a string", getFileAbsolutePath().c_str(), passName, config_setting_name(sett) );
}

void FxFile::_ReadState( const config_setting_t* pass, RenderState& rs, const char* passName )
{
	const int nSettings = config_setting_length( pass );

	bool haveBlendFunc = false;
	bool haveBlendFuncSeparate = false;

	for ( int iset = 0; iset < nSettings; ++iset )
	{
		const config_setting_t* sett = config_setting_get_elem( pass, iset );
		const char* settName = config_setting_name( sett );

		if ( 0 == strcmp( settName, "VertexProgram" )
			|| 0 == strcmp( settName, "FragmentProgram" )
			|| 0 == strcmp( settName, "GeometryProgram" )
			|| 0 == strcmp( settName, "ComputeProgram" )
			)
		{
			// ignore
			continue;
		}
		else if ( 0 == strcmp( settName, "BlendEnable" ) )
		{
			rs.blendState.blendEnabled = _ReadBoolState( sett, passName );
		}
		else if ( 0 == strcmp( settName, "BlendEquation" ) )
		{
			_EnsureSettingIsString( sett, passName );

			const char* tokenValue = config_setting_get_string( sett );
			BlendEquation::Type eq = BlendEquationFromString( tokenValue );
			if ( eq == BlendEquation::count )
				THROW_MESSAGE( "%s: pass '%s', BlendEquation has unsupported value '%s'", getFileAbsolutePath().c_str(), passName, tokenValue );

			rs.blendState.equation = eq;
		}
		else if ( 0 == strcmp( settName, "BlendFunc" ) )
		{
			if ( haveBlendFuncSeparate )
				THROW_MESSAGE( "%s: pass '%s', Can't use BlendFunc and BlendFuncSeparate in a single pass", getFileAbsolutePath().c_str(), passName );

			haveBlendFunc = true;

			int settType = config_setting_type( sett );
			if ( settType == CONFIG_TYPE_LIST )
			{
				int nElements = config_setting_length( sett );
				if ( nElements != 2 )
					THROW_MESSAGE( "%s: pass '%s', BlendFunc must be a list with two strings (\"src\", \"dst\")", getFileAbsolutePath().c_str(), passName );

				const config_setting_t* e0 = config_setting_get_elem( sett, 0 );
				const config_setting_t* e1 = config_setting_get_elem( sett, 1 );
				int e0Type = config_setting_type( e0 );
				int e1Type = config_setting_type( e1 );

				if ( e0Type != CONFIG_TYPE_STRING || e1Type != CONFIG_TYPE_STRING )
					THROW_MESSAGE( "%s: pass '%s', BlendFunc must be a list with two strings (\"src\", \"dst\")", getFileAbsolutePath().c_str(), passName );

				const char* srcVal = config_setting_get_string( e0 );
				const char* dstVal = config_setting_get_string( e1 );

				BlendFactor::Type src = BlendFactorFromString( srcVal );
				if ( src == BlendFactor::count )
					THROW_MESSAGE( "%s: pass '%s', BlendFunc has unsupported src value '%s'", getFileAbsolutePath().c_str(), passName, srcVal );

				BlendFactor::Type dst = BlendFactorFromString( dstVal );
				if ( dst == BlendFactor::count )
					THROW_MESSAGE( "%s: pass '%s', BlendFunc has unsupported dst value '%s'", getFileAbsolutePath().c_str(), passName, dstVal );


				rs.blendState.srcFactor = src;
				rs.blendState.srcFactorAlpha = src;
				rs.blendState.dstFactor = dst;
				rs.blendState.dstFactorAlpha = dst;
			}
			else
				THROW_MESSAGE( "%s: pass '%s', BlendFunc must be a list with two strings (\"src\", \"dst\")", getFileAbsolutePath().c_str(), passName );
		}
		else if ( 0 == strcmp( settName, "BlendFuncSeparate" ) )
		{
			if ( haveBlendFunc )
				THROW_MESSAGE( "%s: pass '%s', Can't use both BlendFunc and BlendFuncSeparate in a single pass", getFileAbsolutePath().c_str(), passName );

			haveBlendFuncSeparate = true;

			int settType = config_setting_type( sett );
			if ( settType == CONFIG_TYPE_LIST )
			{
				int nElements = config_setting_length( sett );
				if ( nElements != 4 )
					THROW_MESSAGE( "%s: pass '%s', BlendFuncSeparate must be a list with four strings (\"src\", \"srcAlpha\", \"dst\", \"dstAlpha\")", getFileAbsolutePath().c_str(), passName );

				BlendFactor::Type bf[4];
				for ( int ie = 0; ie < 4; ++ie )
				{
					const config_setting_t* e = config_setting_get_elem( sett, ie );
					int eType = config_setting_type( e );

					if ( eType != CONFIG_TYPE_STRING )
						THROW_MESSAGE( "%s: pass '%s', BlendFuncSeparate must be a list with four strings (\"src\", \"srcAlpha\", \"dst\", \"dstAlpha\")", getFileAbsolutePath().c_str(), passName );

					const char* val = config_setting_get_string( e );
					bf[ie] = BlendFactorFromString( val );
					if ( bf[ie] == BlendFactor::count )
						THROW_MESSAGE( "%s: pass '%s', BlendFuncSeparate has unsupported factor value '%s'", getFileAbsolutePath().c_str(), passName, val );
				}

				rs.blendState.srcFactor = bf[0];
				rs.blendState.srcFactorAlpha = bf[1];
				rs.blendState.dstFactor = bf[2];
				rs.blendState.dstFactorAlpha = bf[3];
			}
			else
				THROW_MESSAGE( "%s: pass '%s', BlendFuncSeparate must be a list with four strings (\"src\", \"srcAlpha\", \"dst\", \"dstAlpha\")", getFileAbsolutePath().c_str(), passName );
		}
		else if ( 0 == strcmp( settName, "AlphaToCoverageEnable" ) )
		{
			rs.alphaToCoverage = _ReadBoolState( sett, passName );
		}
		else if ( 0 == strcmp( settName, "DepthTestEnable" ) )
		{
			rs.depthTestEnabled = _ReadBoolState( sett, passName );
		}
		else if ( 0 == strcmp( settName, "DepthMask" ) )
		{
			rs.writeDepth = _ReadBoolState( sett, passName );
		}
		else if ( 0 == strcmp( settName, "DepthFunc" ) )
		{
			_EnsureSettingIsString( sett, passName );

			const char* tokenValue = config_setting_get_string( sett );
			CompareFunc::Type df = CompareFuncFromString( tokenValue );
			if ( df == CompareFunc::count )
				THROW_MESSAGE( "%s: pass '%s', DepthFunc has unsupported value '%s'", getFileAbsolutePath().c_str(), passName, tokenValue );

			rs.depthFunc = df;
		}
		else if ( 0 == strcmp( settName, "StencilTestEnable" ) )
		{
			rs.stencilTestEnabled = _ReadBoolState( sett, passName );
		}
		else if ( 0 == strcmp( settName, "StencilFunc" ) )
		{
			_EnsureSettingIsString( sett, passName );

			const char* tokenValue = config_setting_get_string( sett );
			CompareFunc::Type sf = CompareFuncFromString( tokenValue );
			if ( sf == CompareFunc::count )
				THROW_MESSAGE( "%s: pass '%s', StencilFunc has unsupported value '%s'", getFileAbsolutePath().c_str(), passName, tokenValue );

			rs.stencilFunc = sf;
		}
		else if ( 0 == strcmp( settName, "StencilOp" ) )
		{
			_EnsureSettingIsString( sett, passName );
			
			const char* tokenValue = config_setting_get_string( sett );
			StencilOp::Type s = StencilOpFromString( tokenValue );
			if ( s == StencilOp::count )
				THROW_MESSAGE( "%s: pass '%s', StencilOp has unsupported value '%s'", getFileAbsolutePath().c_str(), passName, tokenValue );

			rs.stencilOp = s;
		}
		else if ( 0 == strcmp( settName, "StencilRef" ) )
		{
			int settType = config_setting_type( sett );
			if ( settType == CONFIG_TYPE_INT )
			{
				int ival = config_setting_get_int( sett );
				if ( ival < 0 || ival > 255 )
					THROW_MESSAGE( "%s: pass '%s', StencilRef must be an int in range <0,255> ", getFileAbsolutePath().c_str(), passName );

				rs.stencilRef = (u8)ival;
			}
			else
				THROW_MESSAGE( "%s: pass '%s', StencilRef must be an int in range <0,255> ", getFileAbsolutePath().c_str(), passName );
		}
		else if ( 0 == strcmp( settName, "ColorMask" ) )
		{
			int settType = config_setting_type( sett );
			if ( settType == CONFIG_TYPE_LIST )
			{
				int nElements = config_setting_length( sett );
				if ( nElements != 4 )
					THROW_MESSAGE( "%s: pass '%s', ColorMask_error must be list of four booleans (red, green, blue, alpha)", getFileAbsolutePath().c_str(), passName );

				rs.blendState.writeMaskColorRGBA = 0;

				for ( int ie = 0; ie < 4; ++ie )
				{
					const config_setting_t* e = config_setting_get_elem( sett, ie );
					int eType = config_setting_type( e );
					if ( eType != CONFIG_TYPE_BOOL )
						THROW_MESSAGE( "%s: pass '%s', ColorMask_error must be list of four booleans (red, green, blue, alpha)", getFileAbsolutePath().c_str(), passName );

					int eVal = config_setting_get_bool( e );
					rs.blendState.writeMaskColorRGBA |= ( eVal != 0 ) << ( 3 - ie );
				}
			}
			else
				THROW_MESSAGE( "%s: pass '%s', ColorMask_error must be list of four booleans (red, green, blue, alpha)", getFileAbsolutePath().c_str(), passName );
		}
		else if ( 0 == strcmp( settName, "CullFaceEnable" ) )
		{
			rs.cullFaceEnabled = _ReadBoolState( sett, passName );
		}
		else if ( 0 == strcmp( settName, "CullFace" ) )
		{
			_EnsureSettingIsString( sett, passName );

			const char* tokenValue = config_setting_get_string( sett );
			CullMode::Type s = CullModeFromString( tokenValue );
			if ( s == CullMode::count )
				THROW_MESSAGE( "%s: pass '%s', CullFace has unsupported value '%s'", getFileAbsolutePath().c_str(), passName, tokenValue );

			rs.cullMode = s;
		}
		else if ( 0 == strcmp( settName, "FrontFace" ) )
		{
			_EnsureSettingIsString( sett, passName );

			const char* tokenValue = config_setting_get_string( sett );
			FrontFace::Type s = FrontFaceFromString( tokenValue );
			if ( s == FrontFace::count )
				THROW_MESSAGE( "%s: pass '%s', FrontFace has unsupported value '%s'", getFileAbsolutePath().c_str(), passName, tokenValue );

			rs.frontFace = s;
		}
		else if ( 0 == strcmp( settName, "PolygonMode" ) )
		{
			_EnsureSettingIsString( sett, passName );
			
			const char* tokenValue = config_setting_get_string( sett );
			FillMode::Type s = FillModeFromString( tokenValue );
			if ( s == FillMode::count )
				THROW_MESSAGE( "%s: pass '%s', FillMode has unsupported value '%s'", getFileAbsolutePath().c_str(), passName, tokenValue );

			rs.fillMode = s;
		}
		else if ( 0 == strcmp( settName, "PolygonOffsetEnable" ) )
		{
			rs.polygonOffsetFillEnabled = _ReadBoolState( sett, passName );
		}
		else if ( 0 == strcmp( settName, "PolygonOffset" ) )
		{
			int settType = config_setting_type( sett );
			if ( settType == CONFIG_TYPE_LIST )
			{
				int nElements = config_setting_length( sett );
				if ( nElements != 2 )
					THROW_MESSAGE( "%s: pass '%s', PolygonOffset must be list of two floats (float1, float2)", getFileAbsolutePath().c_str(), passName );

				// special care must be taken for this one
				// user can just write PolygonOffset = ( 100, 1 ) and libconfig will treat 100 and 1 as integers
				// by default, libconfig won't convert int<->float automatically and will return zeros in this case
				// we have to do detect it and convert manually
				// see config_set_auto_convert

				const config_setting_t* e0 = config_setting_get_elem( sett, 0 );
				const config_setting_t* e1 = config_setting_get_elem( sett, 1 );

				int e0Type = config_setting_type( e0 );
				int e1Type = config_setting_type( e1 );
				if (	( e0Type != CONFIG_TYPE_INT && e0Type != CONFIG_TYPE_FLOAT )
					 || ( e1Type != CONFIG_TYPE_INT && e1Type != CONFIG_TYPE_FLOAT )
					 )
					THROW_MESSAGE( "%s: pass '%s', PolygonOffset must be list of two floats (float1, float2)", getFileAbsolutePath().c_str(), passName );

				float f0, f1;
				if( e0Type == CONFIG_TYPE_INT )
					f0 = (float)config_setting_get_int( e0 );
				else
					f0 = (float)config_setting_get_float( e0 );

				if ( e1Type == CONFIG_TYPE_INT )
					f1 = (float)config_setting_get_int( e1 );
				else
					f1 = (float)config_setting_get_float( e1 );

				rs.polygonOffset[0] = f0;
				rs.polygonOffset[1] = f1;
			}
			else
				THROW_MESSAGE( "%s: pass '%s', PolygonOffset must be list of two floats (float1, float2)", getFileAbsolutePath().c_str(), passName );
		}
		else if ( 0 == strcmp( settName, "DepthClipEnable" ) )
		{
			rs.depthClipEnable = _ReadBoolState( sett, passName );
		}
		else
		{
			THROW_MESSAGE( "%s: pass '%s' has unknown setting '%s'!", getFileAbsolutePath().c_str(), passName, settName );
		}
	}
}

void FxFile::_ReadAndParseFxFile( const char* srcFile, const char* srcFileDir, const FxFileCompileOptions& /*options*/, IncludeCache& includeCache )
{
	filename_ = srcFile;
	if ( srcFileDir )
	{
		std::string sf = srcFileDir;
		sf.append( srcFile );
		fileAbsolutePath_ = spad::GetAbsolutePath( sf );
	}
	else
	{
		fileAbsolutePath_ = spad::GetAbsolutePath( filename_ );
	}
	const char* srcFilePathAbsolute = fileAbsolutePath_.c_str();

	//std::string sourceCode = spad::ReadTextFileAsString( srcFilePathAbsolute );
	std::string sourceCode = _ReadSourceFile( srcFilePathAbsolute, includeCache );
	if ( sourceCode.empty() )
	{
		// fix me!!! wchar handled incorrectly
		//
		THROW_MESSAGE( "Failed to read '%s'", srcFilePathAbsolute );
	}

	sourceCode_ = std::move( sourceCode );

	_Parse();
}

std::string FxFile::_ReadSourceFile( const char* filename, IncludeCache& includeCache )
{
	std::string ret;

	HANDLE fileHandle = CreateFile( filename, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL );
	if ( fileHandle == INVALID_HANDLE_VALUE )
		return ret;

	LARGE_INTEGER fileSize;
	BOOL bres = GetFileSizeEx( fileHandle, &fileSize );
	if ( !bres )
	{
		CloseHandle( fileHandle );
		return ret;
	}

	if ( fileSize.QuadPart > 0xffffffff )
	{
		CloseHandle( fileHandle );
		return ret;
	}

	size_t writeOffset;
	const fxlib::IncludeCache::File* fCompilerInclude = includeCache.Get_AlwaysIncludedByFxCompiler();
	if ( fCompilerInclude )
	{
		writeOffset = fCompilerInclude->sourceCode_.size();
		ret.resize( fCompilerInclude->sourceCode_.size() + fileSize.QuadPart );
		memcpy( const_cast<char*>( ret.c_str() ), fCompilerInclude->sourceCode_.c_str(), fCompilerInclude->sourceCode_.size() );
	}
	else
	{
		writeOffset = 0;
		ret.resize( fileSize.QuadPart );
	}

	fileSourceCodeOffset_ = writeOffset;

	DWORD bytesRead = 0;
	bres = ReadFile( fileHandle, &ret[writeOffset], static_cast<DWORD>( fileSize.QuadPart ), &bytesRead, NULL );
	if ( !bres )
	{
		CloseHandle( fileHandle );
		ret.clear();
		return ret;
	}

	CloseHandle( fileHandle );

	return ret;
}

std::unique_ptr<FxFile> ParseFxFile( const char* srcFile, const char* srcFileDir, const FxFileCompileOptions& options, IncludeCache& includeCache, int* err /*= nullptr */ )
{
	try
	{
		std::unique_ptr<FxFile> fxFile = std::make_unique<FxFile>();
		fxFile->_ReadAndParseFxFile( srcFile, srcFileDir, options, includeCache );
		return fxFile;
	}
	catch ( Exception ex )
	{
		logError( "Compile failed!\n%s(1,1)", ex.GetMessage().c_str() );

		if ( err )
			*err = -1;
		return nullptr;
	}
}

bool CheckIfRecompilationIsRequired( const char* srcPath, const char* dependPath, const char* outputPaths[], size_t nOutputPaths, u64 compilerTimestamp, FxCompileConfiguration::Type configuration )
{
	bool srcFound;
	u64 srcTimestamp = GetFileTimestamp( srcPath, srcFound );
	SPAD_ASSERT( srcFound );

	//copySrcToDst = false;

	for ( size_t iOutPath = 0; iOutPath < nOutputPaths; ++iOutPath )
	{
		const char* dstPath = outputPaths[iOutPath];
		bool dstFound;
		u64 dstTimestamp = GetFileTimestamp( dstPath, dstFound );
		if ( !dstFound || ( srcTimestamp > dstTimestamp ) )
		{
			//copySrcToDst = true;
			return true;
		}
	}

	bool dependFound;
	u64 dependTimestamp = GetFileTimestamp( dependPath, dependFound );
	if ( !dependFound || ( srcTimestamp >= dependTimestamp ) )
	{
		// src file is newer than dependency file
		//
		return true;
	}

	if ( compilerTimestamp && compilerTimestamp > dependTimestamp )
	{
		// compiler is newer than dependencies, source and destination files
		return true;
	}

	std::ifstream srcDepends( dependPath );
	std::string line;
	u32 lineIndex = 0;
	while ( !srcDepends.eof() )
	{
		getline( srcDepends, line );
		if ( line.empty() )
			break;

		if ( lineIndex == 0 )
		{
			if ( configuration == FxCompileConfiguration::debug && line != "configuration: debug" )
				return true;
			else if ( configuration == FxCompileConfiguration::release && line != "configuration: release" )
				return true;
			else if ( configuration == FxCompileConfiguration::diagnostic && line != "configuration: diagnostic" )
				return true;
			else if ( configuration == FxCompileConfiguration::shipping && line != "configuration: shipping" )
				return true;

			++lineIndex;
			continue;
		}
		++lineIndex;

		bool fileFound;
		u64 timestamp = GetFileTimestamp( line.c_str(), fileFound );
		if ( !fileFound || ( timestamp > dependTimestamp ) )
		{
			// include file is newer than dependency file
			srcDepends.close();
			return true;
		}
	}
	srcDepends.close();

	return false;
}


bool ExtractCompilerFlags( const FxProgram& fxProg, const char* settingName, std::string& flags, std::vector<std::string>& flagsStorage, std::vector<const char*>& flagsPointers )
{
	const config_setting_t* progSett = fxProg.getSetting();

	if ( CONFIG_TYPE_GROUP != config_setting_type( progSett ) )
		return false;

	const config_setting_t* cflags = config_setting_get_member( progSett, settingName );
	if ( !cflags )
		return false;

	const char* cflagsStr = config_setting_get_string( cflags );
	if ( !cflagsStr )
		THROW_MESSAGE( "ExtractCompilerFlags: config_setting_get_string returned nullptr. FxProg '%s'", fxProg.getEntryName().c_str() );

	flags = cflagsStr;
	std::istringstream f( flags );
	std::string s;
	while ( std::getline( f, s, ' ' ) )
	{
		if ( !s.empty() )
		{
			flagsStorage.push_back( s );
			flagsPointers.push_back( flagsStorage.back().c_str() );
		}
	}

	return !flagsStorage.empty();
}

void WriteCompiledFxFileHeader( FILE* ofs, const FxFile& /*fxFile*/, FxCompileConfiguration::Type configuration )
{
	// magic
	switch ( configuration )
	{
	case spad::fxlib::FxCompileConfiguration::debug:
		AppendString( ofs, "debu" );
		break;
	case spad::fxlib::FxCompileConfiguration::release:
		AppendString( ofs, "rele" );
		break;
	case spad::fxlib::FxCompileConfiguration::diagnostic:
		AppendString( ofs, "diag" );
		break;
	case spad::fxlib::FxCompileConfiguration::shipping:
		AppendString( ofs, "ship" );
		break;
	default:
		THROW_MESSAGE( "Invalid configuration" );
	}
}

void WriteCompiledFxFilePasses( FILE* ofs, const FxFile& fxFile )
{
	// number of passes
	AppendU32( ofs, (u32)fxFile.getPasses().size() );

	for ( const auto& fxPass : fxFile.getPasses() )
	{
		// pass name
		AppendString( ofs, fxPass->getName() );

		// render state
		AppendU32( ofs, sizeof( RenderState ) ); // size for simple validation
		fwrite( &fxPass->getRenderState(), sizeof( RenderState ), 1, ofs );

		const FxPassCombinationsMatrix& combinations = fxPass->getCombinations();
		// total size
		AppendU32( ofs, (u32)combinations.flat_size() );
		// num of dimensions
		AppendU32( ofs, (u32)combinations.num_dims() );
		// size of each dimension
		AppendArray( ofs, &combinations.dims()[0], (u32)combinations.num_dims() );

		// flat array of combinations
		// can be addressed in runtime via [][][]... syntax
		const std::vector<FxPassCombination>& combinationsArray = combinations.flat_array();
		for ( const auto& c : combinationsArray )
		{
			AppendString( ofs, c.getFullName() ); // for debug
			AppendArray( ofs, c.getUniqeProgramIndices(), eProgramType_count );
		}
	}
}

} // namespace fxlib
} // namespace spad
