// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "textureEditor.xsd" "Schema.cs" "TextureEditor" "TextureEditor"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace TextureEditor
{
    public static class Schema
    {
        public const string NS = "TextureEditor";

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
            textureMetadataType.Type = getNodeType("TextureEditor", "textureMetadataType");
            textureMetadataType.uriAttribute = textureMetadataType.Type.GetAttributeInfo("uri");
            textureMetadataType.keywordsAttribute = textureMetadataType.Type.GetAttributeInfo("keywords");
            textureMetadataType.genMipMapsAttribute = textureMetadataType.Type.GetAttributeInfo("genMipMaps");
            textureMetadataType.forceSourceSrgbAttribute = textureMetadataType.Type.GetAttributeInfo("forceSourceSrgb");
            textureMetadataType.flipYAttribute = textureMetadataType.Type.GetAttributeInfo("flipY");
            textureMetadataType.copySourceFileAttribute = textureMetadataType.Type.GetAttributeInfo("copySourceFile");
            textureMetadataType.exportToGnfAttribute = textureMetadataType.Type.GetAttributeInfo("exportToGnf");
            textureMetadataType.presetAttribute = textureMetadataType.Type.GetAttributeInfo("preset");
            textureMetadataType.formatAttribute = textureMetadataType.Type.GetAttributeInfo("format");
            textureMetadataType.widthAttribute = textureMetadataType.Type.GetAttributeInfo("width");
            textureMetadataType.heightAttribute = textureMetadataType.Type.GetAttributeInfo("height");

            resourceMetadataType.Type = getNodeType("TextureEditor", "resourceMetadataType");
            resourceMetadataType.uriAttribute = resourceMetadataType.Type.GetAttributeInfo("uri");
            resourceMetadataType.keywordsAttribute = resourceMetadataType.Type.GetAttributeInfo("keywords");

            textureMetadataEditorType.Type = getNodeType("TextureEditor", "textureMetadataEditorType");
            textureMetadataEditorType.textureMetadataChild = textureMetadataEditorType.Type.GetChildInfo("textureMetadata");

            textureMetadataRootElement = getRootElement(NS, "textureMetadata");
            resourceMetadataRootElement = getRootElement(NS, "resourceMetadata");
            textureMetadataEditorRootElement = getRootElement(NS, "textureMetadataEditor");
        }

        public static class textureMetadataType
        {
            public static DomNodeType Type;
            public static AttributeInfo uriAttribute;
            public static AttributeInfo keywordsAttribute;
            public static AttributeInfo genMipMapsAttribute;
            public static AttributeInfo forceSourceSrgbAttribute;
            public static AttributeInfo flipYAttribute;
            public static AttributeInfo copySourceFileAttribute;
            public static AttributeInfo exportToGnfAttribute;
            public static AttributeInfo presetAttribute;
            public static AttributeInfo formatAttribute;
            public static AttributeInfo widthAttribute;
            public static AttributeInfo heightAttribute;
        }

        public static class resourceMetadataType
        {
            public static DomNodeType Type;
            public static AttributeInfo uriAttribute;
            public static AttributeInfo keywordsAttribute;
        }

        public static class textureMetadataEditorType
        {
            public static DomNodeType Type;
            public static ChildInfo textureMetadataChild;
        }

        public static ChildInfo textureMetadataRootElement;

        public static ChildInfo resourceMetadataRootElement;

        public static ChildInfo textureMetadataEditorRootElement;
    }
}
