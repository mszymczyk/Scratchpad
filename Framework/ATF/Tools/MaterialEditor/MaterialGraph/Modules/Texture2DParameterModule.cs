using System.ComponentModel;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Dom;
using System;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a Texture2DParameterModule</summary>
    public class Texture2DParameterModule : TextureBaseModule, IMaterialParameterModule
    {
        public static DomNodeType texture2DParameterType;
        public static AttributeInfo filenameAttribute;

        public static MaterialGraphPin pinIn_UV_XY;

        /// <summary>
        /// Gets and sets the parameter's color</summary>
        public Uri Filename
        {
            get { return (Uri)DomNode.GetAttribute( filenameAttribute ); }
            set { DomNode.SetAttribute( filenameAttribute, value ); }
        }

        public static void DefineDomNodeType()
        {
            DefineTextureBaseDomNodeType();

            pinIn_UV_XY = MaterialGraphPin.CreateMaterialGraphInputPin( "UV", 0, MaterialGraphPin.ComponentType.RG );

            string prettyName = "Texture2DParameter".Localize();

            DomNodeType dnt = DefineModuleType(
                new XmlQualifiedName( "texture2DParameterType", Schema.NS ),
                prettyName,
                prettyName,
                Resources.LightImage,
                new MaterialGraphPin[]
                {
                    pinIn_UV_XY
                },
                new MaterialGraphPin[]
                {
                    pinOut_RGBA,
                    pinOut_Red,
                    pinOut_Green,
                    pinOut_Blue,
                    pinOut_Alpha
                },
                TexturePaletteCategory,
                TextureBaseModule.textureBaseType
                );

            texture2DParameterType = dnt;
            texture2DParameterType.Define( new ExtensionInfo<Texture2DParameterModule>() );

            PropertyDescriptorCollection pdc = new PropertyDescriptorCollection( null );
            dnt.SetTag( pdc );

            filenameAttribute = CreateUriAttribute( dnt, "filename", "TextureType file (*.dds)|*.dds", prettyName, "Filename", "2D TextureType's Filename" );
        }

        public override string EvaluatePin( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            if ( pin == pinOut_RGBA )
            {
                string uvVarName = EvaluateSubgraph( pinIn_UV_XY, sourceCode );
                Sampler s = CreateSampler();
                string samplerName = sourceCode.AddSampler( Id, s );
                string textureName = sourceCode.AddTexture2D( Id );
                string outputVarName = sourceCode.AddComputedValue( this, pin );

                sourceCode.ComputedValuesEval.Append( outputVarName + " = " + textureName + ".Sample( " + samplerName );
                sourceCode.ComputedValuesEval.Append( ", " + uvVarName + " );" );
                sourceCode.ComputedValuesEval.AppendLine();

                return outputVarName;
            }
            else if ( pin == pinIn_UV_XY )
            {
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.AppendLine( outputVarName + " = mp.texCoord0.xy;" );
                return outputVarName;
            }

            throw new MaterialEvaluationException( this, pin, sourceCode );
        }

        public override bool DoesRequireRefresh( AttributeInfo attr )
        {
            if ( attr.Equals( filenameAttribute ) )
                return true;

            return base.DoesRequireRefresh( attr );
        }
    }
}



