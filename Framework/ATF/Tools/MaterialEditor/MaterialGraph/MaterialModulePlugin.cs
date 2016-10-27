//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

using PropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;

namespace CircuitEditorSample
{
    /// <summary>
    /// Component that adds module types to the editor. 
    /// This class adds some sample audio modules.</summary>
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export( typeof( MaterialModulePlugin ) )]
    public class MaterialModulePlugin : IPaletteClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="paletteService">Palette service</param>
        /// <param name="schemaLoader">Schema loader</param>
        [ImportingConstructor]
        public MaterialModulePlugin(
            IPaletteService paletteService,
            SchemaLoader schemaLoader
            )
        {
            s_paletteService = paletteService;
            s_schemaLoader = schemaLoader;
        }

        public static SchemaLoader SchemaLoader
        {
            get { return s_schemaLoader; }
        }

        private static IPaletteService s_paletteService;
        private static SchemaLoader s_schemaLoader;

        ///// <summary>
        ///// Gets the palette category string for the circuit modules</summary>
        //public readonly string PaletteCategory = "Materials".Localize();

        ///// <summary>
        ///// Gets drawing resource key for boolean pin types</summary>
        //public string BooleanPinTypeName
        //{
        //    get { return BooleanPinType.Name; }
        //}

        ///// <summary>
        ///// Gets boolean pin type</summary>
        //public AttributeType BooleanPinType
        //{
        //    get { return AttributeType.BooleanType; }
        //}

        ///// <summary>
        ///// Gets float pin type name</summary>
        //public string FloatPinTypeName
        //{
        //    get { return FloatPinType.Name; }
        //}

        ///// <summary>
        ///// Gets float pin type</summary>
        //public AttributeType FloatPinType
        //{
        //    get { return AttributeType.FloatType; }
        //}

        ///// <summary>
        ///// Gets float pin type name</summary>
        //public string RGBChannelPinTypeName
        //{
        //    get { return "RGBChannel"; }
        //}

        ///// <summary>
        ///// Gets float pin type name</summary>
        //public string RGBAChannelPinTypeName
        //{
        //    get { return "RGBAChannel"; }
        //}

        ///// <summary>
        ///// Gets float pin type name</summary>
        //public string RedChannelPinTypeName
        //{
        //    get { return "RedChannel"; }
        //}

        ///// <summary>
        ///// Gets float pin type name</summary>
        //public string GreenChannelPinTypeName
        //{
        //    get { return "GreenChannel"; }
        //}

        ///// <summary>
        ///// Gets float pin type name</summary>
        //public string BlueChannelPinTypeName
        //{
        //    get { return "BlueChannel"; }
        //}

        ///// <summary>
        ///// Gets float pin type name</summary>
        //public string AlphaChannelPinTypeName
        //{
        //    get { return "AlphaChannel"; }
        //}

        #region IInitializable Members

        //private void Define_colorConstantType()
        //{
        //    string prettyName = "Color Constant".Localize();

        //    DomNodeType dnt = DefineModuleType(
        //        new XmlQualifiedName( "colorConstantType", Schema.NS ),
        //        prettyName,
        //        prettyName,
        //        Resources.LightImage,
        //        EmptyArray<ElementType.Pin>.Instance,
        //        new MaterialGraphPin[]
        //        {
        //                        //new ElementType.Pin("RGB ".Localize(), RGBChannelPinTypeName, 0),
        //                        //new ElementType.Pin("R ".Localize(), RedChannelPinTypeName, 1),
        //                        //new ElementType.Pin("G ".Localize(), GreenChannelPinTypeName, 2),
        //                        //new ElementType.Pin("B ".Localize(), BlueChannelPinTypeName, 3),
        //                        //new ElementType.Pin("A ".Localize(), AlphaChannelPinTypeName, 4),

        //                        new MaterialGraphPin("RGB ".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 0, MaterialGraphPin.ComponentType.RGB),
        //                        new MaterialGraphPin("R ".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 1, MaterialGraphPin.ComponentType.Red),
        //                        new MaterialGraphPin("G ".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 2, MaterialGraphPin.ComponentType.Green),
        //                        new MaterialGraphPin("B ".Localize(), MaterialGraphPin.MaterialGraphPinTypeName, 3, MaterialGraphPin.ComponentType.Blue),
        //        },
        //        s_schemaLoader );

        //    var colorAttribute = new AttributeInfo( "color".Localize(), AttributeType.IntType );
        //    dnt.Define( colorAttribute );

        //    dnt.SetTag(
        //        new PropertyDescriptorCollection(
        //            new PropertyDescriptor[] {
        //                new AttributePropertyDescriptor(
        //                    "Color".Localize(),
        //                    colorAttribute,
        //                    prettyName, //category
        //                    "Color".Localize(), //description
        //                    false,
        //                    new Sce.Atf.Controls.PropertyEditing.ColorPickerEditor(),
        //                    new Sce.Atf.Controls.PropertyEditing.IntColorConverter()
        //                    ) //is not read-only
        //            } ) );


        //}

        /// <summary>
        /// Finishes initializing component by adding palette information and defining module types</summary>
        public virtual void Initialize()
        {
            // define module types
            MaterialGraphModuleAdapter._Setup( s_paletteService, this, s_schemaLoader );

            MaterialModule.DefineDomNodeType();
            ColorConstantModule.DefineDomNodeType();
            SwizzleModule.DefineDomNodeType();
            Texture2DModule.DefineDomNodeType( Editor.EditorInstance.D2dDiagramTheme );

            // math
            LerpModule.DefineDomNodeType();
            AddModule.DefineDomNodeType();
            MultiplyModule.DefineDomNodeType();
            PowerModule.DefineDomNodeType();

            // util
            LevelOfDetailModule.DefineDomNodeType();
            UserFunctionModule.DefineDomNodeType();
            CurveModule.DefineDomNodeType();

            // parameters
            ColorParameterModule.DefineDomNodeType();
            FloatParameterModule.DefineDomNodeType();
            Texture2DParameterModule.DefineDomNodeType();
        }

        #endregion

        #region IPaletteClient Members

        /// <summary>
        /// Gets display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Info object, which client can fill out</param>
        void IPaletteClient.GetInfo(object item, ItemInfo info)
        {
            var paletteItem = (NodeTypePaletteItem)item;
            if (paletteItem != null)
            {
                info.Label = paletteItem.Name;
                info.Description = paletteItem.Description;
                info.ImageIndex = info.GetImageList().Images.IndexOfKey(paletteItem.ImageName);
                info.HoverText = paletteItem.Description;
            }
        }

        /// <summary>
        /// Converts the palette item into an object that can be inserted into an IInstancingContext</summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Object that can be inserted into an IInstancingContext</returns>
        object IPaletteClient.Convert(object item)
        {
            var paletteItem = (NodeTypePaletteItem)item;
            var node = new DomNode(paletteItem.NodeType);
            if (paletteItem.NodeType.IdAttribute != null)
                node.SetAttribute(paletteItem.NodeType.IdAttribute, paletteItem.Name);
            return node;
        }

        #endregion

        ///// <summary>
        ///// Prepare metadata for the module type, to be used by the palette and circuit drawing engine</summary>
        ///// <param name="name"> Schema full name of the DomNodeType for the module type</param>
        ///// <param name="displayName">Display name for the module type</param>
        ///// <param name="description"></param>
        ///// <param name="imageName">Image name </param>
        ///// <param name="inputs">Define input pins for the module type</param>
        ///// <param name="outputs">Define output pins for the module type</param>
        ///// <param name="loader">XML schema loader </param>
        ///// <param name="domNodeType">optional DomNode type for the module type</param>
        ///// <returns>DomNodeType that was created (or the domNodeType parameter, if it wasn't null)</returns>
        //protected DomNodeType DefineModuleType(
        //    XmlQualifiedName name,
        //    string displayName,
        //    string description,
        //    string imageName,
        //    ICircuitPin[] inputs,
        //    ICircuitPin[] outputs,
        //    SchemaLoader loader,
        //    DomNodeType domNodeType = null)
        //{
        //    if (domNodeType == null)
        //        // create the type
        //        domNodeType = new DomNodeType(
        //        name.ToString(),
        //        Schema.moduleType.Type,
        //        EmptyArray<AttributeInfo>.Instance,
        //        EmptyArray<ChildInfo>.Instance,
        //        EmptyArray<ExtensionInfo>.Instance);

        //    DefineCircuitType(domNodeType, displayName, imageName, inputs, outputs);

        //    // add it to the schema-defined types
        //    loader.AddNodeType(name.ToString(), domNodeType);

        //    // add the type to the palette
        //    s_paletteService.AddItem(
        //        new NodeTypePaletteItem(
        //            domNodeType,
        //            displayName,
        //            description,
        //            imageName),
        //        PaletteCategory,
        //        this);

        //    return domNodeType;
        //}

        //private void DefineCircuitType(
        //    DomNodeType type,
        //    string elementTypeName,
        //    string imageName,
        //    ICircuitPin[] inputs,
        //    ICircuitPin[] outputs)
        //{
        //    // create an element type and add it to the type metadata
        //    // For now, let all circuit elements be used as 'connectors' which means
        //    //  that their pins will be used to create the pins on a master instance.
        //    bool isConnector = true; //(inputs.Length + outputs.Length) == 1;
        //    var image = string.IsNullOrEmpty(imageName) ? null : ResourceUtil.GetImage32(imageName);
        //    type.SetTag<ICircuitElementType>(
        //        new ElementType(
        //            elementTypeName,
        //            isConnector,
        //            new Size(),
        //            image,
        //            inputs,
        //            outputs));


        //}
    }
}
