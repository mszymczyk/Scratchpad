#include "FxLib_pch.h"
#include "FxLib.h"
#include "../Util/hash/HashUtil.h"

#include "../../3rdParty/libconfig/lib/libconfig.h"
#include <fstream>
#include <cctype>

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

_COM_SMARTPTR_TYPEDEF( ID3D11DeviceChild, __uuidof( ID3D11DeviceChild ) );

namespace spad
{
	namespace FxLib
	{
		FxFile::~FxFile()
		{
			//size_t siz = uniquePrograms_.size();
			//for ( size_t i = 0; i < siz; ++i )
			//	delete uniquePrograms_[i];

			//siz = passes_.size();
			//for ( size_t i = 0; i < siz; ++i )
			//	delete passes_[i];
		}

		int FxFile::compileFxFile( const char* srcFilePathAbsolute, const FxFileCompileOptions& options )
		{
			try
			{
				_CompileFxFileImpl( srcFilePathAbsolute, options );

				return 0;
			}
			catch ( HlslException ex )
			{
				logError( "HLSL Compiler error:" );
				logError( ex.GetHlslErrorMessage().c_str() );
				return -1;
			}
			catch ( Exception ex )
			{
				logError( "compileFxFile failed. Err=%s", ex.GetMessage().c_str() );
				return  -1;
			}
		}

		int FxFile::writeCompiledFxFile( const FxFileWriteOptions& options )
		{
			try
			{
				_WriteCompiledHlsl( options );
				return 0;
			}
			catch ( Exception ex )
			{
				logError( "writeCompiledFxFile failed. Err=%s", ex.GetMessage().c_str() );
				return  -1;
			}
		}

		int FxFile::loadCompiledFxFile( const char* srcFilePathAbsolute )
		{
			try
			{
				_LoadFxFileImpl( srcFilePathAbsolute );

				return 0;
			}
			catch ( Exception ex )
			{
				logError( "loadCompiledFxFile failed. Err=%s", ex.GetMessage().c_str() );
				return -1;
			}
		}

		int FxFile::createShaders( ID3D11Device* dxDevice )
		{
			try
			{
				_CreateShaders( dxDevice );

				return 0;
			}
			catch ( Exception ex )
			{
				logError( "createShaders failed. Err=%s", ex.GetMessage().c_str() );
				return -1;
			}
		}

		std::unique_ptr<FxRuntime> FxFile::createFxRuntime( ID3D11Device* dxDevice ) const
		{
			try
			{
				std::unique_ptr<FxRuntime> fx = _CreateFxRuntime( dxDevice );
				return fx;
			}
			catch ( Exception ex )
			{
				logError( "createFxRuntime failed. Err=%s", ex.GetMessage().c_str() );
				return std::unique_ptr<FxRuntime>();
			}
		}

		int FxFile::reflectHlslData()
		{
			for ( const auto& prog : uniquePrograms_ )
			{
				HRESULT hr = D3DReflect( prog->hlslProgramData_.shaderBlob_->GetBufferPointer()
					, prog->hlslProgramData_.shaderBlob_->GetBufferSize()
					, IID_ID3D11ShaderReflection
					, (void**)&prog->hlslProgramData_.reflection_
					);

				if ( FAILED( hr ) )
				{
					logError( "D3DReflect failed. Err=0x%x (%s)", hr, filename_.c_str() );
					return -1;
				}
			}

			return 0;
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

			config_t config;
			config_init( &config );

			try
			{
				int ires = config_read_string( &config, tmpHeader.c_str() );
				if ( !ires )
				{
					THROW_MESSAGE( "config_read_string:\n%s(%d) - %s %d\n", filename_.c_str(), config_error_line( &config ) + lineNo, config_error_text( &config ), config_error_type( &config ) );
				}


				_ReadPasses( &config );

				_HandleCombinations();

				const size_t nFxPasses = passes_.size();
				for ( size_t ipass = 0; ipass < nFxPasses; ++ipass )
				{
					const FxPass* fxPass = passes_[ipass].get();

					const u32 computeEntryIdx = fxPass->entryIdx[eProgramType_computeShader];
					if ( computeEntryIdx != 0xffffffff )
					{
						// make sure, there are no other programs in this pass
						//

						for ( u32 ipp = 0; ipp < eProgramType_count; ++ipp )
						{
							if ( ipp != eProgramType_computeShader )
							{
								const u32 entryIdx = fxPass->entryIdx[ipp];
								if ( entryIdx != 0xffffffff )
								{
									THROW_MESSAGE( "%s: error: pass '%s', Compute program must not be used with other program types.", filename_.c_str(), fxPass->passName.c_str() );
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				config_destroy( &config );

				throw;
			}

			config_destroy( &config );
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

			for ( int ipas = 0; ipas < nPasses; ++ipas )
			{
				const config_setting_t* pas = config_setting_get_elem( passes, ipas );

				const char* pasName = config_setting_name( pas );
				if ( !pasName )
				{
					THROW_MESSAGE( "%s: pass must be a group and must have a name! (Use '{' and '}')", filename_.c_str() );
				}

				int pasType = config_setting_type( pas );
				if ( pasType != CONFIG_TYPE_GROUP )
				{
					THROW_MESSAGE( "%s: pass '%s' is not a group! (Use '{' and '}')", filename_.c_str(), pasName );
				}

				int nSettings = config_setting_length( pas );
				if ( !nSettings )
				{
					THROW_MESSAGE( "%s: pass '%s' doesn't have any settings!", filename_.c_str(), pasName );
				}

				//FxPass* fxPass = new FxPass();
				std::unique_ptr<FxPass> fxPass = std::make_unique<FxPass>();
				fxPass->passName = pasName;

				int settingsOffset = 0;

				for ( int iset = settingsOffset; iset < nSettings; ++iset )
				{
					const config_setting_t* sett = config_setting_get_elem( pas, iset );
					const char* settName = config_setting_name( sett );

					if ( 0 == strcmp( settName, "VertexProgram" ) )
					{
						_ReadProgram( sett, eProgramType_vertexShader, *fxPass );
					}
					else if ( 0 == strcmp( settName, "FragmentProgram" ) )
					{
						_ReadProgram( sett, eProgramType_pixelShader, *fxPass );
					}
					else if ( 0 == strcmp( settName, "GeometryProgram" ) )
					{
						_ReadProgram( sett, eProgramType_geometryShader, *fxPass );
					}
					else if ( 0 == strcmp( settName, "ComputeProgram" ) )
					{
						_ReadProgram( sett, eProgramType_computeShader, *fxPass );
					}

					//////////////////////////////////////////////////////////////////////////
					//////////////////////////////////////////////////////////////////////////
					//////////////////////////////////////////////////////////////////////////
					else
					{
						THROW_MESSAGE( "%s error: pass '%s' has unknown setting '%s'!", filename_.c_str(), pasName, settName );
					}
				}

				passes_.push_back( std::move(fxPass) );
			}

			if ( passes_.size() > 255 )
			{
				THROW_MESSAGE( "%s: Pass count exceeded! Max=255, found=%u", filename_.c_str(), (u32)passes_.size() );
			}
		}

		struct _Array
		{
			const config_setting_t* listDef;
			int index;
			int len;
		};

		struct _Array2
		{
			const u32* progIndices;
			int index;
			int len;
		};

		static std::unique_ptr<FxProgram> _CopyProgram( const std::unique_ptr<FxProgram>& src )
		{
			std::unique_ptr<FxProgram> p = std::make_unique<FxProgram>( *src );
			p->refCount_ = 0;
			p->compilerParams.cdefines2.clear();
			return p;
		}

		static std::unique_ptr<FxPass> _CopyPass( const FxPass& src )
		{
			std::unique_ptr<FxPass> p = std::make_unique<FxPass>( src );
			p->hasCombinations_ = false;
			return p;
		}

		void FxFile::_GenCombinations( std::vector<u32>& combinations, const FxPass& fxPass )
		{
			const std::unique_ptr<FxProgram>& fxProgFirstCombination = uniquePrograms_[combinations[0]];
			const config_setting_t* prog = reinterpret_cast<const config_setting_t*>( fxProgFirstCombination->opaque_ );

			const config_setting_t* cdefines = config_setting_get_member( prog, "cdefines" );
			const int nElements = config_setting_length( cdefines );

			std::vector<_Array> arrays;

			for ( int ie = 0; ie < nElements; ++ie )
			{
				const config_setting_t* def = config_setting_get_elem( cdefines, ie );
				int settType = config_setting_type( def );
				if ( settType == CONFIG_TYPE_LIST )
				{
					int nElementsInList = config_setting_length( def );
					if ( nElementsInList <= 1 )
					{
						THROW_MESSAGE( "%s: pass, '%s', shading program '%s', cdefines doesn't have any settings!", filename_.c_str(), fxPass.passName.c_str(), fxProgFirstCombination->entryName.c_str() );
					}
					else
					{
						_Array a;
						a.listDef = def;
						a.index = 0;
						a.len = nElementsInList;
						arrays.push_back( a );
					}
				}
			}

			bool firstCombination = true;
			bool done = false;
			const size_t nArrays = arrays.size();
			int nCombinations = 1;
			while ( !done )
			{
				// save combination
				//
				if ( !firstCombination )
				{
					nCombinations += 1;

					std::unique_ptr<FxProgram> prCopy = _CopyProgram( fxProgFirstCombination );

					for ( size_t iarray = 0; iarray < nArrays; ++iarray )
					{
						const _Array& arr = arrays[iarray];

						const config_setting_t* def = config_setting_get_elem( arr.listDef, arr.index );
						int defType = config_setting_type( def );
						if ( defType != CONFIG_TYPE_STRING )
						{
							THROW_MESSAGE( "%s: pass '%s', shading program '%s', cdefines, each define must be a string!", filename_.c_str(), fxPass.passName.c_str(), fxProgFirstCombination->entryName.c_str() );
						}

						const char* defName = config_setting_name( arr.listDef );
						const char* defVal = config_setting_get_string( def );
						FR_ASSERT( defName && defVal );

						FxProgDefine d;
						d.name = defName;
						d.value = defVal;

						prCopy->compilerParams.cdefines2.push_back( d );
					}

					int isp = _FindMatchingProgram( prCopy, fxPass );

					if ( isp >= 0 )
					{
						std::unique_ptr<FxProgram>& fxProg = uniquePrograms_[isp];
						fxProg->refCount_ += 1;
						combinations.push_back( isp );
					}
					else
					{
						prCopy->refCount_ = 1;
						combinations.push_back( (u32)uniquePrograms_.size() );
						uniquePrograms_.push_back( std::move(prCopy) );
					}
				}

				firstCombination = false;

				for ( size_t iarray = 0; iarray < nArrays; ++iarray )
				{
					_Array& arr = arrays[iarray];
					arr.index += 1;
					if ( arr.index < arr.len )
					{
						// print next combination
						//
						break;
					}
					else if ( iarray == nArrays - 1 )
					{
						// last array is done
						//
						done = true;
						break;
					}
					else
					{
						arr.index = 0;
					}
				}
			}
		}

		void FxFile::_HandleCombinations()
		{
			const size_t nFxPasses = passes_.size();
			for ( size_t ipass = 0; ipass < nFxPasses; ++ipass )
			{
				FxPass& fxPass = *passes_[ipass];
				const std::string fxPassName = fxPass.passName;
				if ( fxPass.hasCombinations_ )
				{
					std::vector<u32> programStages[eProgramType_count];

					for ( u32 iprofile = 0; iprofile < eProgramType_count; ++iprofile )
					{
						const u32 progIndex = fxPass.entryIdx[iprofile];
						if ( progIndex != 0xffffffff )
						{
							std::unique_ptr<FxProgram>& firstCombination = uniquePrograms_[progIndex];
							programStages[iprofile].push_back( progIndex );

							if ( firstCombination->compilerParams.cdefines2.size() )
							{
								// this program has multiple combinations
								//
								_GenCombinations( programStages[iprofile], fxPass );
							}
						}
					}

					// now generate combinations from programStages
					//
					_Array2 arrays[eProgramType_count];
					size_t nArrays = 0;
					for ( u32 iprofile = 0; iprofile < eProgramType_count; ++iprofile )
					{
						const u32 nCombinations = (u32)programStages[iprofile].size();
						if ( nCombinations )
						{
							_Array2& arr = arrays[nArrays];
							++nArrays;
							arr.progIndices = &programStages[iprofile][0];
							arr.index = 0;
							arr.len = nCombinations;
						}
					}

					bool firstCombination = true;
					bool done = false;
					int nCombinations = 0;
					while ( !done )
					{
						// save combination
						//
						nCombinations += 1;

						if ( passes_.size() > 255 )
						{
							done = true;
							break;
						}

						// print combination
						//
						std::string passName = fxPassName;

						for ( size_t iarray = 0; iarray < nArrays; ++iarray )
						{
							const _Array2& arr = arrays[iarray];
							const u32 progIndex = arr.progIndices[arr.index];
							const FxProgram& fxProg = *uniquePrograms_[progIndex];

							const size_t nDefines = fxProg.compilerParams.cdefines2.size();
							for ( size_t idefine = 0; idefine < nDefines; ++idefine )
							{
								const FxProgDefine& d = fxProg.compilerParams.cdefines2[idefine];
								passName.append( 1, '_' );
								passName.append( d.name );
								passName.append( 1, '=' );
								passName.append( d.value );
							}
						}

						if ( firstCombination )
						{
							firstCombination = false;
							fxPass.passName = passName;
						}
						else
						{
							logInfo( "Adding pass combination '%s'.", passName.c_str() );

							std::unique_ptr<FxPass> p = _CopyPass( fxPass );
							p->passName = passName;

							for ( size_t iarray = 0; iarray < nArrays; ++iarray )
							{
								const _Array2& arr = arrays[iarray];
								const u32 progIndex = arr.progIndices[arr.index];
								const FxProgram& fxProg = *uniquePrograms_[progIndex];
								p->entryIdx[fxProg.profile] = progIndex;
							}

							passes_.push_back( std::move( p ) );
						}

						for ( size_t iarray = 0; iarray < nArrays; ++iarray )
						{
							_Array2& arr = arrays[iarray];
							arr.index += 1;
							if ( arr.index < arr.len )
							{
								// print next combination
								//
								break;
							}
							else if ( iarray == nArrays - 1 )
							{
								// last array is done
								//
								done = true;
								break;
							}
							else
							{
								arr.index = 0;
							}
						}
					}
				}
			}

			const size_t nUniquePrograms = uniquePrograms_.size();
			for ( size_t iup = 0; iup < nUniquePrograms; ++iup )
			{
				FxProgram& fxProg = *uniquePrograms_[iup];
				fxProg.opaque_ = nullptr;
			}

			if ( passes_.size() > 255 )
			{
				THROW_MESSAGE( "Pass count exceeded! Max=255, found=%u", filename_.c_str(), (u32)passes_.size() );
			}
		}

		void FxFile::_ReadProgram( const void* progPtr, e_ProgramType programProfile, FxPass& fxPass )
		{
			const config_setting_t* prog = reinterpret_cast<const config_setting_t*>( progPtr );
			std::unique_ptr<FxProgram> fxProg = std::make_unique<FxProgram>();
			fxProg->profile = programProfile;
			fxProg->opaque_ = const_cast<config_setting_t*>( prog );

			int progType = config_setting_type( prog );
			if ( progType == CONFIG_TYPE_STRING )
			{
				const char* sval = config_setting_get_string( prog );
				FR_ASSERT( sval );
				fxProg->entryName = sval;
			}
			else if ( progType == CONFIG_TYPE_GROUP )
			{
				int nProgSettings = config_setting_length( prog );
				if ( !nProgSettings )
				{
					THROW_MESSAGE( "%s: pass '%s', Shading program doesn't have any settings!", filename_.c_str(), fxPass.passName.c_str() );
				}

				// first element must be entry name
				//
				const config_setting_t* entryNameSett = config_setting_get_elem( prog, 0 );
				const char* entryNameSettName = config_setting_name( entryNameSett );
				if ( strcmp( entryNameSettName, "EntryName" ) )
				{
					THROW_MESSAGE( "%s: pass '%s', First element of shading block must be 'EntryName'! Found '%s'.", filename_.c_str(), fxPass.passName.c_str(), entryNameSettName );
				}

				int settType = config_setting_type( entryNameSett );
				if ( settType != CONFIG_TYPE_STRING )
				{
					THROW_MESSAGE( "%s: pass '%s', Shading program 'EntryName' must be string!", filename_.c_str(), fxPass.passName.c_str() );
				}

				const char* sval = config_setting_get_string( entryNameSett );
				FR_ASSERT( sval );
				fxProg->entryName = sval;

				for ( int ipset = 1; ipset < nProgSettings; ++ipset )
				{
					const config_setting_t* psett = config_setting_get_elem( prog, ipset );
					FR_ASSERT( psett );
					const char* psettName = config_setting_name( psett );
					FR_ASSERT( psettName );

					if ( 0 == strcmp( psettName, "RuntimeCompilation" ) )
					{
						int settType2 = config_setting_type( psett );
						if ( settType2 != CONFIG_TYPE_INT )
						{
							THROW_MESSAGE( "%s: pass '%s', Shading program '%s', RuntimeCompilation must be an int!", filename_.c_str(), fxPass.passName.c_str(), fxProg->entryName.c_str() );
						}

						int ival = config_setting_get_int( psett );
						fxProg->compilerParams.runtimeCompilation = ival;
					}
					else if ( 0 == strcmp( psettName, "cflags_hlsl" ) )
					{
						const config_setting_t* cflags_hlsl = psett;
						if ( cflags_hlsl )
						{
							int settType2 = config_setting_type( cflags_hlsl );
							if ( settType2 != CONFIG_TYPE_STRING )
							{
								THROW_MESSAGE( "%s: pass '%s', Shading program's 'cflags_hlsl' must be a string!", filename_.c_str(), fxPass.passName.c_str() );
							}

							fxProg->compilerParams.cflags_hlsl = config_setting_get_string( cflags_hlsl );
						}
					}
					else if ( 0 == strcmp( psettName, "cdefines" ) )
					{
						// compiler cdefines
						//
						int settType2 = config_setting_type( psett );
						if ( settType2 != CONFIG_TYPE_GROUP )
						{
							THROW_MESSAGE( "%s: pass '%s', Shading program '%s', cdefines must be a group!", filename_.c_str(), fxPass.passName.c_str(), fxProg->entryName.c_str() );
						}

						int nElements = config_setting_length( psett );
						if ( !nElements )
						{
							THROW_MESSAGE( "%s: pass, '%s', Shading program '%s', cflags doesn't have any settings!", filename_.c_str(), fxPass.passName.c_str(), fxProg->entryName.c_str() );
						}

						for ( int ie = 0; ie < nElements; ++ie )
						{
							const config_setting_t* def = config_setting_get_elem( psett, ie );
							FR_ASSERT( def );
							int settType3 = config_setting_type( def );
							if ( settType3 == CONFIG_TYPE_LIST )
							{
								int nElementsInList = config_setting_length( def );
								if ( !nElementsInList )
								{
									THROW_MESSAGE( "%s: pass, '%s', Shading program '%s', cdefines doesn't have any settings!", filename_.c_str(), fxPass.passName.c_str(), fxProg->entryName.c_str() );
								}
								else
								{
									const char* defName = config_setting_name( def );

									// take only the first define from each list
									// combinations will be generated later
									//
									const config_setting_t* ilistDef = config_setting_get_elem( def, 0 );
									FR_ASSERT( ilistDef );
									int ilistDefType = config_setting_type( ilistDef );
									if ( ilistDefType != CONFIG_TYPE_STRING )
									{
										THROW_MESSAGE( "%s: pass '%s', Shading program '%s', cdefines, each define must be a string!", filename_.c_str(), fxPass.passName.c_str(), fxProg->entryName.c_str() );
									}

									const char* defVal = config_setting_get_string( ilistDef );
									FR_ASSERT( defName && defVal );

									FxProgDefine d;
									d.name = defName;
									d.value = defVal;

									if ( nElementsInList > 1 )
									{
										fxPass.hasCombinations_ = true;
										fxProg->compilerParams.cdefines2.push_back( d );
									}
									else
										fxProg->compilerParams.cdefines.push_back( d );
								}
							}
							else if ( settType == CONFIG_TYPE_STRING )
							{
								const char* defName = config_setting_name( def );
								const char* defVal = config_setting_get_string( def );
								FR_ASSERT( defName && defVal );

								FxProgDefine d;
								d.name = defName;
								d.value = defVal;

								fxProg->compilerParams.cdefines.push_back( d );
							}
							else
							{
								THROW_MESSAGE( "%s: pass '%s', Shading program '%s', cdefines, each define must be a string or a list!", filename_.c_str(), fxPass.passName.c_str(), fxProg->entryName.c_str() );
							}
						}
					}
					else
					{
						THROW_MESSAGE( "%s: pass '%s', Shading program '%s' has unknown setting '%s'!", filename_.c_str(), fxPass.passName.c_str(), fxProg->entryName.c_str(), psettName );
					}
				}

			}
			else
			{
				THROW_MESSAGE( "%s: pass '%s', Shading program must be string or group!", filename_.c_str(), fxPass.passName.c_str() );
			}

			int isp = _FindMatchingProgram( fxProg, fxPass );

			if ( isp >= 0 )
			{
				FxProgram& fxProgExisting = *uniquePrograms_[isp];
				fxProgExisting.refCount_ += 1;
				fxPass.entryIdx[programProfile] = (u32)isp;
			}
			else
			{
				fxProg->refCount_ += 1;
				fxPass.entryIdx[programProfile] = (u32)uniquePrograms_.size();
				uniquePrograms_.push_back( std::move(fxProg) );
			}
		}

		int FxFile::_FindMatchingProgram( const std::unique_ptr<FxProgram>& fxProg, const FxPass& fxPass )
		{
			const size_t siz = uniquePrograms_.size();
			size_t isp = 0;
			for ( ; isp < siz; ++isp )
			{
				const FxProgram& sp = *uniquePrograms_[isp];

				if ( sp.entryName == fxProg->entryName )
				{
					if ( sp.profile != fxProg->profile )
					{
						THROW_MESSAGE( "%s: pass '%s', Shading program profile mismatch. First visible as '%d', now '%s.'", filename_.c_str(), fxPass.passName.c_str(), sp.profile, fxProg->profile );
					}

					// compare compiler flags
					//
					const FxProgramCompilerParams& cf0 = sp.compilerParams;
					const FxProgramCompilerParams& cf1 = fxProg->compilerParams;

					if ( cf0.runtimeCompilation != cf1.runtimeCompilation )
						continue;

					if ( cf0.cflags_hlsl != cf1.cflags_hlsl )
						continue;

					if ( cf0.cdefines.size() != cf1.cdefines.size() )
						continue;

					if ( cf0.cdefines2.size() != cf1.cdefines2.size() )
						continue;

					bool doContinue = false;

					const size_t cdefinesSiz = cf0.cdefines.size();
					for ( size_t id = 0; id < cdefinesSiz; ++id )
					{
						const FxProgDefine& d0 = cf0.cdefines[id];
						const FxProgDefine& d1 = cf1.cdefines[id];

						if ( d0.name != d1.name || d0.value != d1.value )
						{
							doContinue = true;
							break;
						}
					}

					if ( doContinue )
						continue;

					const size_t cdefines2Siz = cf0.cdefines2.size();
					for ( size_t id = 0; id < cdefines2Siz; ++id )
					{
						const FxProgDefine& d0 = cf0.cdefines2[id];
						const FxProgDefine& d1 = cf1.cdefines2[id];

						if ( d0.name != d1.name || d0.value != d1.value )
						{
							doContinue = true;
							break;
						}
					}

					if ( doContinue )
						continue;

					break;
				}
			}

			if ( isp < siz )
				return (int)isp;
			else
				return -1;
		}

		void FxFile::_CompileFxFileImpl( const char* srcFilePathAbsolute, const FxFileCompileOptions& options )
		{
			std::string sourceCode = spad::ReadTextFileAsString( srcFilePathAbsolute );
			if ( sourceCode.empty() )
			{
				// fix me!!! wchar handled incorrectly
				//
				THROW_MESSAGE( "Failed to read '%s'", srcFilePathAbsolute );
			}

			filename_ = srcFilePathAbsolute;
			sourceCode_ = std::move( sourceCode );

			_Parse();

			FxFileCompileOptions compileOptions;
			compileOptions.mainFilename_ = srcFilePathAbsolute;

			if ( options.compileForDebugging_ )
			{
				FxProgDefine d;
				d.name = "DEBUG";
				d.value = "1";
				compileOptions.defines_.emplace_back( d );
			}
			else
			{
				{
					FxProgDefine d;
					d.name = "NDEBUG";
					d.value = "1";
					compileOptions.defines_.emplace_back( d );
				}
				{
					FxProgDefine d;
					d.name = "RELEASE";
					d.value = "1";
					compileOptions.defines_.emplace_back( d );
				}
			}

			compileOptions.logProgress_ = true;
			compileOptions.compileForDebugging_ = options.compileForDebugging_;

			FxFileHlslCompileOptions hlslCompileOptions;
			hlslCompileOptions.generatePreprocessedOutput = true;

			_CompileHlsl( compileOptions, hlslCompileOptions );
		}

		void FxFile::_LoadFxFileImpl( const char* srcFilePathAbsolute )
		{
			std::string sourceCode = spad::ReadTextFileAsString( srcFilePathAbsolute );
			if ( sourceCode.empty() )
			{
				// fix me!!! wchar handled incorrectly
				//
				THROW_MESSAGE( "Failed to read '%s'", srcFilePathAbsolute );
			}

			filename_ = srcFilePathAbsolute;
			sourceCode_ = std::move( sourceCode );

			_Parse();

			_ReadCompiledHlsl();
		}

		void FxFile::_CreateShaders( ID3D11Device* dxDevice )
		{
			for ( auto& p : uniquePrograms_ )
			{
				if ( p->profile == eProgramType_vertexShader )
				{
					HRESULT hr = dxDevice->CreateVertexShader( p->hlslProgramData_.shaderBlob_->GetBufferPointer()
						, p->hlslProgramData_.shaderBlob_->GetBufferSize()
						, nullptr
						, &p->hlslProgramData_.vs_ );
					if ( FAILED( hr ) )
						THROW_DX_EXCEPTION( Exception::FormatMessage( "CreateVertexShader failed. (%s, %s)", p->entryName.c_str(), filename_.c_str() ), hr );

					Dx11SetDebugName3( p->hlslProgramData_.vs_, "%s - %s", filename_.c_str(), p->entryName.c_str() );

					hr = D3DGetInputSignatureBlob( p->hlslProgramData_.shaderBlob_->GetBufferPointer()
						, p->hlslProgramData_.shaderBlob_->GetBufferSize()
						, &p->hlslProgramData_.vsInputSignature_ );
					if ( FAILED( hr ) )
						THROW_DX_EXCEPTION( Exception::FormatMessage( "D3DGetInputSignatureBlob failed. (%s, %s)", p->entryName.c_str(), filename_.c_str() ), hr );

					p->hlslProgramData_.vsInputSignatureHash_ = MurmurHash3( p->hlslProgramData_.vsInputSignature_->GetBufferPointer()
						, (int)p->hlslProgramData_.vsInputSignature_->GetBufferSize() );
				}
				else if ( p->profile == eProgramType_pixelShader )
				{
					HRESULT hr = dxDevice->CreatePixelShader( p->hlslProgramData_.shaderBlob_->GetBufferPointer()
						, p->hlslProgramData_.shaderBlob_->GetBufferSize()
						, nullptr
						, &p->hlslProgramData_.ps_ );
					if ( FAILED( hr ) )
						THROW_DX_EXCEPTION( Exception::FormatMessage( "CreatePixelShader failed. (%s, %s)", p->entryName.c_str(), filename_.c_str() ), hr );

					Dx11SetDebugName3( p->hlslProgramData_.ps_, "%s - %s", filename_.c_str(), p->entryName.c_str() );
				}
				else if ( p->profile == eProgramType_geometryShader )
				{
					HRESULT hr = dxDevice->CreateGeometryShader( p->hlslProgramData_.shaderBlob_->GetBufferPointer()
						, p->hlslProgramData_.shaderBlob_->GetBufferSize()
						, nullptr
						, &p->hlslProgramData_.gs_ );
					if ( FAILED( hr ) )
						THROW_DX_EXCEPTION( Exception::FormatMessage( "CreateGeometryShader failed. (%s, %s)", p->entryName.c_str(), filename_.c_str() ), hr );

					Dx11SetDebugName3( p->hlslProgramData_.gs_, "%s - %s", filename_.c_str(), p->entryName.c_str() );
				}
				else if ( p->profile == eProgramType_computeShader )
				{
					HRESULT hr = dxDevice->CreateComputeShader( p->hlslProgramData_.shaderBlob_->GetBufferPointer()
						, p->hlslProgramData_.shaderBlob_->GetBufferSize()
						, nullptr
						, &p->hlslProgramData_.cs_ );
					if ( FAILED( hr ) )
						THROW_DX_EXCEPTION( Exception::FormatMessage( "CreateComputeShader failed. (%s, %s)", p->entryName.c_str(), filename_.c_str() ), hr );

					Dx11SetDebugName3( p->hlslProgramData_.cs_, "%s - %s", filename_.c_str(), p->entryName.c_str() );
				}
				else
				{
					THROW_MESSAGE( "Unsupported shader profile: %s - %s", filename_.c_str(), p->entryName.c_str() );
				}
			}
		}

		std::unique_ptr<FxRuntime> FxFile::_CreateFxRuntime( ID3D11Device* dxDevice ) const
		{
			// handle exceptions here somehow
			//

			std::unique_ptr<FxRuntime> fxr = std::make_unique<FxRuntime>();
			fxr->filename_ = filename_;

			// programs contains auto pointers to programs, they are properly released in case of an exception
			//
			std::vector<ID3D11DeviceChildPtr> programs( uniquePrograms_.size() );
			std::vector<ID3D10BlobPtr> vsInputSignatures( uniquePrograms_.size() );

			for ( const auto& pass : passes_ )
			{
				FxRuntimePass rp;

				{
					u32 vpIdx = pass->entryIdx[eProgramType_vertexShader];
					if ( vpIdx != 0xffffffff )
					{
						if ( !programs[vpIdx] )
						{
							const FxProgram& p = *uniquePrograms_[vpIdx];
							ID3D11VertexShader* vs;
							HRESULT hr = dxDevice->CreateVertexShader( p.hlslProgramData_.shaderBlob_->GetBufferPointer(), p.hlslProgramData_.shaderBlob_->GetBufferSize(), nullptr, &vs );
							if ( FAILED( hr ) )
								THROW_DX_EXCEPTION( Exception::FormatMessage( "CreateVertexShader failed. (%s, %s)", p.entryName.c_str(), filename_.c_str() ), hr );

							Dx11SetDebugName3( vs, "%s - %s:%s", filename_.c_str(), pass->passName.c_str(), p.entryName.c_str() );

							programs[vpIdx] = vs;

							ID3D10Blob* inputSignature = NULL;
							hr = D3DGetInputSignatureBlob( p.hlslProgramData_.shaderBlob_->GetBufferPointer(), p.hlslProgramData_.shaderBlob_->GetBufferSize(), &inputSignature );
							if ( FAILED( hr ) )
								THROW_DX_EXCEPTION( Exception::FormatMessage( "D3DGetInputSignatureBlob failed. (%s, %s)", p.entryName.c_str(), filename_.c_str() ), hr );
							vsInputSignatures[vpIdx] = inputSignature;
						}

						ID3D11DeviceChild* vs = programs[vpIdx];
						vs->AddRef();
						rp.vs_ = reinterpret_cast<ID3D11VertexShader*>( vs );
						rp.vsInputSignature_ = vsInputSignatures[vpIdx];
						rp.vsInputSignature_->AddRef();

						rp.vsInputSignatureHash_ = MurmurHash3( rp.vsInputSignature_->GetBufferPointer(), (int)rp.vsInputSignature_->GetBufferSize() );
					}
				}

				{
					u32 fpIdx = pass->entryIdx[eProgramType_pixelShader];
					if ( fpIdx != 0xffffffff )
					{
						if ( !programs[fpIdx] )
						{
							const FxProgram& p = *uniquePrograms_[fpIdx];
							ID3D11PixelShader* ps;
							HRESULT hr = dxDevice->CreatePixelShader( p.hlslProgramData_.shaderBlob_->GetBufferPointer(), p.hlslProgramData_.shaderBlob_->GetBufferSize(), nullptr, &ps );
							if ( FAILED( hr ) )
								THROW_DX_EXCEPTION( Exception::FormatMessage( "CreatePixelShader failed. (%s, %s)", p.entryName.c_str(), filename_.c_str() ), hr );

							Dx11SetDebugName3( ps, "%s - %s:%s", filename_.c_str(), pass->passName.c_str(), p.entryName.c_str() );

							programs[fpIdx] = ps;
						}

						ID3D11DeviceChild* ps = programs[fpIdx];
						ps->AddRef();
						rp.ps_ = reinterpret_cast<ID3D11PixelShader*>( ps );
					}
				}

				{
					u32 gpIdx = pass->entryIdx[eProgramType_geometryShader];
					if ( gpIdx != 0xffffffff )
					{
						if ( !programs[gpIdx] )
						{
							const FxProgram& p = *uniquePrograms_[gpIdx];
							ID3D11GeometryShader* gs;
							HRESULT hr = dxDevice->CreateGeometryShader( p.hlslProgramData_.shaderBlob_->GetBufferPointer(), p.hlslProgramData_.shaderBlob_->GetBufferSize(), nullptr, &gs );
							if ( FAILED( hr ) )
								THROW_DX_EXCEPTION( Exception::FormatMessage( "CreateGeometryShader failed. (%s, %s)", p.entryName.c_str(), filename_.c_str() ), hr );

							Dx11SetDebugName3( gs, "%s - %s:%s", filename_.c_str(), pass->passName.c_str(), p.entryName.c_str() );

							programs[gpIdx] = gs;
						}

						ID3D11DeviceChild* gs = programs[gpIdx];
						gs->AddRef();
						rp.gs_ = reinterpret_cast<ID3D11GeometryShader*>( gs );
					}
				}

				{
					u32 csIdx = pass->entryIdx[eProgramType_computeShader];
					if ( csIdx != 0xffffffff )
					{
						if ( !programs[csIdx] )
						{
							const FxProgram& p = *uniquePrograms_[csIdx];
							ID3D11ComputeShader* cs;
							HRESULT hr = dxDevice->CreateComputeShader( p.hlslProgramData_.shaderBlob_->GetBufferPointer(), p.hlslProgramData_.shaderBlob_->GetBufferSize(), nullptr, &cs );
							if ( FAILED( hr ) )
								THROW_DX_EXCEPTION( Exception::FormatMessage( "CreateComputeShader failed. (%s, %s)", p.entryName.c_str(), filename_.c_str() ), hr );

							Dx11SetDebugName3( cs, "%s - %s:%s", filename_.c_str(), pass->passName.c_str(), p.entryName.c_str() );

							programs[csIdx] = cs;
						}

						ID3D11DeviceChild* cs = programs[csIdx];
						cs->AddRef();
						rp.cs_ = reinterpret_cast<ID3D11ComputeShader*>( cs );
					}
				}

				fxr->passes_[pass->passName] = rp;
			}

			return fxr;
		}

		std::unique_ptr<FxLib::FxRuntime> loadCompiledFxFile( ID3D11Device* dxDevice, const char* filename )
		{
			FxLib::FxFile fxFile;
			std::string fileFullPath = GetAbsolutePath( filename );
			int ires = fxFile.loadCompiledFxFile( fileFullPath.c_str() );
			FR_ASSERT( ires == 0 );

			return fxFile.createFxRuntime( dxDevice );
		}

	} // namespace FxLib

} // namespace spad
