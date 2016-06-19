#include "Util_pch.h"
#include "SysIncludes.h"
#include "Def.h"
#include "Logger.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

void assertPrintAndBreak( const char* text )
{
	logErrorAlways(text);
	__debugbreak();
}

void assertPrintAndBreak2(const char* text, const char* msg)
{
	logErrorAlways(text);
	logErrorAlways(msg);
	__debugbreak();
}

int fr_snprintf( char* buffer, size_t bufferSize, const char* format, ... )
{
	va_list	args;
	va_start( args, format );
	int ires = vsnprintf( buffer, bufferSize, format, args );
	va_end( args );

#if defined(_MSC_VER)

	if ( ires == (int)bufferSize )
	{
		FR_ASSERT2( false, "fr_snprintf output has been truncated!" );
		buffer[bufferSize - 1] = 0;
		return -1;
	}
	else if ( ires < 0 )
	{
		FR_ASSERT2( false, "fr_snprintf output has been truncated!" );
		buffer[bufferSize - 1] = 0;
		return -1;
	}
	else
	{
		return ires;
	}

#else

	if ( ires >= (int)bufferSize )
	{
		FR_ASSERT( false, "fr_snprintf output has been truncated!" );
		// null terminated character is appended always acoording to spec
		//
		//buffer[bufferSize-1] = 0;
		return -1;
	}
	else if ( ires < 0 )
	{
		FR_ASSERT( false, "fr_snprintf encoding error!" );
		buffer[0] = 0;
		return -1;
	}
	else
	{
		return ires;
	}

#endif //
}
