using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf.Controls.Adaptable.Graphs;

namespace CircuitEditorSample
{
    /// <summary>
    /// Pin for an ElementType</summary>
    public class MaterialGraphException : Exception
    {
        //public MaterialGraphException( )
    }

    public class MaterialEvaluationException : Exception
    {
        public MaterialEvaluationException( MaterialGraphModuleAdapter module, MaterialGraphPin pin, ShaderSourceCode sourceCode )
            : base( "Material generator exception: " )
        {
            m_module = module;
            m_pin = pin;
            m_sourceCode = sourceCode;
        }

        MaterialGraphModuleAdapter m_module = null;
        MaterialGraphPin m_pin = null;
        ShaderSourceCode m_sourceCode = null;
    }
}
