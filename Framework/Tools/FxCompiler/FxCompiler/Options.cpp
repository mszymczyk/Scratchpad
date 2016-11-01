#include <FxCompiler_pch.h>
#include "Options.h"
#include "../FxCompilerLib/Options.h"

using namespace spad;

struct Arg : public option::Arg
{
	static void printError( const char* msg1, const option::Option& opt, const char* msg2 )
	{
		fprintf( stderr, "%s", msg1 );
		fwrite( opt.name, opt.namelen, 1, stderr );
		fprintf( stderr, "%s", msg2 );
	}

	static option::ArgStatus Unknown( const option::Option& option, bool msg )
	{
		if ( msg ) printError( "Unknown option '", option, "'\n" );
		return option::ARG_ILLEGAL;
	}

	static option::ArgStatus Required( const option::Option& option, bool msg )
	{
		if ( option.arg != 0 )
			return option::ARG_OK;

		if ( msg ) printError( "Option '", option, "' requires an argument\n" );
		return option::ARG_ILLEGAL;
	}

	static option::ArgStatus NonEmpty( const option::Option& option, bool msg )
	{
		if ( option.arg != 0 && option.arg[0] != 0 )
			return option::ARG_OK;

		if ( msg ) printError( "Option '", option, "' requires a non-empty argument\n" );
		return option::ARG_ILLEGAL;
	}

	static option::ArgStatus Numeric( const option::Option& option, bool msg )
	{
		char* endptr = 0;
		if ( option.arg != 0 && strtol( option.arg, &endptr, 10 ) ) {};
		if ( endptr != option.arg && *endptr == 0 )
			return option::ARG_OK;

		if ( msg ) printError( "Option '", option, "' requires a numeric argument\n" );
		return option::ARG_ILLEGAL;
	}
};

//enum  optionIndex { UNKNOWN, HELP, OPTIONAL, REQUIRED, NUMERIC, NONEMPTY };
enum optionIndex
{
	UNKNOWN, // file

	cleanOutputs, // 'Clean' target in visual studio or msbuild

	// configuration
	configuration_debug,
	configuration_diagnostic,
	configuration_shipping,

	// general
	fxHeader,

	outputDir,
	intermediateDir,

	no_multithreadedFiles, // process multiple files simultaneously
	no_multithreaded, // process multiple programs within file simultaneously

	// hlsl
	hlsl_outputDir,
	hlsl_intermediateDir,
};



const option::Descriptor usage[] =
{
	{ cleanOutputs, 0, "c", "clean", Arg::None," -c\t--clean\t Cleans outputs" },

	// configuration
	{ configuration_debug, 0, "", "debug", Arg::None," --debug\t Compiles as debug, unoptimized version" },
	{ configuration_diagnostic, 0, "", "diagnostic", Arg::None," --diagnostic\t Generates diagnostic/profiling data" },
	{ configuration_shipping, 0, "", "shipping", Arg::None," --shipping\t Compiles for packaging" },

	// general
	{ fxHeader, 0, "", "fxheader", Arg::None,"  --fxheader" },

	{ outputDir, 0, "","outdir", Arg::Required,"  --outdir=<path>" },
	{ intermediateDir, 0, "","intdir", Arg::Required,"  --intdir=<path>" },

	{ no_multithreadedFiles, 0, "","nomtfiles", Arg::None,"  --nomtfiles" },
	{ no_multithreaded, 0, "","nomt", Arg::None,"  --nomt" },

	// hlsl
	{ hlsl_outputDir, 0, "","hlslOutDir", Arg::Required,"  --hlslOutDir=<path>" },
	{ hlsl_intermediateDir, 0, "","hlslIntDir", Arg::Required,"  --hlslIntDir=<path>" },

	{ UNKNOWN, 0,"", "",        Arg::None, "" },

	{ 0, 0, 0, 0, 0, 0 }
};


int ParseOptions( spad::fxlib::FxFileCompileOptions& options
	, spad::fxlib::hlsl::FxFileHlslCompileOptions& hlslOptions
	, std::vector<std::string>& files
	, spad::fxlib::FxCompileConfiguration::Type& selectedConfiguration
	, bool& cleanOutputFiles // 'Clean' selected in visual studio
)
{
	int argc = options.argc_ - 1;
	const char ** argv = const_cast<const char**>( options.argv_ ) + 1;
	option::Stats stats( usage, argc, argv );

	std::vector<option::Option> parsedOptions( stats.options_max );
	std::vector<option::Option> parsedBuffer( stats.buffer_max );

	option::Parser parse( usage, argc, argv, &parsedOptions[0], &parsedBuffer[0] );

	if ( parse.error() )
		return -1;

	cleanOutputFiles = false;
	if ( parsedOptions[cleanOutputs] )
		cleanOutputFiles = true;

	// configuration

	selectedConfiguration = fxlib::FxCompileConfiguration::release;

	if ( parsedOptions[configuration_debug] )
	{
		selectedConfiguration = fxlib::FxCompileConfiguration::debug;
	}
	else if ( parsedOptions[configuration_diagnostic] )
	{
		selectedConfiguration = fxlib::FxCompileConfiguration::diagnostic;
	}
	else if ( parsedOptions[configuration_shipping] )
	{
		selectedConfiguration = fxlib::FxCompileConfiguration::shipping;
	}

	// general

	if ( parsedOptions[fxHeader] )
		options.defines_.emplace_back( "FX_HEADER", "1" );

	if ( parsedOptions[no_multithreadedFiles] )
		options.multithreadedFiles_ = false;

	if ( parsedOptions[no_multithreaded] )
		options.multithreaded_ = false;



	// hlsl

	if ( const option::Option* opt = parsedOptions[hlsl_outputDir] )
	{
		hlslOptions.outputDirectory_ = opt->arg;
		spad::AppendBackslashToDirectoryName( hlslOptions.outputDirectory_ );
	}

	if ( const option::Option* opt = parsedOptions[hlsl_intermediateDir] )
	{
		hlslOptions.intermediateDirectory_ = opt->arg;
		spad::AppendBackslashToDirectoryName( hlslOptions.intermediateDirectory_ );
	}




	// files

	for ( int i = 0; i < parse.nonOptionsCount(); ++i )
	{
		const char* file = parse.nonOption( i );
		files.emplace_back( file );
	}

	return spad::fxlib::SetupOptions( options );
}
