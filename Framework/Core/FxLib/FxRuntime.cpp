#include "FxLib_pch.h"
#include "FxRuntime.h"

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{
	namespace FxLib
	{
		const FxRuntimePass& FxRuntime::getPass( const char* pass ) const
		{
			PassMap::const_iterator it = passes_.find( pass );
			if ( it == passes_.end() )
				THROW_MESSAGE( "Pass '%s' not found! (%s)", pass, filename_.c_str() );

			return it->second;
		}

	} // namespace FxLib

} // namespace spad
