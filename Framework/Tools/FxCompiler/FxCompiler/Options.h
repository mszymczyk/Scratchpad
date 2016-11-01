#pragma once

#include "../FxCompilerLib/FxCompilerLib.h"
#include "../FxCompilerLibHlsl/HlslCompile.h"
#include <optionparser/src/optionparser.h>


int ParseOptions( spad::fxlib::FxFileCompileOptions& options
	, spad::fxlib::hlsl::FxFileHlslCompileOptions& hlslOptions
	, std::vector<std::string>& files
	, spad::fxlib::FxCompileConfiguration::Type& selectedConfiguration
	, bool& cleanOutputFiles // 'Clean' selected in visual studio
);

