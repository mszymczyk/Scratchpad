using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Dom;

namespace SettingsEditor
{
    /// <summary>
    /// Loads the UI schema, registers data extensions on the DOM types and annotates
    /// the types with display information and PropertyDescriptors</summary>
    [Export( typeof( SchemaLoader ) )]
    [PartCreationPolicy( CreationPolicy.Shared )]
    public class SchemaLoader : XmlSchemaTypeLoader
    {
        /// <summary>
        /// Constructor</summary>
        public SchemaLoader()
        {
            s_schemaLoader = this;
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "SettingsEditor/Schemas");
			Load( "settingsFile.xsd" );
        }

        /// <summary>
        /// Gets the schema namespace</summary>
        public string NameSpace
        {
            get { return m_namespace; }
        }
        private string m_namespace;

        /// <summary>
        /// Gets the schema type collection</summary>
        public XmlSchemaTypeCollection TypeCollection
        {
            get { return m_typeCollection; }
        }
        private XmlSchemaTypeCollection m_typeCollection;

        /// <summary>
        /// Method called after the schema set has been loaded and the DomNodeTypes have been created, but
        /// before the DomNodeTypes have been frozen. This means that DomNodeType.SetIdAttribute, for example, has
        /// not been called on the DomNodeTypes. Is called shortly before OnDomNodeTypesFrozen.
        /// Create property descriptors for types.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                m_namespace = typeCollection.TargetNamespace;
                m_typeCollection = typeCollection;
				Schema.Initialize( typeCollection );

                // register extensions
                Schema.settingsFileType.Type.Define( new ExtensionInfo<DocumentEditingContext>() );
                Schema.settingsFileType.Type.Define( new ExtensionInfo<UniqueIdValidator>() );
                Schema.settingsFileType.Type.Define( new ExtensionInfo<Document>() );
                Schema.settingsFileType.Type.Define( new ExtensionInfo<TreeView>() );
                Schema.settingsFileType.Type.Define( new ExtensionInfo<SettingsReferenceValidator>() );        // tracks preset references

                //m_paramSchema.groupType.Type.Define( new ExtensionInfo<Group>() );
                //m_paramSchema.presetType.Type.Define( new ExtensionInfo<Preset>() );

                Schema.dynamicPropertyType.Type.Define( new ExtensionInfo<DynamicProperty>() );

                Schema.groupType.Type.Define( new ExtensionInfo<GroupProperties>() );
                Schema.groupType.Type.Define( new ExtensionInfo<Group>() );

                Schema.presetType.Type.Define( new ExtensionInfo<Preset>() );

				// Descriptors for armorType.
				string general = "General".Localize();
				var paramFileDescriptors = new PropertyDescriptorCollection( null );
				paramFileDescriptors.Add( new AttributePropertyDescriptor(
						   "SettingsDescFile".Localize(),
                           Schema.settingsFileType.settingsDescFileAttribute,
						   general,
						   "File describing structure of settings file".Localize(),
						   true
					) );

                paramFileDescriptors.Add(new AttributePropertyDescriptor(
                           "ShaderConstantsOutputPath".Localize(),
                           Schema.settingsFileType.shaderOutputFileAttribute,
                           general,
                           "Optional path for writing the shader constant buffer.".Localize(),
                           false
                    ) );

                Schema.settingsFileType.Type.SetTag( paramFileDescriptors );

				// only one namespace
                break;
            }            
        }

        public static SchemaLoader s_schemaLoader;
    }
}
