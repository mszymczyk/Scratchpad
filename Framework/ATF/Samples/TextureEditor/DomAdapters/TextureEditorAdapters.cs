//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

using Sce.Atf;
using Sce.Atf.Dom;

using PropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;

namespace TextureEditor
{
    /// <summary>
    /// Registers DomNodeAdapters for all DomNodeTypes</summary>
	public static class TextureEditorAdapters
    {
        /// <summary>
        /// Register DomNodeAdapters </summary>        
		public static void Initialize( SchemaLoader schemaLoader )
		{
			// resource meta data             
			Schema.resourceMetadataType.Type.Define( new ExtensionInfo<ResourceMetadataDocument>() );
			Schema.textureMetadataType.Type.Define( new ExtensionInfo<ResourceMetadataDocument>() );
			Schema.textureMetadataType.Type.Define( new ExtensionInfo<TextureMetadata>() );
			Schema.textureMetadataType.Type.Define( new ExtensionInfo<TexturePropertyDescriptorNodeAdapter>() );
			Schema.textureMetadataEditorType.Type.Define(new ExtensionInfo<ResourceMetadataEditingContext>());
		}
    }
}
