#include "Util_pch.h"
#include "Exceptions.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{




	std::string Exception::FormatMessage( const char* format, ... )
	{
		va_list	args;

		va_start( args, format );
		char st_buffer[512];
		int st_bufferLen = vsnprintf_s( st_buffer, 512, _TRUNCATE, format, args );
		if ( st_bufferLen >= 512 )
		{
			st_bufferLen = 511;
			st_buffer[511] = 0;
		}
		va_end( args );

		return std::string( st_buffer, st_bufferLen );
	}

} // namespace spad
