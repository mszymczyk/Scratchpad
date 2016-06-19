// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "timeline.xsd" "..\Schema.cs" "timeline" "picoAnimClipEditor"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace picoAnimClipEditor
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
            timelineType.animFilenameAttribute = timelineType.Type.GetAttributeInfo("animFilename");
            timelineType.animUserNameAttribute = timelineType.Type.GetAttributeInfo("animUserName");
            timelineType.animCategoryAttribute = timelineType.Type.GetAttributeInfo("animCategory");
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
            timelineRefType.refAttribute = timelineRefType.Type.GetAttributeInfo("ref");

            groupAnimType.Type = getNodeType("timeline", "groupAnimType");
            groupAnimType.nameAttribute = groupAnimType.Type.GetAttributeInfo("name");
            groupAnimType.expandedAttribute = groupAnimType.Type.GetAttributeInfo("expanded");
            groupAnimType.descriptionAttribute = groupAnimType.Type.GetAttributeInfo("description");
            groupAnimType.trackChild = groupAnimType.Type.GetChildInfo("track");

            trackAnimType.Type = getNodeType("timeline", "trackAnimType");
            trackAnimType.nameAttribute = trackAnimType.Type.GetAttributeInfo("name");
            trackAnimType.descriptionAttribute = trackAnimType.Type.GetAttributeInfo("description");
            trackAnimType.intervalChild = trackAnimType.Type.GetChildInfo("interval");
            trackAnimType.keyChild = trackAnimType.Type.GetChildInfo("key");

            intervalAnimType.Type = getNodeType("timeline", "intervalAnimType");
            intervalAnimType.startAttribute = intervalAnimType.Type.GetAttributeInfo("start");
            intervalAnimType.descriptionAttribute = intervalAnimType.Type.GetAttributeInfo("description");
            intervalAnimType.nameAttribute = intervalAnimType.Type.GetAttributeInfo("name");
            intervalAnimType.lengthAttribute = intervalAnimType.Type.GetAttributeInfo("length");
            intervalAnimType.colorAttribute = intervalAnimType.Type.GetAttributeInfo("color");

            keyTagType.Type = getNodeType("timeline", "keyTagType");
            keyTagType.startAttribute = keyTagType.Type.GetAttributeInfo("start");
            keyTagType.descriptionAttribute = keyTagType.Type.GetAttributeInfo("description");
            keyTagType.nameAttribute = keyTagType.Type.GetAttributeInfo("name");

            intervalTagType.Type = getNodeType("timeline", "intervalTagType");
            intervalTagType.startAttribute = intervalTagType.Type.GetAttributeInfo("start");
            intervalTagType.descriptionAttribute = intervalTagType.Type.GetAttributeInfo("description");
            intervalTagType.nameAttribute = intervalTagType.Type.GetAttributeInfo("name");
            intervalTagType.lengthAttribute = intervalTagType.Type.GetAttributeInfo("length");
            intervalTagType.colorAttribute = intervalTagType.Type.GetAttributeInfo("color");

            keySoundType.Type = getNodeType("timeline", "keySoundType");
            keySoundType.startAttribute = keySoundType.Type.GetAttributeInfo("start");
            keySoundType.descriptionAttribute = keySoundType.Type.GetAttributeInfo("description");
            keySoundType.nameAttribute = keySoundType.Type.GetAttributeInfo("name");
            keySoundType.soundBankAttribute = keySoundType.Type.GetAttributeInfo("soundBank");
            keySoundType.soundAttribute = keySoundType.Type.GetAttributeInfo("sound");
            keySoundType.positionalAttribute = keySoundType.Type.GetAttributeInfo("positional");
            keySoundType.positionAttribute = keySoundType.Type.GetAttributeInfo("position");

            intervalCharacterSoundType.Type = getNodeType("timeline", "intervalCharacterSoundType");
            intervalCharacterSoundType.startAttribute = intervalCharacterSoundType.Type.GetAttributeInfo("start");
            intervalCharacterSoundType.descriptionAttribute = intervalCharacterSoundType.Type.GetAttributeInfo("description");
            intervalCharacterSoundType.nameAttribute = intervalCharacterSoundType.Type.GetAttributeInfo("name");
            intervalCharacterSoundType.lengthAttribute = intervalCharacterSoundType.Type.GetAttributeInfo("length");
            intervalCharacterSoundType.colorAttribute = intervalCharacterSoundType.Type.GetAttributeInfo("color");
            intervalCharacterSoundType.soundBankAttribute = intervalCharacterSoundType.Type.GetAttributeInfo("soundBank");
            intervalCharacterSoundType.soundAttribute = intervalCharacterSoundType.Type.GetAttributeInfo("sound");
            intervalCharacterSoundType.positionalAttribute = intervalCharacterSoundType.Type.GetAttributeInfo("positional");
            intervalCharacterSoundType.positionAttribute = intervalCharacterSoundType.Type.GetAttributeInfo("position");

            timelineRootElement = getRootElement(NS, "timeline");
        }

        public static class timelineType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo animFilenameAttribute;
            public static AttributeInfo animUserNameAttribute;
            public static AttributeInfo animCategoryAttribute;
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
            public static AttributeInfo refAttribute;
        }

        public static class groupAnimType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo descriptionAttribute;
            public static ChildInfo trackChild;
        }

        public static class trackAnimType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo descriptionAttribute;
            public static ChildInfo intervalChild;
            public static ChildInfo keyChild;
        }

        public static class intervalAnimType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
        }

        public static class keyTagType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
        }

        public static class intervalTagType
        {
            public static DomNodeType Type;
            public static AttributeInfo startAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo lengthAttribute;
            public static AttributeInfo colorAttribute;
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

        public static class intervalCharacterSoundType
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

        public static ChildInfo timelineRootElement;
    }
}
