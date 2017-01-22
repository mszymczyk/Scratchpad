#pragma once

#include <Tools/SettingsEditor/ClientLib/SettingsEditor.h>
#include "SampleSettings.h"

namespace SampleSettingsNamespace
{

// Group
//////////////////////////////////////////////////////////////////////////////////
struct Group
{
	SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work


	// Inner
	//////////////////////////////////////////////////////////////////////////////////
	struct Inner
	{
		SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

		bool boolFirst = false;
		float floatFirst = 1;
		bool boolSecond = false;
		float floatSecond = 2;
	} mInner;

};


} // namespace SampleSettingsNamespace
