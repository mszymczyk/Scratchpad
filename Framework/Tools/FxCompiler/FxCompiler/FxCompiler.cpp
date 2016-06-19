#include "FxCompiler_pch.h"
#include "../../Core/FxLib/FxLib.h"

using namespace spad;

#define ARG_OUT_DIR "-outdir"
#define ARG_INT_DIR "-intdir"

int _tmain(int argc, _TCHAR* argv[])
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

	std::vector<std::string> files;
	std::string programName = argv[0];

	std::string outDir;
	std::string intDir;

	for (int i = 1; i < argc; ++i)
		std::cerr << "arg " << i << ": " << argv[i] << std::endl;

	for ( int i = 1; i < argc; ++i )
	{
		std::string arg = argv[i];

		if (arg == ARG_OUT_DIR)
		{
			++i;
			if (i < argc)
			{
				outDir = argv[i];
			}
			else
			{
				std::cerr << programName << " directory expected after -outdir" << std::endl;
				return 1;
			}
		}
		else if (arg == ARG_INT_DIR)
		{
			++i;
			if (i < argc)
			{
				intDir = argv[i];
			}
			else
			{
				std::cerr << programName << " directory expected after -intdir" << std::endl;
				return 1;
			}
		}
		else if (arg[0] == '-')
		{
			std::cerr << programName << " unsupported argument '" << arg << "'" << std::endl;
			return 1;
		}
		else
		{
			files.push_back( arg );
		}
	}

	//std::cerr << "Files to compile: " << std::endl;
	//for ( const auto& file : files )
	//{
	//	std::cerr << file << std::endl;
	//}
	//const char* hlslFile = "..\\shaders\\font.hlsl";

	//std::string hlslFileFullPath = GetAbsolutePath( hlslFile );

	////std::string fxCompilerPath;
	////fxCompilerPath.resize( 1024 );
	////// Get the fully-qualified path of the executable
	////DWORD fxCompilerPathLen = GetModuleFileName( NULL, &fxCompilerPath[0], (DWORD)hlslFileFullPath.size() );
	////if ( fxCompilerPathLen == hlslFileFullPath.size() )
	////{
	////	// the buffer is too small, handle the error somehow
	////	FR_NOT_IMPLEMENTED;
	////	return -1;
	////}

	//bool ingore;
	//options.compilerTimestamp_ = getFileTimestamp( fxCompilerPath.c_str(), ingore );

	//Sleep( 5000 ); // for attaching to process started using msbuild

	FxLib::FxFileCompileOptions compileOptions;

	FxLib::FxFileWriteOptions writeOptions;
	writeOptions.outputDirectory_ = outDir;
	writeOptions.intermediateDirectory_ = intDir;
	writeOptions.logProgress_ = true;

	for (const auto& file : files)
	{
		std::string fileFullPath = GetAbsolutePath( file );

		FxLib::FxFile fxFile;
		int ires = fxFile.compileFxFile( fileFullPath.c_str(), compileOptions );
		if (ires)
		{
			std::cerr << "Compilation failed!" << std::endl;
			return ires > 0 ? ires : -ires;
		}

		ires = fxFile.writeCompiledFxFile( writeOptions );
		if (ires)
		{
			std::cerr << "Compilation failed!" << std::endl;
			return ires > 0 ? ires : -ires;
		}
	}

	//return ires;
	return 0;
}

