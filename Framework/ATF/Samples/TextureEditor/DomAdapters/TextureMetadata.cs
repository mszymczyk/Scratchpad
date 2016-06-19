using System;

using Sce.Atf.Dom;

namespace TextureEditor
{
	/// <summary>
	/// Texture metadata</summary>
	public class TextureMetadata : DomNodeAdapter
	{
		public static readonly string TEXTURE_PRESET_CUSTOM_FORMAT = "Custom Format";

		//public Uri Uri
		//{
		//	get { return GetAttribute<Uri>( Schema.resourceMetadataType.uriAttribute ); }
		//	set { SetAttribute( Schema.resourceMetadataType.uriAttribute, value ); }
		//}

		/// <summary>
		/// Gets or sets game object name</summary>
		public string Keywords
		{
			get { return GetAttribute<string>(Schema.resourceMetadataType.keywordsAttribute); }
			set { SetAttribute(Schema.resourceMetadataType.keywordsAttribute, value); }
		}

		public bool GenMipMaps
		{
			get { return GetAttribute<bool>( Schema.textureMetadataType.genMipMapsAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.genMipMapsAttribute, value ); }
		}

		public bool ForceSourceSrgb
		{
			get { return GetAttribute<bool>( Schema.textureMetadataType.forceSourceSrgbAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.forceSourceSrgbAttribute, value ); }
		}

		public bool FlipY
		{
			get { return GetAttribute<bool>( Schema.textureMetadataType.flipYAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.flipYAttribute, value ); }
		}

		public bool CopySourceFile
		{
			get { return GetAttribute<bool>( Schema.textureMetadataType.copySourceFileAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.copySourceFileAttribute, value ); }
		}

		public bool ExportToGnf
		{
			get { return GetAttribute<bool>( Schema.textureMetadataType.exportToGnfAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.exportToGnfAttribute, value ); }
		}

		//public SharpDX.DXGI.Format Format
		//{
		//	get
		//	{
		//		string sFormat = GetAttribute<string>( Schema.textureMetadataType.formatAttribute );
		//		SharpDX.DXGI.Format eFormat = SharpDX.DXGI.Format.Unknown;
		//		if ( Enum.TryParse<SharpDX.DXGI.Format>( sFormat, out eFormat ) )
		//		{
		//			return eFormat;
		//		}
		//		else
		//		{
		//			throw new Exception( "Unsupported format" );
		//			//return SharpDX.DXGI.Format.Unknown;
		//		}
		//	}
		//	set { SetAttribute( Schema.textureMetadataType.formatAttribute, value ); }
		//}
		public string Preset
		{
			get { return GetAttribute<string>( Schema.textureMetadataType.presetAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.presetAttribute, value ); }
		}

		public SharpDX.DXGI.Format Format
		{
			get
			{
				string sFormat = GetAttribute<string>( Schema.textureMetadataType.formatAttribute );
				SharpDX.DXGI.Format eFormat = SharpDX.DXGI.Format.Unknown;
				if ( Enum.TryParse<SharpDX.DXGI.Format>( sFormat, out eFormat ) )
				{
					return eFormat;
				}
				else
				{
					throw new Exception( "Unsupported format" );
				}
			}
			set { SetAttribute( Schema.textureMetadataType.formatAttribute, value ); }
		}

		public int Width
		{
			get { return GetAttribute<int>( Schema.textureMetadataType.widthAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.widthAttribute, value ); }
		}

		public int Height
		{
			get { return GetAttribute<int>( Schema.textureMetadataType.heightAttribute ); }
			set { SetAttribute( Schema.textureMetadataType.heightAttribute, value ); }
		}

		//public int Depth
		//{
		//	get { return GetAttribute<int>( Schema.textureMetadataType.depthAttribute ); }
		//	set { SetAttribute( Schema.textureMetadataType.depthAttribute, value ); }
		//}

		public string LocalPath { get; set; }
	}
}
