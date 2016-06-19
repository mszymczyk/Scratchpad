#include "FxCompilerDll_pch.h"
#include "FxCompilerDll.h"
#include <FxLib/FxLib.h>

using namespace spad;

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

FXCOMPILERDLL_API int __stdcall FxCompilerDll_Initialize( spad::log::LogCallbackType logCallback )
{
#ifdef _DEBUG
	int flag = _CrtSetDbgFlag( _CRTDBG_REPORT_FLAG );
	flag |= _CRTDBG_LEAK_CHECK_DF; // Perform automatic leak checking at program exit through a call to _CrtDumpMemoryLeaks and generate an error report
	flag |= _CRTDBG_ALLOC_MEM_DF; // Enable debug heap allocations and use of memory block type identifiers, such as _CLIENT_BLOCK
	flag &= ~_CRTDBG_CHECK_ALWAYS_DF; // DON'T Call _CrtCheckMemory at every allocation and deallocation request. CrtCheckMemory must be called explicitly.
	_CrtSetDbgFlag( flag );
#endif

	spad::log::logStartUp();
	spad::log::logSetCallback( logCallback );

	return 0;
}

FXCOMPILERDLL_API void __stdcall FxCompilerDll_Shutdown()
{
	spad::log::logShutDown();
}

FXCOMPILERDLL_API int __stdcall FxCompilerDll_CompileFile( const char* fileFullPath, const char* outDir, const char* intDir )
{
	FxLib::FxFileCompileOptions compileOptions;

	FxLib::FxFile fxFile;
	int ires = fxFile.compileFxFile( fileFullPath, compileOptions );
	if (ires)
	{
		logError( "Compilation failed!" );
		return ires > 0 ? -ires : ires;
	}

	FxLib::FxFileWriteOptions writeOptions;
	writeOptions.outputDirectory_ = outDir;
	writeOptions.intermediateDirectory_ = intDir;
	writeOptions.logProgress_ = true;

	ires = fxFile.writeCompiledFxFile( writeOptions );
	if (ires)
	{
		logError( "Compilation failed!" );
		return ires > 0 ? -ires : ires;
	}

	return 0;
}
