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

	// Inner
	//////////////////////////////////////////////////////////////////////////////////
	struct Inner
	{
		SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

		float floatParam = 1;
		bool boolParam = true;
		int intParam = 1;
		e_Vsync vsync2 = e_Vsync::eVSync_15Hz;

		const Inner* getPreset( const char* presetName ) const
		{
			return reinterpret_cast<const Inner*>( SettingsEditor::DontTouchIt::getPreset( presetName, impl_ ) );
		}

	} mInner;

	// SampleGroup
	//////////////////////////////////////////////////////////////////////////////////
	struct SampleGroup
	{
		SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

	} mSampleGroup;

};


} // namespace FrameworkSettingsNamespace
