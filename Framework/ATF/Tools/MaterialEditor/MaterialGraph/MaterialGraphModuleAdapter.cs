using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;
using System.Text;
using System.ComponentModel;
using Sce.Atf.Controls.PropertyEditing;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a Track</summary>
    public abstract class MaterialGraphModuleAdapter : DomNodeAdapter, IMaterialGraphModule//, ICloneable
    {
        public static IPaletteService PaletteService { get { return s_paletteService; } }
        public static IPaletteClient PaletteClient { get { return s_paletteClient; } }
        public static SchemaLoader SchemaLoader { get { return s_schemaLoader; } }
        public static readonly string PaletteCategory = "Materials";

        private string EvaluatePinWrap( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            if ( sourceCode.HasComputedValue( this, pin ) )
            {
                string valueName = ShaderSourceCode.GetVariableName( this, pin );
                return valueName;
            }

            return EvaluatePin( pin, sourceCode );
        }

        public string EvaluateSubgraph( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            MaterialGraphModuleAdapter srcModule;
            MaterialGraphPin srcPin;
            if ( GetInputModule( pin, out srcModule, out srcPin ) )
            {
                MaterialGraphModuleAdapter srcModuleAdapter = srcModule.As<MaterialGraphModuleAdapter>();
                srcModuleAdapter.EvaluatePinWrap( srcPin.ParentPin != null ? srcPin.ParentPin : srcPin, sourceCode );

                string outputValueName = sourceCode.AddComputedValue( this, pin );

                StringBuilder sb = new StringBuilder();
                int pinNumComponents = MaterialGraphPin.GetNumComponents( pin.Component );

                sb.Append( outputValueName );
                sb.Append( " = " );

                if ( srcPin.ParentPin != null )
                {
                    string sourceComputedValue = ShaderSourceCode.GetVariableName( srcModule, srcPin.ParentPin );

                    sb.Append( sourceComputedValue );
                    if ( srcPin.Component == MaterialGraphPin.ComponentType.Red )
                    {
                        sb.Append( "." );
                        for ( int i = 0; i < pinNumComponents; ++i )
                            sb.Append( "x" );
                    }
                    else if ( srcPin.Component == MaterialGraphPin.ComponentType.Green )
                    {
                        sb.Append( "." );
                        for ( int i = 0; i < pinNumComponents; ++i )
                            sb.Append( "y" );
                    }
                    else if ( srcPin.Component == MaterialGraphPin.ComponentType.Blue )
                    {
                        sb.Append( "." );
                        for ( int i = 0; i < pinNumComponents; ++i )
                            sb.Append( "z" );
                    }
                    else if ( srcPin.Component == MaterialGraphPin.ComponentType.Alpha )
                    {
                        sb.Append( "." );
                        for ( int i = 0; i < pinNumComponents; ++i )
                            sb.Append( "w" );
                    }
                    else
                        throw new InvalidOperationException( "Unsupported pin component" );
                }
                else
                {
                    string sourceComputedValue = ShaderSourceCode.GetVariableName( srcModule, srcPin );

                    if ( srcPin.Component == MaterialGraphPin.ComponentType.RGBA )
                    {
                        sb.Append( sourceComputedValue );

                        if ( pinNumComponents == 1 )
                            sb.Append( ".x" );
                        else if ( pinNumComponents == 2 )
                            sb.Append( ".xy" );
                        else if ( pinNumComponents == 3 )
                            sb.Append( ".xyz" );
                        else if ( pinNumComponents == 4 )
                            sb.Append( ".xyzw" );
                    }
                    else if ( srcPin.Component == MaterialGraphPin.ComponentType.RGB )
                    {
                        if ( pinNumComponents == 1 )
                        {
                            sb.Append( sourceComputedValue );
                            sb.Append( ".x" );
                        }
                        else if ( pinNumComponents == 2 )
                        {
                            sb.Append( sourceComputedValue );
                            sb.Append( ".xy" );
                        }
                        else if ( pinNumComponents == 3 )
                        {
                            sb.Append( sourceComputedValue );
                            sb.Append( ".xyz" );
                        }
                        else if ( pinNumComponents == 4 )
                        {
                            sb.Append( "float4( " );
                            sb.Append( sourceComputedValue );
                            sb.Append( ".xyz, 0 )" );
                        }
                    }
                    else if ( srcPin.Component == MaterialGraphPin.ComponentType.RG )
                    {
                        if ( pinNumComponents == 1 )
                        {
                            sb.Append( sourceComputedValue );
                            sb.Append( ".x" );
                        }
                        else if ( pinNumComponents == 2 )
                        {
                            sb.Append( sourceComputedValue );
                            sb.Append( ".xy" );
                        }
                        else if ( pinNumComponents == 3 )
                        {
                            sb.Append( "float3( " );
                            sb.Append( sourceComputedValue );
                            sb.Append( ".xy, 0 )" );
                        }
                        else if ( pinNumComponents == 4 )
                        {
                            sb.Append( "float4( " );
                            sb.Append( sourceComputedValue );
                            sb.Append( ".xy, 0, 0 )" );
                        }
                    }
                    else if ( srcPin.Component == MaterialGraphPin.ComponentType.Red )
                    {
                        if ( pinNumComponents == 1 )
                        {
                            sb.Append( sourceComputedValue );
                            sb.Append( ".x" );
                        }
                        else if ( pinNumComponents == 2 )
                        {
                            sb.Append( sourceComputedValue );
                            sb.Append( ".xx" );
                        }
                        else if ( pinNumComponents == 3 )
                        {
                            sb.Append( "float3( " );
                            sb.Append( sourceComputedValue );
                            sb.Append( ".xxx )" );
                        }
                        else if ( pinNumComponents == 4 )
                        {
                            sb.Append( "float4( " );
                            sb.Append( sourceComputedValue );
                            sb.Append( ".xxxx )" );
                        }
                    }
                    else
                        throw new InvalidOperationException( "Unsupported pin component" );
                }

                sb.Append( ";" );

                sourceCode.ComputedValuesEval.AppendLine( sb.ToString() );

                return outputValueName;
            }
            else
            {
                return EvaluatePinWrap( pin, sourceCode );
            }
        }

        public string Id
        {
            get { return this.Cast<Module>().Id; }
        }

        public abstract string EvaluatePin( MaterialGraphPin pin, ShaderSourceCode sourceCode );

        //#region ICloneable Members

        ///// <summary>
        ///// Copies this timeline object, returning a new timeline object that is not in any timeline-related
        ///// container. If the copy can't be done, null is returned.</summary>
        ///// <returns>A copy of this timeline object or null if copy fails</returns>
        //public virtual object Clone()
        //{
        //    DomNode domCopy = DomNode.Copy(new DomNode[] { DomNode })[0];
        //    return domCopy.As<ITimelineObject>();
        //}

        //#endregion

        /// <summary>
        /// Prepare metadata for the module type, to be used by the palette and circuit drawing engine</summary>
        /// <param name="name"> Schema full name of the DomNodeType for the module type</param>
        /// <param name="displayName">Display name for the module type</param>
        /// <param name="description"></param>
        /// <param name="imageName">Image name </param>
        /// <param name="inputs">Define input pins for the module type</param>
        /// <param name="outputs">Define output pins for the module type</param>
        /// <param name="loader">XML schema schemaLoader </param>
        /// <param name="domNodeType">optional DomNode type for the module type</param>
        /// <returns>DomNodeType that was created (or the domNodeType parameter, if it wasn't null)</returns>
        protected static DomNodeType DefineModuleType(
            XmlQualifiedName name,
            string displayName,
            string description,
            string imageName,
            ICircuitPin[] inputs,
            ICircuitPin[] outputs,
            string paletteCategory = null,
            DomNodeType baseType = null,
            SchemaLoader schemaLoader = null,
            DomNodeType domNodeType = null
            )
        {
            if ( domNodeType == null )
                // create the type
                domNodeType = new DomNodeType(
                name.ToString(),
                baseType ?? Schema.moduleType.Type,
                EmptyArray<AttributeInfo>.Instance,
                EmptyArray<ChildInfo>.Instance,
                EmptyArray<ExtensionInfo>.Instance );

            DefineCircuitType( domNodeType, displayName, imageName, inputs, outputs );

            if ( schemaLoader == null )
                schemaLoader = s_schemaLoader;

            // add it to the schema-defined types
            schemaLoader.AddNodeType( name.ToString(), domNodeType );

            // add the type to the palette
            s_paletteService.AddItem(
                new NodeTypePaletteItem(
                    domNodeType,
                    displayName,
                    description,
                    imageName ),
                string.IsNullOrEmpty(paletteCategory) ? PaletteCategory : paletteCategory,
                s_paletteClient );

            if ( domNodeType.GetTagLocal<PropertyDescriptorCollection>() == null )
            {
                PropertyDescriptorCollection pdc = new PropertyDescriptorCollection( null );
                domNodeType.SetTag( pdc );
            }

            return domNodeType;
        }

        public static AttributeInfo CreateBoundedIntAttribute( DomNodeType dnt, string name, int defaultValue, int minValue, int maxValue, string uiCategory, string uiName, string uiDesc )
        {
            XmlAttributeType attributeType = new XmlAttributeType( "http://www.w3.org/2001/XMLSchema:int", typeof( System.Int32 ), 1, System.Xml.Schema.XmlTypeCode.Int );
            AttributeInfo ai = new AttributeInfo( name, attributeType );
            ai.DefaultValue = defaultValue;
            dnt.Define( ai );

            PropertyDescriptorCollection pdc = dnt.GetTag<PropertyDescriptorCollection>();

            pdc.Add(
                new AttributePropertyDescriptor(
                uiName,
                ai,
                uiCategory, //category
                uiDesc, //description
                false,
                new BoundedIntEditor( minValue, maxValue )
                )
            );

            return ai;
        }

        public static AttributeInfo CreateBoundedFloatAttribute( DomNodeType dnt, string name, float defaultValue, float minValue, float maxValue, string uiCategory, string uiName, string uiDesc )
        {
            XmlAttributeType attributeType = new XmlAttributeType( "http://www.w3.org/2001/XMLSchema:float", typeof( System.Single ), 1, System.Xml.Schema.XmlTypeCode.Float );
            AttributeInfo ai = new AttributeInfo( name, attributeType );
            ai.DefaultValue = defaultValue;
            dnt.Define( ai );

            PropertyDescriptorCollection pdc = dnt.GetTag<PropertyDescriptorCollection>();

            pdc.Add(
                new AttributePropertyDescriptor(
                uiName,
                ai,
                uiCategory, //category
                uiDesc, //description
                false,
                new BoundedFloatEditor( minValue, maxValue )
                )
            );

            return ai;
        }

        public static AttributeInfo CreateEnumAttribute( DomNodeType dnt, string name, string[] values, string uiCategory, string uiName, string uiDesc )
        {
            XmlAttributeType attributeType = new XmlAttributeType( "http://www.w3.org/2001/XMLSchema:string", typeof( System.String ), 1, System.Xml.Schema.XmlTypeCode.String );
            AttributeInfo ai = new AttributeInfo( name, attributeType );
            ai.DefaultValue = values[0];
            dnt.Define( ai );

            PropertyDescriptorCollection pdc = dnt.GetTag<PropertyDescriptorCollection>();

            var editor = new LongEnumEditor();
            editor.DefineEnum( values );

            pdc.Add(
                new AttributePropertyDescriptor(
                uiName,
                ai,
                uiCategory, //category
                uiDesc, //description
                false,
                editor
                )
            );

            return ai;
        }

        public static AttributeInfo CreateUriAttribute( DomNodeType dnt, string name, string fileFilter, string uiCategory, string uiName, string uiDesc )
        {
            XmlAttributeType attributeType = new XmlAttributeType( "http://www.w3.org/2001/XMLSchema:anyURI", typeof( System.Uri ), 1, System.Xml.Schema.XmlTypeCode.AnyUri );
            AttributeInfo ai = new AttributeInfo( name, attributeType );
            dnt.Define( ai );

            PropertyDescriptorCollection pdc = dnt.GetTag<PropertyDescriptorCollection>();

            pdc.Add(
                new AttributePropertyDescriptor(
                uiName,
                ai,
                uiCategory, //category
                uiDesc, //description
                false,
                new FileUriEditor( fileFilter )
                ) //is not read-only
            );

            return ai;
        }

        public static AttributeInfo CreateStringAttribute( DomNodeType dnt, string name, string uiCategory, string uiName, string uiDesc )
        {
            //XmlAttributeType attributeType = new XmlAttributeType( "http://www.w3.org/2001/XMLSchema:anyURI", typeof( System.String ), 1, System.Xml.Schema.XmlTypeCode.String );
            AttributeInfo ai = new AttributeInfo( name, AttributeType.StringType );
            dnt.Define( ai );

            PropertyDescriptorCollection pdc = dnt.GetTag<PropertyDescriptorCollection>();

            pdc.Add(
                new AttributePropertyDescriptor(
                uiName,
                ai,
                uiCategory, //category
                uiDesc, //description
                false,
                null
                ) //is not read-only
            );

            return ai;
        }

        private static void DefineCircuitType(
            DomNodeType type,
            string elementTypeName,
            string imageName,
            ICircuitPin[] inputs,
            ICircuitPin[] outputs )
        {
            // create an element type and add it to the type metadata
            // For now, let all circuit elements be used as 'connectors' which means
            //  that their pins will be used to create the pins on a master instance.
            bool isConnector = true; //(inputs.Length + outputs.Length) == 1;
            var image = string.IsNullOrEmpty( imageName ) ? null : ResourceUtil.GetImage32( imageName );
            type.SetTag<ICircuitElementType>(
                new ElementType(
                    elementTypeName,
                    isConnector,
                    new Size(),
                    image,
                    inputs,
                    outputs ) );
        }

        internal static void _Setup( IPaletteService paletteService, IPaletteClient paletteClient, SchemaLoader schemaLoader )
        {
            s_paletteService = paletteService;
            s_paletteClient = paletteClient;
            s_schemaLoader = schemaLoader;
        }

        private static IPaletteService s_paletteService;
        private static IPaletteClient s_paletteClient;
        private static SchemaLoader s_schemaLoader;

        public bool GetInputModule( MaterialGraphPin pin, out MaterialGraphModuleAdapter srcModule, out MaterialGraphPin srcPin )
        {
            Circuit circuit = DomNode.GetRoot().As<Circuit>();

            foreach ( Connection conn in circuit.Wires )
            {
                if ( conn.InputPin == pin )
                {
                    // can't just use conn.OutputElement and conn.OutputPin
                    // to handle grouping we need to use conn.OutputPinTarget
                    srcModule = conn.OutputPinTarget.LeafDomNode.As<MaterialGraphModuleAdapter>();
                    Module m = conn.OutputPinTarget.LeafDomNode.As<Module>();
                    srcPin = m.OutputPin( conn.OutputPinTarget.LeafPinIndex ).As<MaterialGraphPin>();
                    return true;
                }
            }

            srcModule = null;
            srcPin = null;
            return false;
        }

        public virtual bool DoesRequireRecompile( AttributeInfo attr )
        {
            return false;
        }

        public virtual bool DoesRequireRefresh( AttributeInfo attr )
        {
            return false;
        }
    }
}



