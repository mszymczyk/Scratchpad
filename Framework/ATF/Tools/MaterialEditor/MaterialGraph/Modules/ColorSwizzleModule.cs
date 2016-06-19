using System.ComponentModel;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a SwizzleModule</summary>
    public class SwizzleModule : MaterialGraphModuleAdapter
    {
        public static DomNodeType swizzleType;
        public static MaterialGraphPin pinOut_RGBA;
        public static MaterialGraphPin pinIn_Red;
        public static MaterialGraphPin pinIn_Green;
        public static MaterialGraphPin pinIn_Blue;
        public static MaterialGraphPin pinIn_Alpha;

        public static void DefineDomNodeType()
        {
            pinOut_RGBA = new MaterialGraphPin( "RGBA".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.RGBA );
            pinIn_Red = new MaterialGraphPin( "R".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.Red );
            pinIn_Green = new MaterialGraphPin( "G".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 1, MaterialGraphPin.ComponentType.Green );
            pinIn_Blue = new MaterialGraphPin( "B".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 2, MaterialGraphPin.ComponentType.Blue );
            pinIn_Alpha = new MaterialGraphPin( "A".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 3, MaterialGraphPin.ComponentType.Alpha );

            string prettyName = "Swizzle".Localize();

            DomNodeType dnt = DefineModuleType(
                new XmlQualifiedName( "swizzleType", Schema.NS ),
                prettyName,
                prettyName,
                Resources.LightImage,
                new MaterialGraphPin[]
                {
                    pinIn_Red,
                    pinIn_Green,
                    pinIn_Blue,
                    pinIn_Alpha
                },
                new MaterialGraphPin[]
                {
                    pinOut_RGBA
                },
                "Util" );

            swizzleType = dnt;
            swizzleType.Define( new ExtensionInfo<SwizzleModule>() );

            //var colorAttribute = new AttributeInfo( "color".Localize(), AttributeType.IntType );
            //dnt.Define( colorAttribute );

            //dnt.SetTag(
            //    new PropertyDescriptorCollection(
            //        new Sce.Atf.Dom.PropertyDescriptor[] {
            //            new AttributePropertyDescriptor(
            //                "Color".Localize(),
            //                colorAttribute,
            //                prettyName, //category
            //                "Color".Localize(), //description
            //                false,
            //                new Sce.Atf.Controls.PropertyEditing.ColorPickerEditor(),
            //                new Sce.Atf.Controls.PropertyEditing.IntColorConverter()
            //                ) //is not read-only
            //        } ) );

        }


        public override string EvaluatePin( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            if ( pin == pinOut_RGBA )
            {
                string inRedVarName = EvaluateSubgraph( pinIn_Red, sourceCode );
                string inGreenVarName = EvaluateSubgraph( pinIn_Green, sourceCode );
                string inBlueVarName = EvaluateSubgraph( pinIn_Blue, sourceCode );
                string inAlphaVarName = EvaluateSubgraph( pinIn_Alpha, sourceCode );

                //string inRedVarName = ShaderSourceCode.GetVariableName( this, pinIn_Red );
                //string inGreenVarName = ShaderSourceCode.GetVariableName( this, pinIn_Green );
                //string inBlueVarName = ShaderSourceCode.GetVariableName( this, pinIn_Blue );

                //string outputVarName = ShaderSourceCode.GetVariableName( this, pin );
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                //sourceCode.ComputedValuesEval.Append( " = float3( 1.0f, 1.0f, 1.0f );" );
                sourceCode.ComputedValuesEval.Append( " = float4( " );
                sourceCode.ComputedValuesEval.Append( inRedVarName );
                sourceCode.ComputedValuesEval.Append( ", " );
                sourceCode.ComputedValuesEval.Append( inGreenVarName );
                sourceCode.ComputedValuesEval.Append( ", " );
                sourceCode.ComputedValuesEval.Append( inBlueVarName );
                sourceCode.ComputedValuesEval.Append( ", " );
                sourceCode.ComputedValuesEval.Append( inAlphaVarName );
                sourceCode.ComputedValuesEval.AppendLine( " );" );

                return outputVarName;
            }
            else if ( pin == pinIn_Red || pin == pinIn_Green || pin == pinIn_Blue || pin == pinIn_Alpha )
            {
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.AppendLine( outputVarName + " = 0;" );
                return outputVarName;
            }

            throw new MaterialEvaluationException( this, pin, sourceCode );
        }

    }
}



