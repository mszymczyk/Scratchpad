using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Adaptation;
using System.Text;
using System.IO;

namespace CircuitEditorSample
{
    /// <summary>
    /// Pin for an ElementType</summary>
    public class MaterialGenerator
    {
        public MaterialGenerator( Circuit circuit )
        {
            m_circuit = circuit;
        }

        public void Generate()
        {
            Module material = null;
            foreach ( Module m in m_circuit.Elements )
                if ( m.DomNode.Type.Equals( MaterialModule.materialType ) )
                {
                    material = m;
                    break;
                }

            if ( material == null )
                return;

            //HashSet<Element> modules = new HashSet<Element>();
            //List<Wire> internalConnections = new List<Wire>();
            //List<Wire> incomingConnections = new List<Wire>();
            //List<Wire> outgoingConnections = new List<Wire>();
            //CircuitUtil.GetSubGraph( m_circuit, new object[] { material }, modules, internalConnections, incomingConnections, outgoingConnections );

            //MaterialGraphPin baseColor = material.InputPin( 0 ) as MaterialGraphPin;
            //Module baseColorSource = GetConnectedModule( material, baseColor );

            ShaderSourceCode sourceCode = new ShaderSourceCode();
            //sourceCode.LoadMaterialTemplate( @"G:\_git\scratchpad\code\shaders\hlsl\MaterialTemplate.hlsl" );
            //sourceCode.LoadMaterialTemplate( @"G:\_Scratchpad\Assets\Materials\MaterialTemplate.hlsl" );
            string templatePath = misz.Paths.DataPathToAbsolutePath("Assets\\Materials\\MaterialTemplate.hlsl");
            sourceCode.LoadMaterialTemplate(templatePath);

            //StringBuilder materialEval = new StringBuilder();
            StringBuilder materialEval = sourceCode.ComputedValuesEval;

            MaterialGraphModuleAdapter materialAdapter = material.As<MaterialGraphModuleAdapter>();
            materialAdapter.EvaluateSubgraph( MaterialModule.pin_BaseColor, sourceCode );
            string baseColorValue = ShaderSourceCode.GetVariableName( materialAdapter, MaterialModule.pin_BaseColor );

            materialEval.AppendLine();
            materialEval.Append( "baseColor = " );
            materialEval.Append( baseColorValue );
            materialEval.AppendLine( ";" );

            string fullSourceCode = sourceCode.Finish();
            System.Diagnostics.Debug.WriteLine( fullSourceCode );

            CircuitDocument document = material.DomNode.GetRoot().As<CircuitDocument>();
            string documentPath = document.Uri.LocalPath;

            //string outFile = @"G:\_Scratchpad\Data\shaders\Material.hlsl";
            string outFile = System.IO.Path.ChangeExtension( documentPath, "hlsl" );
            sourceCode.WriteMaterialToFile( outFile );

            string outFileRelative = misz.Paths.AbsolutePathToDataPath(outFile);

            FxCompiler.FxCompilerInterop.CompileFile(outFileRelative);
        }

        //private Module GetConnectedModule( Module module, MaterialGraphPin pin )
        //{
        //    foreach ( Connection conn in m_circuit.Wires )
        //    {
        //        if ( conn.InputPin == pin )
        //        {
        //            Module srcModule = conn.OutputElement.As<Module>();
        //            return srcModule;
        //        }
        //    }

        //    return null;
        //}

        private Circuit m_circuit;
    }
}
