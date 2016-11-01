#include "FxCompilerLibHlsl_pch.h"
#include "HlslCompile.h"
#include <Util/Threading.h>
#include <fstream>

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{
namespace fxlib
{
namespace hlsl
{


std::string CreateDebugFileName( const HlslCompileContext& hlslContext, const FxFile& fxFile, const FxProgram& fxProg, const char* fileExt )
{
	std::stringstream ss;
	ss << hlslContext.outputDiagnosticsDir;
	ss << GetFileNameWithoutExtension( fxFile.getFilename() );
	ss << '_';
	ss << fxProg.getEntryName();
	ss << '_';
	ss << fxProg.getIndex();
	ss << fileExt;

	return ss.str();
}

int CompileFxHlsl( const FxFile& fxFile, const CompileContext& ctx, const FxFileCompileOptions& options, const FxFileHlslCompileOptions& hlslOptions )
{
	std::string fileExt = spad::GetFileExtension( fxFile.getFilename() );
	if (fileExt != "hlsl")
		return 0;

	try
	{
		HlslCompileContext hlslContext;
		SetupHlslCompileContext( hlslContext, hlslOptions, fxFile );

		const char* outPaths[2] = { nullptr };
		u32 nOutPaths = 0;
		outPaths[nOutPaths++] = hlslContext.outputCompiledFileAbsolute.c_str();
		if (options.writeSource_)
			outPaths[nOutPaths++] = hlslContext.outputSourceFileAbsolute.c_str();

		if (!options.forceRecompile_
			&& !CheckIfRecompilationIsRequired( fxFile.getFilename().c_str(), hlslContext.outputDependFileAbsolute.c_str(), outPaths, nOutPaths, options.compilerTimestamp_, options.configuration_ ))
		{
			// all files are up-to-date
			logInfo( "%s: hlsl output is up-to-date", fxFile.getFilename().c_str() );
			return 0;
		}

		logInfo( "%s: compiling hlsl", fxFile.getFilename().c_str() );

		CreateDirectoryRecursive( hlslOptions.outputDirectory_ );
		CreateDirectoryRecursive( hlslOptions.intermediateDirectory_ );
		CreateDirectoryRecursive( hlslOptions.outputDirectory_ );
		if (   hlslOptions.generatePreprocessedOutput
			|| hlslOptions.generateDisassembly
			)
			CreateDirectoryRecursive( hlslContext.outputDiagnosticsDir );

		IncludeDependencies includeDependencies;

		const FxProgramArray& uniquePrograms = fxFile.getUniquePrograms();
		const size_t nUniquePrograms = uniquePrograms.size();

		// compile all programs

		std::vector<HlslProgramData> compiledData;
		compiledData.resize( nUniquePrograms );

		u32 nHardwareThreads = GetNumHardwareThreads();
		// limiting number of threads gives better perf
		// there will be more threads in flight than hardware can support due to multiple files being compiled simultaneously
		// using builtin parallel_for seems to be little bit slower

		//ParallerFor( 0, uniquePrograms.size(), false, [&]( size_t index ) {
		//ParallelFor_threadPool( 0, uniquePrograms.size(), options.multithreaded_, [&]( size_t index ) {
		//ParallelFor( 0, uniquePrograms.size(), options.multithreaded_ ? -1 : 1, [&]( size_t index ) {
		ParallelFor( 0, uniquePrograms.size(), options.multithreaded_ ? (nHardwareThreads / 2) : 1, [&]( size_t index ) {
			const FxProgram& fxProg = *uniquePrograms[index].get();
			CompileHlslProgram( compiledData[index], hlslContext, ctx, includeDependencies, hlslOptions, options, fxFile, fxProg );
			return 0;
		} );

		// write compiled programs

		WriteCompiledFx( compiledData, hlslContext, hlslOptions, options, fxFile );

		CreateDirectoryRecursive( hlslOptions.intermediateDirectory_, hlslContext.outputDependFile );
		includeDependencies.writeToFile( hlslContext.outputDependFileAbsolute, options.configuration_ );

		return 0;
	}
	catch (HlslException ex)
	{
		logError( "HLSL Compiler error:" );
		logError( ex.GetHlslErrorMessage().c_str() );
		return -1;
	}
	catch (Exception ex)
	{
		logError( "compileFxFile failed. Err=%s", ex.GetMessage().c_str() );
		return -1;
	}
}

void SetupHlslCompileContext( HlslCompileContext& ctx, const FxFileHlslCompileOptions& hlslOptions, const FxFile& fxFile )
{
	const std::string& dstDir = hlslOptions.outputDirectory_;
	ctx.outputSourceFile = "src\\" + fxFile.getFilename();
	ctx.outputSourceFileAbsolute = dstDir + ctx.outputSourceFile;

	std::string pathWithoutExt = GetFilePathWithoutExtension( fxFile.getFilename() );

	std::stringstream cf;
	cf << "compiled\\";
	cf << pathWithoutExt;
	cf << ".hlslc_packed";
	ctx.outputCompiledFile = cf.str();
	ctx.outputCompiledFileAbsolute = dstDir + ctx.outputCompiledFile;

	ctx.outputDependFile = GetFilePathWithoutExtension( fxFile.getFilename() ) + ".hlsl_depend";
	ctx.outputDependFileAbsolute = hlslOptions.intermediateDirectory_ + ctx.outputDependFile;

	ctx.outputDiagnosticsDir = dstDir + "compiledDiag\\" + fxFile.getFilename();
}

static std::string _FixHlslErrorMessage( const char* msg, const HlslShaderInclude& includes, IncludeCache* includeCache )
{
	// sometimes hlsl output contains path to a file that is relative and visual studio isn't smart enough to
	// redirect us to correct file/line when double clicking on error message
	// we try to replace this relative file with absolute file path so the visual studio knows where to jump
	std::istringstream ssIn( msg );
	std::string line;
	std::stringstream ss;

	const std::set<std::string>& visitedDirectories = includes.GetVisitedDirectories();

	while ( ssIn )
	{
		getline( ssIn, line );
		ss << line << "\n";

		if ( line.empty() )
		{
			continue;
		}

		std::string::size_type openBracePos = line.find( '(' );
		if ( openBracePos == std::string::npos )
		{
			continue;
		}

		std::string::size_type closeBracePos = line.find( "): ", openBracePos );
		if ( closeBracePos == std::string::npos )
		{
			continue;
		}

		if ( closeBracePos > openBracePos + 3 )
		{
			std::string filenameWithDir = line.substr( 0, openBracePos );
			for ( const auto& dir : visitedDirectories )
			{
				std::string filenameAbs = dir + filenameWithDir;
				const IncludeCache::File* f = includeCache->getFile( filenameAbs.c_str() );
				if ( f )
				{
					line.replace( 0, openBracePos, f->absolutePath_ );
					ss << line << " (guessed filename)\n";
					break;
				}
			}
		}
	}

	return ss.str();
}

void CompileHlslProgram( HlslProgramData& outData, const HlslCompileContext& hlslContext, const CompileContext& ctx, IncludeDependencies& fxFileIncludeDependencies, const FxFileHlslCompileOptions& hlslOptions, const FxFileCompileOptions& options, const FxFile& fxFile, const FxProgram& fxProg )
{
	if ( options.logProgress_ )
		logInfo( "Compiling %s:%s", fxFile.getFileAbsolutePath().c_str(), fxProg.getUniqueName().c_str() );

	const char* profiles[eProgramType_count] = {
		"vs_",
		"ps_",
		"gs_",
		"cs_"
	};

	char profileName[16];
	strcpy( profileName, profiles[fxProg.getProgramType()] );
	strcpy( profileName + 3, "5_0" );

	// defines
	//
	const std::vector<FxProgDefine>& progCDefines = fxProg.getCdefines();
	const size_t nProgCDefines = progCDefines.size();

	std::vector<D3D10_SHADER_MACRO> defines;
	defines.reserve( nProgCDefines + options.defines_.size() + 6 );

	for ( size_t icd = 0; icd < nProgCDefines; ++icd )
	{
		const FxProgDefine& src = progCDefines[icd];
		defines.push_back( { src.name_.c_str(), src.value_.c_str() } );
	}

	for ( size_t iud = 0; iud < options.defines_.size(); ++iud )
	{
		const FxProgDefine& src = options.defines_[iud];
		defines.push_back( { src.name_.c_str(), src.value_.c_str() } );
	}

	char entryDefine[256];
	spad_snprintf( entryDefine, 256, "prog_%s", fxProg.getEntryName().c_str() );
	{
		defines.push_back( { entryDefine, "1" } );
	}

	if ( fxProg.getProgramType() == eProgramType_vertexShader )
	{
		defines.push_back( { "progType_vp", "1" } );
		defines.push_back( { "__VERTEX__", "1" } );
	}
	else if ( fxProg.getProgramType() == eProgramType_pixelShader )
	{
		defines.push_back( { "progType_fp", "1" } );
		defines.push_back( { "__PIXEL__", "1" } );
	}
	else if ( fxProg.getProgramType() == eProgramType_geometryShader )
	{
		defines.push_back( { "progType_gp", "1" } );
		defines.push_back( { "__GEOMETRY__", "1" } );
	}
	else if ( fxProg.getProgramType() == eProgramType_computeShader )
	{
		defines.push_back( { "progType_cp", "1" } );
		defines.push_back( { "__COMPUTE__", "1" } );
	}

	if ( options.configuration_ == FxCompileConfiguration::shipping )
		defines.push_back( { "SHIPPING", "1" } );

	defines.push_back( { "__HLSL__", "1" } );

	// hlsl compiler requires that defines end with two nullptrs
	defines.push_back( { nullptr, nullptr } );

	// compiler flags
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

	//std::string cflags;
	//std::vector<std::string> cflagsStorage;
	//std::vector<const char*> cflagsPointers;
	//if ( ExtractCompilerFlags( fxProg, "cflags_hlsl", cflags, cflagsStorage, cflagsPointers ) )
	//	if ( ParseOptions( &cflagsPointers[0], (int)cflagsPointers.size(), sceOptions ) )
	//		THROW_MESSAGE( "Error while parsing hlsl compiler flags! %s.%s", fxFile.getFilename().c_str(), fxProg.entryName.c_str() );

	HRESULT hr = S_OK;
	ID3D10BlobPtr shaderBlob = NULL;
	ID3D10BlobPtr errBlob = NULL;

	IncludeDependencies includeDependencies; // local include dependencies, merged later with global
	HlslShaderInclude hlslInclude( ctx, options, fxFile, includeDependencies );

	hr = D3DCompile(
		fxFile.getSourceCode().c_str(),
		fxFile.getSourceCode().length(),
		fxFile.getFilename().c_str(),
		&defines[0], // __in   const D3D10_SHADER_MACRO *pDefines,
		&hlslInclude,
		fxProg.getEntryName().c_str(), //__in   LPCSTR pFunctionName,
		profileName, //__in   LPCSTR pProfile,
		flagCombination, // __in   UINT Flags1,
		0, // __in   UINT Flags2,
		&shaderBlob, // __out  ID3D10Blob **ppShader,
		&errBlob // __out  ID3D10Blob **ppErrorMsgs,
	);

	if ( FAILED( hr ) )
	{
		std::string errMsg = Exception::FormatMessage( "Error '0x%x' while compiling program '%s:%s'!", (u32)hr, fxFile.getFileAbsolutePath().c_str(), fxProg.getUniqueName().c_str() );
		if ( errBlob )
		{
			const char* err = reinterpret_cast<const char*>( errBlob->GetBufferPointer() );
			std::string errFixed = _FixHlslErrorMessage( err, hlslInclude, ctx.includeCache );
			THROW_HLSL_EXCEPTION(
				std::move( errMsg ),
				errFixed.c_str(),
				hr
			);
		}
		else
		{
			THROW_HLSL_EXCEPTION( std::move( errMsg ), "", hr );
		}
	}

	outData.shaderBlob_ = shaderBlob;

	if ( fxProg.getProgramType() == eProgramType_vertexShader )
	{
		hr = D3DGetInputSignatureBlob( shaderBlob->GetBufferPointer(), shaderBlob->GetBufferSize(), &outData.vsSignatureBlob_ );
		if ( FAILED( hr ) )
		{
			std::string errMsg = Exception::FormatMessage( "D3DGetInputSignatureBlob failed. Err=0x%x while compiling program '%s:%s'!", (u32)hr, fxFile.getFileAbsolutePath().c_str(), fxProg.getUniqueName().c_str() );
			THROW_HLSL_EXCEPTION( std::move( errMsg ), "", hr );
		}
	}

	if ( hlslOptions.generatePreprocessedOutput )
	{
		ID3D10BlobPtr shaderPreprocessBlob = NULL;

		hr = D3DPreprocess(
			fxFile.getSourceCode().c_str(),
			fxFile.getSourceCode().length(),
			fxFile.getFilename().c_str(),
			&defines[0],
			&hlslInclude,
			&shaderPreprocessBlob,
			&errBlob
		);

		if ( FAILED( hr ) )
		{
			std::string errMsg = Exception::FormatMessage( "Error '0x%x' while preprocessing program '%s:%s'!", (u32)hr, fxFile.getFileAbsolutePath().c_str(), fxProg.getUniqueName().c_str() );
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

		const std::string filename = CreateDebugFileName( hlslContext, fxFile, fxProg, ".hlslc_prep" );
		CreateDirectoryRecursive( filename );

		std::ofstream of( filename.c_str() );
		if ( ! of )
			THROW_MESSAGE( "Couldn't open file '%s' for writing", filename.c_str() );

		//of << "// cflags passed to compiler:" << std::endl;
		//of << "// " << cflags << std::endl;
		//of << std::endl;

		const size_t nDefines = defines.size();

		of << "// defines passed to compiler:" << std::endl;
		for ( size_t idefine = 0; idefine < nDefines - 1; ++idefine )
		{
			const D3D10_SHADER_MACRO& d = defines[idefine];
			of << "// " << d.Name << '=' << d.Definition << std::endl;
		}
		of << std::endl;

		of.write( (const char*)shaderPreprocessBlob->GetBufferPointer(), shaderPreprocessBlob->GetBufferSize() );

		of.close();
	}

	if ( hlslOptions.generateDisassembly )
	{
		// generate disassembly and dump it to file

		ID3D10BlobPtr dissasemblyBlob = NULL;

		hr = D3DDisassemble(
			outData.shaderBlob_->GetBufferPointer(),
			outData.shaderBlob_->GetBufferSize(),
			D3D_DISASM_ENABLE_INSTRUCTION_NUMBERING | D3D_DISASM_ENABLE_DEFAULT_VALUE_PRINTS,
			NULL,
			&dissasemblyBlob );

		if ( FAILED( hr ) )
		{
			std::string errMsg = Exception::FormatMessage( "Error '0x%x' while disassembling program '%s:%s'!", (u32)hr, fxFile.getFileAbsolutePath().c_str(), fxProg.getUniqueName().c_str() );
			THROW_HLSL_EXCEPTION( std::move( errMsg ), "", hr );
		}

		const std::string filename = CreateDebugFileName( hlslContext, fxFile, fxProg, ".hlslc_disassembly" );
		CreateDirectoryRecursive( filename );

		CallResult( WriteFileSync( filename.c_str(), dissasemblyBlob->GetBufferPointer(), dissasemblyBlob->GetBufferSize() ) );
	}

	// thread safe merge this file dependencies with global list
	fxFileIncludeDependencies.merge( includeDependencies );
}

void WriteCompiledFx( const std::vector<HlslProgramData>& outData, const HlslCompileContext& hlslContext, const FxFileHlslCompileOptions& hlslOptions, const FxFileCompileOptions& options, const FxFile& fxFile )
{
	if ( options.writeSource_ )
	{
		if ( PathsPointToTheSameFile( hlslContext.outputSourceFileAbsolute, fxFile.getFileAbsolutePath() ) )
		{
			logInfo( "Skipping write '%s' (it's the source file?)", hlslContext.outputSourceFileAbsolute.c_str() );
		}
		else
		{
			if ( options.logProgress_ )
				logInfo( "Writing '%s'", hlslContext.outputSourceFileAbsolute.c_str() );

			CreateDirectoryRecursive( hlslOptions.outputDirectory_, hlslContext.outputSourceFile );
			const char* source;
			size_t sourceLen;
			fxFile.getOrigSourceCode( source, sourceLen );
			CallResult( WriteFileSync( hlslContext.outputSourceFileAbsolute.c_str(), reinterpret_cast<const void*>( source ), sourceLen ) );
		}
	}

	if ( options.writeCompiledPacked_ )
	{
		//if ( options.logProgress_ )
			logInfo( "Writing '%s'", hlslContext.outputCompiledFileAbsolute.c_str() );

		CreateDirectoryRecursive( hlslOptions.outputDirectory_, hlslContext.outputCompiledFile );

		FILE* ofs = fopen( hlslContext.outputCompiledFileAbsolute.c_str(), "wb" );
		if ( !ofs )
			THROW_MESSAGE( "Couldn't open file '%s' for writing", hlslContext.outputCompiledFileAbsolute.c_str() );

		//const char* fxHeader;
		//size_t fxHeaderLength;
		//fxFile.getFxHeader( fxHeader, fxHeaderLength );

		//u32 headLen = (u32)fxHeaderLength;
		//fwrite( &headLen, 4, 1, ofs );
		//fwrite( fxHeader, fxHeaderLength, 1, ofs );

		// write header
		WriteCompiledFxFileHeader( ofs, fxFile, options.configuration_ );

		// write all unique programs
		const FxProgramArray& uniquePrograms = fxFile.getUniquePrograms();
		const size_t nUniquePrograms = uniquePrograms.size();
		SPAD_ASSERT( nUniquePrograms == outData.size() );

		AppendU32( ofs, (u32)nUniquePrograms );

		for ( size_t iprog = 0; iprog < nUniquePrograms; ++iprog )
		{
			const FxProgram& prog = *uniquePrograms[iprog];
			const HlslProgramData& data = outData[iprog];

			AppendU32( ofs, (u32)prog.getProgramType() );
			AppendString( ofs, prog.getUniqueName() );

			AppendU32( ofs, (u32)data.shaderBlob_->GetBufferSize() );
			fwrite( data.shaderBlob_->GetBufferPointer(), data.shaderBlob_->GetBufferSize(), 1, ofs );

			if ( prog.getProgramType() == eProgramType_vertexShader )
			{
				AppendU32( ofs, (u32)data.vsSignatureBlob_->GetBufferSize() );
				fwrite( data.vsSignatureBlob_->GetBufferPointer(), data.vsSignatureBlob_->GetBufferSize(), 1, ofs );
			}
		}

		// write passes and combinations
		WriteCompiledFxFilePasses( ofs, fxFile );

		fclose( ofs );
	}
}

HRESULT HlslShaderInclude::Open( D3D_INCLUDE_TYPE /*IncludeType*/, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes )
{
	const IncludeCache::File* f = nullptr;
	const IncludeCache::File* fp = nullptr;

	// treat all includes the same

	//if ( IncludeType == D3D_INCLUDE_LOCAL )
	//{
	//	std::string absPath = currentDir_.back().name_ + pFileName;
	//	f = ctx_.includeCache->getFile( absPath.c_str() );
	//}
	//else
	//{
	//	SPAD_ASSERT( IncludeType == D3D_INCLUDE_SYSTEM );
	//	f = ctx_.includeCache->searchFile( pFileName );
	//}

	if ( !pParentData )
	{
		// search in directory of input file
		std::string dir = GetDirectoryFromFilePath( fx_.getFileAbsolutePath() );
		std::string absPath = dir + pFileName;
		f = ctx_.includeCache->getFile( absPath.c_str() );
	}
	else
	{
		// search in directory of parent file
		fp = ctx_.includeCache->getFileByDataPtr( pParentData );
		std::string dir = GetDirectoryFromFilePath( fp->absolutePath_ );
		std::string absPath = dir + pFileName;
		f = ctx_.includeCache->getFile( absPath.c_str() );
	}

	if ( !f )
		// look for file in all given search directories
		f = ctx_.includeCache->searchFile( pFileName );

	if ( f )
	{
		//_SetCurrentDir( GetDirectoryFromFilePath( f->absolutePath_ ) );
		visitedDirectories_.insert( GetDirectoryFromFilePath( f->absolutePath_ ) );
		includeDependencies_.addDependencyNoLock( f->absolutePath_ );

		*ppData = f->sourceCode_.c_str();
		*pBytes = (UINT)f->sourceCode_.length();
		return S_OK;
	}

	if ( fp )
	{
		// try provide meaningful file and line in the error output
		std::string filename = pFileName;

		std::istringstream ss( fp->sourceCode_ );
		std::string line;
		bool foundLine = false;
		u32 lineNo = 0;
		while ( ss )
		{
			++lineNo;

			getline( ss, line );
			if ( line.empty() )
				continue;

			std::string::size_type filenamePos = line.rfind( filename );
			if ( filenamePos != std::string::npos )
			{
				foundLine = true;
				break;
			}
		}

		if ( foundLine )
		{
			logError( "%s(%u,1): error: Couldn't open include '%s'", fp->absolutePath_.c_str(), lineNo, pFileName );
			return S_FALSE;
		}
	}
	
	logError( "%s(1,1): error: Couldn't open include '%s'", fx_.getFileAbsolutePath().c_str(), pFileName );

	return S_FALSE;
}

HRESULT __stdcall HlslShaderInclude::Close( LPCVOID /*pData*/ )
{
	// pData is the pointer we returned in Open callback
	//const IncludeCache::File* f = ctx_.includeCache->getFileByDataPtr( pData );
	//if ( f )
	//{
	//	std::string dir = GetDirectoryFromFilePath( f->absolutePath_ );
	//	SPAD_ASSERT( currentDir_.back().name_ == dir && currentDir_.back().refCount_ > 0 );
	//	currentDir_.back().refCount_ -= 1;
	//	if ( currentDir_.back().refCount_ == 0 )
	//		currentDir_.pop_back();
	//}

	return S_OK;
}

//void HlslShaderInclude::_SetCurrentDir( std::string dir )
//{
//	if ( !currentDir_.empty() )
//	{
//		if ( currentDir_.back().name_ == dir )
//		{
//			currentDir_.back().refCount_ += 1;
//			return;
//		}
//	}
//
//	currentDir_.emplace_back( dir, 1 );
//}


} // namespace hlsl
} // namespace fxlib
} // namespace spad
