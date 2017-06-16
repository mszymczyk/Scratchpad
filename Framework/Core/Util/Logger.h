#pragma once

#define FR_ENABLE_LOGGER

namespace spad
{
	namespace log
	{
		enum e_LogLevel
		{
			eLogLevel_fatal = 0,
			eLogLevel_error = 1,
			eLogLevel_warning = 2,
			eLogLevel_info = 3,
			eLogLevel_debug = 4,
			eLogLevel_trace = 5,
			eLogLevel_message = 6 // always visible, even in release final final builds
		};

		void logFormattedText( e_LogLevel level, const char* tag, const char* filename, const int lineNo, const char* format, ... );
		void wlogFormattedText( e_LogLevel level, const char* tag, const char* filename, const int lineNo, const wchar* format, ... );

		typedef int( __stdcall * LogCallbackType )( int messageType, const char* text );

		void logStartUp();
		void logShutDown();
		void logToStdError( bool yesno );
		void logSetCallback( LogCallbackType callback );
		LogCallbackType logGetCallback();
	}

#ifdef FR_ENABLE_LOGGER

	/**
	 *	Macro for displaying warning and error on all builds.
	 */
#define logInfo(...)					( spad::log::logFormattedText(spad::log::eLogLevel_info, "general", __FILE__, __LINE__, __VA_ARGS__)	)
#define logWarning(...)					( spad::log::logFormattedText(spad::log::eLogLevel_warning, "general", __FILE__, __LINE__, __VA_ARGS__)	)
#define logError(...)					( spad::log::logFormattedText(spad::log::eLogLevel_error, "general", __FILE__, __LINE__, __VA_ARGS__)	)
#define wlogError(...)					( spad::log::wlogFormattedText(spad::log::eLogLevel_error, "general", __FILE__, __LINE__, __VA_ARGS__)	)

#define logErrorTag(tag, ...)			( spad::log::logFormattedText(spad::log::eLogLevel_error, tag, __FILE__, __LINE__, __VA_ARGS__)	)
#define logWarningTag(tag, ...)			( spad::log::logFormattedText(spad::log::eLogLevel_warning, tag, __FILE__, __LINE__, __VA_ARGS__)	)
#define logInfoTag(tag, ...)			( spad::log::logFormattedText(spad::log::eLogLevel_info, tag, __FILE__, __LINE__, __VA_ARGS__)	)
#define logDebugTag(tag, ...)			( spad::log::logFormattedText(spad::log::eLogLevel_debug, tag, __FILE__, __LINE__, __VA_ARGS__)	)

#else

#define logInfo(...)
#define logWarning(...)
#define logError(...)

#define logErrorTag(tag, ...)
#define logWarningTag(tag, ...)
#define logInfoTag(tag, ...)
#define logDebugTag(tag, ...)

#endif // FR_ENABLE_LOGGER

#define logErrorAlways(...)				( spad::log::logFormattedText(spad::log::eLogLevel_error, "general", __FILE__, __LINE__, __VA_ARGS__)	)

	// this will print/write to file regardless of build configuration
	//
#define logMessageAlways(...)			( spad::log::logFormattedText(spad::log::eLogLevel_message, "message", __FILE__, __LINE__, __VA_ARGS__)	)

} // namespace spad
