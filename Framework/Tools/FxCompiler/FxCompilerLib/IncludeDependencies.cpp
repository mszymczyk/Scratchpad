#include "FxCompilerLib_pch.h"
#include "IncludeDependencies.h"
#include <fstream>

#if defined(_MSC_VER) && defined(_DEBUG)
#define new _DEBUG_NEW
#endif

namespace spad
{
namespace fxlib
{


int IncludeDependencies::writeToFile( const std::string& filename, FxCompileConfiguration::Type configuration )
{
	std::lock_guard<std::mutex> lck( mutex_ );

	std::ofstream dstf( filename );

	switch ( configuration )
	{
	case spad::fxlib::FxCompileConfiguration::debug:
		dstf << "configuration: debug" << std::endl;
		break;
	case spad::fxlib::FxCompileConfiguration::release:
		dstf << "configuration: release" << std::endl;
		break;
	case spad::fxlib::FxCompileConfiguration::diagnostic:
		dstf << "configuration: diagnostic" << std::endl;
		break;
	case spad::fxlib::FxCompileConfiguration::shipping:
		dstf << "configuration: shipping" << std::endl;
		break;
	default:
		return -1;
	}

	for ( const auto& d : filenames_ )
		dstf << d << std::endl;

	dstf.close();

	return 0;
}


} // namespace fxlib
} // namespace spad
