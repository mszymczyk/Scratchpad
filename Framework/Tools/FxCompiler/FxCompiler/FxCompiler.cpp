#include "FxCompiler_pch.h"
#include "../FxCompilerLib/FxCompilerLib.h"
#include "../FxCompilerLibHlsl/HlslCompile.h"
#include "Options.h"
#include <Util/Threading.h>
#include <Util/Timer.h>
//#include <direct.h>

using namespace spad;

//struct _DoFileParallelContext
//{
//	_DoFileParallelContext( const fxlib::FxFileCompileOptions& _compileOptions, const std::vector<std::string>& _files, std::vector<int>& _results )
//		: compileOptions( _compileOptions )
//		, files( _files )
//		, results( _results )
//	{	}
//
//	const fxlib::FxFileCompileOptions& compileOptions;
//	const std::vector<std::string>& files;
//	std::vector<int>& results;
//};
//
//int doFileParallel( u32 index, void* userPtr )
//{
//	_DoFileParallelContext* ctx = reinterpret_cast<_DoFileParallelContext*>( userPtr );
//	const std::string& file = ctx->files[index];
//	int ires = CompileFxFile( file.c_str(), ctx->compileOptions );
//	ctx->results[index] = ires;
//	return ires;
//}

int doIt( const std::string& programName
	, const fxlib::CompileContext& compileContext
	, const fxlib::FxFileCompileOptions& options
	, const fxlib::hlsl::FxFileHlslCompileOptions& hlslOptions
	, const std::vector<std::string>& files )
{
	std::vector<int> results( files.size(), 0 );

	u32 nHardwareThreads = GetNumHardwareThreads();
	// limiting number of threads gives better perf
	// there will be more threads in flight than hardware can support due to multiple files being compiled simultaneously
	// using builtin parallel_for seems to be little bit slower

	//ParallelFor_threadPool( 0, files.size(), compileOptions.multithreadedFiles_, [&](size_t index) {
	//ParallelFor2( 0, files.size(), compileOptions.multithreadedFiles_ ? -1 : 1, [&]( size_t index ) {
	ParallelFor( 0, files.size(), options.multithreadedFiles_ ? (nHardwareThreads / 2) : 1, [&]( size_t index ) {
		const std::string& file = files[index];

		int ires = 0;
		std::unique_ptr<fxlib::FxFile> fxFile = ParseFxFile( file.c_str(), nullptr, options, *compileContext.includeCache, &ires );
		if (fxFile)
			ires = fxlib::hlsl::CompileFxHlsl( *fxFile.get(), compileContext, options, hlslOptions );

		results[index] = ires;
		return ires;
	} );

	//_DoFileParallelContext ctx( compileOptions, files, results );
	//ParallelFor( doFileParallel, &ctx, 0, (u32)files.size(), compileOptions.multithreadedFiles_ ? -1 : 1 );

	for (auto res : results)
	{
		if (res != 0)
		{
			std::cerr << programName << ": " << "Compilation failed for at least one input file!" << std::endl;
			return res;
		}
	}

	return 0;
}

int RemoveDirectoryRecursivelyIfExists( const std::string& directory )
{
	if (DirectoryExists( directory ))
	{
		std::cerr << "Removing directory: " << directory << "\n";
		return RemoveDirectoryRecursively( directory );
	}

	return 0;
}

int _tmain( int argc, char* argv[] )
{
#ifdef _DEBUG
	int flag = _CrtSetDbgFlag( _CRTDBG_REPORT_FLAG );
	flag |= _CRTDBG_LEAK_CHECK_DF;
	//flag &= ~_CRTDBG_LEAK_CHECK_DF;
	flag |= _CRTDBG_ALLOC_MEM_DF;
	//flag |= _CRTDBG_CHECK_ALWAYS_DF;
	flag &= ~_CRTDBG_CHECK_ALWAYS_DF;
	_CrtSetDbgFlag( flag );

	//_CrtSetAllocHook( MyAllocHook );
#endif

	int ires = 0;

	//Sleep( 5000 );

	CpuTimeQuery totalDuration;
	BeginCpuTimeQuery( totalDuration );

	std::string programName = argv[0];

	const char* SCRATCHPAD_DIR_env = getenv( "SCRATCHPAD_DIR" );
	if ( !SCRATCHPAD_DIR_env )
	{
		std::cerr << programName << ": " << "SCRATCHPAD_DIR env variable is not defined!" << std::endl;
		return 100;
	}

	std::string SCRATCHPAD_DIR = SCRATCHPAD_DIR_env;

	// make those vars to look nice
	SCRATCHPAD_DIR = GetAbsolutePath( SCRATCHPAD_DIR );
	AppendBackslashToDirectoryName( SCRATCHPAD_DIR );

	//for (int i = 1; i < argc; ++i)
	//	std::cerr << "arg " << i << ": " << argv[i] << std::endl;

	std::vector<std::string> files;

	fxlib::FxFileCompileOptions options;
	options.argv_ = argv;
	options.argc_ = argc;

	fxlib::hlsl::FxFileHlslCompileOptions hlslOptions;
	fxlib::FxCompileConfiguration::Type configuration = fxlib::FxCompileConfiguration::shipping;
	bool cleanOutputFiles = false;

	ires = ParseOptions( options, hlslOptions,
		files, configuration, cleanOutputFiles );// , compilerMode );
	if (ires)
	{
		std::cerr << programName << ": " << "ParseOptions failed!" << std::endl;
		return ires > 0 ? ires : -ires;
	}

	//configuration = fxlib::FxCompileConfiguration::diagnostic; // for debug

	options.configuration_ = configuration;

#ifdef _DEBUG
	options.forceRecompile_ = true;
	options.multithreaded_ = false;
	options.multithreadedFiles_ = false;
#endif //

	if (configuration == fxlib::FxCompileConfiguration::diagnostic)
	{
		// hlsl
		hlslOptions.generateDisassembly = true;
		hlslOptions.generatePreprocessedOutput = true;
	}
	else if (configuration == fxlib::FxCompileConfiguration::debug)
	{
		options.compileForDebugging_ = true;
	}

	// setup output dirs
	hlslOptions.outputDirectory_ = SCRATCHPAD_DIR + "dataWin\\Shaders\\hlsl\\";
	hlslOptions.intermediateDirectory_ = SCRATCHPAD_DIR + "Build\\dataWin\\Shaders\\hlsl\\";

	if (cleanOutputFiles)
	{
		//std::cerr << "Cleaning outputs:" << std::endl;

		//for ( const auto& f : files )
		//	std::cerr << "\t" << f << std::endl;

		RemoveDirectoryRecursivelyIfExists( hlslOptions.outputDirectory_ );
		RemoveDirectoryRecursivelyIfExists( hlslOptions.intermediateDirectory_ );

		return 0;
	}

	fxlib::IncludeCache includeCache;

	std::string shadersDir = SCRATCHPAD_DIR + "Framework\\Shaders\\hlsl\\";
	includeCache.AddSearchPath( shadersDir );

	//char cwdBuffer[MAX_PATH];
	//const char* currentWorkingDir = _getcwd( cwdBuffer, MAX_PATH );
	//if ( !currentWorkingDir )
	//{
	//	std::cerr << programName << ": " << "_getcwd failed!" << std::endl;
	//	return 200;
	//}
	//includeCache.AddSearchPath( currentWorkingDir );

	ires = includeCache.Load_AlwaysIncludedByFxCompiler();
	if (ires)
	{
		std::cerr << "Couldn't read 'AlwaysIncludedByFxCompiler.h'" << std::endl;
		return ires > 0 ? ires : -ires;
	}

	//char compilerFilename[MAX_PATH];
	//DWORD compilerFilenameLen = GetModuleFileName( NULL, compilerFilename, MAX_PATH );
	//if ( compilerFilenameLen == 0 || compilerFilenameLen >= MAX_PATH )
	//{
	//	std::cerr << programName << ": " << "GetModuleFileName failed!" << std::endl;
	//	return 300;
	//}

	bool compilerFound = false;
	options.compilerTimestamp_ = spad::GetFileTimestamp( SCRATCHPAD_DIR + "Framework\\bin\\FxCompiler.exe", compilerFound );
	//options.compilerTimestamp_ = spad::GetFileTimestamp( compilerFilename, compilerFound );
	//compileOptions.compilerTimestamp_ = spad::GetFileTimestamp( programName, compilerFound ); // program name must not necesarilly be path to our exe (it can be for instance, FxCompiler - without .exe)
	SPAD_ASSERT( compilerFound );

	fxlib::CompileContext compileContext;
	compileContext.includeCache = &includeCache;

	ires = doIt( programName, compileContext, options, hlslOptions, files );

	EndCpuTimeQuery( totalDuration );

	std::string td = FormatTime( totalDuration.durationUS_ );

	if (!ires)
		std::cerr << programName << ": " << "Compilation succeeded for all '" << files.size() << "' input files in " << td << std::endl;

	return ires;
}

