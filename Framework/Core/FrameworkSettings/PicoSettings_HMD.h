#pragma once

#include <picoCore/SettingsEditor.h>
#include "PicoSettings.h"

namespace PicoSettingsNamespace
{

// HMD
//////////////////////////////////////////////////////////////////////////////////
struct HMD
{
	SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

	bool Supported = true;
	bool OculusSupported = true;
	bool OculusEnabled = true;
	bool MorpheusSupported = true;
	bool MorpheusEnabled = true;

	// Generic
	//////////////////////////////////////////////////////////////////////////////////
	struct Generic
	{
		SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

		enum class e_CamChangeFadeType
		{
			Linear = 0,
			Smoothstep = 1,
			NumValues
		};

		SettingsEditor::FloatBool hmdOversample = SettingsEditor::FloatBool( 1.5f, true );
		bool DistortionCancellerEnabled = true;
		SettingsEditor::FloatBool AffectAllCameraNodesPositionScale = SettingsEditor::FloatBool( 1.5f, true );
		SettingsEditor::FloatBool NearPlaneOverride = SettingsEditor::FloatBool( 0.3f, true );
		e_CamChangeFadeType FadeType = e_CamChangeFadeType::Smoothstep;
		float FadeInDuration = 0.2f;
		float FadeOutDuration = 0.2f;
		float FadeInDurationCutscene = 0.15f;
		float FadeOutDurationCutscene = 0.15f;
		float PlayerSpeedMin = 5;
		float PlayerSpeedMax = 6;
		float HudScale = 1;
	} mGeneric;

	// Morpheus
	//////////////////////////////////////////////////////////////////////////////////
	struct Morpheus
	{
		SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

		enum class e_RefreshRate
		{
			RefreshRate_90Hz = 0,
			RefreshRate_120Hz = 1,
			NumValues
		};

		e_RefreshRate RefreshRate = e_RefreshRate::RefreshRate_120Hz;
		bool PredicationEnabled = true;
		float PredicationExtraTime = 0;
		bool ReprojectionEnabled = true;
		bool HTileClearOptimization = true;
		float HTileClearOptimizationFactor = 1;
		bool LowLatencyTrackingEnabled = false;
	} mMorpheus;

};


} // namespace PicoSettingsNamespace
