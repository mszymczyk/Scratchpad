using System.ComponentModel;
using System.Drawing;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a ColorParameterModule</summary>
    public class FloatParameterModule : MaterialGraphModuleAdapter, IMaterialParameterModule
    {
        public static DomNodeType colorParameterType;
        public static AttributeInfo valueAttribute;
        public static MaterialGraphPin pin_Red;

        public float FloatValue
        {
            get { return (float)DomNode.GetAttribute( valueAttribute ); }
            set { DomNode.SetAttribute( valueAttribute, value ); }
        }

        public static void DefineDomNodeType()
        {
            pin_Red = new MaterialGraphPin("R".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.Red );

            string prettyName = "FloatParameter".Localize();

            DomNodeType dnt = DefineModuleType(
                new XmlQualifiedName( "floatParameterType", Schema.NS ),
                prettyName,
                prettyName,
                Resources.LightImage,
                EmptyArray<ElementType.Pin>.Instance,
                new MaterialGraphPin[]
                {
                    pin_Red,
                },
                "Parameters" );

            colorParameterType = dnt;
            colorParameterType.Define( new ExtensionInfo<FloatParameterModule>() );

            valueAttribute = new AttributeInfo( "value", AttributeType.FloatType );
            valueAttribute.DefaultValue = 0.0f;
            dnt.Define( valueAttribute );

            dnt.SetTag(
                new PropertyDescriptorCollection(
                    new Sce.Atf.Dom.PropertyDescriptor[] {
                        new AttributePropertyDescriptor(
                            "FloatValue".Localize(),
                            valueAttribute,
                            prettyName, //category
                            "FloatValue".Localize(), //description
                            false,
                            new Sce.Atf.Controls.PropertyEditing.NumericEditor()
                            ) //is not read-only
                    } ) );

        }

        public override string EvaluatePin( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            if ( pin == pin_Red )
            {
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                string uniformName = sourceCode.AddUniformParameter( Id, UniformType.Float );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                //float floatVal = (float)DomNode.GetAttribute( valueAttribute );
                sourceCode.ComputedValuesEval.Append( " = " + uniformName + ";" );
                sourceCode.ComputedValuesEval.AppendLine();

                return outputVarName;
            }

            throw new MaterialEvaluationException( this, pin, sourceCode );
        }

        ///// <summary>
        ///// Gets and sets the module's color</summary>
        //public Color Color
        //{
        //    get { return Color.FromArgb( (int)DomNode.GetAttribute( colorAttribute ) ); }
        //    set { DomNode.SetAttribute( colorAttribute, value.ToArgb() ); }
        //}

        public override bool DoesRequireRefresh( AttributeInfo attr )
        {
            if ( attr.Equals( valueAttribute ) )
                return true;

            return false;
        }
    }
}



