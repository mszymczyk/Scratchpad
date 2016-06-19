using System.ComponentModel;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a PowerModule</summary>
    public class PowerModule : MaterialGraphModuleAdapter
    {
        public static DomNodeType powerType;
        public static MaterialGraphPin pinOut_RGBA;
        public static MaterialGraphPin pinOut_Red;
        public static MaterialGraphPin pinOut_Green;
        public static MaterialGraphPin pinOut_Blue;
        public static MaterialGraphPin pinOut_Alpha;

        public static MaterialGraphPin pinIn_Base_RGBA;
        public static MaterialGraphPin pinIn_Exponent_RGBA;

        public static void DefineDomNodeType()
        {
            pinOut_RGBA = new MaterialGraphPin( "RGBA".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.RGBA );
            pinOut_Red = new MaterialGraphPin( "R".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 1, MaterialGraphPin.ComponentType.Red, pinOut_RGBA );
            pinOut_Green = new MaterialGraphPin( "G".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 2, MaterialGraphPin.ComponentType.Green, pinOut_RGBA );
            pinOut_Blue = new MaterialGraphPin( "B".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 3, MaterialGraphPin.ComponentType.Blue, pinOut_RGBA );
            pinOut_Alpha = new MaterialGraphPin( "A".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 4, MaterialGraphPin.ComponentType.Alpha, pinOut_RGBA );

            pinIn_Base_RGBA = MaterialGraphPin.CreateMaterialGraphInputPin( "Base", 0, MaterialGraphPin.ComponentType.RGBA );
            pinIn_Exponent_RGBA = MaterialGraphPin.CreateMaterialGraphInputPin( "Exponent", 1, MaterialGraphPin.ComponentType.RGBA );

            string prettyName = "Power".Localize();

            DomNodeType dnt = DefineModuleType(
                new XmlQualifiedName( "powerType", Schema.NS ),
                prettyName,
                prettyName,
                Resources.AndImage,
                new MaterialGraphPin[]
                {
                    pinIn_Base_RGBA,
                    pinIn_Exponent_RGBA,
                },
                new MaterialGraphPin[]
                {
                    pinOut_RGBA,
                    pinOut_Red,
                    pinOut_Green,
                    pinOut_Blue,
                    pinOut_Alpha,
                },
                "Math" );

            powerType = dnt;
            powerType.Define( new ExtensionInfo<PowerModule>() );
        }

        public override string EvaluatePin( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            if ( pin == pinOut_RGBA )
            {
                string inRGBA_1 = EvaluateSubgraph( pinIn_Base_RGBA, sourceCode );
                string inRGBA_2 = EvaluateSubgraph( pinIn_Exponent_RGBA, sourceCode );

                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = pow( " );
                sourceCode.ComputedValuesEval.Append( inRGBA_1 );
                sourceCode.ComputedValuesEval.Append( ", " );
                sourceCode.ComputedValuesEval.Append( inRGBA_2 );
                sourceCode.ComputedValuesEval.AppendLine( " );" );

                return outputVarName;
            }
            else if ( pin == pinIn_Base_RGBA )
            {
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = float4( 1, 1, 1, 1 );" );
                sourceCode.ComputedValuesEval.AppendLine();

                return outputVarName;
            }
            else if ( pin == pinIn_Exponent_RGBA )
            {
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = float4( 1, 1, 1, 1 );" );
                sourceCode.ComputedValuesEval.AppendLine();

                return outputVarName;
            }

            throw new MaterialEvaluationException( this, pin, sourceCode );
        }
    }
}



