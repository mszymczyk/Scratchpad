using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Adaptation;
using System.Text;
using System;
using System.IO;
using Sce.Atf;

namespace CircuitEditorSample
{
    /// <summary>
    /// Pin for an ElementType</summary>
    public class ShaderSourceCode
    {
        public static string GetFloatType( MaterialGraphPin pin )
        {
            if ( pin.Component == MaterialGraphPin.ComponentType.Red
                || pin.Component == MaterialGraphPin.ComponentType.Green
                || pin.Component == MaterialGraphPin.ComponentType.Blue
                || pin.Component == MaterialGraphPin.ComponentType.Alpha
                || pin.Component == MaterialGraphPin.ComponentType.Red
                )
                return "float";
            else if ( pin.Component == MaterialGraphPin.ComponentType.RGB )
                return "float3";
            else if ( pin.Component == MaterialGraphPin.ComponentType.RGBA )
                return "float4";
            else if ( pin.Component == MaterialGraphPin.ComponentType.RG )
                return "float2";
            else
                throw new InvalidOperationException( "Unsupported pin component" );
        }

        public static string GetVariableName( MaterialGraphModuleAdapter module, MaterialGraphPin pin )
        {
            return module.As<Module>().Id + "_" + pin.ShaderName;
        }

        public bool HasComputedValue( MaterialGraphModuleAdapter module, MaterialGraphPin pin )
        {
            string valueName = GetVariableName( module, pin );
            return m_computedValuesSet.Contains( valueName );
        }

        public string AddComputedValue( MaterialGraphModuleAdapter module, MaterialGraphPin pin )
        {
            string valueName = GetVariableName( module, pin );
            if ( m_computedValuesSet.Contains( valueName ) )
                throw new InvalidOperationException( "Value " + valueName + " is already on the list" );

            m_computedValuesSet.Add( valueName );
            m_computedValues.Append( GetFloatType( pin ) );
            m_computedValues.Append( ' ' );
            m_computedValues.Append( valueName );
            m_computedValues.AppendLine( ";" );
            return valueName;
        }

        public string AddUniformParameter( string name, UniformType uniformType )
        {
            if ( m_uniforms.Contains( name ) )
                return name;

            m_uniforms.Add( name );
            string typeName = ShaderTypes.GetUniformTypeName( uniformType );
            m_uniformsEval.Append( typeName );
            m_uniformsEval.Append( ' ' );
            m_uniformsEval.Append( name );
            m_uniformsEval.AppendLine( ";" );

            return name;
        }

        public string AddTexture2D( string name )
        {
            string fullName = name + "_tex";
            if ( m_resourceParameters.Contains( fullName ) )
                return fullName;

            m_resourceParameters.Add( fullName );
            m_resourceParametersEval.Append( "Texture2D" );
            m_resourceParametersEval.Append( ' ' );
            m_resourceParametersEval.Append( fullName );
            m_resourceParametersEval.Append( ';' );

            return fullName;
        }

        public string AddSampler( string name, Sampler s )
        {
            string fullName = name + "_samp";
            if ( m_samplers.ContainsKey( fullName ) )
                return fullName;

            // try find matching sampler among declared, reusing helps in generating better shader code
            foreach( KeyValuePair<string, Sampler> p in m_samplers )
            {
                Sampler es = p.Value;
                if ( es.Filter == s.Filter
                    && es.UAddressMode == s.UAddressMode
                    && es.VAddressMode == s.VAddressMode
                    && es.WAddressMode == s.WAddressMode
                    && es.MinLod == s.MinLod
                    && es.MaxLod == s.MaxLod
                    && Math.Abs( es.LodBias - s.LodBias ) <= 0.01f
                    && es.MaxAnisotropy == s.MaxAnisotropy
                    )
                {
                    return p.Key;
                }
            }

            m_samplers.Add( fullName, s );
            m_samplersEval.Append( "SamplerState " );
            m_samplersEval.Append( fullName );
            m_samplersEval.Append( ';' );

            return fullName;
        }

        public StringBuilder ComputedValues
        {
            get { return m_computedValues; }
        }

        public StringBuilder ComputedValuesEval
        {
            get { return m_computedValuesEval; }
        }

        public StringBuilder UserFunctionsEval
        {
            get { return m_userFunctionsEval; }
        }

        private static readonly string UNIFORMS_DECLARATION = "UNIFORMS_DECLARATION";
        private static readonly string RESOURCE_DECLARATION = "RESOURCE_DECLARATION";
        private static readonly string SHADER_EVALUATION = "SHADER_EVALUATION";
        private static readonly string USER_FUNCTIONS = "USER_FUNCTIONS";

        public string Finish()
        {
            StringBuilder ue = new StringBuilder();
            ue.AppendLine( "// uniforms" );
            ue.AppendLine( "//" );
            if ( m_uniforms.Count > 0 )
            {
                ue.AppendLine( "cbuffer MaterialParams : register(b2)" );
                ue.AppendLine( "{" );
                ue.Append( m_uniformsEval.ToString() );
                ue.AppendLine( "}" );
                ue.AppendLine();
            }
            else
            {
                ue.AppendLine( "// nothing declared" );
            }

            m_finalSourceCode = m_materialTemplate.Replace( UNIFORMS_DECLARATION, ue.ToString() );

            StringBuilder te = new StringBuilder();
            te.AppendLine( "// textures, buffers" );
            te.AppendLine( "//" );
            if ( m_resourceParameters.Count > 0 )
            {
                te.AppendLine( m_resourceParametersEval.ToString() );
                te.AppendLine();
            }
            else
            {
                ue.AppendLine( "// nothing declared" );
            }
            te.AppendLine( "// samplers" );
            te.AppendLine( "//" );
            if ( m_samplers.Count > 0 )
            {
                te.AppendLine( m_samplersEval.ToString() );
                te.AppendLine();
            }
            else
            {
                ue.AppendLine( "// nothing declared" );
            }

            m_finalSourceCode = m_finalSourceCode.Replace( RESOURCE_DECLARATION, te.ToString() );

            // user functions
            m_finalSourceCode = m_finalSourceCode.Replace( USER_FUNCTIONS, m_userFunctionsEval.ToString() );

            StringBuilder ge = new StringBuilder();
            ge.AppendLine( "// graph eval" );
            ge.AppendLine( "//" );
            ge.AppendLine( m_computedValues.ToString() );
            ge.AppendLine();
            ge.AppendLine( m_computedValuesEval.ToString() );

            m_finalSourceCode = m_finalSourceCode.Replace( SHADER_EVALUATION, ge.ToString() );

            return m_finalSourceCode;
        }

        public void LoadMaterialTemplate( string filePath )
        {
            m_materialTemplate = File.ReadAllText( filePath );
            //if ( File.Exists( filePath ) )
            //{
            //    // read the existing templates document
            //    using ( FileStream stream = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
            //    {

            //    }
            //}
        }

        public void WriteMaterialToFile( string filePath )
        {
            File.WriteAllText( filePath, m_finalSourceCode );
        }

        public enum LevelOfDetail
        {
            Lod0,
            Lod1,
            Lod2,
            Lod3
        }

        public LevelOfDetail CurrentLevelOfDetail
        {
            get { return m_currentLevelOfDetail; }
        }

        private string m_materialTemplate;
        private string m_finalSourceCode;

        private HashSet<string> m_computedValuesSet = new HashSet<string>();
        private StringBuilder m_computedValues = new StringBuilder();
        private StringBuilder m_computedValuesEval = new StringBuilder();

        private HashSet<string> m_uniforms = new HashSet<string>(); // variables that will be put into constant buffer
        private StringBuilder m_uniformsEval = new StringBuilder();

        private HashSet<string> m_resourceParameters = new HashSet<string>(); // variables like textures and buffers
        private StringBuilder m_resourceParametersEval = new StringBuilder();

        //private HashSet<string> m_samplers = new HashSet<string>(); // variables like textures and buffers
        private Dictionary<string, Sampler> m_samplers = new Dictionary<string, Sampler>(); // variables like textures and buffers
        private StringBuilder m_samplersEval = new StringBuilder();

        private StringBuilder m_userFunctionsEval = new StringBuilder( "// user functions\n//\n" );

        private LevelOfDetail m_currentLevelOfDetail = LevelOfDetail.Lod0;
    }
}
