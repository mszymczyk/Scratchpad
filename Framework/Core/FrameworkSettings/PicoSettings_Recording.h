#pragma once

#include <picoCore/SettingsEditor.h>
#include "PicoSettings.h"

namespace PicoSettingsNamespace
{

// Recording
//////////////////////////////////////////////////////////////////////////////////
struct Recording
{
	SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

	bool HideLetterBox = false;
	bool HideCutsceneTexts = false;
};


} // namespace PicoSettingsNamespace
