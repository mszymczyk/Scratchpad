using System.ComponentModel;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a AddModule</summary>
    public class AddModule : MaterialGraphModuleAdapter
    {
        public static DomNodeType addType;
        public static MaterialGraphPin pinOut_RGBA;
        public static MaterialGraphPin pinOut_Red;
        public static MaterialGraphPin pinOut_Green;
        public static MaterialGraphPin pinOut_Blue;
        public static MaterialGraphPin pinOut_Alpha;

        public static MaterialGraphPin pinIn_A_RGBA;
        public static MaterialGraphPin pinIn_B_RGBA;

        public static void DefineDomNodeType()
        {
            pinOut_RGBA = new MaterialGraphPin( "RGBA".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.RGBA );
            pinOut_Red = new MaterialGraphPin( "R".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 1, MaterialGraphPin.ComponentType.Red, pinOut_RGBA );
            pinOut_Green = new MaterialGraphPin( "G".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 2, MaterialGraphPin.ComponentType.Green, pinOut_RGBA );
            pinOut_Blue = new MaterialGraphPin( "B".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 3, MaterialGraphPin.ComponentType.Blue, pinOut_RGBA );
            pinOut_Alpha = new MaterialGraphPin( "A".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 4, MaterialGraphPin.ComponentType.Alpha, pinOut_RGBA );

            pinIn_A_RGBA = MaterialGraphPin.CreateMaterialGraphInputPin( "A", 0, MaterialGraphPin.ComponentType.RGBA );
            pinIn_B_RGBA = MaterialGraphPin.CreateMaterialGraphInputPin( "B", 1, MaterialGraphPin.ComponentType.RGBA );

            string prettyName = "Add".Localize();

            DomNodeType dnt = DefineModuleType(
                new XmlQualifiedName( "addType", Schema.NS ),
                prettyName,
                prettyName,
                Resources.AdditionImage,
                new MaterialGraphPin[]
                {
                    pinIn_A_RGBA,
                    pinIn_B_RGBA,
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

            addType = dnt;
            addType.Define( new ExtensionInfo<AddModule>() );
        }

        public override string EvaluatePin( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            if ( pin == pinOut_RGBA )
            {
                string inRGBA_1 = EvaluateSubgraph( pinIn_A_RGBA, sourceCode );
                string inRGBA_2 = EvaluateSubgraph( pinIn_B_RGBA, sourceCode );

                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = " );
                sourceCode.ComputedValuesEval.Append( inRGBA_1 );
                sourceCode.ComputedValuesEval.Append( " + " );
                sourceCode.ComputedValuesEval.Append( inRGBA_2 );
                sourceCode.ComputedValuesEval.AppendLine( ";" );

                return outputVarName;
            }
            else if ( pin == pinIn_A_RGBA )
            {
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = float4( 0, 0, 0, 0 );" );
                sourceCode.ComputedValuesEval.AppendLine();

                return outputVarName;
            }
            else if ( pin == pinIn_B_RGBA )
            {
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = float4( 0, 0, 0, 0 );" );
                sourceCode.ComputedValuesEval.AppendLine();

                return outputVarName;
            }

            throw new MaterialEvaluationException( this, pin, sourceCode );
        }
    }
}



