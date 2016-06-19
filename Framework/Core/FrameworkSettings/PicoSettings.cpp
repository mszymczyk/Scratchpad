#include <picoCore_pch.h>
#include "PicoSettings.h"

#include "PicoSettings_Renderer.h"
#include "PicoSettings_HMD.h"
#include "PicoSettings_Stats.h"
#include "PicoSettings_PS4Gnm.h"
#include "PicoSettings_TextUtil.h"
#include "PicoSettings_Recording.h"

using namespace SettingsEditor;

namespace PicoSettingsNamespace
{


	namespace PicoSettings_Renderer_Shadows_Cascades_NS
	{
		const unsigned int Cascades_Field_Count = 18;
		const unsigned int Cascades_NestedStructures_Count = 0;
		FieldDescription Cascades_Fields[Cascades_Field_Count];

		StructDescription Cascades_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "Cascades";
			d.parentStructure_ = parent; 
			d.fields_ = Cascades_Fields;
			d.nestedStructures_ = nullptr;
			d.nFields_ = Cascades_Field_Count;
			d.nNestedStructures_ = Cascades_NestedStructures_Count;
			d.offset_ = offsetof( Renderer::Shadows, mCascades );
			d.sizeInBytes_ = sizeof(Renderer::Shadows::Cascades);
			Cascades_Fields[0] = FieldDescription( "forceNumCascades", offsetof(Renderer::Shadows::Cascades, forceNumCascades), eParamType_bool );
			Cascades_Fields[1] = FieldDescription( "numCascades", offsetof(Renderer::Shadows::Cascades, numCascades), eParamType_int );
			Cascades_Fields[2] = FieldDescription( "forcePlayerCameraFarPlaneOverride", offsetof(Renderer::Shadows::Cascades, forcePlayerCameraFarPlaneOverride), eParamType_bool );
			Cascades_Fields[3] = FieldDescription( "playerCameraFarPlaneOverride", offsetof(Renderer::Shadows::Cascades, playerCameraFarPlaneOverride), eParamType_floatBool );
			Cascades_Fields[4] = FieldDescription( "forceSplitSettings", offsetof(Renderer::Shadows::Cascades, forceSplitSettings), eParamType_bool );
			Cascades_Fields[5] = FieldDescription( "manualSplits", offsetof(Renderer::Shadows::Cascades, manualSplits), eParamType_bool );
			Cascades_Fields[6] = FieldDescription( "splitWeight", offsetof(Renderer::Shadows::Cascades, splitWeight), eParamType_float );
			Cascades_Fields[7] = FieldDescription( "split0", offsetof(Renderer::Shadows::Cascades, split0), eParamType_float );
			Cascades_Fields[8] = FieldDescription( "split1", offsetof(Renderer::Shadows::Cascades, split1), eParamType_float );
			Cascades_Fields[9] = FieldDescription( "split2", offsetof(Renderer::Shadows::Cascades, split2), eParamType_float );
			Cascades_Fields[10] = FieldDescription( "split3", offsetof(Renderer::Shadows::Cascades, split3), eParamType_float );
			Cascades_Fields[11] = FieldDescription( "forceBlendFactor", offsetof(Renderer::Shadows::Cascades, forceBlendFactor), eParamType_bool );
			Cascades_Fields[12] = FieldDescription( "cascadesBlendFactor", offsetof(Renderer::Shadows::Cascades, cascadesBlendFactor), eParamType_float );
			Cascades_Fields[13] = FieldDescription( "forceShadowBiasSettings", offsetof(Renderer::Shadows::Cascades, forceShadowBiasSettings), eParamType_bool );
			Cascades_Fields[14] = FieldDescription( "shadowBias0", offsetof(Renderer::Shadows::Cascades, shadowBias0), eParamType_float );
			Cascades_Fields[15] = FieldDescription( "shadowBias1", offsetof(Renderer::Shadows::Cascades, shadowBias1), eParamType_float );
			Cascades_Fields[16] = FieldDescription( "shadowBias2", offsetof(Renderer::Shadows::Cascades, shadowBias2), eParamType_float );
			Cascades_Fields[17] = FieldDescription( "shadowBias3", offsetof(Renderer::Shadows::Cascades, shadowBias3), eParamType_float );
			return d;
		}
	} // namespace PicoSettings_Renderer_Shadows_Cascades_NS


	namespace PicoSettings_Renderer_Shadows_Photomode_NS
	{
		const unsigned int Photomode_Field_Count = 5;
		const unsigned int Photomode_NestedStructures_Count = 0;
		FieldDescription Photomode_Fields[Photomode_Field_Count];

		StructDescription Photomode_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "Photomode";
			d.parentStructure_ = parent; 
			d.fields_ = Photomode_Fields;
			d.nestedStructures_ = nullptr;
			d.nFields_ = Photomode_Field_Count;
			d.nNestedStructures_ = Photomode_NestedStructures_Count;
			d.offset_ = offsetof( Renderer::Shadows, mPhotomode );
			d.sizeInBytes_ = sizeof(Renderer::Shadows::Photomode);
			Photomode_Fields[0] = FieldDescription( "extraCascades", offsetof(Renderer::Shadows::Photomode, extraCascades), eParamType_int );
			Photomode_Fields[1] = FieldDescription( "split0", offsetof(Renderer::Shadows::Photomode, split0), eParamType_float );
			Photomode_Fields[2] = FieldDescription( "split1", offsetof(Renderer::Shadows::Photomode, split1), eParamType_float );
			Photomode_Fields[3] = FieldDescription( "split2", offsetof(Renderer::Shadows::Photomode, split2), eParamType_float );
			Photomode_Fields[4] = FieldDescription( "split3", offsetof(Renderer::Shadows::Photomode, split3), eParamType_float );
			return d;
		}
	} // namespace PicoSettings_Renderer_Shadows_Photomode_NS


	namespace PicoSettings_Renderer_Shadows_Debug_NS
	{
		const unsigned int Debug_Field_Count = 20;
		const unsigned int Debug_NestedStructures_Count = 0;
		FieldDescription Debug_Fields[Debug_Field_Count];

		StructDescription Debug_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "Debug";
			d.parentStructure_ = parent; 
			d.fields_ = Debug_Fields;
			d.nestedStructures_ = nullptr;
			d.nFields_ = Debug_Field_Count;
			d.nNestedStructures_ = Debug_NestedStructures_Count;
			d.offset_ = offsetof( Renderer::Shadows, mDebug );
			d.sizeInBytes_ = sizeof(Renderer::Shadows::Debug);
			Debug_Fields[0] = FieldDescription( "cascadesCamera", offsetof(Renderer::Shadows::Debug, cascadesCamera), eParamType_string );
			Debug_Fields[1] = FieldDescription( "reloadShader", offsetof(Renderer::Shadows::Debug, reloadShader), eParamType_bool );
			Debug_Fields[2] = FieldDescription( "showCascades", offsetof(Renderer::Shadows::Debug, showCascades), eParamType_bool );
			Debug_Fields[3] = FieldDescription( "showCameraFrustum", offsetof(Renderer::Shadows::Debug, showCameraFrustum), eParamType_bool );
			Debug_Fields[4] = FieldDescription( "showCascadeFrustum", offsetof(Renderer::Shadows::Debug, showCascadeFrustum), eParamType_bool );
			Debug_Fields[5] = FieldDescription( "showCascadeBoundingSphere", offsetof(Renderer::Shadows::Debug, showCascadeBoundingSphere), eParamType_bool );
			Debug_Fields[6] = FieldDescription( "showWorldAABB", offsetof(Renderer::Shadows::Debug, showWorldAABB), eParamType_bool );
			Debug_Fields[7] = FieldDescription( "fakeMorpheusFieldOfView", offsetof(Renderer::Shadows::Debug, fakeMorpheusFieldOfView), eParamType_bool );
			Debug_Fields[8] = FieldDescription( "visibleShadowMap", offsetof(Renderer::Shadows::Debug, visibleShadowMap), eParamType_enum );
			Debug_Fields[9] = FieldDescription( "shadowMapOpacity", offsetof(Renderer::Shadows::Debug, shadowMapOpacity), eParamType_float );
			Debug_Fields[10] = FieldDescription( "shadowMapDepthScale", offsetof(Renderer::Shadows::Debug, shadowMapDepthScale), eParamType_float );
			Debug_Fields[11] = FieldDescription( "shadowMapDepthMin", offsetof(Renderer::Shadows::Debug, shadowMapDepthMin), eParamType_float );
			Debug_Fields[12] = FieldDescription( "shadowMapDepthMax", offsetof(Renderer::Shadows::Debug, shadowMapDepthMax), eParamType_float );
			Debug_Fields[13] = FieldDescription( "valientStableCascades", offsetof(Renderer::Shadows::Debug, valientStableCascades), eParamType_bool );
			Debug_Fields[14] = FieldDescription( "texelSnapping", offsetof(Renderer::Shadows::Debug, texelSnapping), eParamType_bool );
			Debug_Fields[15] = FieldDescription( "tightBoundingSphere", offsetof(Renderer::Shadows::Debug, tightBoundingSphere), eParamType_bool );
			Debug_Fields[16] = FieldDescription( "fitCascadeNearFarToWorldAABB", offsetof(Renderer::Shadows::Debug, fitCascadeNearFarToWorldAABB), eParamType_bool );
			Debug_Fields[17] = FieldDescription( "fitCascadeFarToCameraFrustum", offsetof(Renderer::Shadows::Debug, fitCascadeFarToCameraFrustum), eParamType_bool );
			Debug_Fields[18] = FieldDescription( "clipAgainstExtraFrustumPlanes", offsetof(Renderer::Shadows::Debug, clipAgainstExtraFrustumPlanes), eParamType_bool );
			Debug_Fields[19] = FieldDescription( "initializeWithPrevCascadesFrustum", offsetof(Renderer::Shadows::Debug, initializeWithPrevCascadesFrustum), eParamType_bool );
			return d;
		}
	} // namespace PicoSettings_Renderer_Shadows_Debug_NS


	namespace PicoSettings_Renderer_Shadows_NS
	{
		const unsigned int Shadows_Field_Count = 27;
		const unsigned int Shadows_NestedStructures_Count = 3;
		FieldDescription Shadows_Fields[Shadows_Field_Count];
		const StructDescription* Shadows_NestedStructures[Shadows_NestedStructures_Count];

		StructDescription Shadows_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "Shadows";
			d.parentStructure_ = parent; 
			d.fields_ = Shadows_Fields;
			d.nestedStructures_ = Shadows_NestedStructures;
			d.nFields_ = Shadows_Field_Count;
			d.nNestedStructures_ = Shadows_NestedStructures_Count;
			d.offset_ = offsetof( Renderer, mShadows );
			d.sizeInBytes_ = sizeof(Renderer::Shadows);
			Shadows_Fields[0] = FieldDescription( "forceShadowMapResolution", offsetof(Renderer::Shadows, forceShadowMapResolution), eParamType_bool );
			Shadows_Fields[1] = FieldDescription( "shadowMapResolution", offsetof(Renderer::Shadows, shadowMapResolution), eParamType_enum );
			Shadows_Fields[2] = FieldDescription( "forceShadowMapFormat", offsetof(Renderer::Shadows, forceShadowMapFormat), eParamType_bool );
			Shadows_Fields[3] = FieldDescription( "shadowMapFormat", offsetof(Renderer::Shadows, shadowMapFormat), eParamType_enum );
			Shadows_Fields[4] = FieldDescription( "forceUpVector", offsetof(Renderer::Shadows, forceUpVector), eParamType_bool );
			Shadows_Fields[5] = FieldDescription( "filter", offsetof(Renderer::Shadows, filter), eParamType_enum );
			Shadows_Fields[6] = FieldDescription( "forceShadowNormalBiasSettings", offsetof(Renderer::Shadows, forceShadowNormalBiasSettings), eParamType_bool );
			Shadows_Fields[7] = FieldDescription( "shadowNormalBiasNear", offsetof(Renderer::Shadows, shadowNormalBiasNear), eParamType_float );
			Shadows_Fields[8] = FieldDescription( "shadowNormalBiasFar", offsetof(Renderer::Shadows, shadowNormalBiasFar), eParamType_float );
			Shadows_Fields[9] = FieldDescription( "shadowNormalBiasFiltered", offsetof(Renderer::Shadows, shadowNormalBiasFiltered), eParamType_float );
			Shadows_Fields[10] = FieldDescription( "useCS", offsetof(Renderer::Shadows, useCS), eParamType_bool );
			Shadows_Fields[11] = FieldDescription( "useSmartBlur", offsetof(Renderer::Shadows, useSmartBlur), eParamType_bool );
			Shadows_Fields[12] = FieldDescription( "msaa", offsetof(Renderer::Shadows, msaa), eParamType_enum );
			Shadows_Fields[13] = FieldDescription( "enableMipMaps", offsetof(Renderer::Shadows, enableMipMaps), eParamType_bool );
			Shadows_Fields[14] = FieldDescription( "blur", offsetof(Renderer::Shadows, blur), eParamType_bool );
			Shadows_Fields[15] = FieldDescription( "separableBlur", offsetof(Renderer::Shadows, separableBlur), eParamType_bool );
			Shadows_Fields[16] = FieldDescription( "blurRadius", offsetof(Renderer::Shadows, blurRadius), eParamType_int );
			Shadows_Fields[17] = FieldDescription( "anisotropy", offsetof(Renderer::Shadows, anisotropy), eParamType_int );
			Shadows_Fields[18] = FieldDescription( "mipMapBias", offsetof(Renderer::Shadows, mipMapBias), eParamType_float );
			Shadows_Fields[19] = FieldDescription( "clampExponents", offsetof(Renderer::Shadows, clampExponents), eParamType_bool );
			Shadows_Fields[20] = FieldDescription( "evsmPositiveExponent", offsetof(Renderer::Shadows, evsmPositiveExponent), eParamType_float );
			Shadows_Fields[21] = FieldDescription( "evsmNegativeExponent", offsetof(Renderer::Shadows, evsmNegativeExponent), eParamType_float );
			Shadows_Fields[22] = FieldDescription( "vsmBias", offsetof(Renderer::Shadows, vsmBias), eParamType_float );
			Shadows_Fields[23] = FieldDescription( "evsmLightBleedingReduction", offsetof(Renderer::Shadows, evsmLightBleedingReduction), eParamType_float );
			Shadows_Fields[24] = FieldDescription( "msmDepthBias", offsetof(Renderer::Shadows, msmDepthBias), eParamType_float );
			Shadows_Fields[25] = FieldDescription( "msmMomentBias", offsetof(Renderer::Shadows, msmMomentBias), eParamType_float );
			Shadows_Fields[26] = FieldDescription( "msmLightBleedingReduction", offsetof(Renderer::Shadows, msmLightBleedingReduction), eParamType_float );
			Shadows_NestedStructures[0] = Renderer::Shadows::Cascades::GetDesc();
			Shadows_NestedStructures[1] = Renderer::Shadows::Photomode::GetDesc();
			Shadows_NestedStructures[2] = Renderer::Shadows::Debug::GetDesc();
			return d;
		}
	} // namespace PicoSettings_Renderer_Shadows_NS


	namespace PicoSettings_Renderer_GpuOcclusionCulling_NS
	{
		const unsigned int GpuOcclusionCulling_Field_Count = 19;
		const unsigned int GpuOcclusionCulling_NestedStructures_Count = 0;
		FieldDescription GpuOcclusionCulling_Fields[GpuOcclusionCulling_Field_Count];

		StructDescription GpuOcclusionCulling_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "GpuOcclusionCulling";
			d.parentStructure_ = parent; 
			d.fields_ = GpuOcclusionCulling_Fields;
			d.nestedStructures_ = nullptr;
			d.nFields_ = GpuOcclusionCulling_Field_Count;
			d.nNestedStructures_ = GpuOcclusionCulling_NestedStructures_Count;
			d.offset_ = offsetof( Renderer, mGpuOcclusionCulling );
			d.sizeInBytes_ = sizeof(Renderer::GpuOcclusionCulling);
			GpuOcclusionCulling_Fields[0] = FieldDescription( "occlusionMapSize", offsetof(Renderer::GpuOcclusionCulling, occlusionMapSize), eParamType_enum );
			GpuOcclusionCulling_Fields[1] = FieldDescription( "enabledColor", offsetof(Renderer::GpuOcclusionCulling, enabledColor), eParamType_bool );
			GpuOcclusionCulling_Fields[2] = FieldDescription( "enabledCascade0", offsetof(Renderer::GpuOcclusionCulling, enabledCascade0), eParamType_bool );
			GpuOcclusionCulling_Fields[3] = FieldDescription( "enabledCascade1", offsetof(Renderer::GpuOcclusionCulling, enabledCascade1), eParamType_bool );
			GpuOcclusionCulling_Fields[4] = FieldDescription( "enabledCascade2", offsetof(Renderer::GpuOcclusionCulling, enabledCascade2), eParamType_bool );
			GpuOcclusionCulling_Fields[5] = FieldDescription( "enabledCascade3", offsetof(Renderer::GpuOcclusionCulling, enabledCascade3), eParamType_bool );
			GpuOcclusionCulling_Fields[6] = FieldDescription( "enabledCascade4", offsetof(Renderer::GpuOcclusionCulling, enabledCascade4), eParamType_bool );
			GpuOcclusionCulling_Fields[7] = FieldDescription( "enabledCascade5", offsetof(Renderer::GpuOcclusionCulling, enabledCascade5), eParamType_bool );
			GpuOcclusionCulling_Fields[8] = FieldDescription( "enabledCascade6", offsetof(Renderer::GpuOcclusionCulling, enabledCascade6), eParamType_bool );
			GpuOcclusionCulling_Fields[9] = FieldDescription( "enabledCascade7", offsetof(Renderer::GpuOcclusionCulling, enabledCascade7), eParamType_bool );
			GpuOcclusionCulling_Fields[10] = FieldDescription( "initializeWithPlayersFrustum", offsetof(Renderer::GpuOcclusionCulling, initializeWithPlayersFrustum), eParamType_bool );
			GpuOcclusionCulling_Fields[11] = FieldDescription( "initializeWithPrevCascadesFrustum", offsetof(Renderer::GpuOcclusionCulling, initializeWithPrevCascadesFrustum), eParamType_bool );
			GpuOcclusionCulling_Fields[12] = FieldDescription( "debugShow", offsetof(Renderer::GpuOcclusionCulling, debugShow), eParamType_bool );
			GpuOcclusionCulling_Fields[13] = FieldDescription( "debugDrawOpacityColor", offsetof(Renderer::GpuOcclusionCulling, debugDrawOpacityColor), eParamType_float );
			GpuOcclusionCulling_Fields[14] = FieldDescription( "debugDrawDepthScale", offsetof(Renderer::GpuOcclusionCulling, debugDrawDepthScale), eParamType_float );
			GpuOcclusionCulling_Fields[15] = FieldDescription( "debugDrawDepthMin", offsetof(Renderer::GpuOcclusionCulling, debugDrawDepthMin), eParamType_float );
			GpuOcclusionCulling_Fields[16] = FieldDescription( "debugDrawDepthMax", offsetof(Renderer::GpuOcclusionCulling, debugDrawDepthMax), eParamType_float );
			GpuOcclusionCulling_Fields[17] = FieldDescription( "debugDrawMipLevel", offsetof(Renderer::GpuOcclusionCulling, debugDrawMipLevel), eParamType_float );
			GpuOcclusionCulling_Fields[18] = FieldDescription( "debugReloadShader", offsetof(Renderer::GpuOcclusionCulling, debugReloadShader), eParamType_bool );
			return d;
		}
	} // namespace PicoSettings_Renderer_GpuOcclusionCulling_NS


	namespace PicoSettings_Renderer_NS
	{
		const unsigned int Renderer_Field_Count = 9;
		const unsigned int Renderer_NestedStructures_Count = 2;
		FieldDescription Renderer_Fields[Renderer_Field_Count];
		const StructDescription* Renderer_NestedStructures[Renderer_NestedStructures_Count];

		StructDescription Renderer_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "Renderer";
			d.parentStructure_ = parent; 
			d.fields_ = Renderer_Fields;
			d.nestedStructures_ = Renderer_NestedStructures;
			d.nFields_ = Renderer_Field_Count;
			d.nNestedStructures_ = Renderer_NestedStructures_Count;
			d.offset_ = offsetof( PicoSettings, mRenderer );
			d.sizeInBytes_ = sizeof(Renderer);
			Renderer_Fields[0] = FieldDescription( "visibleRenderTarget", offsetof(Renderer, visibleRenderTarget), eParamType_enum );
			Renderer_Fields[1] = FieldDescription( "vsync", offsetof(Renderer, vsync), eParamType_enum );
			Renderer_Fields[2] = FieldDescription( "displayOrientation", offsetof(Renderer, displayOrientation), eParamType_enum );
			Renderer_Fields[3] = FieldDescription( "msaa", offsetof(Renderer, msaa), eParamType_enum );
			Renderer_Fields[4] = FieldDescription( "fxaa", offsetof(Renderer, fxaa), eParamType_bool );
			Renderer_Fields[5] = FieldDescription( "DebugCamera", offsetof(Renderer, DebugCamera), eParamType_string );
			Renderer_Fields[6] = FieldDescription( "faderEnabled", offsetof(Renderer, faderEnabled), eParamType_bool );
			Renderer_Fields[7] = FieldDescription( "drawFrustumOntopCascades", offsetof(Renderer, drawFrustumOntopCascades), eParamType_bool );
			Renderer_Fields[8] = FieldDescription( "drawPrevCascadesFrustumOntop", offsetof(Renderer, drawPrevCascadesFrustumOntop), eParamType_bool );
			Renderer_NestedStructures[0] = Renderer::Shadows::GetDesc();
			Renderer_NestedStructures[1] = Renderer::GpuOcclusionCulling::GetDesc();
			return d;
		}
	} // namespace PicoSettings_Renderer_NS


	namespace PicoSettings_HMD_Generic_NS
	{
		const unsigned int Generic_Field_Count = 12;
		const unsigned int Generic_NestedStructures_Count = 0;
		FieldDescription Generic_Fields[Generic_Field_Count];

		StructDescription Generic_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "Generic";
			d.parentStructure_ = parent; 
			d.fields_ = Generic_Fields;
			d.nestedStructures_ = nullptr;
			d.nFields_ = Generic_Field_Count;
			d.nNestedStructures_ = Generic_NestedStructures_Count;
			d.offset_ = offsetof( HMD, mGeneric );
			d.sizeInBytes_ = sizeof(HMD::Generic);
			Generic_Fields[0] = FieldDescription( "hmdOversample", offsetof(HMD::Generic, hmdOversample), eParamType_floatBool );
			Generic_Fields[1] = FieldDescription( "DistortionCancellerEnabled", offsetof(HMD::Generic, DistortionCancellerEnabled), eParamType_bool );
			Generic_Fields[2] = FieldDescription( "AffectAllCameraNodesPositionScale", offsetof(HMD::Generic, AffectAllCameraNodesPositionScale), eParamType_floatBool );
			Generic_Fields[3] = FieldDescription( "NearPlaneOverride", offsetof(HMD::Generic, NearPlaneOverride), eParamType_floatBool );
			Generic_Fields[4] = FieldDescription( "FadeType", offsetof(HMD::Generic, FadeType), eParamType_enum );
			Generic_Fields[5] = FieldDescription( "FadeInDuration", offsetof(HMD::Generic, FadeInDuration), eParamType_float );
			Generic_Fields[6] = FieldDescription( "FadeOutDuration", offsetof(HMD::Generic, FadeOutDuration), eParamType_float );
			Generic_Fields[7] = FieldDescription( "FadeInDurationCutscene", offsetof(HMD::Generic, FadeInDurationCutscene), eParamType_float );
			Generic_Fields[8] = FieldDescription( "FadeOutDurationCutscene", offsetof(HMD::Generic, FadeOutDurationCutscene), eParamType_float );
			Generic_Fields[9] = FieldDescription( "PlayerSpeedMin", offsetof(HMD::Generic, PlayerSpeedMin), eParamType_float );
			Generic_Fields[10] = FieldDescription( "PlayerSpeedMax", offsetof(HMD::Generic, PlayerSpeedMax), eParamType_float );
			Generic_Fields[11] = FieldDescription( "HudScale", offsetof(HMD::Generic, HudScale), eParamType_float );
			return d;
		}
	} // namespace PicoSettings_HMD_Generic_NS


	namespace PicoSettings_HMD_Morpheus_NS
	{
		const unsigned int Morpheus_Field_Count = 7;
		const unsigned int Morpheus_NestedStructures_Count = 0;
		FieldDescription Morpheus_Fields[Morpheus_Field_Count];

		StructDescription Morpheus_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "Morpheus";
			d.parentStructure_ = parent; 
			d.fields_ = Morpheus_Fields;
			d.nestedStructures_ = nullptr;
			d.nFields_ = Morpheus_Field_Count;
			d.nNestedStructures_ = Morpheus_NestedStructures_Count;
			d.offset_ = offsetof( HMD, mMorpheus );
			d.sizeInBytes_ = sizeof(HMD::Morpheus);
			Morpheus_Fields[0] = FieldDescription( "RefreshRate", offsetof(HMD::Morpheus, RefreshRate), eParamType_enum );
			Morpheus_Fields[1] = FieldDescription( "PredicationEnabled", offsetof(HMD::Morpheus, PredicationEnabled), eParamType_bool );
			Morpheus_Fields[2] = FieldDescription( "PredicationExtraTime", offsetof(HMD::Morpheus, PredicationExtraTime), eParamType_float );
			Morpheus_Fields[3] = FieldDescription( "ReprojectionEnabled", offsetof(HMD::Morpheus, ReprojectionEnabled), eParamType_bool );
			Morpheus_Fields[4] = FieldDescription( "HTileClearOptimization", offsetof(HMD::Morpheus, HTileClearOptimization), eParamType_bool );
			Morpheus_Fields[5] = FieldDescription( "HTileClearOptimizationFactor", offsetof(HMD::Morpheus, HTileClearOptimizationFactor), eParamType_float );
			Morpheus_Fields[6] = FieldDescription( "LowLatencyTrackingEnabled", offsetof(HMD::Morpheus, LowLatencyTrackingEnabled), eParamType_bool );
			return d;
		}
	} // namespace PicoSettings_HMD_Morpheus_NS


	namespace PicoSettings_HMD_NS
	{
		const unsigned int HMD_Field_Count = 5;
		const unsigned int HMD_NestedStructures_Count = 2;
		FieldDescription HMD_Fields[HMD_Field_Count];
		const StructDescription* HMD_NestedStructures[HMD_NestedStructures_Count];

		StructDescription HMD_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "HMD";
			d.parentStructure_ = parent; 
			d.fields_ = HMD_Fields;
			d.nestedStructures_ = HMD_NestedStructures;
			d.nFields_ = HMD_Field_Count;
			d.nNestedStructures_ = HMD_NestedStructures_Count;
			d.offset_ = offsetof( PicoSettings, mHMD );
			d.sizeInBytes_ = sizeof(HMD);
			HMD_Fields[0] = FieldDescription( "Supported", offsetof(HMD, Supported), eParamType_bool );
			HMD_Fields[1] = FieldDescription( "OculusSupported", offsetof(HMD, OculusSupported), eParamType_bool );
			HMD_Fields[2] = FieldDescription( "OculusEnabled", offsetof(HMD, OculusEnabled), eParamType_bool );
			HMD_Fields[3] = FieldDescription( "MorpheusSupported", offsetof(HMD, MorpheusSupported), eParamType_bool );
			HMD_Fields[4] = FieldDescription( "MorpheusEnabled", offsetof(HMD, MorpheusEnabled), eParamType_bool );
			HMD_NestedStructures[0] = HMD::Generic::GetDesc();
			HMD_NestedStructures[1] = HMD::Morpheus::GetDesc();
			return d;
		}
	} // namespace PicoSettings_HMD_NS


	namespace PicoSettings_Stats_NS
	{
		const unsigned int Stats_Field_Count = 2;
		const unsigned int Stats_NestedStructures_Count = 0;
		FieldDescription Stats_Fields[Stats_Field_Count];

		StructDescription Stats_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "Stats";
			d.parentStructure_ = parent; 
			d.fields_ = Stats_Fields;
			d.nestedStructures_ = nullptr;
			d.nFields_ = Stats_Field_Count;
			d.nNestedStructures_ = Stats_NestedStructures_Count;
			d.offset_ = offsetof( PicoSettings, mStats );
			d.sizeInBytes_ = sizeof(Stats);
			Stats_Fields[0] = FieldDescription( "Enabled", offsetof(Stats, Enabled), eParamType_bool );
			Stats_Fields[1] = FieldDescription( "NumberOfMarkerLevels", offsetof(Stats, NumberOfMarkerLevels), eParamType_int );
			return d;
		}
	} // namespace PicoSettings_Stats_NS


	namespace PicoSettings_PS4Gnm_NS
	{
		const unsigned int PS4Gnm_Field_Count = 2;
		const unsigned int PS4Gnm_NestedStructures_Count = 0;
		FieldDescription PS4Gnm_Fields[PS4Gnm_Field_Count];

		StructDescription PS4Gnm_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "PS4Gnm";
			d.parentStructure_ = parent; 
			d.fields_ = PS4Gnm_Fields;
			d.nestedStructures_ = nullptr;
			d.nFields_ = PS4Gnm_Field_Count;
			d.nNestedStructures_ = PS4Gnm_NestedStructures_Count;
			d.offset_ = offsetof( PicoSettings, mPS4Gnm );
			d.sizeInBytes_ = sizeof(PS4Gnm);
			PS4Gnm_Fields[0] = FieldDescription( "AsyncComputeEnabled", offsetof(PS4Gnm, AsyncComputeEnabled), eParamType_bool );
			PS4Gnm_Fields[1] = FieldDescription( "maxWavesPerSIMD", offsetof(PS4Gnm, maxWavesPerSIMD), eParamType_int );
			return d;
		}
	} // namespace PicoSettings_PS4Gnm_NS


	namespace PicoSettings_TextUtil_NS
	{
		const unsigned int TextUtil_Field_Count = 12;
		const unsigned int TextUtil_NestedStructures_Count = 0;
		FieldDescription TextUtil_Fields[TextUtil_Field_Count];

		StructDescription TextUtil_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "TextUtil";
			d.parentStructure_ = parent; 
			d.fields_ = TextUtil_Fields;
			d.nestedStructures_ = nullptr;
			d.nFields_ = TextUtil_Field_Count;
			d.nNestedStructures_ = TextUtil_NestedStructures_Count;
			d.offset_ = offsetof( PicoSettings, mTextUtil );
			d.sizeInBytes_ = sizeof(TextUtil);
			TextUtil_Fields[0] = FieldDescription( "ReloadTexts", offsetof(TextUtil, ReloadTexts), eParamType_bool );
			TextUtil_Fields[1] = FieldDescription( "Language", offsetof(TextUtil, Language), eParamType_enum );
			TextUtil_Fields[2] = FieldDescription( "subtitleWindowPositionX", offsetof(TextUtil, subtitleWindowPositionX), eParamType_float );
			TextUtil_Fields[3] = FieldDescription( "subtitleWindowPositionY", offsetof(TextUtil, subtitleWindowPositionY), eParamType_float );
			TextUtil_Fields[4] = FieldDescription( "subtitleWindowWidth", offsetof(TextUtil, subtitleWindowWidth), eParamType_float );
			TextUtil_Fields[5] = FieldDescription( "hmdSubtitleWindowPositionX", offsetof(TextUtil, hmdSubtitleWindowPositionX), eParamType_float );
			TextUtil_Fields[6] = FieldDescription( "hmdSubtitleWindowWidth", offsetof(TextUtil, hmdSubtitleWindowWidth), eParamType_float );
			TextUtil_Fields[7] = FieldDescription( "hmdSubtitleWindowPositionY", offsetof(TextUtil, hmdSubtitleWindowPositionY), eParamType_float );
			TextUtil_Fields[8] = FieldDescription( "iconWindowPositionX", offsetof(TextUtil, iconWindowPositionX), eParamType_float );
			TextUtil_Fields[9] = FieldDescription( "iconWindowPositionY", offsetof(TextUtil, iconWindowPositionY), eParamType_float );
			TextUtil_Fields[10] = FieldDescription( "hmdIconWindowPositionX", offsetof(TextUtil, hmdIconWindowPositionX), eParamType_float );
			TextUtil_Fields[11] = FieldDescription( "hmdIconWindowPositionY", offsetof(TextUtil, hmdIconWindowPositionY), eParamType_float );
			return d;
		}
	} // namespace PicoSettings_TextUtil_NS


	namespace PicoSettings_Recording_NS
	{
		const unsigned int Recording_Field_Count = 2;
		const unsigned int Recording_NestedStructures_Count = 0;
		FieldDescription Recording_Fields[Recording_Field_Count];

		StructDescription Recording_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "Recording";
			d.parentStructure_ = parent; 
			d.fields_ = Recording_Fields;
			d.nestedStructures_ = nullptr;
			d.nFields_ = Recording_Field_Count;
			d.nNestedStructures_ = Recording_NestedStructures_Count;
			d.offset_ = offsetof( PicoSettings, mRecording );
			d.sizeInBytes_ = sizeof(Recording);
			Recording_Fields[0] = FieldDescription( "HideLetterBox", offsetof(Recording, HideLetterBox), eParamType_bool );
			Recording_Fields[1] = FieldDescription( "HideCutsceneTexts", offsetof(Recording, HideCutsceneTexts), eParamType_bool );
			return d;
		}
	} // namespace PicoSettings_Recording_NS


	namespace PicoSettings_NS
	{
		const unsigned int PicoSettings_Field_Count = 0;
		const unsigned int PicoSettings_NestedStructures_Count = 6;
		const StructDescription* PicoSettings_NestedStructures[PicoSettings_NestedStructures_Count];

		StructDescription PicoSettings_CreateDescription( const StructDescription* parent )
		{
			StructDescription d;
			d.name_ = "PicoSettings";
			d.parentStructure_ = parent; 
			d.fields_ = nullptr;
			d.nestedStructures_ = PicoSettings_NestedStructures;
			d.nFields_ = PicoSettings_Field_Count;
			d.nNestedStructures_ = PicoSettings_NestedStructures_Count;
			d.offset_ = 0;
			d.sizeInBytes_ = sizeof(PicoSettings);
			PicoSettings_NestedStructures[0] = Renderer::GetDesc();
			PicoSettings_NestedStructures[1] = HMD::GetDesc();
			PicoSettings_NestedStructures[2] = Stats::GetDesc();
			PicoSettings_NestedStructures[3] = PS4Gnm::GetDesc();
			PicoSettings_NestedStructures[4] = TextUtil::GetDesc();
			PicoSettings_NestedStructures[5] = Recording::GetDesc();
			return d;
		}
	} // namespace PicoSettings_NS


	StructDescription PicoSettings::__desc = PicoSettings_NS::PicoSettings_CreateDescription( nullptr );
	StructDescription Renderer::__desc = PicoSettings_Renderer_NS::Renderer_CreateDescription( PicoSettings::GetDesc() );
	StructDescription Renderer::Shadows::__desc = PicoSettings_Renderer_Shadows_NS::Shadows_CreateDescription( Renderer::GetDesc() );
	StructDescription Renderer::Shadows::Cascades::__desc = PicoSettings_Renderer_Shadows_Cascades_NS::Cascades_CreateDescription( Renderer::Shadows::GetDesc() );
	StructDescription Renderer::Shadows::Photomode::__desc = PicoSettings_Renderer_Shadows_Photomode_NS::Photomode_CreateDescription( Renderer::Shadows::GetDesc() );
	StructDescription Renderer::Shadows::Debug::__desc = PicoSettings_Renderer_Shadows_Debug_NS::Debug_CreateDescription( Renderer::Shadows::GetDesc() );
	StructDescription Renderer::GpuOcclusionCulling::__desc = PicoSettings_Renderer_GpuOcclusionCulling_NS::GpuOcclusionCulling_CreateDescription( Renderer::GetDesc() );
	StructDescription HMD::__desc = PicoSettings_HMD_NS::HMD_CreateDescription( PicoSettings::GetDesc() );
	StructDescription HMD::Generic::__desc = PicoSettings_HMD_Generic_NS::Generic_CreateDescription( HMD::GetDesc() );
	StructDescription HMD::Morpheus::__desc = PicoSettings_HMD_Morpheus_NS::Morpheus_CreateDescription( HMD::GetDesc() );
	StructDescription Stats::__desc = PicoSettings_Stats_NS::Stats_CreateDescription( PicoSettings::GetDesc() );
	StructDescription PS4Gnm::__desc = PicoSettings_PS4Gnm_NS::PS4Gnm_CreateDescription( PicoSettings::GetDesc() );
	StructDescription TextUtil::__desc = PicoSettings_TextUtil_NS::TextUtil_CreateDescription( PicoSettings::GetDesc() );
	StructDescription Recording::__desc = PicoSettings_Recording_NS::Recording_CreateDescription( PicoSettings::GetDesc() );

	void PicoSettingsWrap::load( const char* filePath )
	{
		if ( __settingsFile_ )
			return;
		
		mRenderer = new Renderer;
		mHMD = new HMD;
		mStats = new Stats;
		mPS4Gnm = new PS4Gnm;
		mTextUtil = new TextUtil;
		mRecording = new Recording;

		const void* addresses[6];
		addresses[0] = mRenderer;
		addresses[1] = mHMD;
		addresses[2] = mStats;
		addresses[3] = mPS4Gnm;
		addresses[4] = mTextUtil;
		addresses[5] = mRecording;

		__settingsFile_ = SettingsEditor::createSettingsFile( filePath, GetDesc(), addresses );
	}

	void PicoSettingsWrap::unload()
	{
		SettingsEditor::releaseSettingsFile( __settingsFile_ );

		delete mRenderer; mRenderer = nullptr;
		delete mHMD; mHMD = nullptr;
		delete mStats; mStats = nullptr;
		delete mPS4Gnm; mPS4Gnm = nullptr;
		delete mTextUtil; mTextUtil = nullptr;
		delete mRecording; mRecording = nullptr;
	}
	

} // namespace PicoSettingsNamespace

PicoSettingsNamespace::PicoSettingsWrap* gPicoSettings = nullptr;

