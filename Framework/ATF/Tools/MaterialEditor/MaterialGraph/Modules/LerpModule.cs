using System.ComponentModel;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a LerpModule</summary>
    public class LerpModule : MaterialGraphModuleAdapter
    {
        public static DomNodeType lerpType;
        public static MaterialGraphPin pinOut_RGBA;
        public static MaterialGraphPin pinOut_Red;
        public static MaterialGraphPin pinOut_Green;
        public static MaterialGraphPin pinOut_Blue;
        public static MaterialGraphPin pinOut_Alpha;

        public static MaterialGraphPin pinIn_RGBA_1;
        public static MaterialGraphPin pinIn_RGBA_2;
        public static MaterialGraphPin pinIn_T;

        public static void DefineDomNodeType()
        {
            pinOut_RGBA = new MaterialGraphPin( "RGBA".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.RGBA );
            pinOut_Red = new MaterialGraphPin( "R".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 1, MaterialGraphPin.ComponentType.Red, pinOut_RGBA );
            pinOut_Green = new MaterialGraphPin( "G".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 2, MaterialGraphPin.ComponentType.Green, pinOut_RGBA );
            pinOut_Blue = new MaterialGraphPin( "B".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 3, MaterialGraphPin.ComponentType.Blue, pinOut_RGBA );
            pinOut_Alpha = new MaterialGraphPin( "A".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 4, MaterialGraphPin.ComponentType.Alpha, pinOut_RGBA );

            pinIn_RGBA_1 = new MaterialGraphPin( "RGBA_1".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.RGBA );
            pinIn_RGBA_2 = new MaterialGraphPin( "RGBA_2".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 1, MaterialGraphPin.ComponentType.RGBA );
            pinIn_T = new MaterialGraphPin( "T".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 2, MaterialGraphPin.ComponentType.Red );

            string prettyName = "Lerp".Localize();

            DomNodeType dnt = DefineModuleType(
                new XmlQualifiedName( "lerpType", Schema.NS ),
                prettyName,
                prettyName,
                Resources.LightImage,
                new MaterialGraphPin[]
                {
                    pinIn_RGBA_1,
                    pinIn_RGBA_2,
                    pinIn_T
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

            lerpType = dnt;
            lerpType.Define( new ExtensionInfo<LerpModule>() );
        }

        public override string EvaluatePin( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            if ( pin == pinOut_RGBA )
            {
                string inRGBA_1 = EvaluateSubgraph( pinIn_RGBA_1, sourceCode );
                string inRGBA_2 = EvaluateSubgraph( pinIn_RGBA_2, sourceCode );
                string inT = EvaluateSubgraph( pinIn_T, sourceCode );

                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = lerp( " );
                sourceCode.ComputedValuesEval.Append( inRGBA_1 );
                sourceCode.ComputedValuesEval.Append( ", " );
                sourceCode.ComputedValuesEval.Append( inRGBA_2 );
                sourceCode.ComputedValuesEval.Append( ", " );
                sourceCode.ComputedValuesEval.Append( inT );
                sourceCode.ComputedValuesEval.AppendLine( " );" );

                return outputVarName;
            }
            else if ( pin == pinIn_RGBA_1 )
            {
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = float4( 0, 0, 0, 0 );" );
                sourceCode.ComputedValuesEval.AppendLine();

                return outputVarName;
            }
            else if ( pin == pinIn_RGBA_2 )
            {
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = float4( 1, 1, 1, 1 );" );
                sourceCode.ComputedValuesEval.AppendLine();

                return outputVarName;
            }
            else if ( pin == pinIn_T )
            {
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = 0.5f;" );
                sourceCode.ComputedValuesEval.AppendLine();

                return outputVarName;
            }

            throw new MaterialEvaluationException( this, pin, sourceCode );
        }
    }
}



