using System.ComponentModel;
using System.Drawing;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a ColorConstantModule</summary>
    public class ColorConstantModule : MaterialGraphModuleAdapter
    {
        public static DomNodeType colorConstantType;
        public static AttributeInfo colorAttribute;
        public static MaterialGraphPin pin_RGB;
        public static MaterialGraphPin pin_Red;
        public static MaterialGraphPin pin_Green;
        public static MaterialGraphPin pin_Blue;

        public static void DefineDomNodeType()
        {
            pin_RGB = new MaterialGraphPin("RGB".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.RGB );
            pin_Red = new MaterialGraphPin("R".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 1, MaterialGraphPin.ComponentType.Red, pin_RGB );
            pin_Green = new MaterialGraphPin("G".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 2, MaterialGraphPin.ComponentType.Green, pin_RGB );
            pin_Blue = new MaterialGraphPin( "B".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 3, MaterialGraphPin.ComponentType.Blue, pin_RGB );

            string prettyName = "ColorConstant".Localize();

            DomNodeType dnt = DefineModuleType(
                new XmlQualifiedName( "colorConstantType", Schema.NS ),
                prettyName,
                prettyName,
                Resources.LightImage,
                EmptyArray<ElementType.Pin>.Instance,
                new MaterialGraphPin[]
                {
                    pin_RGB,
                    pin_Red,
                    pin_Green,
                    pin_Blue
                },
                "Constants" );

            colorConstantType = dnt;
            colorConstantType.Define( new ExtensionInfo<ColorConstantModule>() );

            colorAttribute = new AttributeInfo( "color", AttributeType.IntType );
            colorAttribute.DefaultValue = -1;
            dnt.Define( colorAttribute );

            dnt.SetTag(
                new PropertyDescriptorCollection(
                    new Sce.Atf.Dom.PropertyDescriptor[] {
                        new AttributePropertyDescriptor(
                            "Color".Localize(),
                            colorAttribute,
                            prettyName, //category
                            "Color".Localize(), //description
                            false,
                            new Sce.Atf.Controls.PropertyEditing.ColorPickerEditor(),
                            new Sce.Atf.Controls.PropertyEditing.IntColorConverter()
                            ) //is not read-only
                    } ) );

        }

        public override string EvaluatePin( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            if ( pin == pin_RGB )
            {
                //string outputVarName = ShaderSourceCode.GetVariableName( this, pin );
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                //sourceCode.ComputedValuesEval.Append( " = float3( 1.0f, 1.0f, 1.0f );" );
                float r, g, b;
                pico.picoColor.ConvertColor( Color, 1.0f, out r, out g, out b );
                sourceCode.ComputedValuesEval.Append( string.Format( " = float3( {0}, {1}, {2} );", r, g, b ) );
                sourceCode.ComputedValuesEval.AppendLine();

                return outputVarName;
            }

            throw new MaterialEvaluationException( this, pin, sourceCode );
        }

        /// <summary>
        /// Gets and sets the module's color</summary>
        public Color Color
        {
            get { return Color.FromArgb( (int)DomNode.GetAttribute( colorAttribute ) ); }
            set { DomNode.SetAttribute( colorAttribute, value.ToArgb() ); }
        }

        public override bool DoesRequireRecompile( AttributeInfo attr )
        {
            if ( attr.Equals( colorAttribute ) )
                return true;

            return false;
        }
    }
}



