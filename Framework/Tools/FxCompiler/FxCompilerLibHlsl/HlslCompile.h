#pragma once

#include <Core/Dx11Util/Dx11Util.h>
#include "../FxCompilerLib/FxCompilerLib.h"
#include "../FxCompilerLib/IncludeDependencies.h"

namespace spad
{
namespace fxlib
{
namespace hlsl
{

struct FxFileHlslCompileOptions
{
	u32 dxCompilerFlags = 0;
	bool generatePreprocessedOutput = false;
	bool generateDisassembly = false;

	std::string outputDirectory_; // contains backslash at the end
	std::string intermediateDirectory_; // contains backslash at the end
};


struct HlslCompileContext
{
	std::string outputSourceFile; // source code copy
	std::string outputSourceFileAbsolute;
	std::string outputCompiledFile; // single file containing FX_HEADER and all compiled programs
	std::string outputCompiledFileAbsolute;
	std::string outputDependFile; // dependency file for checking if shader must be recompiled
	std::string outputDependFileAbsolute;
	std::string outputDiagnosticsDir; // directory containing compilation output files that are aiding development (preprocessor output, sdb)
};

struct HlslProgramData
{
	ID3D10BlobPtr shaderBlob_;
	ID3D10BlobPtr vsSignatureBlob_;
};

class HlslException : public Exception
{
public:
	HlslException( std::string&& exceptionMessage, const char* hlslMessage, HRESULT hres, const char* file, int line )
		: Exception( std::move( exceptionMessage ), file, line )
		, hlslMessage_( hlslMessage )
		, hres_( hres )
	{	}

	const std::string& GetHlslErrorMessage() const throw( )
	{
		return hlslMessage_;
	}

protected:
	std::string hlslMessage_; // The error message
	HRESULT hres_;
};

#define THROW_HLSL_EXCEPTION(msg, hlslMsg, hres) throw HlslException( msg, hlslMsg, hres, __FILE__, __LINE__ )



class HlslShaderInclude : public ID3DInclude
{
public:
	HlslShaderInclude( const CompileContext& ctx, const FxFileCompileOptions& options, const FxFile& fxFile/*, IncludeCache& includeCache*/, IncludeDependencies& includeDependencies )
		: ctx_( ctx )
		, options_( options )
		, fx_( fxFile )
		, includeDependencies_( includeDependencies )
	{
		//_SetCurrentDir( GetDirectoryFromFilePath( fxFile.getFileAbsolutePath() ) );
	}

	HRESULT __stdcall Open(
		D3D_INCLUDE_TYPE IncludeType,
		LPCSTR pFileName,
		LPCVOID pParentData,
		LPCVOID *ppData,
		UINT *pBytes );

	HRESULT __stdcall Close( LPCVOID pData );

	const std::set<std::string>& GetVisitedDirectories() const { return visitedDirectories_; }

private:
	//void _SetCurrentDir( std::string dir );

private:
	const CompileContext& ctx_;
	const FxFileCompileOptions& options_;
	const FxFile& fx_;
	IncludeDependencies& includeDependencies_; // independent from CompileContext for better perf

	//struct _CurDir
	//{
	//	_CurDir( const std::string& n, u32 refCount )
	//		: name_( n )
	//		, refCount_( refCount )
	//	{	}
	//	std::string name_;
	//	u32 refCount_ = 0;
	//};
	//std::vector<_CurDir> currentDir_;
	std::set<std::string> visitedDirectories_;
};

int CompileFxHlsl( const FxFile& fxFile, const CompileContext& ctx, const FxFileCompileOptions& options, const FxFileHlslCompileOptions& hlslOptions );
void SetupHlslCompileContext( HlslCompileContext& ctx, const FxFileHlslCompileOptions& hlslOptions, const FxFile& fxFile );
void CompileHlslProgram( HlslProgramData& outData, const HlslCompileContext& hlslContext, const CompileContext& ctx, IncludeDependencies& fxFileIncludeDependencies, const FxFileHlslCompileOptions& hlslOptions, const FxFileCompileOptions& options, const FxFile& fxFile, const FxProgram& fxProg );
void WriteCompiledFx( const std::vector<HlslProgramData>& outData, const HlslCompileContext& hlslContext, const FxFileHlslCompileOptions& hlslOptions, const FxFileCompileOptions& options, const FxFile& fxFile );


} // namespace hlsl
} // namespace fxlib
} // namespace spad
