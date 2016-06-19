using System.ComponentModel;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a TextureBaseModule</summary>
    public abstract class TextureBaseModule : MaterialGraphModuleAdapter
    {
        public static readonly string TexturePaletteCategory = "Texture";
        public static readonly string SamplerPropertyCategory = "Sampler";

        public static DomNodeType textureBaseType;
        public static AttributeInfo filterAttribute;
        public static AttributeInfo uAddressModeAttribute;
        public static AttributeInfo vAddressModeAttribute;
        public static AttributeInfo wAddressModeAttribute;
        public static AttributeInfo minLodAttribute;
        public static AttributeInfo maxLodAttribute;
        public static AttributeInfo lodBiasAttribute;
        public static AttributeInfo maxAnisotropyAttribute;

        public static MaterialGraphPin pinOut_RGBA;
        public static MaterialGraphPin pinOut_Red;
        public static MaterialGraphPin pinOut_Green;
        public static MaterialGraphPin pinOut_Blue;
        public static MaterialGraphPin pinOut_Alpha;

        protected static void DefineTextureBaseDomNodeType()
        {
            if ( textureBaseType != null )
                return;

            pinOut_RGBA = new MaterialGraphPin("RGBA", MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.RGBA );
            pinOut_Red = new MaterialGraphPin("R", MaterialGraphPin.MaterialGraphPinTypeName, 1, MaterialGraphPin.ComponentType.Red, pinOut_RGBA );
            pinOut_Green = new MaterialGraphPin("G", MaterialGraphPin.MaterialGraphPinTypeName, 2, MaterialGraphPin.ComponentType.Green, pinOut_RGBA );
            pinOut_Blue = new MaterialGraphPin( "B", MaterialGraphPin.MaterialGraphPinTypeName, 3, MaterialGraphPin.ComponentType.Blue, pinOut_RGBA );
            pinOut_Alpha = new MaterialGraphPin( "A", MaterialGraphPin.MaterialGraphPinTypeName, 4, MaterialGraphPin.ComponentType.Alpha, pinOut_RGBA );

            string prettyName = "TextureBase".Localize();

            DomNodeType dnt = DefineModuleType(
                new XmlQualifiedName( "textureBaseType", Schema.NS ),
                prettyName,
                prettyName,
                Resources.LightImage,
                new MaterialGraphPin[]
                { },
                new MaterialGraphPin[]
                { },
                TexturePaletteCategory );

            textureBaseType = dnt;
            //textureBaseType.Define( new ExtensionInfo<TextureBaseModule>() );


            string[] samplerFilters = ShaderTypes.GetSamplerFilterModes();
            string[] samplerAddressModes = ShaderTypes.GetSamplerAddressModes();

            filterAttribute = CreateEnumAttribute( textureBaseType, "filter", samplerFilters, SamplerPropertyCategory, "Filter", "Filtering method to use when sampling a texture" );
            uAddressModeAttribute = CreateEnumAttribute( textureBaseType, "uAddressMode", samplerAddressModes, SamplerPropertyCategory, "U Address Mode", "Method to use for resolving a U texture coordinate that is outside the 0 to 1 range" );
            vAddressModeAttribute = CreateEnumAttribute( textureBaseType, "vAddressMode", samplerAddressModes, SamplerPropertyCategory, "V Address Mode", "Method to use for resolving a V texture coordinate that is outside the 0 to 1 range" );
            wAddressModeAttribute = CreateEnumAttribute( textureBaseType, "wAddressMode", samplerAddressModes, SamplerPropertyCategory, "W Address Mode", "Method to use for resolving a W texture coordinate that is outside the 0 to 1 range" );

            minLodAttribute = CreateBoundedIntAttribute( textureBaseType, "minLod", 0, 0, 20, SamplerPropertyCategory, "Min Lod", "Lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed." );
            maxLodAttribute = CreateBoundedIntAttribute( textureBaseType, "maxLod", 20, 0, 20, SamplerPropertyCategory, "Max Lod", "Upper end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed. This value must be greater than or equal to MinLOD. To have no upper limit on LOD set this to a 20." );
            lodBiasAttribute = CreateBoundedFloatAttribute( textureBaseType, "lodBias", 0, -20, 20, SamplerPropertyCategory, "Lod Bias", "Offset from the calculated mipmap level. For example, if Direct3D calculates that a texture should be sampled at mipmap level 3 and MipLODBias is 2, then the texture will be sampled at mipmap level 5." );

            maxAnisotropyAttribute = CreateEnumAttribute( textureBaseType, "maxAnisotropy", ShaderTypes.GetAnisotropyModes(), SamplerPropertyCategory, "Max Anisotropy", "" );
        }

        protected Sampler CreateSampler()
        {
            Sampler s = new Sampler();

            s.UAddressMode = ShaderTypes.ToSamplerAddressMode( (string)DomNode.GetAttribute( uAddressModeAttribute ) );
            s.VAddressMode = ShaderTypes.ToSamplerAddressMode( (string)DomNode.GetAttribute( vAddressModeAttribute ) );
            s.WAddressMode = ShaderTypes.ToSamplerAddressMode( (string)DomNode.GetAttribute( wAddressModeAttribute ) );
            s.Filter = ShaderTypes.ToSamplerFilterMode( (string)DomNode.GetAttribute( filterAttribute ) );
            int minLod = (int)DomNode.GetAttribute( minLodAttribute );
            int maxLod = (int)DomNode.GetAttribute( maxLodAttribute );
            maxLod = MathUtil.Max<int>( minLod, maxLod );
            s.MinLod = minLod;
            s.MaxLod = maxLod;
            s.LodBias = (float)DomNode.GetAttribute( lodBiasAttribute );
            s.MaxAnisotropy = ShaderTypes.ToSamplerAnisotropyMode( (string)DomNode.GetAttribute( maxAnisotropyAttribute ) );

            return s;
        }

        public override bool DoesRequireRecompile( AttributeInfo attr )
        {
            if ( attr.Equals( filterAttribute )
                || attr.Equals( uAddressModeAttribute )
                || attr.Equals( vAddressModeAttribute )
                || attr.Equals( wAddressModeAttribute )
                || attr.Equals( minLodAttribute )
                || attr.Equals( maxLodAttribute )
                || attr.Equals( lodBiasAttribute )
                || attr.Equals( maxAnisotropyAttribute )
                )
                return true;

            return base.DoesRequireRecompile( attr );
        }
    }
}



