using System.ComponentModel;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a LevelOfDetailModule</summary>
    public class LevelOfDetailModule : MaterialGraphModuleAdapter
    {
        public static DomNodeType levelOfDetailType;
        public static MaterialGraphPin pinOut_RGBA;
        public static MaterialGraphPin pinOut_Red;
        public static MaterialGraphPin pinOut_Green;
        public static MaterialGraphPin pinOut_Blue;
        public static MaterialGraphPin pinOut_Alpha;

        public static MaterialGraphPin pinIn_RGBA_Lod0;
        public static MaterialGraphPin pinIn_RGBA_Lod1;
        public static MaterialGraphPin pinIn_RGBA_Lod2;
        public static MaterialGraphPin pinIn_RGBA_Lod3;

        public static void DefineDomNodeType()
        {
            pinOut_RGBA = new MaterialGraphPin( "RGBA".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.RGBA );
            pinOut_Red = new MaterialGraphPin( "R".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 1, MaterialGraphPin.ComponentType.Red, pinOut_RGBA );
            pinOut_Green = new MaterialGraphPin( "G".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 2, MaterialGraphPin.ComponentType.Green, pinOut_RGBA );
            pinOut_Blue = new MaterialGraphPin( "B".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 3, MaterialGraphPin.ComponentType.Blue, pinOut_RGBA );
            pinOut_Alpha = new MaterialGraphPin( "A".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 4, MaterialGraphPin.ComponentType.Alpha, pinOut_RGBA );

            pinIn_RGBA_Lod0 = MaterialGraphPin.CreateMaterialGraphInputPin( "Lod0", 0, MaterialGraphPin.ComponentType.RGBA );
            pinIn_RGBA_Lod1 = MaterialGraphPin.CreateMaterialGraphInputPin( "Lod1", 1, MaterialGraphPin.ComponentType.RGBA );
            pinIn_RGBA_Lod2 = MaterialGraphPin.CreateMaterialGraphInputPin( "Lod2", 2, MaterialGraphPin.ComponentType.RGBA );
            pinIn_RGBA_Lod3 = MaterialGraphPin.CreateMaterialGraphInputPin( "Lod3", 3, MaterialGraphPin.ComponentType.RGBA );

            string prettyName = "LevelOfDetail".Localize();

            DomNodeType dnt = DefineModuleType(
                new XmlQualifiedName( "levelOfDetailType", Schema.NS ),
                prettyName,
                prettyName,
                Resources.LightImage,
                new MaterialGraphPin[]
                {
                    pinIn_RGBA_Lod0,
                    pinIn_RGBA_Lod1,
                    pinIn_RGBA_Lod2,
                    pinIn_RGBA_Lod3
                },
                new MaterialGraphPin[]
                {
                    pinOut_RGBA,
                    pinOut_Red,
                    pinOut_Green,
                    pinOut_Blue,
                    pinOut_Alpha,
                },
                "Util" );

            levelOfDetailType = dnt;
            levelOfDetailType.Define( new ExtensionInfo<LevelOfDetailModule>() );
        }

        public override string EvaluatePin( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            if ( pin == pinOut_RGBA )
            {
                string inGraph;

                // select input based on what level we're compiling
                switch ( sourceCode.CurrentLevelOfDetail )
                {
                    case ShaderSourceCode.LevelOfDetail.Lod0:
                        inGraph = EvaluateSubgraph( pinIn_RGBA_Lod0, sourceCode );
                        break;
                    case ShaderSourceCode.LevelOfDetail.Lod1:
                        inGraph = EvaluateSubgraph( pinIn_RGBA_Lod1, sourceCode );
                        break;
                    case ShaderSourceCode.LevelOfDetail.Lod2:
                        inGraph = EvaluateSubgraph( pinIn_RGBA_Lod2, sourceCode );
                        break;
                    case ShaderSourceCode.LevelOfDetail.Lod3:
                        inGraph = EvaluateSubgraph( pinIn_RGBA_Lod3, sourceCode );
                        break;
                    default:
                        throw new MaterialEvaluationException( this, pin, sourceCode );
                }

                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = " + inGraph + " );" );

                return outputVarName;
            }
            else if ( pin == pinIn_RGBA_Lod0 )
            {
                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = float4( 1, 0, 0, 1 );" );
                sourceCode.ComputedValuesEval.AppendLine();

                return outputVarName;
            }
            else if ( pin == pinIn_RGBA_Lod1 || pin == pinIn_RGBA_Lod2 || pin == pinIn_RGBA_Lod3 )
            {
                return EvaluateSubgraph( pinIn_RGBA_Lod0, sourceCode );
            }

            throw new MaterialEvaluationException( this, pin, sourceCode );
        }
    }
}



