#pragma once

#include <Gfx\Dx11\Dx11.h>
#include <Util\Timer.h>

namespace spad
{

	class AppBase
	{
	public:
		~AppBase()
		{
			ShutDownBase();
		}

		struct Param
		{
			const char* appName = nullptr;
			u32 windowWidth = 1280;
			u32 windowHeight = 720;
			bool debugDxDevice_ = false;
		};

		bool StartUpBase( const Param& param );

	private:
		void ShutDownBase();
	public:

		void Loop();

		virtual void UpdateAndRender( const Timer& timer )
		{
			(void)timer;
		}

	private:
		static LRESULT CALLBACK WinProcStatic( HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam );
		LRESULT WinProc( HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam );

	protected:
		std::string appName_;
		HINSTANCE hThisInst_ = nullptr;
		HWND hWnd_ = nullptr;
		bool continueLoop_ = true;

		Timer timer_;

		std::unique_ptr<Dx11> dx11_;

		static AppBase* instance_;
	};

} // namespace spad