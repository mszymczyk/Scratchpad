using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf.Controls.Adaptable.Graphs;

namespace CircuitEditorSample
{
    /// <summary>
    /// Pin for an ElementType</summary>
    public class MaterialGraphPin : ICircuitPin
    {
        /// <summary>
        /// Gets float pin type name</summary>
        public static readonly string MaterialGraphPinTypeName = "MaterialGraphPin";

        public enum ComponentType
        {
            Red = 0,
            Green = 1,
            Blue = 2,
            Alpha = 3,
            RG = 4,
            RGB = 5,
            RGBA = 6,
            //Float = 6,
            //FloatXY = 7,
        }

        public static int GetNumComponents( ComponentType componentType )
        {
            switch( componentType )
            {
                case ComponentType.Red:
                case ComponentType.Green:
                case ComponentType.Blue:
                case ComponentType.Alpha:
                    return 1;
                case ComponentType.RG:
                    return 2;
                case ComponentType.RGB:
                    return 3;
                case ComponentType.RGBA:
                    return 4;
                //case ComponentType.Float:
                //    return 1;
                default:
                    throw new InvalidOperationException( "Unsupported pin component" );
            }
        }

        //public static bool IsColorComponent( ComponentType componentType )
        //{
        //    int ict = (int)componentType;
        //    if ( ict >= (int)ComponentType.Red && ict <= (int)ComponentType.RGBA )
        //        return true;

        //    return false;
        //}

        public static bool IsFloatComponent( ComponentType componentType )
        {
            //int ict = (int)componentType;
            //if ( ict >= (int)ComponentType.Float && ict <= (int)ComponentType.FloatXY )
            //    return true;

            //return false;
            return true; // all components are floats right now
        }

        private static string GetComponentTypeChannels( ComponentType componentType )
        {
            switch( componentType )
            {
                case ComponentType.Red:
                    return "R";
                case ComponentType.Green:
                    return "G";
                case ComponentType.Blue:
                    return "B";
                case ComponentType.Alpha:
                    return "A";
                case ComponentType.RG:
                    return "RG";
                case ComponentType.RGB:
                    return "RGB";
                case ComponentType.RGBA:
                    return "RGBA";
                default:
                    throw new InvalidOperationException( "Unsupported component type" );
            }
        }

        /// <summary>
        /// Constructor specifying Pin type's attributes</summary>
        /// <param name="name">Pin's name</param>
        /// <param name="typeName">Pin's type's name</param>
        /// <param name="index">Index of pin on module</param>
        public MaterialGraphPin( string name, string typeName, int index, ComponentType channelType, MaterialGraphPin parentPin = null )
        {
            m_name = name;
            m_shaderName = name;
            m_typeName = typeName;
            m_index = index;
            m_componentType = channelType;
            m_parentPin = parentPin;
        }

        private MaterialGraphPin()
        {

        }

        public static MaterialGraphPin CreateMaterialGraphInputPin( string name, int index, ComponentType channelType, MaterialGraphPin parentPin = null )
        {
            MaterialGraphPin mp = new MaterialGraphPin();
            mp.m_name = string.Format( "{0} ({1})", name, GetComponentTypeChannels( channelType ) );
            mp.m_shaderName = name;
            mp.m_typeName = MaterialGraphPinTypeName;
            mp.m_index = index;
            mp.m_componentType = channelType;
            mp.m_parentPin = parentPin;
            return mp;
        }

        /// <summary>
        /// Gets pin's name</summary>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// Gets pin's shader name</summary>
        public string ShaderName
        {
            get { return m_shaderName; }
        }

        /// <summary>
        /// Gets pin's type's name</summary>
        public string TypeName
        {
            get { return m_typeName; }
        }

        /// <summary>
        /// Gets index of pin on module</summary>
        public int Index
        {
            get { return m_index; }
        }

        /// <summary>
        /// Gets whether connection fan in to pin is allowed</summary>
        public bool AllowFanIn
        {
            get { return false; }
        }

        /// <summary>
        /// Gets whether connection fan out from pin is allowed</summary>
        public bool AllowFanOut
        {
            get { return true; }
        }

        public bool CanConnectTo( ICircuitPin otherPin )
        {
            if ( TypeName != otherPin.TypeName )
                return false;

            MaterialGraphPin dst = otherPin as MaterialGraphPin;
            if ( dst == null )
                return false;

            //if ( Component == ComponentType.RGB && dst.Component != ComponentType.RGB )
            //    return false;

            //if ( Component == ComponentType.RGBA && dst.Component != ComponentType.RGBA )
            //    return false;

            if ( IsFloatComponent( Component ) && IsFloatComponent( dst.Component ) )
                return true;

            return false;
        }

        public ComponentType Component
        {
            get { return m_componentType; }
        }

        public MaterialGraphPin ParentPin
        {
            get { return m_parentPin; }
        }

        private string m_name; // display name
        private string m_shaderName; // name used in shader source code
        private string m_typeName;
        private int m_index;
        private ComponentType m_componentType;
        private MaterialGraphPin m_parentPin; // non null for child pins (like Red channel of RGB pin)
    }
}
