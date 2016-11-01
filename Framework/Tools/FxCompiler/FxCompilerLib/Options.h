#pragma once

#include "FxCompilerLib.h"
#include <optionparser/src/optionparser.h>

namespace spad
{
namespace fxlib
{


int SetupOptions( FxFileCompileOptions& options );


bool TryGetU32Arg( const option::Option& opt, uint32_t& dst );
bool TryGetS32Arg( const option::Option& opt, int32_t& dst );


} // namespace fxlib
} // namespace spad
