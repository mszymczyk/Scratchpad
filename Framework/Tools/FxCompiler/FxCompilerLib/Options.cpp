#include <FxCompilerLib_pch.h>
#include "Options.h"

namespace spad
{
namespace fxlib
{


int SetupOptions( FxFileCompileOptions& options )
{
	if ( options.compileForDebugging_ )
	{
		options.defines_.emplace_back( "DEBUG", "1" );
	}
	else
	{
		options.defines_.emplace_back( "NDEBUG", "1" );
		options.defines_.emplace_back( "RELEASE", "1" );
	}

	return 0;
}

bool TryGetU32Arg( const option::Option& opt, uint32_t& dst )
{
	if ( !opt )
		return true;

	if ( !opt.arg )
		return false;

	std::stringstream ss( opt.arg );
	uint32_t tmp;
	ss >> tmp;
	if ( ss.fail() )
		return false;

	dst = tmp;

	return true;
}

bool TryGetS32Arg( const option::Option& opt, int32_t& dst )
{
	if ( !opt )
		return true;

	if ( !opt.arg )
		return false;

	std::stringstream ss( opt.arg );
	int32_t tmp;
	ss >> tmp;
	if ( ss.fail() )
		return false;

	dst = tmp;
	return true;
}


} // namespace fxlib
} // namespace spad
