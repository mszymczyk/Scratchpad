// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "statsViewer.xsd" "..\StatsViewerSchema.cs" "statsViewer" "misz.StatsViewer"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace misz.StatsViewer
{
    public static class StatsViewerSchema
    {
        public const string NS = "statsViewer";

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
            sessionType.Type = getNodeType("statsViewer", "sessionType");
            sessionType.groupChild = sessionType.Type.GetChildInfo("group");

            groupType.Type = getNodeType("statsViewer", "groupType");
            groupType.nameAttribute = groupType.Type.GetAttributeInfo("name");
            groupType.expandedAttribute = groupType.Type.GetAttributeInfo("expanded");
            groupType.trackChild = groupType.Type.GetChildInfo("track");

            trackType.Type = getNodeType("statsViewer", "trackType");
            trackType.nameAttribute = trackType.Type.GetAttributeInfo("name");
            trackType.statChild = trackType.Type.GetChildInfo("stat");

            statType.Type = getNodeType("statsViewer", "statType");
            statType.nameAttribute = statType.Type.GetAttributeInfo("name");
            statType.statValueChild = statType.Type.GetChildInfo("statValue");

            statValueType.Type = getNodeType("statsViewer", "statValueType");
            statValueType.timeAttribute = statValueType.Type.GetAttributeInfo("time");
            statValueType.valueAttribute = statValueType.Type.GetAttributeInfo("value");

            sessionRootElement = getRootElement(NS, "session");
        }

        public static class sessionType
        {
            public static DomNodeType Type;
            public static ChildInfo groupChild;
        }

        public static class groupType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static ChildInfo trackChild;
        }

        public static class trackType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo statChild;
        }

        public static class statType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo statValueChild;
        }

        public static class statValueType
        {
            public static DomNodeType Type;
            public static AttributeInfo timeAttribute;
            public static AttributeInfo valueAttribute;
        }

        public static ChildInfo sessionRootElement;
    }
}
