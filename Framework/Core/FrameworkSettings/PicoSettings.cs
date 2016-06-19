using System;
using SettingsEditor;

// The field 'xxx' is assigned but its value is never used
#pragma warning disable 414

namespace PicoSettings
{
    public class GeneratorConfig
    {
        public static readonly string SettingsEditorHeaderInclude = "#include <picoCore/SettingsEditor.h>";
        public static readonly string SettingsEditorCppInclude = "#include <picoCore_pch.h>";
    }

    class Renderer
    {
        #region Enums

        enum e_VisibleRenderTarget
        {
            [EnumLabel( "Color0" )]
            eVisibleRenderTarget_color0,
            [EnumLabel( "Depth" )]
            eVisibleRenderTarget_depth,
            [EnumLabel( "ShadowMap" )]
            eVisibleRenderTarget_shadowMap,
            [EnumLabel( "Normals" )]
            eVisibleRenderTarget_normals,
            [EnumLabel( "Glow" )]
            eVisibleRenderTarget_glow
        }

        enum e_Msaa
        {
            [EnumLabel( "1x" )]
            eMsaa_1x = 1,
            [EnumLabel( "2x" )]
            eMsaa_2x = 2,
            [EnumLabel( "4x" )]
            eMsaa_4x = 4,
            [EnumLabel( "8x" )]
            eMsaa_8x = 8,
        }

        enum e_Vsync
        {
            [EnumLabel("disabled")]
            eVSync_disabled = 0,
            [EnumLabel( "60 Hz" )]
            eVSync_60Hz = 1,
            [EnumLabel( "30 Hz" )]
            eVSync_30Hz = 2,
            [EnumLabel( "20 Hz" )]
            eVSync_20Hz = 3
        }

        enum e_DisplayOrientation
        {
            [EnumLabel( "horizontal" )]
            horizontal = 0,
            [EnumLabel( "vertical" )]
            vertical,
        }

        #endregion

        #region Renderer Settings

        [Category("Output")]
        [DisplayName("Visible Render Target")]
        e_VisibleRenderTarget visibleRenderTarget = e_VisibleRenderTarget.eVisibleRenderTarget_color0;

        [Category( "Output" )]
        [DisplayName( "VSync" )]
        e_Vsync vsync = e_Vsync.eVSync_60Hz;

        [Category( "Output" )]
        [DisplayName( "Display Orientation" )]
        e_DisplayOrientation displayOrientation = e_DisplayOrientation.horizontal;

        [DisplayName("MSAA")]
        [Category( "Antialiasing" )]
        e_Msaa msaa = e_Msaa.eMsaa_1x;

        [DisplayName("FXAA")]
        [Category( "Antialiasing" )]
        [HelpText( "Perform FXAA additionally to MSAA when copying framebuffer onto screen" )]
        bool fxaa = false;

        [Category( "Debug" )]
        [HelpText( "Overrides rendering camera. Has highest priority. Format 'containerName:nodeName'" )]
        string DebugCamera = "";

        [DisplayName( "Fader Enabled" )]
        [Category( "Debug" )]
        [HelpText( "Turns fader on/off. If disabled, fader will be disabled no matter what game logic is." )]
        bool faderEnabled = true;

        bool drawFrustumOntopCascades = true;
        bool drawPrevCascadesFrustumOntop = true;

        #endregion

        class Shadows
        {
            #region Enums

            enum ShadowMapResolution
            {
                [EnumLabel( "512x512" )]
                Resolution_512x512 = 512,

                [EnumLabel( "1024x1024" )]
                Resolution_1024x1024 = 1024,

                [EnumLabel( "2048x2048" )]
                Resolution_2048x2048 = 2048,

                [EnumLabel( "4096x4096" )]
                Resolution_4096x4096 = 4096,
            };
            enum ShadowMapFormat
            {
                [EnumLabel("16bit")]
                Format_16bit = 0,

                [EnumLabel("32bit")]
                Format_32bit,
            };

            // From ShadowDefinitions.h
            enum Filter
            {
                None = 0,
                PointGather_2x2 = 1,
                Gauss_3x3 = 6,
                PCF_3x3 = 7,
                PCF_Optimized = 3,
                EVSM_16bit_2Components = 9,
                EVSM_32bit_2Components = 9  + 0x0100, // Settings Editor gets lost with the same enum values, so we apply extra bit and clear it in cpp.
                EVSM_16bit_4Components = 10 + 0x0200,
                EVSM_32bit_4Components = 10 + 0x0300,
                MSM_Hausdorff_16bit = 11 + 0x0400,
                MSM_Hausdorff_32bit = 12 + 0x0500,
                MSM_Hamburger_16bit = 13 + 0x0600,
                MSM_Hamburger_32bit = 14 + 0x0700
            };

            #endregion

            #region General

            [DisplayName( "Force shadow map resolution" )]
            [HelpText("If set, PicoSettings take over")]
            bool forceShadowMapResolution = true;

            [DisplayName("Shadow map resolution")]
            [DependsOn( "forceShadowMapResolution" )]
            ShadowMapResolution shadowMapResolution = ShadowMapResolution.Resolution_2048x2048;

            [DisplayName( "Force shadow map format" )]
            [HelpText( "If set, PicoSettings take over" )]
            bool forceShadowMapFormat = true;
            
            [DisplayName( "Shadow map format" )]
            [DependsOn( "forceShadowMapFormat" )]
            ShadowMapFormat shadowMapFormat = ShadowMapFormat.Format_16bit;

            [DisplayName("Force up vector")]
            [HelpText("Forces cascades camera up Vector to (0, 1, 0). Otherwise it is taken from picoSunlightNode transformation matrix set in Maya.")]
            bool forceUpVector = true;

            [DisplayName("Filter type")]
            [HelpText("Set DEBUG_FILTERS to 1 in picoShaderResourceMap.h and recompile all native shaders if you want to change filters in real time.\nOptimized PCF uses kernel size chosen in native shader.")]
            Filter filter = Filter.PCF_Optimized;

            [HelpText("If set, PicoSettings take over. Level, view or cutscene settings are ignored.")]
            [DisplayName("Force shadow normal bias settings")]
            bool forceShadowNormalBiasSettings = false;

            [DisplayName("Normal bias at depth = 0")]
            [Min(-10)]
            [Max(10)]
            [SoftMin(0)]
            [SoftMax(2)]
            [Step(0.001f)]
            float shadowNormalBiasNear = 0.1f;

            [DisplayName("Normal bias at far plane")]
            [Min(-10)]
            [Max(10)]
            [SoftMin(0)]
            [SoftMax(2)]
            [Step(0.001f)]
            float shadowNormalBiasFar = 0.1f;

            [DisplayName("Normal bias EVSM/MSM")]
            [Min(-10)]
            [Max(10)]
            [SoftMin(0)]
            [SoftMax(2)]
            [Step(0.001f)]
            float shadowNormalBiasFiltered = 0.1f;

            [DisplayName("Use compute shaders")]
            [Category("EVSM/MSM")]
            bool useCS = true;

            [DisplayName("Use smart blur")]
            [Category("EVSM/MSM")]
            [HelpText("In-place blur during conversion pass on 2x2 pixel quads")]
            bool useSmartBlur = false;

            [DisplayName("Shadow MSAA")]
            [Category("EVSM/MSM")]
            e_Msaa msaa = e_Msaa.eMsaa_1x;

            //[DisplayName("Resolve MSAA during conversion")]
            //[Category("EVSM/MSM")]
            //[HelpText("MSAA depths are averaged during EVSM/MSM conversion instead of being written separately to a bigger texture.")]
            //bool resolveMSAA = false;

            [DisplayName("Use mip maps")]
            [Category("EVSM/MSM")]
            bool enableMipMaps = false;

            [DisplayName("Use blur")]
            [Category("EVSM/MSM")]
            bool blur = false;

            [DisplayName("Separable blur")]
            [Category("EVSM/MSM")]
            bool separableBlur = false;

            [DisplayName("Blur radius")]
            [Category("EVSM/MSM")]
            [Min(3)]
            [Max(15)]
            [Step(2)]
            int blurRadius = 5;

            [DisplayName("Anisotropy")]
            [Min(0)]
            [Max(16)]
            [Category("EVSM/MSM")]
            int anisotropy = 16;

            [DisplayName("Mip map bias")]
            [Min(-12)]
            [Max(12)]
            [Category("EVSM/MSM")]
            float mipMapBias = 0;

            [DisplayName("Clamp EVSM exponents")]
            [Category("EVSM/MSM")]
            [HelpText("Clamps exponents to the maximum range of float (5.54 for 16 bit RT or 42.0 for 32 bit RT)")]
            bool clampExponents = true;

            [DisplayName("EVSM positive exponent")]
            [Category("EVSM/MSM")]
            [Min(0)]
            [Max(80)]
            float evsmPositiveExponent = 60;

            [DisplayName("EVSM negative exponent")]
            [Category("EVSM/MSM")]
            [Min(0)]
            [Max(80)]
            float evsmNegativeExponent = 0;

            [DisplayName("EVSM bias")]
            [Category("EVSM/MSM")]
            float vsmBias = 0;

            [DisplayName("EVSM light bleeding reduction")]
            [Category("EVSM/MSM")]
            [Max(0.99f)]
            float evsmLightBleedingReduction = 0.5f;

            [DisplayName("MSM depth bias (x1000)")]
            [Category("EVSM/MSM")]
            [Min(0)]
            [Max(10)]
            float msmDepthBias = 0.000f;

            [DisplayName("MSM moment bias (x1000)")]
            [Category("EVSM/MSM")]
            [Min(0)]
            [Max(1)]
            float msmMomentBias = 0.003f;

            [DisplayName("MSM light bleeding reduction")]
            [Category("EVSM/MSM")]
            [Max(0.99f)]
            float msmLightBleedingReduction = 0.5f;

            #endregion

            class Cascades
            {
                [DisplayName("Force Num Cascades")]
                [HelpText("If set, PicoSettings take over. Level, view or cutscene settings are ignored.")]
                bool forceNumCascades = true;

                [DisplayName("Num Cascades")]
                [Min(0)]
                [Max(8)]
                int numCascades = 4;

                [HelpText("If set, PicoSettings take over. Level, view or cutscene settings are ignored.")]
                [DisplayName("Force camera's far plane override")]
                bool forcePlayerCameraFarPlaneOverride = false;

                [DisplayName("Camera's far plane override")]
                [Min(0)]
                [Max(1000)]
                [SoftMin(10)]
                [SoftMax(300)]
                [Step(1)]
                [CheckBox(true)]
                [HelpText("Overrides camera's far plane. This important optimization helps to improve shadow quality. The shorter the distance between near and far planes" +
                    " the more shadow map texels we get per world unit. This can be less than camera's far plane due to how partitioning into cascades works in conjunction with Valient's stable cascades")]
                float playerCameraFarPlaneOverride = 120;

                [HelpText("If set, PicoSettings take over")]
                [DisplayName("Force split settings")]
                bool forceSplitSettings = false;

                [HelpText("Enables manual control of partitions")]
                [DisplayName("Enable manual splits")]
                bool manualSplits = false;

                [HelpText("Lerp factor between logarithmic (1.0) and uniform (0.0) distribution of partitions")]
                [DisplayName("Split weight")]
                [DependsOn("manualSplits", false)]
                float splitWeight = 0.75f;

                [DependsOn("manualSplits")]
                [DisplayName("Split 0")]
                [HelpText("Enables manual control of partitions")]
                float split0 = 0.1f;

                [DependsOn("manualSplits")]
                [DisplayName("Split 1")]
                float split1 = 0.25f;

                [DependsOn("manualSplits")]
                [DisplayName("Split 2")]
                float split2 = 0.5f;

                [DependsOn("manualSplits")]
                [DisplayName("Split 3")]
                float split3 = 1.0f;

                [HelpText("If set, PicoSettings take over")]
                [DisplayName("Force blend factor")]
                bool forceBlendFactor = false;

                [Max(0.5f)]
                [SoftMax(0.1f)]
                [Step(0.001f)]
                [HelpText("Controls how much cascades overlap between each other")]
                [DisplayName("Blend Factor")]
                float cascadesBlendFactor = 0.05f;

                [HelpText("If set, PicoSettings take over. Level, view or cutscene settings are ignored.")]
                [DisplayName("Force shadow bias settings")]
                bool forceShadowBiasSettings = false;

                [DisplayName("Shadow bias 0")]
                [Min(-10)]
                [Max(10)]
                [SoftMin(0)]
                [SoftMax(2)]
                [Step(0.001f)]
                float shadowBias0 = 0.1f;

                [DisplayName("Shadow bias 1")]
                [Min(-10)]
                [Max(10)]
                [SoftMin(0)]
                [SoftMax(2)]
                [Step(0.001f)]
                float shadowBias1 = 0.5f;

                [DisplayName("Shadow bias 2")]
                [Min(-10)]
                [Max(10)]
                [SoftMin(0)]
                [SoftMax(2)]
                [Step(0.001f)]
                float shadowBias2 = 0.6f;

                [DisplayName("Shadow bias 3")]
                [Min(-10)]
                [Max(10)]
                [SoftMin(0)]
                [SoftMax(2)]
                [Step(0.001f)]
                float shadowBias3 = 1.5f;
            }

            class Photomode
            {
                [DisplayName("Extra cascades")]
                [HelpText("How mamy more cascades to use in photomode compared to gameplay. Cascades splits interpolate between camera override zFar and original zFar")]
                [Min(0)]
                [Max(4)]
                int extraCascades = 2;

                [DisplayName("Split 0")]
                [Min(1.0f)]
                [Max(2.0f)]
                float split0 = 1.5f;

                [DisplayName("Split 1")]
                [Min(1.0f)]
                [Max(2.0f)]
                float split1 = 2.0f;

                [DisplayName("Split 2")]
                [Min(1.0f)]
                [Max(2.0f)]
                float split2 = 2.0f;

                [DisplayName("Split 3")]
                [Min(1.0f)]
                [Max(2.0f)]
                float split3 = 2.0f;
            }

            class Debug
            {
                #region Debug

                [Category("Debug")]
                [DisplayName("Camera for cascades")]
                [HelpText("Camera used for shadow/cascades calculation. Has highest priorty. Overrides player camera, debug camera, cutscene camera...")]
                string cascadesCamera = "";

                [Category("Debug")]
                [DisplayName("Reload shadows shader")]
                bool reloadShader = true;

                [Category("Debug")]
                bool showCascades = false;

                [Category("Debug")]
                bool showCameraFrustum = false;

                [Category("Debug")]
                bool showCascadeFrustum = false;

                [Category("Debug")]
                bool showCascadeBoundingSphere = false;

                [Category("Debug")]
                bool showWorldAABB = false;

                [Category("Debug")]
                [HelpText("In 2D mode, overrides camera projection with one that matches Morpheus's FOV (two eyes at once). Used for testing shadow quality/performance.")]
                bool fakeMorpheusFieldOfView = false;

                enum e_VisibleShadowMap
                {
                    [EnumLabel("AllCascades")]
                    eVisibleShadowMap_AllCascades = 0xff,
                    [EnumLabel("Cascade 0")]
                    eVisibleShadowMap_Cascade0 = 0,
                    [EnumLabel("Cascade 1")]
                    eVisibleShadowMap_Cascade1 = 1,
                    [EnumLabel("Cascade 2")]
                    eVisibleShadowMap_Cascade2 = 2,
                    [EnumLabel("Cascade 3")]
                    eVisibleShadowMap_Cascade3 = 3,
                }

                [Category("Debug")]
                [DisplayName("Visible shadow map")]
                e_VisibleShadowMap visibleShadowMap = e_VisibleShadowMap.eVisibleShadowMap_AllCascades;

                [Category("Debug")]
                float shadowMapOpacity = 1.0f;

                [Category("Debug")]
                [Step(0.001f)]
                float shadowMapDepthScale = 1.0f;

                [Category("Debug")]
                [Step(0.001f)]
                float shadowMapDepthMin = 0.0f;

                [Category("Debug")]
                [Step(0.001f)]
                float shadowMapDepthMax = 1.0f;

                #endregion Debug

                #region Optimization

                [Category("Optimization")]
                [DisplayName("Valient's stable cascades")]
                [HelpText("Should be always enabled. Reduces effective shadow map resolution but prevents shadows from flickering and removes/reduces 'walking' shadow edges while camera rotates")]
                bool valientStableCascades = true;

                [Category("Optimization")]
                [DisplayName("Texel snapping")]
                [HelpText("Should be always enabled. Used in conjunction with valientsStableCascades, moves bounding sphere in 'texel size increments', reducing flickering while moving")]
                bool texelSnapping = true;

                [Category("Optimization")]
                [DisplayName("Tight bounding sphere")]
                [HelpText("Should be always enabled. Calculates smallest bounding sphere around frustum corners. Improves shadow map resolution")]
                bool tightBoundingSphere = false;

                [Category("Optimization")]
                [DisplayName("Fit cascade near/far to world AABB")]
                [HelpText("Should be always enabled. Fit's cascades near and far planes to world bounding box. It can sometimes reduce shadow quality (16bit might not be able to represent wide range of depths), " +
                    "but makes sure that every potential occluder gets into this frustum.")]
                bool fitCascadeNearFarToWorldAABB = true;

                [Category("Optimization")]
                [DisplayName("Fit cascade far to camera frustum")]
                [HelpText("Shortens range between cascade's near and far planes, increasing shadow map precision. It's an extension to 'Fit cascade near/far to world AABB'. " +
                    "It will cause near-far range to change every frame. Didn't observe any visible artifacts but this may lead to shadow shimmering.")]
                bool fitCascadeFarToCameraFrustum = true;

                [Category("Optimization")]
                [DisplayName("Extra frustum planes")]
                [HelpText("Experimental. Finds two extra planes that might improve frustum culling. Frustum culling in SOA version operates on 8 planes (typical frustum uses only 6), so is't possible to add two " +
                    "extra planes for free")]
                bool clipAgainstExtraFrustumPlanes = true;

                [Category("Optimization")]
                [DisplayName("Clear with prev cascade")]
                [HelpText("Draws previous cascade 'bounding box' at near plane to improve EarlyZ/HiZ/GpuOcclusionCulling. "
                + "Contents that would be rendered in this region are already in previous cascade. Should be disabled if using interval selection.")]
                bool initializeWithPrevCascadesFrustum = true;

                #endregion
            }

        }

        #region GpuOcclusionCulling

        class GpuOcclusionCulling
        {
            enum e_OcclusionMapSize
            {
                [EnumLabel( "128x128" )]
                eOcclusionMapSize_128x128 = 128,
                [EnumLabel( "256x256" )]
                eOcclusionMapSize_256x256 = 256,
                [EnumLabel( "512x512" )]
                eOcclusionMapSize_512x512 = 512,
                [EnumLabel( "1024x1024" )]
                eOcclusionMapSize_1024x1024 = 1024,
                [EnumLabel( "2048x2048" )]
                eOcclusionMapSize_2048x2048 = 2048,
            }

            [DisplayName("Occlusion Map Size")]
            [HelpText(
                  "512x512 is recommended size. Setting 256x256 can lead to false occlusion for small objects (512x512 has this caveat too, but it happens rarely). "
                + "Very low resolution looses small holes/gaps between objects. Those gaps are crucial for correct occlusion calculation. "
                + "Imagine situation with two big occluders, close to the camera, separated by only one-pixel-thick line in native resolution (this width depends on occlusion map resolution). "
                + "Object visible through this gap in native resolution will be false-culled with resolution of occlusion map less than native. "
                + "Line will disappear and two blocks will merge in low resolution occlusion map"
                )]
            e_OcclusionMapSize occlusionMapSize = e_OcclusionMapSize.eOcclusionMapSize_256x256;

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

            [Category( "Debug" )]
            bool debugShow = false;

            [Category( "Debug" )]
            float debugDrawOpacityColor = 1.0f;

            [Category( "Debug" )]
            [Step( 0.001f )]
            float debugDrawDepthScale = 1.0f;

            [Category( "Debug" )]
            [Step( 0.001f )]
            float debugDrawDepthMin = 1.0f;

            [Category( "Debug" )]
            [Step( 0.001f )]
            float debugDrawDepthMax = 1.0f;

            [Category( "Debug" )]
            [Max( 14.0f )]
            float debugDrawMipLevel = 0;

            [Category( "Debug" )]
            bool debugReloadShader = false;
        }

        #endregion
    }

    class HMD
    {
        [HelpText( "Determines if switching to hmd is possible. If false, pico won't even try to detect connected hmd's" )]
        bool Supported = true;

        [Category( "Oculus" )]
        [HelpText( "Determines if switching to Oculus HMD is possible. If false, pico won't even try to detect it" )]
        [DependsOn( "Supported" )]
        bool OculusSupported = true;

        [Category("Oculus")]
        [HelpText( "Automatically switches to hmd mode on startup" )]
        [DependsOn( "Supported" )]
        [DependsOn( "OculusSupported" )]
        bool OculusEnabled = true;

        [Category( "Morpheus" )]
        [HelpText( "Determines if switching to Morpheus HMD is possible. If false, pico won't even try to detect it" )]
        [DependsOn( "Supported" )]
        bool MorpheusSupported = true;

        [Category( "Morpheus" )]
        [HelpText( "Automatically switches to hmd mode on startup" )]
        [DependsOn( "Supported" )]
        [DependsOn( "MorpheusSupported" )]
        bool MorpheusEnabled = true;

        [DependsOn( "HMD.Supported" )]
        class Generic
        {
            [Min(0.5f)]
            [Max(4.0f)]
            [CheckBox(true)]
            float hmdOversample = 1.5f;

            bool DistortionCancellerEnabled = true;

            [Min( 0.1f )]
            [Max( 10 )]
            [CheckBox( true )]
            float AffectAllCameraNodesPositionScale = 1.5f;

            [Min( 0.1f )]
            [Max( 100 )]
            [CheckBox( true )]
            float NearPlaneOverride = 0.3f;

            enum e_CamChangeFadeType
            {
                Linear,
                Smoothstep
            };

            [Category("Camera Change Fader")]
            e_CamChangeFadeType FadeType = e_CamChangeFadeType.Smoothstep;

            [Min(0)][Max(10)][Step(0.05f)][SoftMax(1)]
            [Category( "Camera Change Fader" )]
            float FadeInDuration = 0.2f;

            [Min( 0 )]
            [Max( 10 )]
            [Step( 0.05f )]
            [SoftMax( 1 )]
            [Category( "Camera Change Fader" )]
            float FadeOutDuration = 0.2f;

            [Min( 0 )]
            [Max( 10 )]
            [Step( 0.05f )]
            [SoftMax( 1 )]
            [Category( "Camera Change Fader" )]
            float FadeInDurationCutscene = 0.15f;

            [Min( 0 )]
            [Max( 10 )]
            [Step( 0.05f )]
            [SoftMax( 1 )]
            [Category( "Camera Change Fader" )]
            float FadeOutDurationCutscene = 0.15f;

            [Min( 1 )]
            [Max( 20 )]
            [Step( 0.05f )]
            [Category( "Camera Change Fader" )]
            float PlayerSpeedMin = 5.0f;

            [Min( 1 )]
            [Max( 20 )]
            [Step( 0.05f )]
            [Category( "Camera Change Fader" )]
            float PlayerSpeedMax = 6.0f;

            [Min( 0 )]
            [Max( 4 )]
            [Step( 0.01f )]
            [Category( "HUD" )]
            float HudScale = 1.0f;
        }

        [DependsOn( "HMD.Supported" )]
        [DependsOn( "HMD.MorpheusSupported" )]
        class Morpheus
        {
            enum e_RefreshRate
            {
                [EnumLabel( "90 Hz" )]
                RefreshRate_90Hz = 0,

                [EnumLabel( "120 Hz" )]
                RefreshRate_120Hz,
            };

            [DisplayName("Refresh rate")]
            e_RefreshRate RefreshRate = e_RefreshRate.RefreshRate_120Hz;

            bool PredicationEnabled = true;
            [Min(0)]
            [Max(100)]
            float PredicationExtraTime = 0;
            bool ReprojectionEnabled = true;

            [HelpText("Turns on optimization, that causes corners of the depth buffer to be cleared with 0, leading to efficient sample rejection (less pixel shaders running)")]
            bool HTileClearOptimization = true;
            [Min( 0 )]
            [Max( 2 )]

            [HelpText("How much of the depth buffer to clear. See HTileClearOptimization for more info")]
            float HTileClearOptimizationFactor = 1.0f;

            [HelpText("Fetches camera orientation and position as late in the frame as possible to reduce time from reading sensor data to displaying image on the hmd. Not fully supported!")]
            bool LowLatencyTrackingEnabled = false;
        }
    }

    class Stats
    {
        bool Enabled = false;
        
        [Min(0)] [Max(10)] [SoftMax(6)]
        int NumberOfMarkerLevels = 0;
    }

    class PS4Gnm
    {
        bool AsyncComputeEnabled = true;

        [HelpText("Reduces max waves that can be used by async pipes. Setting it to maximum may result in \"Gfx\" pipe being starved.")]
        [Min(0)]
        [Max(10)]
        [DisplayName("Wave Limit Per SIMD")]
        int maxWavesPerSIMD = 8;
    };

    class TextUtil
    {
        bool ReloadTexts = false;
        enum e_Language
        {
            systemDefault,
            english,
            japanese,
            french,
            spanish,
            german,
            italian,
            dutch,
            portuguese,
            russian,
            korean,
            chineseTr,
            chineseSp,
            portugueseBr,
            spanishLa,
            frenchCa
        }

        e_Language Language = e_Language.systemDefault;

        [Category("Subtitles")]
        float subtitleWindowPositionX = 0.5f;
        [Category( "Subtitles" )]
        float subtitleWindowPositionY = 0.94f;
        [Category( "Subtitles" )]
        float subtitleWindowWidth = 0.7f;

        [Category( "Subtitles HMD" )]
        float hmdSubtitleWindowPositionX = 0.5f;
        [Category( "Subtitles HMD" )]
        float hmdSubtitleWindowWidth = 0.7f;
        [Category( "Subtitles HMD" )]
        float hmdSubtitleWindowPositionY = 0.94f;

        [Category( "Icon" )]
        float iconWindowPositionX = 0.8f;
        [Category( "Icon" )]
        float iconWindowPositionY = 0.85f;

        [Category( "Icon HMD" )]
        float hmdIconWindowPositionX = 0.8f;
        [Category( "Icon HMD" )]
        float hmdIconWindowPositionY = 0.45f;
    };

    class Recording
    {
        [HelpText("For trailer recording purposes")]

        bool HideLetterBox = false;
        bool HideCutsceneTexts = false;
    }
}
