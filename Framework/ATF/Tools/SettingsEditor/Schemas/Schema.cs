// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "settingsFile.xsd" "Schema.cs" "SettingsEditor" "SettingsEditor"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace SettingsEditor
{
    public static class Schema
    {
        public const string NS = "SettingsEditor";

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
            settingsFileType.Type = getNodeType("SettingsEditor", "settingsFileType");
            settingsFileType.settingsDescFileAttribute = settingsFileType.Type.GetAttributeInfo("settingsDescFile");
            settingsFileType.shaderOutputFileAttribute = settingsFileType.Type.GetAttributeInfo("shaderOutputFile");
            settingsFileType.groupChild = settingsFileType.Type.GetChildInfo("group");

            groupType.Type = getNodeType("SettingsEditor", "groupType");
            groupType.nameAttribute = groupType.Type.GetAttributeInfo("name");
            groupType.selectedPresetRefAttribute = groupType.Type.GetAttributeInfo("selectedPresetRef");
            groupType.propChild = groupType.Type.GetChildInfo("prop");
            groupType.presetChild = groupType.Type.GetChildInfo("preset");
            groupType.groupChild = groupType.Type.GetChildInfo("group");

            dynamicPropertyType.Type = getNodeType("SettingsEditor", "dynamicPropertyType");
            dynamicPropertyType.nameAttribute = dynamicPropertyType.Type.GetAttributeInfo("name");
            dynamicPropertyType.typeAttribute = dynamicPropertyType.Type.GetAttributeInfo("type");
            dynamicPropertyType.fvalAttribute = dynamicPropertyType.Type.GetAttributeInfo("fval");
            dynamicPropertyType.ivalAttribute = dynamicPropertyType.Type.GetAttributeInfo("ival");
            dynamicPropertyType.bvalAttribute = dynamicPropertyType.Type.GetAttributeInfo("bval");
            dynamicPropertyType.evalAttribute = dynamicPropertyType.Type.GetAttributeInfo("eval");
            dynamicPropertyType.svalAttribute = dynamicPropertyType.Type.GetAttributeInfo("sval");
            dynamicPropertyType.f4valAttribute = dynamicPropertyType.Type.GetAttributeInfo("f4val");
            dynamicPropertyType.dirvalAttribute = dynamicPropertyType.Type.GetAttributeInfo("dirval");
            dynamicPropertyType.colvalAttribute = dynamicPropertyType.Type.GetAttributeInfo("colval");
            dynamicPropertyType.minAttribute = dynamicPropertyType.Type.GetAttributeInfo("min");
            dynamicPropertyType.maxAttribute = dynamicPropertyType.Type.GetAttributeInfo("max");
            dynamicPropertyType.stepAttribute = dynamicPropertyType.Type.GetAttributeInfo("step");
            dynamicPropertyType.extraNameAttribute = dynamicPropertyType.Type.GetAttributeInfo("extraName");
            dynamicPropertyType.checkedAttribute = dynamicPropertyType.Type.GetAttributeInfo("checked");
            dynamicPropertyType.curveChild = dynamicPropertyType.Type.GetChildInfo("curve");

            curveType.Type = getNodeType("SettingsEditor", "curveType");
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

            controlPointType.Type = getNodeType("SettingsEditor", "controlPointType");
            controlPointType.xAttribute = controlPointType.Type.GetAttributeInfo("x");
            controlPointType.yAttribute = controlPointType.Type.GetAttributeInfo("y");
            controlPointType.tangentInAttribute = controlPointType.Type.GetAttributeInfo("tangentIn");
            controlPointType.tangentInTypeAttribute = controlPointType.Type.GetAttributeInfo("tangentInType");
            controlPointType.tangentOutAttribute = controlPointType.Type.GetAttributeInfo("tangentOut");
            controlPointType.tangentOutTypeAttribute = controlPointType.Type.GetAttributeInfo("tangentOutType");
            controlPointType.brokenTangentsAttribute = controlPointType.Type.GetAttributeInfo("brokenTangents");

            presetType.Type = getNodeType("SettingsEditor", "presetType");
            presetType.nameAttribute = presetType.Type.GetAttributeInfo("name");
            presetType.selectedPresetRefAttribute = presetType.Type.GetAttributeInfo("selectedPresetRef");
            presetType.propChild = presetType.Type.GetChildInfo("prop");
            presetType.presetChild = presetType.Type.GetChildInfo("preset");
            presetType.groupChild = presetType.Type.GetChildInfo("group");

            settingsFileRootElement = getRootElement(NS, "settingsFile");
        }

        public static class settingsFileType
        {
            public static DomNodeType Type;
            public static AttributeInfo settingsDescFileAttribute;
            public static AttributeInfo shaderOutputFileAttribute;
            public static ChildInfo groupChild;
        }

        public static class groupType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo selectedPresetRefAttribute;
            public static ChildInfo propChild;
            public static ChildInfo presetChild;
            public static ChildInfo groupChild;
        }

        public static class dynamicPropertyType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo fvalAttribute;
            public static AttributeInfo ivalAttribute;
            public static AttributeInfo bvalAttribute;
            public static AttributeInfo evalAttribute;
            public static AttributeInfo svalAttribute;
            public static AttributeInfo f4valAttribute;
            public static AttributeInfo dirvalAttribute;
            public static AttributeInfo colvalAttribute;
            public static AttributeInfo minAttribute;
            public static AttributeInfo maxAttribute;
            public static AttributeInfo stepAttribute;
            public static AttributeInfo extraNameAttribute;
            public static AttributeInfo checkedAttribute;
            public static ChildInfo curveChild;
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

        public static class presetType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo selectedPresetRefAttribute;
            public static ChildInfo propChild;
            public static ChildInfo presetChild;
            public static ChildInfo groupChild;
        }

        public static ChildInfo settingsFileRootElement;
    }
}
