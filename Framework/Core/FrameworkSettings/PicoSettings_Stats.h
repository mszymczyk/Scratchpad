#pragma once

#include <picoCore/SettingsEditor.h>
#include "PicoSettings.h"

namespace PicoSettingsNamespace
{

// Stats
//////////////////////////////////////////////////////////////////////////////////
struct Stats
{
	SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

	bool Enabled = false;
	int NumberOfMarkerLevels = 0;
};


} // namespace PicoSettingsNamespace
