#pragma once

#include <picoCore/SettingsEditor.h>
#include "PicoSettings.h"

namespace PicoSettingsNamespace
{

// TextUtil
//////////////////////////////////////////////////////////////////////////////////
struct TextUtil
{
	SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

	enum class e_Language
	{
		systemDefault = 0,
		english = 1,
		japanese = 2,
		french = 3,
		spanish = 4,
		german = 5,
		italian = 6,
		dutch = 7,
		portuguese = 8,
		russian = 9,
		korean = 10,
		chineseTr = 11,
		chineseSp = 12,
		portugueseBr = 13,
		spanishLa = 14,
		frenchCa = 15,
		NumValues
	};

	bool ReloadTexts = false;
	e_Language Language = e_Language::systemDefault;
	float subtitleWindowPositionX = 0.5f;
	float subtitleWindowPositionY = 0.94f;
	float subtitleWindowWidth = 0.7f;
	float hmdSubtitleWindowPositionX = 0.5f;
	float hmdSubtitleWindowWidth = 0.7f;
	float hmdSubtitleWindowPositionY = 0.94f;
	float iconWindowPositionX = 0.8f;
	float iconWindowPositionY = 0.85f;
	float hmdIconWindowPositionX = 0.8f;
	float hmdIconWindowPositionY = 0.45f;
};


} // namespace PicoSettingsNamespace
