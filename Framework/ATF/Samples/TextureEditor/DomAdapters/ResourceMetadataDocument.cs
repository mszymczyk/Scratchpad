//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Diagnostics;
using System.IO;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

//using LevelEditorCore;

namespace TextureEditor
{
    public class ResourceMetadataDocument : DomDocument
    {
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += DomNode_AttributeChanged;
        }

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
			TextureMetadata tm = DomNode.As<TextureMetadata>();
			if ( ( tm.Preset == SharpDX.DXGI.Format.Unknown.ToString() || ( tm.Preset == TextureMetadata.TEXTURE_PRESET_CUSTOM_FORMAT && tm.Format == SharpDX.DXGI.Format.Unknown ) ) && !tm.CopySourceFile )
			{
				if ( System.IO.File.Exists( Uri.LocalPath ) )
					System.IO.File.Delete( Uri.LocalPath );
				return;
			}

			SchemaLoader schemaTypeLoader = Globals.MEFContainer.GetExportedValue<SchemaLoader>();
			string filePath = Uri.LocalPath;
			FileMode fileMode = File.Exists( filePath ) ? FileMode.Truncate : FileMode.OpenOrCreate;
			using ( FileStream stream = new FileStream( filePath, fileMode ) )
			{
				var writer = new DomXmlWriter( schemaTypeLoader.TypeCollection );
				writer.PersistDefaultAttributes = true;
				writer.Write( DomNode, stream, Uri );
			}
        }
    }
}
