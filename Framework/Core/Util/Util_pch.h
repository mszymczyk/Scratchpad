#pragma once

#include "Def.h"
#include "Logger.h"
#include "SysIncludes.h"

// to understand why '#include "Util_pch.h"' can confuse IntelliSense
// see http://stackoverflow.com/questions/31943634/visual-studio-2015-intellisense-errors-but-solution-compiles
// when including pch in cpp file that lies in directory other than pch's dir, IntelliSense won't be able to locate "physical" file on filesystem
// solution is to create helper file as explained in the post above
// or #include <Util_pch.h> and add $(ProjectDir) to Additional Include Directories project property
