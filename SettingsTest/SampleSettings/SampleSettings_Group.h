#pragma once

#include <Util/SettingsEditor/SettingsEditor.h>
#include "SampleSettings.h"

namespace SampleSettingsNamespace
{

// Group
//////////////////////////////////////////////////////////////////////////////////
struct Group
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

	enum class e_SampleEnum
	{
		e_first = 0,
		e_second = 1,
		e_third = 2,
		NumValues
	};

	e_Vsync vsync = e_Vsync::eVSync_60Hz;
	float sampleFloat = 3;
	SettingsEditor::FloatBool checkFloat = SettingsEditor::FloatBool( 3, true );
	SettingsEditor::Color color = SettingsEditor::Color(1, 0, 0);
	const char* sampleString = "string";
	SettingsEditor::Direction sampleDir = SettingsEditor::Direction(0, 0, 1);
	SettingsEditor::AnimCurve animCurve;

	// Inner
	//////////////////////////////////////////////////////////////////////////////////
	struct Inner
	{
		SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

		bool boolParam = true;
		int intParam = 1;
		e_SampleEnum enumParam = e_SampleEnum::e_first;
		float floatParam = 1;
		SettingsEditor::Float4 float4Param = SettingsEditor::Float4(1, 2, 3, 4);
		SettingsEditor::Color color = SettingsEditor::Color(1, 0, 0);
		SettingsEditor::AnimCurve animCurve;
		const char* stringParam = "config.txt";

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


} // namespace SampleSettingsNamespace
