// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the FXCOMPILERDLL_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// FXCOMPILERDLL_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef FXCOMPILERDLL_EXPORTS
#define FXCOMPILERDLL_API __declspec(dllexport)
#else
#define FXCOMPILERDLL_API __declspec(dllimport)
#endif

extern "C"
{
	FXCOMPILERDLL_API int __stdcall FxCompilerDll_Initialize( spad::log::LogCallbackType logCallback );
	FXCOMPILERDLL_API void __stdcall FxCompilerDll_Shutdown();

	FXCOMPILERDLL_API int __stdcall FxCompilerDll_CompileFile( const char* fileFullPath );

} // extern "C"