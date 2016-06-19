#include "picoFxLib_pch.h"
/*******************************************************************************
*   2005-2015, plastic demoscene group
*	picoEngine
*******************************************************************************/
#include "picoFxLibOptionParser.h"
#include <sstream>

namespace picoFxLibOptionParser
{
	int parseOptions( const std::string& cflags, const option::Descriptor usage[], ParseOptionsCallback optionParseCallback, void* userData )
	{
		std::vector<string> strings;
		istringstream f( cflags );
		string s;
		while ( getline( f, s, ' ' ) )
		{
			if ( !s.empty() )
			{
				strings.push_back( s );
			}
		}

		std::vector<const char*> strings_c;
		strings_c.resize( strings.size() );
		for ( size_t i = 0; i < strings.size(); ++i )
		{
			strings_c[i] = strings[i].c_str();
		}

		const int argc = (int)strings_c.size();
		const char** argv = &strings_c[0];

		//po::options_description desc( "Allowed options" );
		//desc.add_options()
		//	( "fastmath", po::value<int32_t>(&sceOptions.useFastmath)->default_value( 1 ), "fastmath" )
		//	;

		//po::variables_map vm;
		//po::store( po::command_line_parser( strings ).options( desc ).run(), vm );
		////po::store( po::parse_command_line( args, argv, desc ), vm );
		//po::notify( vm );

		//if ( vm.count( "fastmath" ) )
		//{
		//	sceOptions.useFastmath = 1;
		//}


		option::Stats stats( usage, argc, argv, 0, true );
		std::vector<option::Option> options( stats.options_max );
		std::vector<option::Option> buffer( stats.buffer_max );
		option::Parser parse( usage, argc, argv, &options[0], &buffer[0], 0, true );

		if ( parse.error() )
			return 1;

		//if ( options[psslc_options::fastmath] )
		//	sceOptions.useFastmath = 1;
		//if ( options[psslc_options::nofastmath] )
		//	sceOptions.useFastmath = 0;

		//if ( !psslc_options::tryGetU32Arg( options[psslc_options::max_shader_vgpr_count], sceOptions.shaderVGPRs ) )
		//	return -1;

		//return 0;
		return optionParseCallback( options, userData );
	}

	bool tryGetU32Arg( const option::Option& opt, uint32_t& dst )
	{
		if ( !opt )
			return true;

		if ( !opt.arg )
		{
			picoLogError( "tryGetU32Arg: '%s' arg is null!", opt.name );
			return false;
		}

		//int ival = atoi( opt.arg );
		//if ( ival < 0 )
		//{
		//	picoLogError( "tryGetU32Arg: arg '%s' < 0!", opt.name );
		//	return false;
		//}

		std::stringstream ss( opt.arg );
		uint32_t tmp;
		ss >> tmp;
		if ( ss.fail() )
		{
			picoLogError( "tryGetU32Arg: error converting '%s' to uint32_t!", opt.name );
			return false;
		}

		dst = tmp;
		return true;
	}

	bool tryGetS32Arg( const option::Option& opt, int32_t& dst )
	{
		if ( !opt )
			return true;

		if ( !opt.arg )
		{
			picoLogError( "tryGetS32Arg: '%s' arg is null!", opt.name );
			return false;
		}

		std::stringstream ss( opt.arg );
		int32_t tmp;
		ss >> tmp;
		if ( ss.fail() )
		{
			picoLogError( "tryGetS32Arg: error converting '%s' to int32_t!", opt.name );
			return false;
		}

		dst = tmp;
		return true;
	}

} // namespace picoFxLibOptionParser