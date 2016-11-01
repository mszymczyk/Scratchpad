#include "FxCompilerDll_pch.h"
#include "FxCompilerDll.h"
#include "../FxCompilerLib/FxCompilerLib.h"
#include "../FxCompilerLibHlsl/HlslCompile.h"
//#include "Options.h"
#include <Core/Util/Threading.h>
#include <Core/Util/Timer.h>

using namespace spad;

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

std::string SCRATCHPAD_DIR;

FXCOMPILERDLL_API int __stdcall FxCompilerDll_Initialize( spad::log::LogCallbackType logCallback )
{
#ifdef _DEBUG
	int flag = _CrtSetDbgFlag( _CRTDBG_REPORT_FLAG );
	flag |= _CRTDBG_LEAK_CHECK_DF; // Perform automatic leak checking at program exit through a call to _CrtDumpMemoryLeaks and generate an error report
	flag |= _CRTDBG_ALLOC_MEM_DF; // Enable debug heap allocations and use of memory block type identifiers, such as _CLIENT_BLOCK
	flag &= ~_CRTDBG_CHECK_ALWAYS_DF; // DON'T Call _CrtCheckMemory at every allocation and deallocation request. CrtCheckMemory must be called explicitly.
	_CrtSetDbgFlag( flag );
#endif

	int ires = 0;

	log::logStartUp();
	log::logSetCallback( logCallback );

	SCRATCHPAD_DIR = getenv( "SCRATCHPAD_DIR" );
	if (SCRATCHPAD_DIR.empty() )
	{
		logErrorAlways( "SCRATCHPAD_DIR env variable is not defined!" );
		return 100;
	}

	// make those vars to look nice
	SCRATCHPAD_DIR = GetAbsolutePath( SCRATCHPAD_DIR );
	AppendBackslashToDirectoryName( SCRATCHPAD_DIR );

	logInfo( "FxCompilerDll_Initialize successfull" );

	return 0;
}

FXCOMPILERDLL_API void __stdcall FxCompilerDll_Shutdown()
{
	log::logShutDown();
}


int doIt( const fxlib::CompileContext& compileContext
	, const fxlib::FxFileCompileOptions& options
	, const fxlib::hlsl::FxFileHlslCompileOptions& hlslOptions
	, const std::vector<std::string>& files
)
{
	std::vector<int> results( files.size(), 0 );

	u32 nHardwareThreads = GetNumHardwareThreads();
	// limiting number of threads gives better perf
	// there will be more threads in flight than hardware can support due to multiple files being compiled simultaneously
	// using builtin parallel_for seems to be little bit slower

	ParallelFor( 0, files.size(), options.multithreadedFiles_ ? ( nHardwareThreads / 2 ) : 1, [&]( size_t index ) {
		const std::string& file = files[index];

		int ires = 0;
		std::unique_ptr<fxlib::FxFile> fxFile = ParseFxFile( file.c_str(), options, *compileContext.includeCache, &ires );
		if (fxFile)
			ires = fxlib::hlsl::CompileFxHlsl( *fxFile.get(), compileContext, options, hlslOptions );

		results[index] = ires;
		return ires;
	} );

	for ( auto res : results )
	{
		if ( res != 0 )
		{
			logError( "Compilation failed for at least one input file!" );
			return res;
		}
	}

	return 0;
}


FXCOMPILERDLL_API int __stdcall FxCompilerDll_CompileFile( const char* filePathWithinDataRoot )
{

	//fxlib::FxCompileConfiguration::Type configuration = fxlib::FxCompileConfiguration::shipping;
	//fxlib::FxCompileConfiguration::Type configuration = fxlib::FxCompileConfiguration::release;
	fxlib::FxCompileConfiguration::Type configuration = fxlib::FxCompileConfiguration::diagnostic;

	fxlib::FxFileCompileOptions options;
	options.configuration_ = configuration;
	options.writeSource_ = true;

#ifdef _DEBUG
	options.forceRecompile_ = true;
	options.multithreaded_ = false;
	options.multithreadedFiles_ = false;
#endif //

	fxlib::hlsl::FxFileHlslCompileOptions hlslOptions;

	// setup output dirs
	hlslOptions.outputDirectory_ = SCRATCHPAD_DIR + "dataWin\\Materials\\hlsl\\";
	hlslOptions.intermediateDirectory_ = SCRATCHPAD_DIR + ".build\\dataWin\\Materials\\hlsl\\";

	// hlsl
	hlslOptions.generateDisassembly = false;
	hlslOptions.generatePreprocessedOutput = false;

	if ( configuration == fxlib::FxCompileConfiguration::diagnostic )
	{
		// common
		options.forceRecompile_ = true;


		// hlsl
		hlslOptions.generateDisassembly = true;
		hlslOptions.generatePreprocessedOutput = true;
	}
	else if ( configuration == fxlib::FxCompileConfiguration::debug )
	{
		options.compileForDebugging_ = true;
	}

	std::vector<std::string> files;
	files.emplace_back( filePathWithinDataRoot );

	// recreate cache with each compilation
	// this way, external changes of include files will be included in compilation
	// IncludeCache doesn't check timestamps of included files
	fxlib::IncludeCache includeCache;

	std::string shadersDir = SCRATCHPAD_DIR + "Framework\\Shaders\\hlsl\\";
	includeCache.AddSearchPath( shadersDir );

	int ires = includeCache.Load_AlwaysIncludedByCompiler();
	if (ires)
	{
		std::cerr << "Couldn't read 'AlwaysIncludedByCompiler.h'" << std::endl;
		return ires > 0 ? ires : -ires;
	}

	fxlib::CompileContext compileContext;
	compileContext.includeCache = &includeCache;

	ires = doIt( compileContext, options, hlslOptions, files );
	return ires;
}
