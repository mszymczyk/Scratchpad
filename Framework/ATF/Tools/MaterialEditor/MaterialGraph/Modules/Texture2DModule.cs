using System.ComponentModel;
using System.Drawing;
using System.Xml;
using System.IO;
using Sce.Atf;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using System;
using Sce.Atf.Direct2D;
using Sce.Atf.Controls.Adaptable;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a Texture2DModule</summary>
    public class Texture2DModule : TextureBaseModule, ID2dCircuitElementRenderer
    {
        public static DomNodeType texture2DType;
        public static AttributeInfo filenameAttribute;

        public static MaterialGraphPin pinIn_UV_XY;

        private static D2dDiagramTheme m_theme;

        public Uri Filename
        {
            get { return (Uri)DomNode.GetAttribute( filenameAttribute ); }
            set { DomNode.SetAttribute( filenameAttribute, value ); }
        }

        public static void DefineDomNodeType( D2dDiagramTheme theme )
        {
            DefineTextureBaseDomNodeType();

            m_theme = theme;

            pinIn_UV_XY = MaterialGraphPin.CreateMaterialGraphInputPin( "UV", 0, MaterialGraphPin.ComponentType.RG );

            string prettyName = "Texture2D".Localize();

            DomNodeType dnt = DefineModuleType2(
                new XmlQualifiedName( "texture2DType", Schema.NS ),
                prettyName,
                prettyName,
                Resources.LightImage,
                new Size( 64, 64 ),
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

            texture2DType = dnt;
            texture2DType.Define( new ExtensionInfo<Texture2DModule>() );

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

        public void DrawInterior( ICircuitElement element, D2dGraphics g, RectangleF bounds )
        {
            D2dBitmap bitMap = null;

            if ( m_image != null )
            {
                bitMap = m_theme.GetBitmap( m_image );
                if ( bitMap == null )
                {
                    m_theme.RegisterBitmap( m_image, m_image );
                    bitMap = m_theme.GetBitmap( m_image );
                }
            }

            if ( bitMap != null )
                g.DrawBitmap( bitMap, bounds, 1, D2dBitmapInterpolationMode.Linear );
            else
                g.FillRectangle( bounds, Color.Red );

        }

        /// <summary>
        /// Performs one-time initialization when this adapter's DomNode property is set.
        /// The DomNode property is only ever set once for the lifetime of this adapter.</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += DomNode_AttributeChanged;

            ReloadImage();
        }

        private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
        {
            if ( e.AttributeInfo.Equivalent( filenameAttribute ) )
            {
                ReloadImage();
                //m_image = new Image
            }
        }

        private void ReloadImage()
        {
            if ( m_image != null )
            {
                
            }

            Texture2DModule t2d = DomNode.As<Texture2DModule>();
            Uri uri = t2d.Filename;
            if ( uri != null )
            {
                string filename = uri.LocalPath;
                string dir = Path.GetDirectoryName( filename );
                string file = Path.GetFileNameWithoutExtension( filename );
                string thumbnailFilename = dir + "\\~" + file + ".png";

                m_image = Image.FromFile( thumbnailFilename );
            }
            else
                m_image = null;
        }

        Image m_image;
    }
}



