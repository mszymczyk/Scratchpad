using System;
using System.ComponentModel;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a UserFunctionModule</summary>
    public class UserFunctionModule : MaterialGraphModuleAdapter, ISourceCode
    {
        public static DomNodeType userFunctionType;
        public static MaterialGraphPin pinOut_RGBA;
        public static MaterialGraphPin pinOut_Red;
        public static MaterialGraphPin pinOut_Green;
        public static MaterialGraphPin pinOut_Blue;
        public static MaterialGraphPin pinOut_Alpha;

        public static MaterialGraphPin pinIn_A_RGBA;
        public static MaterialGraphPin pinIn_B_RGBA;
        public static MaterialGraphPin pinIn_C_RGBA;

        public static AttributeInfo functionSourceCodeAttribute;

        public static void DefineDomNodeType()
        {
            pinOut_RGBA = new MaterialGraphPin( "RGBA".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.RGBA );
            pinOut_Red = new MaterialGraphPin( "R".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 1, MaterialGraphPin.ComponentType.Red, pinOut_RGBA );
            pinOut_Green = new MaterialGraphPin( "G".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 2, MaterialGraphPin.ComponentType.Green, pinOut_RGBA );
            pinOut_Blue = new MaterialGraphPin( "B".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 3, MaterialGraphPin.ComponentType.Blue, pinOut_RGBA );
            pinOut_Alpha = new MaterialGraphPin( "A".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 4, MaterialGraphPin.ComponentType.Alpha, pinOut_RGBA );

            pinIn_A_RGBA = MaterialGraphPin.CreateMaterialGraphInputPin( "A", 0, MaterialGraphPin.ComponentType.RGBA );
            pinIn_B_RGBA = MaterialGraphPin.CreateMaterialGraphInputPin( "B", 1, MaterialGraphPin.ComponentType.RGBA );
            pinIn_C_RGBA = MaterialGraphPin.CreateMaterialGraphInputPin( "C", 2, MaterialGraphPin.ComponentType.RGBA );

            string prettyName = "UserFunction".Localize();

            DomNodeType dnt = DefineModuleType(
                new XmlQualifiedName( "userFunctionType", Schema.NS ),
                prettyName,
                prettyName,
                Resources.LightImage,
                new MaterialGraphPin[]
                {
                    pinIn_A_RGBA,
                    pinIn_B_RGBA,
                    pinIn_C_RGBA,
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

            userFunctionType = dnt;
            userFunctionType.Define( new ExtensionInfo<UserFunctionModule>() );

            functionSourceCodeAttribute = CreateStringAttribute( userFunctionType, "sourceCode", "Source Code", "Source Code", "Body of a user defined function" );
        }

        #region ISourceCode

        string ISourceCode.Name
        {
            get
            {
                return Id;
            }
        }

        string ISourceCode.DisplayName
        {
            get
            {
                return Id;
            }
        }

        string ISourceCode.Text
        {
            get
            {
                return (string)DomNode.GetAttribute( functionSourceCodeAttribute );
            }
            set
            {
                DomNode.SetAttribute( functionSourceCodeAttribute, value );
            }
        }

        #endregion

        public string Text
        {
            get { return (string)DomNode.GetAttribute( functionSourceCodeAttribute ); }
            set { DomNode.SetAttribute( functionSourceCodeAttribute, value ); }
        }

        public override string EvaluatePin( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            if ( pin == pinOut_RGBA )
            {
                string inRGBA_A = EvaluateSubgraph( pinIn_A_RGBA, sourceCode );
                string inRGBA_B = EvaluateSubgraph( pinIn_B_RGBA, sourceCode );
                string inRGBA_C = EvaluateSubgraph( pinIn_C_RGBA, sourceCode );

                // add functionSourceCodeAttribute to material source somehow
                sourceCode.UserFunctionsEval.AppendLine( "float4 " + Id + "( float4 A, float4 B, float4 C )" );
                sourceCode.UserFunctionsEval.AppendLine( "{" );
                sourceCode.UserFunctionsEval.AppendLine( Text );
                sourceCode.UserFunctionsEval.AppendLine( "}" );

                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = " );
                sourceCode.ComputedValuesEval.Append( Id + "( " );
                sourceCode.ComputedValuesEval.Append( inRGBA_A );
                sourceCode.ComputedValuesEval.Append( ", " );
                sourceCode.ComputedValuesEval.Append( inRGBA_B );
                sourceCode.ComputedValuesEval.Append( ", " );
                sourceCode.ComputedValuesEval.Append( inRGBA_C );
                sourceCode.ComputedValuesEval.AppendLine( " );" );

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
            else if ( pin == pinIn_C_RGBA )
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



