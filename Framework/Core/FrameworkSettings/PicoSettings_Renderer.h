#pragma once

#include <picoCore/SettingsEditor.h>
#include "PicoSettings.h"

namespace PicoSettingsNamespace
{

// Renderer
//////////////////////////////////////////////////////////////////////////////////
struct Renderer
{
	SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

	enum class e_VisibleRenderTarget
	{
		eVisibleRenderTarget_color0 = 0,
		eVisibleRenderTarget_depth = 1,
		eVisibleRenderTarget_shadowMap = 2,
		eVisibleRenderTarget_normals = 3,
		eVisibleRenderTarget_glow = 4,
		NumValues
	};

	enum class e_Msaa
	{
		eMsaa_1x = 1,
		eMsaa_2x = 2,
		eMsaa_4x = 4,
		eMsaa_8x = 8,
		NumValues
	};

	enum class e_Vsync
	{
		eVSync_disabled = 0,
		eVSync_60Hz = 1,
		eVSync_30Hz = 2,
		eVSync_20Hz = 3,
		NumValues
	};

	enum class e_DisplayOrientation
	{
		horizontal = 0,
		vertical = 1,
		NumValues
	};

	e_VisibleRenderTarget visibleRenderTarget = e_VisibleRenderTarget::eVisibleRenderTarget_color0;
	e_Vsync vsync = e_Vsync::eVSync_60Hz;
	e_DisplayOrientation displayOrientation = e_DisplayOrientation::horizontal;
	e_Msaa msaa = e_Msaa::eMsaa_1x;
	bool fxaa = false;
	const char* DebugCamera = nullptr;
	bool faderEnabled = true;
	bool drawFrustumOntopCascades = true;
	bool drawPrevCascadesFrustumOntop = true;

	// Shadows
	//////////////////////////////////////////////////////////////////////////////////
	struct Shadows
	{
		SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

		enum class ShadowMapResolution
		{
			Resolution_512x512 = 512,
			Resolution_1024x1024 = 1024,
			Resolution_2048x2048 = 2048,
			Resolution_4096x4096 = 4096,
			NumValues
		};

		enum class ShadowMapFormat
		{
			Format_16bit = 0,
			Format_32bit = 1,
			NumValues
		};

		enum class Filter
		{
			None = 0,
			PointGather_2x2 = 1,
			PCF_Optimized = 3,
			Gauss_3x3 = 6,
			PCF_3x3 = 7,
			EVSM_16bit_2Components = 9,
			EVSM_32bit_2Components = 265,
			EVSM_16bit_4Components = 522,
			EVSM_32bit_4Components = 778,
			MSM_Hausdorff_16bit = 1035,
			MSM_Hausdorff_32bit = 1292,
			MSM_Hamburger_16bit = 1549,
			MSM_Hamburger_32bit = 1806,
			NumValues
		};

		bool forceShadowMapResolution = true;
		ShadowMapResolution shadowMapResolution = ShadowMapResolution::Resolution_2048x2048;
		bool forceShadowMapFormat = true;
		ShadowMapFormat shadowMapFormat = ShadowMapFormat::Format_16bit;
		bool forceUpVector = true;
		Filter filter = Filter::PCF_Optimized;
		bool forceShadowNormalBiasSettings = false;
		float shadowNormalBiasNear = 0.1f;
		float shadowNormalBiasFar = 0.1f;
		float shadowNormalBiasFiltered = 0.1f;
		bool useCS = true;
		bool useSmartBlur = false;
		e_Msaa msaa = e_Msaa::eMsaa_1x;
		bool enableMipMaps = false;
		bool blur = false;
		bool separableBlur = false;
		int blurRadius = 5;
		int anisotropy = 16;
		float mipMapBias = 0;
		bool clampExponents = true;
		float evsmPositiveExponent = 60;
		float evsmNegativeExponent = 0;
		float vsmBias = 0;
		float evsmLightBleedingReduction = 0.5f;
		float msmDepthBias = 0;
		float msmMomentBias = 0.003f;
		float msmLightBleedingReduction = 0.5f;

		// Cascades
		//////////////////////////////////////////////////////////////////////////////////
		struct Cascades
		{
			SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

			bool forceNumCascades = true;
			int numCascades = 4;
			bool forcePlayerCameraFarPlaneOverride = false;
			SettingsEditor::FloatBool playerCameraFarPlaneOverride = SettingsEditor::FloatBool( 120, true );
			bool forceSplitSettings = false;
			bool manualSplits = false;
			float splitWeight = 0.75f;
			float split0 = 0.1f;
			float split1 = 0.25f;
			float split2 = 0.5f;
			float split3 = 1;
			bool forceBlendFactor = false;
			float cascadesBlendFactor = 0.05f;
			bool forceShadowBiasSettings = false;
			float shadowBias0 = 0.1f;
			float shadowBias1 = 0.5f;
			float shadowBias2 = 0.6f;
			float shadowBias3 = 1.5f;
		} mCascades;

		// Photomode
		//////////////////////////////////////////////////////////////////////////////////
		struct Photomode
		{
			SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

			int extraCascades = 2;
			float split0 = 1.5f;
			float split1 = 2;
			float split2 = 2;
			float split3 = 2;
		} mPhotomode;

		// Debug
		//////////////////////////////////////////////////////////////////////////////////
		struct Debug
		{
			SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

			enum class e_VisibleShadowMap
			{
				eVisibleShadowMap_Cascade0 = 0,
				eVisibleShadowMap_Cascade1 = 1,
				eVisibleShadowMap_Cascade2 = 2,
				eVisibleShadowMap_Cascade3 = 3,
				eVisibleShadowMap_AllCascades = 255,
				NumValues
			};

			const char* cascadesCamera = nullptr;
			bool reloadShader = true;
			bool showCascades = false;
			bool showCameraFrustum = false;
			bool showCascadeFrustum = false;
			bool showCascadeBoundingSphere = false;
			bool showWorldAABB = false;
			bool fakeMorpheusFieldOfView = false;
			e_VisibleShadowMap visibleShadowMap = e_VisibleShadowMap::eVisibleShadowMap_AllCascades;
			float shadowMapOpacity = 1;
			float shadowMapDepthScale = 1;
			float shadowMapDepthMin = 0;
			float shadowMapDepthMax = 1;
			bool valientStableCascades = true;
			bool texelSnapping = true;
			bool tightBoundingSphere = false;
			bool fitCascadeNearFarToWorldAABB = true;
			bool fitCascadeFarToCameraFrustum = true;
			bool clipAgainstExtraFrustumPlanes = true;
			bool initializeWithPrevCascadesFrustum = true;
		} mDebug;

	} mShadows;

	// GpuOcclusionCulling
	//////////////////////////////////////////////////////////////////////////////////
	struct GpuOcclusionCulling
	{
		SETTINGS_EDITOR_STRUCT_DESC // this macro is required for SettingsEditor to work

		enum class e_OcclusionMapSize
		{
			eOcclusionMapSize_128x128 = 128,
			eOcclusionMapSize_256x256 = 256,
			eOcclusionMapSize_512x512 = 512,
			eOcclusionMapSize_1024x1024 = 1024,
			eOcclusionMapSize_2048x2048 = 2048,
			NumValues
		};

		e_OcclusionMapSize occlusionMapSize = e_OcclusionMapSize::eOcclusionMapSize_256x256;
		bool enabledColor = true;
		bool enabledCascade0 = true;
		bool enabledCascade1 = true;
		bool enabledCascade2 = true;
		bool enabledCascade3 = true;
		bool enabledCascade4 = true;
		bool enabledCascade5 = true;
		bool enabledCascade6 = true;
		bool enabledCascade7 = true;
		bool initializeWithPlayersFrustum = true;
		bool initializeWithPrevCascadesFrustum = true;
		bool debugShow = false;
		float debugDrawOpacityColor = 1;
		float debugDrawDepthScale = 1;
		float debugDrawDepthMin = 1;
		float debugDrawDepthMax = 1;
		float debugDrawMipLevel = 0;
		bool debugReloadShader = false;
	} mGpuOcclusionCulling;

};


} // namespace PicoSettingsNamespace
