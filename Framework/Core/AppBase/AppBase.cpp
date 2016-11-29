#include "AppBase_pch.h"
#include "AppBase.h"
#include <Tools\SettingsEditor\ClientLib\SettingsEditor.h>
#include <Tools\SettingsEditor\ZMQHubLib\ZMQHubCommunication.h>
#include <Util\ZMQHubUtil.h>
#include <Gfx\DebugDraw.h>
#include <FrameworkSettings/FrameworkSettings.h>
#include <FrameworkSettings/FrameworkSettings_General.h>
#include <Util\FileIO.h>

namespace spad
{
	AppBase* AppBase::instance_;

	const char* SETTINGS_TAG = "settings";

	void SettingsEditorLogInfo(const char* message)
	{
		logInfoTag(SETTINGS_TAG, message);
	}

	void SettingsEditorLogWarning(const char* message)
	{
		logWarningTag(SETTINGS_TAG, message);
	}

	void SettingsEditorLogError(const char* message)
	{
		logErrorTag(SETTINGS_TAG, message);
	}

	int SettingsEditorReadFile(const char* filename, unsigned char** fileBuf, size_t* fileBufSize)
	{
		std::string fileContents = ReadTextFileAsString(filename);
		if (fileContents.empty())
			return -1;

		u8* fc = new u8[fileContents.size() + 1];
		memcpy(fc, fileContents.c_str(), fileContents.size() + 1);
		*fileBuf = fc;
		*fileBufSize = (u32)fileContents.size();
		return 0;
	}
	
	void SettingsEditorFreeFile(const char* /*filename*/, unsigned char* fileBuf, size_t /*fileBufSize*/)
	{
		delete[] fileBuf;
	}

	bool AppBase::StartUpBase( const Param& param )
	{
		appName_ = param.appName;
		instance_ = this;

		ZMQHubUtil::startUp();
		::SettingsEditor::_internal::StartUpParam seParam;
		seParam.readFile = SettingsEditorReadFile;
		seParam.freeFile = SettingsEditorFreeFile;
		seParam.logInfo = SettingsEditorLogInfo;
		seParam.logWarning = SettingsEditorLogWarning;
		seParam.logError = SettingsEditorLogError;
		SettingsEditor::_internal::startUp( seParam );
		SettingsEditorZMQ::startUp();

		gFrameworkSettings = new FrameworkSettingsNamespace::FrameworkSettingsWrap("Data\\FrameworkSettings.settings");

		hThisInst_ = GetModuleHandle( NULL );

		WNDCLASS wincl;
		ZeroMemory( &wincl, sizeof( WNDCLASS ) );
		wincl.hInstance = hThisInst_;
		wincl.lpszClassName = appName_.c_str();
		wincl.lpfnWndProc = WinProcStatic;
		wincl.style = CS_HREDRAW | CS_VREDRAW;// | CS_OWNDC | CS_DBLCLKS;;
		wincl.hIcon = LoadIcon( hThisInst_, IDI_APPLICATION );
		wincl.hCursor = LoadCursor( hThisInst_, IDC_CROSS );
		wincl.lpszMenuName = NULL;
		wincl.cbClsExtra = 0;
		wincl.cbWndExtra = 0;
		wincl.hbrBackground = NULL;

		if ( !RegisterClass( &wincl ) )
		{
			return false;
		}

		RECT client;
		client.left = 0;
		client.top = 0;
		client.right = param.windowWidth;
		client.bottom = param.windowHeight;

		BOOL ret = AdjustWindowRectEx( &client, WS_OVERLAPPEDWINDOW, FALSE, 0 );
		(void)ret;

		hWnd_ = CreateWindow( appName_.c_str(), appName_.c_str(), WS_OVERLAPPEDWINDOW, 0, 0, client.right - client.left, client.bottom - client.top,
			nullptr, nullptr, hThisInst_, NULL );

		if ( hWnd_ == nullptr )
			return false;

		Dx11::Param dx11Param;
		dx11Param.hWnd = hWnd_;
		dx11Param.debugDevice = param.debugDxDevice_;
		dx11_ = std::make_unique<Dx11>();
		bool res = dx11_->StartUp( dx11Param );
		if ( !res )
			return false;

		debugDraw::DontTouchThis::Initialize( dx11_->getDevice() );

		return true;
	}

	LRESULT CALLBACK AppBase::WinProcStatic( HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam )
	{
		return instance_->WinProc( hwnd, message, wParam, lParam );
	}

	LRESULT AppBase::WinProc( HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam )
	{
		switch ( message )
		{
		case WM_DESTROY:
			continueLoop_ = false;
			PostQuitMessage( 0 );
			break;

		default:
			return DefWindowProc( hwnd, message, wParam, lParam );
		};

		return 0;
	}

	void AppBase::ShutDownBase()
	{
		debugDraw::DontTouchThis::DeInitialize();

		dx11_.reset();

		if ( hWnd_ != nullptr )
		{
			DestroyWindow( hWnd_ );
			hWnd_ = nullptr;
		}

		if ( hThisInst_ && !appName_.empty() )
		{
			UnregisterClass( appName_.c_str(), hThisInst_ );
		}

		delete gFrameworkSettings;
		gFrameworkSettings = nullptr;

		SettingsEditorZMQ::shutDown();
		::SettingsEditor::_internal::shutDown();
		ZMQHubUtil::shutDown();

		hThisInst_ = nullptr;
		appName_.clear();
	}

	void AppBase::Loop()
	{
		ShowWindow( hWnd_, SW_SHOW );
		UpdateWindow( hWnd_ );

		MSG msg;
		msg.wParam = 0;

		for( ;; )
		{
			while ( PeekMessage( &msg, hWnd_, 0, 0, PM_REMOVE ) )
			{
				TranslateMessage( &msg );
				DispatchMessage( &msg );
			}

			if ( !continueLoop_ )
				break;

			timer_.Update();

			UpdateAndRender( timer_ );

			ZMQHubUtil::processReceivedData( nullptr );
			::SettingsEditor::_internal::update();

			dx11_->Present();
		}
	}

} // namespace spad
