#include "SettingsTest_pch.h"
#include "SettingsTestApp.h"

using namespace spad;

int main(int /*argc*/, _TCHAR* /*argv*/[])
{
#ifdef _DEBUG
	int flag = _CrtSetDbgFlag( _CRTDBG_REPORT_FLAG );
	flag |= _CRTDBG_LEAK_CHECK_DF;
	//flag &= ~_CRTDBG_LEAK_CHECK_DF;
	flag |= _CRTDBG_ALLOC_MEM_DF;
	//flag |= _CRTDBG_CHECK_ALWAYS_DF;
	flag &= ~_CRTDBG_CHECK_ALWAYS_DF;
	_CrtSetDbgFlag( flag );
#endif

	CoInitialize( NULL );

	SettingsTestApp app;

	SettingsTestApp::Param param;
	param.appName = "SettingsTest";
	param.debugDxDevice_ = true;

	if ( app.StartUpBase( param ) )
	{
		if ( app.StartUp() )
		{
			app.Loop();
		}
	}
	
	CoUninitialize();

	return 0;
}

