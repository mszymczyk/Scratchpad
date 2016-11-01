#pragma once

#include "Def.h"
#include "Logger.h"
#include "SysIncludes.h"

namespace spad
{
	// Base class for all exceptions
	//
	class Exception
	{
	public:
		//Exception()
		//{
		//}

		Exception( const std::string& exceptionMessage )
			: message_( exceptionMessage )
		{
			logError( "Exception thrown: %s", message_.c_str() );
		}

		Exception( std::string&& exceptionMessage )
			: message_( std::move(exceptionMessage) )
		{
			logError( "Exception thrown: %s", message_.c_str() );
		}

		Exception( const std::string& exceptionMessage, const char* file, int line )
			: message_( exceptionMessage )
			, file_( file )
			, line_( line )
		{
			//wlogError( L"Exception thrown: %s", exceptionMessage.c_str() );
			log::logFormattedText( log::eLogLevel_error, "exception", file, line, "Exception thrown: %s", message_.c_str() );
		}

		Exception( std::string&& exceptionMessage, const char* file, int line )
			: message_( std::move(exceptionMessage) )
			, file_( file )
			, line_( line )
		{
			//wlogError( L"Exception thrown: %s", exceptionMessage.c_str() );
			log::logFormattedText( log::eLogLevel_error, "exception", file, line, "Exception thrown: %s", message_.c_str() );
		}

		const std::string& GetMessage() const throw( )
		{
			return message_;
		}

		//void ShowErrorMessage() const throw ( )
		//{
		//	MessageBox( NULL, message.c_str(), L"Error", MB_OK | MB_ICONERROR );
		//}

		//static std::wstring FormatMessageW( const wchar* format, ... );
		static std::string FormatMessage( const char* format, ... );

	protected:

		std::string message_; // The error message
		std::string file_;
		int line_;
	};

#define THROW_EXCEPTION(msg) throw Exception( msg, __FILE__, __LINE__ )
#define THROW_MESSAGE(...) throw Exception( Exception::FormatMessage(__VA_ARGS__), __FILE__, __LINE__ )


	inline std::string GetWin32ErrorStringAnsi(DWORD errorCode)
	{
		char errorString[512];
		::FormatMessageA(FORMAT_MESSAGE_FROM_SYSTEM,
			0,
			errorCode,
			MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
			errorString,
			512,
			NULL);

		std::string message = "Win32 Error: ";
		message += errorString;
		return message;
	}


	// Exception thrown when a Win function fails.
	class WinException : public Exception
	{

	public:

		// Obtains a string for the specified Win32 error code
		WinException( std::string&& exceptionMessage, DWORD errorCode, const char* file, int line )
			: Exception( std::move( exceptionMessage ), file, line )
			, errorCode_( errorCode )
		{
			message_ += "Win Error: ";
			std::string winMsg = GetWin32ErrorStringAnsi(errorCode_);
			message_ += winMsg;
		}

		// Retrieve the error code
		DWORD GetErrorCode() const
		{
			return errorCode_;
		}

	protected:

		DWORD  errorCode_;    // The Win error code
	};

#define THROW_WIN_EXCEPTION(msg, errorCode) throw WinException( msg, errorCode, __FILE__, __LINE__ )



#define Win32Call(x)                                                            \
	__pragma(warning(push))                                                     \
	__pragma(warning(disable:4127))                                             \
	do                                                                          \
	{                                                                           \
		BOOL res_ = x;                                                          \
		SPAD_ASSERT2(res_ != 0, GetWin32ErrorStringAnsi(GetLastError()).c_str()); \
	}                                                                           \
	while(0)                                                                    \
	__pragma(warning(pop))

	//#define Win32Call(x) (x)



#define CallResult(expression)                                                            \
	__pragma(warning(push))                                                     \
	__pragma(warning(disable:4127))                                             \
	do                                                                          \
	{                                                                           \
		int res = (expression);                                                          \
		if ( res != 0 ) throw Exception( (#expression), __FILE__, __LINE__ ); \
	}                                                                           \
	while(0)                                                                    \
	__pragma(warning(pop))


} // namespace spad
