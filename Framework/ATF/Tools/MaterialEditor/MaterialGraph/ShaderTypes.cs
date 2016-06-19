using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Adaptation;
using System.Text;
using System;
using System.IO;

namespace CircuitEditorSample
{
    public enum UniformType
    {
        Float,
        Float2,
        Float3,
        Float4,
        Float3x3,
        Float4x4,
        Int
    }

    public enum TextureType
    {
        Texture1D,
        Texture2D,
        Texture3D,
    }

    public enum SamplerFilterMode
    {
        Trilinear,
        Bilinear,
        Nearest,
    }

    public enum SamplerAddressMode
    {
        ClampToEdge,
        Repeat,
        Mirror,
    }

    public enum SamplerAnisotropyMode
    {
        Aniso_1x,
        Aniso_2x,
        Aniso_4x,
        Aniso_8x,
        Aniso_16x,
    }

    public class Sampler
    {
        public SamplerFilterMode Filter = SamplerFilterMode.Trilinear;
        public SamplerAddressMode UAddressMode = SamplerAddressMode.ClampToEdge;
        public SamplerAddressMode VAddressMode = SamplerAddressMode.ClampToEdge;
        public SamplerAddressMode WAddressMode = SamplerAddressMode.ClampToEdge;
        public int MinLod = 0;
        public int MaxLod = 20;
        public float LodBias = 0;
        public SamplerAnisotropyMode MaxAnisotropy = SamplerAnisotropyMode.Aniso_1x;
    }

    public class ShaderTypes
    {
        public static string GetUniformTypeName( UniformType uniformType )
        {
            switch ( uniformType )
            {
                case UniformType.Float:
                    return "float";
                case UniformType.Float2:
                    return "float2";
                case UniformType.Float3:
                    return "float3";
                case UniformType.Float4:
                    return "float4";
                case UniformType.Float3x3:
                    return "float3x3";
                case UniformType.Float4x4:
                    return "float4x4";
                case UniformType.Int:
                    return "int";

                default:
                    throw new ArgumentException( "Unsupported type: " + uniformType.ToString() );
            }
        }

        public static string GetTextureTypeName( TextureType textureType )
        {
            switch ( textureType )
            {
                case TextureType.Texture1D:
                    return "Texture1D";
                case TextureType.Texture2D:
                    return "Texture2D";
                case TextureType.Texture3D:
                    return "Texture3D";

                default:
                    throw new ArgumentException( "Unsupported type: " + textureType.ToString() );
            }
        }

        public static string[] GetSamplerAddressModes()
        {
            return Enum.GetNames( typeof( SamplerAddressMode ) );
        }

        public static SamplerAddressMode ToSamplerAddressMode( string mode )
        {
            SamplerAddressMode e;
            if ( Enum.TryParse<SamplerAddressMode>( mode, out e ) )
            {
                return e;
            }

            throw new InvalidCastException( string.Format( "Cannot parse {0} as SamplerAddressMode", mode ) );
        }

        public static string[] GetSamplerFilterModes()
        {
            return Enum.GetNames( typeof( SamplerFilterMode ) );
        }

        public static SamplerFilterMode ToSamplerFilterMode( string mode )
        {
            SamplerFilterMode e;
            if ( Enum.TryParse<SamplerFilterMode>( mode, out e ) )
            {
                return e;
            }

            throw new InvalidCastException( string.Format( "Cannot parse {0} as SamplerFilterMode", mode ) );
        }


        public static SamplerAnisotropyMode ToSamplerAnisotropyMode( string mode )
        {
            SamplerAnisotropyMode e;
            if ( Enum.TryParse<SamplerAnisotropyMode>( mode, out e ) )
            {
                return e;
            }

            throw new InvalidCastException( string.Format( "Cannot parse {0} as SamplerAnisotropyMode", mode ) );
        }

        public static string[] GetAnisotropyModes()
        {
            string[] anisotropyModes = new string[] {
                    "1",
                    "2",
                    "4",
                    "8",
                    "16"
                    };
            return anisotropyModes;
        }

        public static string GetFloatType( MaterialGraphPin pin )
        {
            if ( pin.Component == MaterialGraphPin.ComponentType.Red
                || pin.Component == MaterialGraphPin.ComponentType.Green
                || pin.Component == MaterialGraphPin.ComponentType.Blue
                || pin.Component == MaterialGraphPin.ComponentType.Alpha
                || pin.Component == MaterialGraphPin.ComponentType.Red
                )
                return "float";
            else if ( pin.Component == MaterialGraphPin.ComponentType.RGB )
                return "float3";
            else if ( pin.Component == MaterialGraphPin.ComponentType.RGBA )
                return "float4";
            else if ( pin.Component == MaterialGraphPin.ComponentType.RG )
                return "float2";
            else
                throw new InvalidOperationException( "Unsupported pin component" );
        }
    }
}
