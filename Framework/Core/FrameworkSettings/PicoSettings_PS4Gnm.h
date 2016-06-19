#pragma once

#include <picoCore/SettingsEditor.h>
#include "PicoSettings.h"

namespace PicoSettingsNamespace
{

// PS4Gnm
//////////////////////////////////////////////////////////////////////////////////
struct PS4Gnm
{
	SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

	bool AsyncComputeEnabled = true;
	int maxWavesPerSIMD = 8;
};


} // namespace PicoSettingsNamespace
