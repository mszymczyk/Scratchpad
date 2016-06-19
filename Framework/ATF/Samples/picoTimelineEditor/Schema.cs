// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "timeline.xsd" "..\Schema.cs" "timeline" "picoTimelineEditor"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace picoTimelineEditor
{
    public static class Schema
    {
        public const string NS = "timeline";

        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            Initialize((ns,name)=>typeCollection.GetNodeType(ns,name),
                (ns,name)=>typeCollection.GetRootElement(ns,name));
        }

        public static void Initialize(IDictionary<string, XmlSchemaTypeCollection> typeCollections)
        {
            Initialize((ns,name)=>typeCollections[ns].GetNodeType(name),
                (ns,name)=>typeCollections[ns].GetRootElement(name));
        }

        private static void Initialize(Func<string, string, DomNodeType> getNodeType, Func<string, string, ChildInfo> getRootElement)
        {
            timelineType.Type = getNodeType("timeline", "timelineType");
            timelineType.nameAttribute = timelineType.Type.GetAttributeInfo("name");
            timelineType.groupChild = timelineType.Type.GetChildInfo("group");
            timelineType.markerChild = timelineType.Type.GetChildInfo("marker");
            timelineType.timelineRefChild = timelineType.Type.GetChildInfo("timelineRef");

            groupType.Type = getNodeType("timeline", "groupType");
            groupType.nameAttribute = groupType.Type.GetAttributeInfo("name");
            groupType.expandedAttribute = groupType.Type.GetAttributeInfo("expanded");
            groupType.descriptionAttribute = groupType.Type.GetAttributeInfo("description");
            groupType.trackChild = groupType.Type.GetChildInfo("track");

            trackType.Type = getNodeType("timeline", "trackType");
            trackType.nameAttribute = trackType.Type.GetAttributeInfo("name");
            trackType.descriptionAttribute = trackType.Type.GetAttributeInfo("description");
            trackType.intervalChild = trackType.Type.GetChildInfo("interval");
            trackType.keyChild = trackType.Type.GetChildInfo("key");

            intervalType.Type = getNodeType("timeline", "intervalType");
            intervalType.startAttribute = intervalType.Type.GetAttributeInfo("start");
            intervalType.descriptionAttribute = intervalType.Type.GetAttributeInfo("description");
            intervalType.nameAttribute = intervalType.Type.GetAttributeInfo("name");
            intervalType.lengthAttribute = intervalType.Type.GetAttributeInfo("length");
            intervalType.colorAttribute = intervalType.Type.GetAttributeInfo("color");

            eventType.Type = getNodeType("timeline", "eventType");
            eventType.startAttribute = eventType.Type.GetAttributeInfo("start");
            eventType.descriptionAttribute = eventType.Type.GetAttributeInfo("description");
            eventType.nameAttribute = eventType.Type.GetAttributeInfo("name");

            keyType.Type = getNodeType("timeline", "keyType");
            keyType.startAttribute = keyType.Type.GetAttributeInfo("start");
            keyType.descriptionAttribute = keyType.Type.GetAttributeInfo("description");
            keyType.nameAttribute = keyType.Type.GetAttributeInfo("name");

            markerType.Type = getNodeType("timeline", "markerType");
            markerType.startAttribute = markerType.Type.GetAttributeInfo("start");
            markerType.descriptionAttribute = markerType.Type.GetAttributeInfo("description");
            markerType.nameAttribute = markerType.Type.GetAttributeInfo("name");
            markerType.colorAttribute = markerType.Type.GetAttributeInfo("color");

            timelineRefType.Type = getNodeType("timeline", "timelineRefType");
            timelineRefType.nameAttribute = timelineRefType.Type.GetAttributeInfo("name");
            timelineRefType.startAttribute = timelineRefType.Type.GetAttributeInfo("start");
            timelineRefType.descriptionAttribute = timelineRefType.Type.GetAttributeInfo("description");
            timelineRefType.colorAttribute = timelineRefType.Type.GetAttributeInfo("color");
            timelineRefType.timelineFilenameAttribute = timelineRefType.Type.GetAttributeInfo("timelineFilename");

            controlPointType.Type = getNodeType("timeline", "controlPointType");
            controlPointType.xAttribute = controlPointType.Type.GetAttributeInfo("x");
            controlPointType.yAttribute = controlPointType.Type.GetAttributeInfo("y");
            controlPointType.tangentInAttribute = controlPointType.Type.GetAttributeInfo("tangentIn");
            controlPointType.tangentInTypeAttribute = controlPointType.Type.GetAttributeInfo("tangentInType");
            controlPointType.tangentOutAttribute = controlPointType.Type.GetAttributeInfo("tangentOut");
            controlPointType.tangentOutTypeAttribute = controlPointType.Type.GetAttributeInfo("tangentOutType");
            controlPointType.brokenTangentsAttribute = controlPointType.Type.GetAttributeInfo("brokenTangents");

            curveType.Type = getNodeType("timeline", "curveType");
            curveType.nameAttribute = curveType.Type.GetAttributeInfo("name");
            curveType.displayNameAttribute = curveType.Type.GetAttributeInfo("displayName");
            curveType.minXAttribute = curveType.Type.GetAttributeInfo("minX");
            curveType.maxXAttribute = curveType.Type.GetAttributeInfo("maxX");
            curveType.minYAttribute = curveType.Type.GetAttributeInfo("minY");
            curveType.maxYAttribute = curveType.Type.GetAttributeInfo("maxY");
            curveType.preInfinityAttribute = curveType.Type.GetAttributeInfo("preInfinity");
            curveType.postInfinityAttribute = curveType.Type.GetAttributeInfo("postInfinity");
            curveType.colorAttribute = curveType.Type.GetAttributeInfo("color");
            curveType.xLabelAttribute = curveType.Type.GetAttributeInfo("xLabel");
            curveType.yLabelAttribute = curveType.Type.GetAttributeInfo("yLabel");
            curveType.controlPointChild = curveType.Type.GetChildInfo("controlPoint");

            keyLuaScriptType.Type = getNodeType("timeline", "keyLuaScriptType");
            keyLuaScriptType.startAttribute = keyLuaScriptType.Type.GetAttributeInfo("start");
            keyLuaScriptType.descriptionAttribute = keyLuaScriptType.Type.GetAttributeInfo("description");
            keyLuaScriptType.nameAttribute = keyLuaScriptType.Type.GetAttributeInfo("name");
            keyLuaScriptType.sourceCodeAttribute = keyLuaScriptType.Type.GetAttributeInfo("sourceCode");

            groupCameraType.Type = getNodeType("timeline", "groupCameraType");
            groupCameraType.nameAttribute = groupCameraType.Type.GetAttributeInfo("name");
            groupCameraType.expandedAttribute = groupCameraType.Type.GetAttributeInfo("expanded");
            groupCameraType.descriptionAttribute = groupCameraType.Type.GetAttributeInfo("description");
            groupCameraType.outputCameraAttribute = groupCameraType.Type.GetAttributeInfo("outputCamera");
            groupCameraType.preCutsceneCameraAttribute = groupCameraType.Type.GetAttributeInfo("preCutsceneCamera");
            groupCameraType.postCutsceneCameraAttribute = groupCameraType.Type.GetAttributeInfo("postCutsceneCamera");
            groupCameraType.blendInDurationAttribute = groupCameraType.Type.GetAttributeInfo("blendInDuration");
            groupCameraType.blendOutDurationAttribute = groupCameraType.Type.GetAttributeInfo("blendOutDuration");
            groupCameraType.trackChild = groupCameraType.Type.GetChildInfo("track");

            trackCameraAnimType.Type = getNodeType("timeline", "trackCameraAnimType");
            trackCameraAnimType.nameAttribute = trackCameraAnimType.Type.GetAttributeInfo("name");
            trackCameraAnimType.descriptionAttribute = trackCameraAnimType.Type.GetAttributeInfo("description");
            trackCameraAnimType.intervalChild = trackCameraAnimType.Type.GetChildInfo("interval");
            trackCameraAnimType.keyChild = trackCameraAnimType.Type.GetChildInfo("key");
            trackCameraAnimType.intervalCameraAnimTypeChild = trackCameraAnimType.Type.GetChildInfo("intervalCameraAnimType");

            intervalCameraAnimType.Type = getNodeType("timeline", "intervalCameraAnimType");
            intervalCameraAnimType.startAttribute = intervalCameraAnimType.Type.GetAttributeInfo("start");
            intervalCameraAnimType.descriptionAttribute = intervalCameraAnimType.Type.GetAttributeInfo("description");
            intervalCameraAnimType.nameAttribute = intervalCameraAnimType.Type.GetAttributeInfo("name");
            intervalCameraAnimType.lengthAttribute = intervalCameraAnimType.Type.GetAttributeInfo("length");
            intervalCameraAnimType.colorAttribute = intervalCameraAnimType.Type.GetAttributeInfo("color");
            intervalCameraAnimType.animOffsetAttribute = intervalCameraAnimType.Type.GetAttributeInfo("animOffset");
            intervalCameraAnimType.animFileAttribute = intervalCameraAnimType.Type.GetAttributeInfo("animFile");
            intervalCameraAnimType.cameraViewAttribute = intervalCameraAnimType.Type.GetAttributeInfo("cameraView");
            intervalCameraAnimType.fovAttribute = intervalCameraAnimType.Type.GetAttributeInfo("fov");
            intervalCameraAnimType.nearClipPlaneAttribute = intervalCameraAnimType.Type.GetAttributeInfo("nearClipPlane");
            intervalCameraAnimType.farClipPlaneAttribute = intervalCameraAnimType.Type.GetAttributeInfo("farClipPlane");

            groupAnimControllerType.Type = getNodeType("timeline", "groupAnimControllerType");
            groupAnimControllerType.nameAttribute = groupAnimControllerType.Type.GetAttributeInfo("name");
            groupAnimControllerType.expandedAttribute = groupAnimControllerType.Type.GetAttributeInfo("expanded");
            groupAnimControllerType.descriptionAttribute = groupAnimControllerType.Type.GetAttributeInfo("description");
            groupAnimControllerType.skelFileAttribute = groupAnimControllerType.Type.GetAttributeInfo("skelFile");
            groupAnimControllerType.rootNodeAttribute = groupAnimControllerType.Type.GetAttributeInfo("rootNode");
            groupAnimControllerType.trackChild = groupAnimControllerType.Type.GetChildInfo("track");

            trackAnimControllerType.Type = getNodeType("timeline", "trackAnimControllerType");
            trackAnimControllerType.nameAttribute = trackAnimControllerType.Type.GetAttributeInfo("name");
            trackAnimControllerType.descriptionAttribute = trackAnimControllerType.Type.GetAttributeInfo("description");
            trackAnimControllerType.intervalChild = trackAnimControllerType.Type.GetChildInfo("interval");
            trackAnimControllerType.keyChild = trackAnimControllerType.Type.GetChildInfo("key");
            trackAnimControllerType.intervalAnimControllerTypeChild = trackAnimControllerType.Type.GetChildInfo("intervalAnimControllerType");

            intervalAnimControllerType.Type = getNodeType("timeline", "intervalAnimControllerType");
            intervalAnimControllerType.startAttribute = intervalAnimControllerType.Type.GetAttributeInfo("start");
            intervalAnimControllerType.descriptionAttribute = intervalAnimControllerType.Type.GetAttributeInfo("description");
            intervalAnimControllerType.nameAttribute = intervalAnimControllerType.Type.GetAttributeInfo("name");
            intervalAnimControllerType.lengthAttribute = intervalAnimControllerType.Type.GetAttributeInfo("length");
            intervalAnimControllerType.colorAttribute = intervalAnimControllerType.Type.GetAttributeInfo("color");
            intervalAnimControllerType.animOffsetAttribute = intervalAnimControllerType.Type.GetAttributeInfo("animOffset");
            intervalAnimControllerType.animFileAttribute = intervalAnimControllerType.Type.GetAttributeInfo("animFile");

            intervalCurveType.Type = getNodeType("timeline", "intervalCurveType");
            intervalCurveType.startAttribute = intervalCurveType.Type.GetAttributeInfo("start");
            intervalCurveType.descriptionAttribute = intervalCurveType.Type.GetAttributeInfo("description");
            intervalCurveType.nameAttribute = intervalCurveType.Type.GetAttributeInfo("name");
            intervalCurveType.lengthAttribute = intervalCurveType.Type.GetAttributeInfo("length");
            intervalCurveType.colorAttribute = intervalCurveType.Type.GetAttributeInfo("color");
            intervalCurveType.curveChild = intervalCurveType.Type.GetChildInfo("curve");

            trackFaderType.Type = getNodeType("timeline", "trackFaderType");
            trackFaderType.nameAttribute = trackFaderType.Type.GetAttributeInfo("name");
            trackFaderType.descriptionAttribute = trackFaderType.Type.GetAttributeInfo("description");
            trackFaderType.intervalChild = trackFaderType.Type.GetChildInfo("interval");
            trackFaderType.keyChild = trackFaderType.Type.GetChildInfo("key");

            intervalFaderType.Type = getNodeType("timeline", "intervalFaderType");
            intervalFaderType.startAttribute = intervalFaderType.Type.GetAttributeInfo("start");
            intervalFaderType.descriptionAttribute = intervalFaderType.Type.GetAttributeInfo("description");
            intervalFaderType.nameAttribute = intervalFaderType.Type.GetAttributeInfo("name");
            intervalFaderType.lengthAttribute = intervalFaderType.Type.GetAttributeInfo("length");
            intervalFaderType.colorAttribute = intervalFaderType.Type.GetAttributeInfo("color");
            intervalFaderType.curveChild = intervalFaderType.Type.GetChildInfo("curve");

            intervalNodeAnimationType.Type = getNodeType("timeline", "intervalNodeAnimationType");
            intervalNodeAnimationType.startAttribute = intervalNodeAnimationType.Type.GetAttributeInfo("start");
            intervalNodeAnimationType.descriptionAttribute = intervalNodeAnimationType.Type.GetAttributeInfo("description");
            intervalNodeAnimationType.nameAttribute = intervalNodeAnimationType.Type.GetAttributeInfo("name");
            intervalNodeAnimationType.lengthAttribute = intervalNodeAnimationType.Type.GetAttributeInfo("length");
            intervalNodeAnimationType.colorAttribute = intervalNodeAnimationType.Type.GetAttributeInfo("color");
            intervalNodeAnimationType.nodeNameAttribute = intervalNodeAnimationType.Type.GetAttributeInfo("nodeName");
            intervalNodeAnimationType.channelsAttribute = intervalNodeAnimationType.Type.GetAttributeInfo("channels");
            intervalNodeAnimationType.curveChild = intervalNodeAnimationType.Type.GetChildInfo("curve");

            groupCharacterControllerType.Type = getNodeType("timeline", "groupCharacterControllerType");
            groupCharacterControllerType.nameAttribute = groupCharacterControllerType.Type.GetAttributeInfo("name");
            groupCharacterControllerType.expandedAttribute = groupCharacterControllerType.Type.GetAttributeInfo("expanded");
            groupCharacterControllerType.descriptionAttribute = groupCharacterControllerType.Type.GetAttributeInfo("description");
            groupCharacterControllerType.nodeNameAttribute = groupCharacterControllerType.Type.GetAttributeInfo("nodeName");
            groupCharacterControllerType.blendInDurationAttribute = groupCharacterControllerType.Type.GetAttributeInfo("blendInDuration");
            groupCharacterControllerType.blendOutDurationAttribute = groupCharacterControllerType.Type.GetAttributeInfo("blendOutDuration");
            groupCharacterControllerType.trackChild = groupCharacterControllerType.Type.GetChildInfo("track");

            trackCharacterControllerAnimType.Type = getNodeType("timeline", "trackCharacterControllerAnimType");
            trackCharacterControllerAnimType.nameAttribute = trackCharacterControllerAnimType.Type.GetAttributeInfo("name");
            trackCharacterControllerAnimType.descriptionAttribute = trackCharacterControllerAnimType.Type.GetAttributeInfo("description");
            trackCharacterControllerAnimType.intervalChild = trackCharacterControllerAnimType.Type.GetChildInfo("interval");
            trackCharacterControllerAnimType.keyChild = trackCharacterControllerAnimType.Type.GetChildInfo("key");

            intervalCharacterControllerAnimType.Type = getNodeType("timeline", "intervalCharacterControllerAnimType");
            intervalCharacterControllerAnimType.startAttribute = intervalCharacterControllerAnimType.Type.GetAttributeInfo("start");
            intervalCharacterControllerAnimType.descriptionAttribute = intervalCharacterControllerAnimType.Type.GetAttributeInfo("description");
            intervalCharacterControllerAnimType.nameAttribute = intervalCharacterControllerAnimType.Type.GetAttributeInfo("name");
            intervalCharacterControllerAnimType.lengthAttribute = intervalCharacterControllerAnimType.Type.GetAttributeInfo("length");
            intervalCharacterControllerAnimType.colorAttribute = intervalCharacterControllerAnimType.Type.GetAttributeInfo("color");
            intervalCharacterControllerAnimType.animFileAttribute = intervalCharacterControllerAnimType.Type.GetAttributeInfo("animFile");
            intervalCharacterControllerAnimType.animOffsetAttribute = intervalCharacterControllerAnimType.Type.GetAttributeInfo("animOffset");

            keySoundType.Type = getNodeType("timeline", "keySoundType");
            keySoundType.startAttribute = keySoundType.Type.GetAttributeInfo("start");
            keySoundType.descriptionAttribute = keySoundType.Type.GetAttributeInfo("description");
            keySoundType.nameAttribute = keySoundType.Type.GetAttributeInfo("name");
            keySoundType.soundBankAttribute = keySoundType.Type.GetAttributeInfo("soundBank");
            keySoundType.soundAttribute = keySoundType.Type.GetAttributeInfo("sound");
            keySoundType.positionalAttribute = keySoundType.Type.GetAttributeInfo("positional");
            keySoundType.positionAttribute = keySoundType.Type.GetAttributeInfo("position");

            intervalSoundType.Type = getNodeType("timeline", "intervalSoundType");
            intervalSoundType.startAttribute = intervalSoundType.Type.GetAttributeInfo("start");
            intervalSoundType.descriptionAttribute = intervalSoundType.Type.GetAttributeInfo("description");
            intervalSoundType.nameAttribute = intervalSoundType.Type.GetAttributeInfo("name");
            intervalSoundType.lengthAttribute = intervalSoundType.Type.GetAttributeInfo("length");
            intervalSoundType.colorAttribute = intervalSoundType.Type.GetAttributeInfo("color");
            intervalSoundType.soundBankAttribute = intervalSoundType.Type.GetAttributeInfo("soundBank");
            intervalSoundType.soundAttribute = intervalSoundType.Type.GetAttributeInfo("sound");
            intervalSoundType.positionalAttribute = intervalSoundType.Type.GetAttributeInfo("positional");
            intervalSoundType.positionAttribute = intervalSoundType.Type.GetAttributeInfo("position");

            refChangeLevelType.Type = getNodeType("timeline", "refChangeLevelType");
            refChangeLevelType.nameAttribute = refChangeLevelType.Type.GetAttributeInfo("name");
            refChangeLevelType.startAttribute = refChangeLevelType.Type.GetAttributeInfo("start");
            refChangeLevelType.descriptionAttribute = refChangeLevelType.Type.GetAttributeInfo("description");
            refChangeLevelType.colorAttribute = refChangeLevelType.Type.GetAttributeInfo("color");
            refChangeLevelType.timelineFilenameAttribute = refChangeLevelType.Type.GetAttributeInfo("timelineFilename");
            refChangeLevelType.levelNameAttribute = refChangeLevelType.Type.GetAttributeInfo("levelName");
            refChangeLevelType.unloadCurrentlevelAttribute = refChangeLevelType.Type.GetAttributeInfo("unloadCurrentlevel");

            refPlayTimelineType.Type = getNodeType("timeline", "refPlayTimelineType");
            refPlayTimelineType.nameAttribute = refPlayTimelineType.Type.GetAttributeInfo("name");
            refPlayTimelineType.startAttribute = refPlayTimelineType.Type.GetAttributeInfo("start");
            refPlayTimelineType.descriptionAttribute = refPlayTimelineType.Type.GetAttributeInfo("description");
            refPlayTimelineType.colorAttribute = refPlayTimelineType.Type.GetAttributeInfo("color");
            refPlayTimelineType.timelineFilenameAttribute = refPlayTimelineType.Type.GetAttributeInfo("timelineFilename");

            intervalTextType.Type = getNodeType("timeline", "intervalTextType");
            intervalTextType.startAttribute = intervalTextType.Type.GetAttributeInfo("start");
            intervalTextType.descriptionAttribute = intervalTextType.Type.GetAttributeInfo("description");
            intervalTextType.nameAttribute = intervalTextType.Type.GetAttributeInfo("name");
            intervalTextType.lengthAttribute = intervalTextType.Type.GetAttributeInfo("length");
            intervalTextType.colorAttribute = intervalTextType.Type.GetAttributeInfo("color");
            intervalTextType.textNodeNameAttribute = intervalTextType.Type.GetAttributeInfo("textNodeName");
            intervalTextType.textTagAttribute = intervalTextType.Type.GetAttributeInfo("textTag");

            trackBlendFactorType.Type = getNodeType("timeline", "trackBlendFactorType");
            trackBlendFactorType.nameAttribute = trackBlendFactorType.Type.GetAttributeInfo("name");
            trackBlendFactorType.descriptionAttribute = trackBlendFactorType.Type.GetAttributeInfo("description");
            trackBlendFactorType.intervalChild = trackBlendFactorType.Type.GetChildInfo("interval");
            trackBlendFactorType.keyChild = trackBlendFactorType.Type.GetChildInfo("key");

            intervalBlendFactorType.Type = getNodeType("timeline", "intervalBlendFactorType");
            intervalBlendFactorType.startAttribute = intervalBlendFactorType.Type.GetAttributeInfo("start");
            intervalBlendFactorType.descriptionAttribute = intervalBlendFactorType.Type.GetAttributeInfo("description");
            intervalBlendFactorType.nameAttribute = intervalBlendFactorType.Type.GetAttributeInfo("name");
            intervalBlendFactorType.lengthAttribute = intervalBlendFactorType.Type.GetAttributeInfo("length");
            intervalBlendFactorType.colorAttribute = intervalBlendFactorType.Type.GetAttributeInfo("color");
            intervalBlendFactorType.curveChild = intervalBlendFactorType.Type.GetChildInfo("curve");

            settingType.Type = getNodeType("timeline", "settingType");

            cresLodSettingType.Type = getNodeType("timeline", "cresLodSettingType");
            cresLodSettingType.nodeNameAttribute = cresLodSettingType.Type.GetAttributeInfo("nodeName");
            cresLodSettingType.lod0DistanceAttribute = cresLodSettingType.Type.GetAttributeInfo("lod0Distance");
            cresLodSettingType.lod1DistanceAttribute = cresLodSettingType.Type.GetAttributeInfo("lod1Distance");
            cresLodSettingType.lod2DistanceAttribute = cresLodSettingType.Type.GetAttributeInfo("lod2Distance");
            cresLodSettingType.cullDistanceAttribute = cresLodSettingType.Type.GetAttributeInfo("cullDistance");
            cresLodSettingType.lod0DistanceShadowAttribute = cresLodSettingType.Type.GetAttributeInfo("lod0DistanceShadow");
            cresLodSettingType.lod1DistanceShadowAttribute = cresLodSettingType.Type.GetAttributeInfo("lod1DistanceShadow");
            cresLodSettingType.lod2DistanceShadowAttribute = cresLodSettingType.Type.GetAttributeInfo("lod2DistanceShadow");

            intervalSettingType.Type = getNodeType("timeline", "intervalSettingType");
            intervalSettingType.startAttribute = intervalSettingType.Type.GetAttributeInfo("start");
            intervalSettingType.descriptionAttribute = intervalSettingType.Type.GetAttributeInfo("description");
            intervalSettingType.nameAttribute = intervalSettingType.Type.GetAttributeInfo("name");
            intervalSettingType.lengthAttribute = intervalSettingType.Type.GetAttributeInfo("length");
            intervalSettingType.colorAttribute = intervalSettingType.Type.GetAttributeInfo("color");
            intervalSettingType.settingChild = intervalSettingType.Type.GetChildInfo("setting");

            timelineRootElement = getRootElement(NS, "timeline");
        }

        public static class timelineType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo groupChild;
            public static ChildInfo markerChild;
            public static ChildInfo timelineRefChild;
        }

        public static class groupType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo descriptionAttribute;
            public static ChildInfo trackChild;
        }

        public static class trackType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo descriptionAttribute;
            public static ChildInfo intervalChild;
            public static ChildInfo keyChild;
        }

        public static class intervalType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
        }

        public static class eventType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
        }

        public static class keyType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
        }

        public static class markerType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo colorAttribute;
        }

        public static class timelineRefType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo timelineFilenameAttribute;
        }

        public static class controlPointType
        {
            public static DomNodeType Type;
            public static AttributeInfo xAttribute;
            public static AttributeInfo yAttribute;
            public static AttributeInfo tangentInAttribute;
            public static AttributeInfo tangentInTypeAttribute;
            public static AttributeInfo tangentOutAttribute;
            public static AttributeInfo tangentOutTypeAttribute;
            public static AttributeInfo brokenTangentsAttribute;
        }

        public static class curveType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo displayNameAttribute;
            public static AttributeInfo minXAttribute;
            public static AttributeInfo maxXAttribute;
            public static AttributeInfo minYAttribute;
            public static AttributeInfo maxYAttribute;
            public static AttributeInfo preInfinityAttribute;
            public static AttributeInfo postInfinityAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo xLabelAttribute;
            public static AttributeInfo yLabelAttribute;
            public static ChildInfo controlPointChild;
        }

        public static class keyLuaScriptType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo sourceCodeAttribute;
        }

        public static class groupCameraType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo outputCameraAttribute;
            public static AttributeInfo preCutsceneCameraAttribute;
            public static AttributeInfo postCutsceneCameraAttribute;
            public static AttributeInfo blendInDurationAttribute;
            public static AttributeInfo blendOutDurationAttribute;
            public static ChildInfo trackChild;
        }

        public static class trackCameraAnimType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo descriptionAttribute;
            public static ChildInfo intervalChild;
            public static ChildInfo keyChild;
            public static ChildInfo intervalCameraAnimTypeChild;
        }

        public static class intervalCameraAnimType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo animOffsetAttribute;
            public static AttributeInfo animFileAttribute;
            public static AttributeInfo cameraViewAttribute;
            public static AttributeInfo fovAttribute;
            public static AttributeInfo nearClipPlaneAttribute;
            public static AttributeInfo farClipPlaneAttribute;
        }

        public static class groupAnimControllerType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo skelFileAttribute;
            public static AttributeInfo rootNodeAttribute;
            public static ChildInfo trackChild;
        }

        public static class trackAnimControllerType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo descriptionAttribute;
            public static ChildInfo intervalChild;
            public static ChildInfo keyChild;
            public static ChildInfo intervalAnimControllerTypeChild;
        }

        public static class intervalAnimControllerType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo animOffsetAttribute;
            public static AttributeInfo animFileAttribute;
        }

        public static class intervalCurveType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
            public static ChildInfo curveChild;
        }

        public static class trackFaderType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo descriptionAttribute;
            public static ChildInfo intervalChild;
            public static ChildInfo keyChild;
        }

        public static class intervalFaderType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
            public static ChildInfo curveChild;
        }

        public static class intervalNodeAnimationType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo nodeNameAttribute;
            public static AttributeInfo channelsAttribute;
            public static ChildInfo curveChild;
        }

        public static class groupCharacterControllerType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nodeNameAttribute;
            public static AttributeInfo blendInDurationAttribute;
            public static AttributeInfo blendOutDurationAttribute;
            public static ChildInfo trackChild;
        }

        public static class trackCharacterControllerAnimType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo descriptionAttribute;
            public static ChildInfo intervalChild;
            public static ChildInfo keyChild;
        }

        public static class intervalCharacterControllerAnimType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo animFileAttribute;
            public static AttributeInfo animOffsetAttribute;
        }

        public static class keySoundType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo soundBankAttribute;
            public static AttributeInfo soundAttribute;
            public static AttributeInfo positionalAttribute;
            public static AttributeInfo positionAttribute;
        }

        public static class intervalSoundType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo soundBankAttribute;
            public static AttributeInfo soundAttribute;
            public static AttributeInfo positionalAttribute;
            public static AttributeInfo positionAttribute;
        }

        public static class refChangeLevelType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo timelineFilenameAttribute;
            public static AttributeInfo levelNameAttribute;
            public static AttributeInfo unloadCurrentlevelAttribute;
        }

        public static class refPlayTimelineType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo timelineFilenameAttribute;
        }

        public static class intervalTextType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo textNodeNameAttribute;
            public static AttributeInfo textTagAttribute;
        }

        public static class trackBlendFactorType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo descriptionAttribute;
            public static ChildInfo intervalChild;
            public static ChildInfo keyChild;
        }

        public static class intervalBlendFactorType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
            public static ChildInfo curveChild;
        }

        public static class settingType
        {
            public static DomNodeType Type;
        }

        public static class cresLodSettingType
        {
            public static DomNodeType Type;
            public static AttributeInfo nodeNameAttribute;
            public static AttributeInfo lod0DistanceAttribute;
            public static AttributeInfo lod1DistanceAttribute;
            public static AttributeInfo lod2DistanceAttribute;
            public static AttributeInfo cullDistanceAttribute;
            public static AttributeInfo lod0DistanceShadowAttribute;
            public static AttributeInfo lod1DistanceShadowAttribute;
            public static AttributeInfo lod2DistanceShadowAttribute;
        }

        public static class intervalSettingType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
            public static ChildInfo settingChild;
        }

        public static ChildInfo timelineRootElement;
    }
}
