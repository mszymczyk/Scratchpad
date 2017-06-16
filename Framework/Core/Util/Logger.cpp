#include "Util_pch.h"
#include "Logger.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{
	namespace log
	{
		struct _LogImpl
		{
			_LogImpl()
				: logToStdError_( false )
				, initColors_( 0 )
				, origAttributes_( 0 )
				, backgroundAttributes_( 0 )
				, callback_( nullptr )
			{	}

			std::mutex mutex_;

			bool logToStdError_;
			unsigned int initColors_;
			WORD origAttributes_;
			WORD backgroundAttributes_;
			LogCallbackType callback_;
		};

		_LogImpl* _gLog;

		void logFormattedText( e_LogLevel level, const char* /*tag*/, const char* filename, const int lineNo, const char* format, ... )
		{
			va_list	args;

			va_start( args, format );
			const u32 bufferOnStackSize = 2 * 1024;
			char str_buffer_onStack[bufferOnStackSize];
			const char* str_buffer = str_buffer_onStack;
			std::string str_buffer_onHeap;
			int str_bufferLen = vsnprintf( str_buffer_onStack, bufferOnStackSize, format, args );
			if ( str_bufferLen >= bufferOnStackSize || str_bufferLen < 0 )
			{
				// when there's not enough space in buffer, vsnprintf returns "encoding error" (value < 0)
				// in this case, alloc mem dynamically and call it again
				//
				const u32 bufferOnHeapSize = 32 * 1024;
				str_buffer_onHeap.resize( bufferOnHeapSize );
				str_buffer = str_buffer_onHeap.c_str();
				str_bufferLen = vsnprintf( &str_buffer_onHeap[0], bufferOnHeapSize, format, args );
				if (str_bufferLen >= bufferOnHeapSize)
					str_buffer_onHeap[bufferOnHeapSize - 1] = 0;
			}
			va_end( args );

			const char* shortFilename = filename;
			char where_buffer[512];
			int where_bufferLen = 0;

			if ( level < eLogLevel_debug )
			{
				if ( filename )
				{
					size_t filenameLen = strlen( filename );
					if ( filenameLen )
					{
						shortFilename = filename + filenameLen - 1; // point at last character
						for ( size_t i = filenameLen - 1; i > 0; --i )
						{
							char c = *shortFilename;
							--shortFilename;

							if ( c == '/' || c == '\\' )
							{
								shortFilename += 2;
								break;
							}
						}
					}

					if ( level < eLogLevel_debug )
					{
						where_bufferLen = spad_snprintf( where_buffer, 512, " at %s(%d)", shortFilename, lineNo );
						if ( where_bufferLen >= 512 )
						{
							where_buffer[511] = 0;
							where_bufferLen = 511;
						}
					}
				}
			}

			if ( level < eLogLevel_debug || level == eLogLevel_message )
			{
				if ( _gLog )
				{
					std::lock_guard<std::mutex> lck( _gLog->mutex_ );

					if ( !_gLog->logToStdError_ )
					{
						HANDLE output = GetStdHandle( STD_OUTPUT_HANDLE );
						if ( _gLog->initColors_ == 0 )
						{
							if ( _gLog->initColors_ == 0 )
							{
								CONSOLE_SCREEN_BUFFER_INFO con_info;
								GetConsoleScreenBufferInfo( output, &con_info );
								_gLog->origAttributes_ = con_info.wAttributes;
								_gLog->backgroundAttributes_ = _gLog->origAttributes_ & 0xF0;
								_gLog->initColors_ = 1;
							}
						}

						switch ( level )
						{
						case eLogLevel_info:
						case eLogLevel_message:
							SetConsoleTextAttribute( output, _gLog->backgroundAttributes_ | 2 );
							break;
						case eLogLevel_warning:
							SetConsoleTextAttribute( output, _gLog->backgroundAttributes_ | 6 );
							break;
						case eLogLevel_fatal:
						case eLogLevel_error:
							SetConsoleTextAttribute( output, _gLog->backgroundAttributes_ | 12 );
							break;
						};

						DWORD written = 0;

						WriteConsole( output, str_buffer, (DWORD)str_bufferLen, &written, 0 );
						SetConsoleTextAttribute( output, _gLog->origAttributes_ );
						if ( where_bufferLen )
							WriteConsole( output, where_buffer, (DWORD)where_bufferLen, &written, 0 );
						WriteConsole( output, "\r\n", 2, &written, 0 );
					}
					else
					{
						std::cerr << str_buffer << std::endl;
					}
				}
				else
				{
					std::cerr << str_buffer << std::endl;
				}

				if (_gLog && _gLog->callback_)
					_gLog->callback_( level, str_buffer );

			} // if ( level < eLogLevel_debug || level == eLogLevel_message )
		}

		void wlogFormattedText( e_LogLevel level, const char* /*tag*/, const char* filename, const int lineNo, const wchar* format, ... )
		{
			va_list	args;

			va_start( args, format );
			wchar st_buffer[512];
			int st_bufferLen = _vsnwprintf_s( st_buffer, 512, _TRUNCATE, format, args );
			if ( st_bufferLen >= 512 )
			{
				st_bufferLen = 511;
				st_buffer[511] = 0;
			}
			va_end( args );

			const char* shortFilename = filename;
			char where_buffer[512];
			int where_bufferLen = 0;

			if ( level < eLogLevel_debug )
			{
				if ( _gLog && filename )
				{
					size_t filenameLen = strlen( filename );
					if ( filenameLen )
					{
						shortFilename = filename + filenameLen - 1; // point at last character
						for ( size_t i = filenameLen - 1; i > 0; --i )
						{
							char c = *shortFilename;
							--shortFilename;

							if ( c == '/' || c == '\\' )
							{
								shortFilename += 2;
								break;
							}
						}
					}

					if ( level < eLogLevel_debug )
					{
						where_bufferLen = spad_snprintf( where_buffer, 512, " at %s(%d)", shortFilename, lineNo );
						if ( where_bufferLen >= 512 )
						{
							where_buffer[511] = 0;
							where_bufferLen = 511;
						}
					}
				}
			}

			if ( level < eLogLevel_debug || level == eLogLevel_message )
			{
				if ( _gLog )
				{
					std::lock_guard<std::mutex> lck( _gLog->mutex_ );

					if ( !_gLog->logToStdError_ )
					{
						HANDLE output = GetStdHandle( STD_OUTPUT_HANDLE );
						if ( _gLog->initColors_ == 0 )
						{
							if ( _gLog->initColors_ == 0 )
							{
								CONSOLE_SCREEN_BUFFER_INFO con_info;
								GetConsoleScreenBufferInfo( output, &con_info );
								_gLog->origAttributes_ = con_info.wAttributes;
								_gLog->backgroundAttributes_ = _gLog->origAttributes_ & 0xF0;
								_gLog->initColors_ = 1;
							}
						}

						switch ( level )
						{
						case eLogLevel_info:
						case eLogLevel_message:
							SetConsoleTextAttribute( output, _gLog->backgroundAttributes_ | 2 );
							break;
						case eLogLevel_warning:
							SetConsoleTextAttribute( output, _gLog->backgroundAttributes_ | 6 );
							break;
						case eLogLevel_fatal:
						case eLogLevel_error:
							SetConsoleTextAttribute( output, _gLog->backgroundAttributes_ | 12 );
							break;
						};

						DWORD written = 0;

						WriteConsole( output, st_buffer, (DWORD)st_bufferLen, &written, 0 );
						SetConsoleTextAttribute( output, _gLog->origAttributes_ );
						if ( where_bufferLen )
							WriteConsole( output, where_buffer, (DWORD)where_bufferLen, &written, 0 );
						WriteConsole( output, "\r\n", 2, &written, 0 );
					}
					else
					{
						std::wcerr << st_buffer << std::endl;
					}
				}
				else
				{
					std::wcerr << st_buffer << std::endl;
				}

			} // if ( level < eLogLevel_debug || level == eLogLevel_message )
		}

		void logStartUp()
		{
			SPAD_ASSERT2( !_gLog, "loggger already initialized" );
			_gLog = new _LogImpl();
		}

		void logShutDown()
		{
			delete _gLog;
			_gLog = NULL;
		}

		void logToStdError( bool yesno )
		{
			SPAD_ASSERT2( _gLog, "loggger not initialized" );
			_gLog->logToStdError_ = yesno;
		}

		void logSetCallback( LogCallbackType callback )
		{
			SPAD_ASSERT2( _gLog, "loggger not initialized" );
			_gLog->callback_ = callback;
		}

		spad::log::LogCallbackType logGetCallback()
		{
			return _gLog->callback_;
		}

	} // namespace log
} // namespace spad
