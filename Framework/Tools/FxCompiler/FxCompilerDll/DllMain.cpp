#include "FxCompilerDll_pch.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

BOOL APIENTRY DllMain( HMODULE /*hModule*/,
	DWORD  ul_reason_for_call,
	LPVOID /*lpReserved*/
	)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}


