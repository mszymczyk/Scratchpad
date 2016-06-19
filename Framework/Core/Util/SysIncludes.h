#pragma once

// Windows Header Files:

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#define STRICT                          // Use strict declarations for Windows types
#define NOMINMAX

#include <windows.h>

#include <Shlwapi.h>
#pragma comment(lib, "Shlwapi.lib")

#if defined _DEBUG /*&& (FR_COMPILER == FR_COMPILER_MSVC)*/
	#ifdef _MSC_VER
		#define _CRTDBG_MAP_ALLOC
		//#define _CRTDBG_MAP_ALLOC_NEW
		#include <stdlib.h>
		#include <crtdbg.h>
	#elif __ANDROID__
		#include <stdlib.h>
	#else	
	#error unsupported platform
	#endif //
#else
	#include <stdlib.h>
#endif

// DirectX Math
#include <DirectXMath.h>
#include <DirectXPackedVector.h>

using namespace DirectX;
