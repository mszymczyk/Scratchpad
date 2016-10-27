#pragma once

#include <Util/SettingsEditor/SettingsEditor.h>
#include "FrameworkSettings.h"

namespace FrameworkSettingsNamespace
{

// General
//////////////////////////////////////////////////////////////////////////////////
struct General
{
	SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

	enum class e_Vsync
	{
		eVSync_disabled = 0,
		eVSync_60Hz = 1,
		eVSync_30Hz = 2,
		eVSync_15Hz = 3,
		NumValues
	};

	e_Vsync vsync = e_Vsync::eVSync_60Hz;
};


} // namespace FrameworkSettingsNamespace
