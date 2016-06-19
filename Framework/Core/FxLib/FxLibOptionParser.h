#pragma once
/*******************************************************************************
*   2005-2016, plastic demoscene group
*	picoEngine
*******************************************************************************/
#include <external/optionparser/src/optionparser.h>

namespace picoFxLibOptionParser
{
	typedef int( *ParseOptionsCallback ) (
		std::vector<option::Option>& options,
		void* userData
		);

	struct Arg : public option::Arg
	{
		static option::ArgStatus Required( const option::Option& option, bool )
		{
			return option.arg == 0 ? option::ARG_ILLEGAL : option::ARG_OK;
		}
		//static option::ArgStatus Empty( const option::Option& option, bool )
		//{
		//	return ( option.arg == 0 || option.arg[0] == 0 ) ? option::ARG_OK : option::ARG_IGNORE;
		//}
	};

	int parseOptions( const std::string& cflags, const option::Descriptor usage[], ParseOptionsCallback optionParseCallback, void* userData );
	bool tryGetU32Arg( const option::Option& opt, uint32_t& dst );
	bool tryGetS32Arg( const option::Option& opt, int32_t& dst );

} // namespace picoFxLibOptionParser
