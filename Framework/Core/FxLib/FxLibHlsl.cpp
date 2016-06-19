#include "FxLib_pch.h"
#include "FxLib.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

_COM_SMARTPTR_TYPEDEF( ID3D10Blob, __uuidof( ID3D10Blob ) );

namespace spad
{
namespace FxLib
{

void FxFile::_CompileHlslProgram( size_t progIdx, const FxFileCompileOptions& options, const FxFileHlslCompileOptions& hlslOptions )
{
	FxProgram& fxProg = *uniquePrograms_[progIdx];

	if ( options.logProgress_ )
		logInfo( "Compiling %s:%s", options.mainFilename_.c_str(), fxProg.entryName.c_str() );

	const char* profiles[eProgramType_count] = {
		"vs_",
		"ps_",
		"gs_",
		"cs_"
	};

	char profileName[16];
	strcpy( profileName, profiles[fxProg.profile] );
	strcpy( profileName + 3, "5_0" );

	// defines
	//
	const std::vector<FxProgDefine>& progCDefines = fxProg.compilerParams.cdefines;
	const std::vector<FxProgDefine>& progCDefines2 = fxProg.compilerParams.cdefines2;
	const size_t nProgCDefines = progCDefines.size();
	const size_t nProgCDefines2 = progCDefines2.size();

	std::vector<D3D10_SHADER_MACRO> defines;
	defines.resize( nProgCDefines + nProgCDefines2 + options.defines_.size() + 2 + 1 + 1 );

	u32 defIdx = 0;

	for ( size_t icd = 0; icd < nProgCDefines; ++icd )
	{
		const FxProgDefine& src = progCDefines[icd];
		D3D10_SHADER_MACRO& dst = defines[defIdx];
		++defIdx;
		dst.Name = src.name.c_str();
		dst.Definition = src.value.c_str();
	}

	for ( size_t icd = 0; icd < nProgCDefines2; ++icd )
	{
		const FxProgDefine& src = progCDefines2[icd];
		D3D10_SHADER_MACRO& dst = defines[defIdx];
		++defIdx;
		dst.Name = src.name.c_str();
		dst.Definition = src.value.c_str();
	}

	for ( size_t iud = 0; iud < options.defines_.size(); ++iud )
	{
		const FxProgDefine& src = options.defines_[iud];
		D3D10_SHADER_MACRO& dst = defines[defIdx];
		++defIdx;
		dst.Name = src.name.c_str();
		dst.Definition = src.value.c_str();
	}

	char entryDefine[256];
	fr_snprintf( entryDefine, 256, "prog_%s", fxProg.entryName.c_str() );
	{
		D3D10_SHADER_MACRO& d = defines[defIdx];
		++defIdx;
		d.Name = entryDefine;
		d.Definition = "1";
	}

	{
		D3D10_SHADER_MACRO& d = defines[defIdx];
		++defIdx;

		if ( fxProg.profile == eProgramType_vertexShader )
		{
			d.Name = "progType_vp";
			d.Definition = "1";
		}
		else if ( fxProg.profile == eProgramType_pixelShader )
		{
			d.Name = "progType_fp";
			d.Definition = "1";
		}
		else if ( fxProg.profile == eProgramType_geometryShader )
		{
			d.Name = "progType_gp";
			d.Definition = "1";
		}
		else if ( fxProg.profile == eProgramType_computeShader )
		{
			d.Name = "progType_cp";
			d.Definition = "1";
		}
		else
		{
			FR_NOT_IMPLEMENTED;
		}
	}

	{
		D3D10_SHADER_MACRO& d = defines[defIdx];
		++defIdx;
		d.Name = "__HLSL__";
		d.Definition = "1";
	}

	defines.back().Name = NULL;
	defines.back().Definition = NULL;

	// compiler flags
	//
	UINT flagCombination = 0;

	if ( options.compileForDebugging_ )
	{
		flagCombination |= D3DCOMPILE_DEBUG;
		flagCombination |= D3DCOMPILE_SKIP_OPTIMIZATION;
		flagCombination |= D3DCOMPILE_AVOID_FLOW_CONTROL;
	}
	else
	{
		if ( !( flagCombination & ( D3DCOMPILE_OPTIMIZATION_LEVEL0 | D3DCOMPILE_OPTIMIZATION_LEVEL1 | D3DCOMPILE_OPTIMIZATION_LEVEL2 | D3DCOMPILE_OPTIMIZATION_LEVEL3 ) ) )
		{
			flagCombination |= D3DCOMPILE_OPTIMIZATION_LEVEL1;
		}
	}

	flagCombination |= hlslOptions.dxCompilerFlags;

	HRESULT hr = S_OK;
	ID3D10Blob* shaderBlob = NULL;
	ID3D10BlobPtr errBlob = NULL;

	hr = D3DCompileFromFile(
		stringToWstring( options.mainFilename_ ).c_str(), // __in   LPCSTR pFileName,
		&defines[0], // __in   const D3D10_SHADER_MACRO *pDefines,
		D3D_COMPILE_STANDARD_FILE_INCLUDE, // _In_opt_ ID3DInclude* pInclude
		fxProg.entryName.c_str(), //__in   LPCSTR pFunctionName,
		profileName, //__in   LPCSTR pProfile,
		flagCombination, // __in   UINT Flags1,
		0, // __in   UINT Flags2,
		&shaderBlob, // __out  ID3D10Blob **ppShader,
		&errBlob // __out  ID3D10Blob **ppErrorMsgs,
		);

	if ( FAILED( hr ) )
	{
		std::string errMsg = Exception::FormatMessage( "Error '0x%x' while compiling program '%s_%s_%u.%s'!", (u32)hr, options.mainFilename_.c_str(), fxProg.entryName.c_str(), (u32)progIdx, profileName );
		if ( errBlob )
		{
			THROW_HLSL_EXCEPTION(
				std::move( errMsg ),
				reinterpret_cast<const char*>( errBlob->GetBufferPointer() ),
				hr
				);
		}
		else
		{
			THROW_HLSL_EXCEPTION( std::move( errMsg ), "", hr );
		}
	}

	fxProg.hlslProgramData_.shaderBlob_ = shaderBlob;

}

void FxFile::_CompileHlsl( const FxFileCompileOptions& options, const FxFileHlslCompileOptions& hlslOptions )
{
	const size_t nUniquePrograms = uniquePrograms_.size();

	for ( size_t iprog = 0; iprog < nUniquePrograms; ++iprog )
	{
		_CompileHlslProgram( iprog, options, hlslOptions );
	}
}

void FxFile::_WriteCompiledHlsl( const FxFileWriteOptions& options )
{
	std::string srcFilenameWithoutExtension = GetFileNameWithoutExtension( filename_ );
	std::string srcFilenameExtension = GetFileExtension( filename_ );

	//std::string srcDir = GetDirectoryFromFilePath( filename_ );
	std::string outDir = options.outputDirectory_;
	AppendSlashToDirectoryName( outDir );

	if ( options.writeSource_ )
	{
		CreateDirectoryRecursive( outDir );

		std::stringstream sf;
		sf << outDir;
		sf << srcFilenameWithoutExtension;
		sf << ".hlsl";
		std::string dstPath = sf.str();

		if ( options.logProgress_ )
			logInfo( "Writing file '%s'", dstPath.c_str() );

		if ( WriteFileSync( dstPath.c_str(), sourceCode_.c_str(), (uint32_t)sourceCode_.size() ) )
		{
			THROW_MESSAGE( "Error while writing source hlsl file '%s'!", dstPath.c_str() );
		}
	}

	bool createDir = true;

	// write compiled programs
	//
	for ( size_t pidx = 0; pidx < uniquePrograms_.size(); ++pidx )
	{
		const FxProgram& fxProg = *uniquePrograms_[pidx];

		if ( options.writeCompiled_ )
		{
			ID3D10Blob* compiledBlob = fxProg.hlslProgramData_.shaderBlob_;

			// build dst path
			//
			std::stringstream cf;
			cf << outDir;
			cf << "compiled\\";
			cf << srcFilenameWithoutExtension;
			cf << "_";
			cf << fxProg.entryName;
			cf << "_";
			cf << pidx;
			cf << ".hlslc";
			std::string dstPath = cf.str();

			if ( createDir )
			{
				// create directory in first iteration
				//
				createDir = false;
				CreateDirectoryRecursive( dstPath );
			}

			if ( options.logProgress_ )
				logInfo( "Writing file '%s'", dstPath.c_str() );

			if ( WriteFileSync( dstPath.c_str(), (u8*)compiledBlob->GetBufferPointer(), (u32)compiledBlob->GetBufferSize() ) )
			{
				THROW_MESSAGE( "Error while writing compiled hlsl file '%s'!", dstPath.c_str() );
			}
		}
	}
}

void FxFile::_ReadCompiledHlsl()
{
	std::string srcFilenameWithoutExtension = GetFileNameWithoutExtension( filename_ );
	std::string srcFilenameExtension = GetFileExtension( filename_ );

	std::string srcDir = GetDirectoryFromFilePath( filename_ );

	// write compiled programs
	//
	for ( size_t pidx = 0; pidx < uniquePrograms_.size(); ++pidx )
	{
		FxProgram& fxProg = *uniquePrograms_[pidx];

		// build dst path
		//
		std::stringstream cf;
		cf << srcDir;
		cf << "compiled\\";
		cf << srcFilenameWithoutExtension;
		cf << "_";
		cf << fxProg.entryName;
		cf << "_";
		cf << pidx;
		cf << ".hlslc";
		std::string programPath = cf.str();

		std::vector<uint8_t> fileData = ReadFileAsVector( programPath.c_str() );
		if ( fileData.empty() )
		{
			THROW_MESSAGE( "Error while reading compiled hlsl file '%s'!", programPath.c_str() );
		}

		ID3D10Blob* compiledBlob = nullptr;
		HRESULT hr = D3D10CreateBlob( fileData.size(), &compiledBlob );
		if ( FAILED( hr ) )
		{
			THROW_MESSAGE( "D3D10CreateBlob failed. '%s'!", programPath.c_str() );
		}

		fxProg.hlslProgramData_.shaderBlob_ = compiledBlob;
		memcpy( compiledBlob->GetBufferPointer(), &fileData[0], fileData.size() );
	}
}

} // namespace FxLib

} // namespace spad
