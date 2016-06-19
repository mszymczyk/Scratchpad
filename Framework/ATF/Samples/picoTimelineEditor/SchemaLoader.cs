//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using picoTimelineEditor.DomNodeAdapters;

using pico.Controls.PropertyEditing;

namespace picoTimelineEditor
{
    /// <summary>
    /// Loads the UI schema, registers data extensions on the DOM types and annotates
    /// the types with display information and PropertyDescriptors</summary>
    public class SchemaLoader : XmlSchemaTypeLoader
    {
        /// <summary>
        /// Constructor</summary>
        public SchemaLoader()
        {
            // set resolver to locate embedded .xsd file
			SchemaResolver = new ResourceStreamResolver( Assembly.GetExecutingAssembly(), "picoTimelineEditor.schemas" );
            Load("timeline.xsd");
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
        /// not been called on the DomNodeTypes. Is called shortly before OnDomNodeTypesFrozen.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                m_namespace = typeCollection.TargetNamespace;
                m_typeCollection = typeCollection;
                Schema.Initialize(typeCollection);

                // There's no document type, so we will use the timeline type.
				Schema.timelineType.Type.Define( new ExtensionInfo<TimelineDocument>() );
                Schema.timelineType.Type.Define(new ExtensionInfo<TimelineContext>());
                Schema.timelineType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());

                // register extensions
                Schema.timelineType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
                Schema.timelineType.Type.Define(new ExtensionInfo<ReferenceValidator>());
                Schema.timelineType.Type.Define(new ExtensionInfo<TimelineValidator>());
				Schema.timelineType.Type.Define( new ExtensionInfo<DataValidator>() );
				Schema.timelineType.Type.Define( new ExtensionInfo<picoTimelineDomValidator>() ); // for ensuring there's only one GroupCamera group in timeline

                // register the timeline model interfaces
                Schema.timelineType.Type.Define(new ExtensionInfo<Timeline>());
                Schema.groupType.Type.Define(new ExtensionInfo<Group>());
                Schema.trackType.Type.Define(new ExtensionInfo<Track>());
                Schema.intervalType.Type.Define(new ExtensionInfo<Interval>());
                Schema.eventType.Type.Define(new ExtensionInfo<BaseEvent>());
				//Schema.keyType.Type.Define(new ExtensionInfo<Key>());
                Schema.markerType.Type.Define(new ExtensionInfo<Marker>());
                Schema.timelineRefType.Type.Define(new ExtensionInfo<TimelineReference>());

				// pico
				//Schema.intervalCurveType.Type.Define( new ExtensionInfo<IntervalCurve>() );

				// random stuff
				//
				Schema.keyLuaScriptType.Type.Define( new ExtensionInfo<LuaScript>() );
				Schema.intervalTextType.Type.Define( new ExtensionInfo<IntervalText>() );
				Schema.intervalNodeAnimationType.Type.Define( new ExtensionInfo<IntervalNodeAnimation>() );
				Schema.trackBlendFactorType.Type.Define( new ExtensionInfo<TrackBlendFactor>() );
				Schema.intervalBlendFactorType.Type.Define( new ExtensionInfo<IntervalBlendFactor>() );

				// references
				//
				Schema.refChangeLevelType.Type.Define( new ExtensionInfo<ReferenceChangeLevel>() );
				Schema.refPlayTimelineType.Type.Define( new ExtensionInfo<ReferencePlayTimeline>() );

				// sound
				//
				Schema.keySoundType.Type.Define( new ExtensionInfo<KeySound>() );
				Schema.intervalSoundType.Type.Define( new ExtensionInfo<IntervalSound>() );

				// fader
				//
				Schema.intervalFaderType.Type.Define( new ExtensionInfo<IntervalFader>() );
				Schema.trackFaderType.Type.Define( new ExtensionInfo<TrackFader>() );


				// camera
				//
				Schema.groupCameraType.Type.Define( new ExtensionInfo<GroupCamera>() );
				Schema.trackCameraAnimType.Type.Define( new ExtensionInfo<TrackCameraAnim>() );
				Schema.intervalCameraAnimType.Type.Define( new ExtensionInfo<IntervalCameraAnim>() );

				// anim controller
				Schema.groupAnimControllerType.Type.Define( new ExtensionInfo<GroupAnimController>() );
				Schema.trackAnimControllerType.Type.Define( new ExtensionInfo<TrackAnimController>() );
				Schema.intervalAnimControllerType.Type.Define( new ExtensionInfo<IntervalAnimController>() );

				// curve
				//
				Schema.curveType.Type.Define( new ExtensionInfo<Curve>() );
				Schema.curveType.Type.Define( new ExtensionInfo<CurveLimitValidator>() );
				Schema.controlPointType.Type.Define( new ExtensionInfo<ControlPoint>() );

				// character controller
				//
				Schema.groupCharacterControllerType.Type.Define( new ExtensionInfo<GroupCharacterController>() );
				Schema.trackCharacterControllerAnimType.Type.Define( new ExtensionInfo<TrackCharacterControllerAnim>() );
				Schema.intervalCharacterControllerAnimType.Type.Define( new ExtensionInfo<IntervalCharacterControllerAnim>() );

				// settings
				//
				Schema.intervalSettingType.Type.Define( new ExtensionInfo<IntervalSetting>() );
				Schema.cresLodSettingType.Type.Define( new ExtensionInfo<CResLodSetting>() );

                // the timeline schema defines only one type collection
                break;
            }
        }

        /// <summary>
        /// Parses annotations in schema sets. Override this to handle custom annotations.
        /// Supports annotations for property descriptors and palette items.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        /// <param name="annotations">Dictionary of annotations in schema</param>
        protected override void ParseAnnotations(
            XmlSchemaSet schemaSet,
            IDictionary<NamedMetadata, IList<XmlNode>> annotations)
		{
			base.ParseAnnotations( schemaSet, annotations );

			IList<XmlNode> xmlNodes;

			foreach ( DomNodeType nodeType in m_typeCollection.GetNodeTypes() )
			{
				// parse XML annotation for property descriptors
				if ( annotations.TryGetValue( nodeType, out xmlNodes ) )
				{
					PropertyDescriptorCollection propertyDescriptors = Sce.Atf.Dom.PropertyDescriptor.ParseXml( nodeType, xmlNodes );

					// Customizations
					// The flags and enum support from annotation used to be in ATF 2.8.
					//  Please request this feature from the ATF team if you need it and a ParseXml overload
					//  can probably be created.
					System.ComponentModel.PropertyDescriptor gameFlow = propertyDescriptors["Special Event"];
					if ( gameFlow != null )
					{
						FlagsUITypeEditor editor = (FlagsUITypeEditor)gameFlow.GetEditor( typeof( FlagsUITypeEditor ) );
						editor.DefineFlags( new string[] {
                            "Reward==Give player the reward",
                            "Trophy==Give player the trophy",
                            "LevelUp==Level up",
                            "BossDies==Boss dies",
                            "PlayerDies==Player dies",
                            "EndCinematic==End cinematic",
                            "EndGame==End game",
                         } );
					}

					nodeType.SetTag<PropertyDescriptorCollection>( propertyDescriptors );

					// parse type annotation to create palette items
					XmlNode xmlNode = FindElement( xmlNodes, "scea.dom.editors" );
					if ( xmlNode != null )
					{
						string menuText = FindAttribute( xmlNode, "menuText" );
						if ( menuText != null ) // must have menu text and category
						{
							string description = FindAttribute( xmlNode, "description" );
							string image = FindAttribute( xmlNode, "image" );
							NodeTypePaletteItem item = new NodeTypePaletteItem( nodeType, menuText, description, image );
							nodeType.SetTag<NodeTypePaletteItem>( item );
						}
					}
				}
			}


			{
				// FlagsUITypeEditor store flags as int.
				// doesn't implement IPropertyEditor

				PropertyDescriptorCollection propDescCollection = Schema.intervalNodeAnimationType.Type.GetTag<PropertyDescriptorCollection>();

				string[] channelNames = Enum.GetNames( typeof( IntervalNodeAnimation.ChannelType ) );
				int[] channelValues = GetEnumIntValues( typeof( IntervalNodeAnimation.ChannelType ) );

				FlagsUITypeEditor channelsEditor = new FlagsUITypeEditor( channelNames, channelValues );
				FlagsTypeConverter channelsConverter = new FlagsTypeConverter( channelNames, channelValues );
				propDescCollection.Add(
				 new AttributePropertyDescriptor(
						"Channels".Localize(),
						Schema.intervalNodeAnimationType.channelsAttribute,
						"Animation".Localize(),
						"Channels to edit".Localize(),
						false,
						channelsEditor,
						channelsConverter
						) );
			}

			{
				PropertyDescriptorCollection propDescCollection = Schema.intervalCameraAnimType.Type.GetTag<PropertyDescriptorCollection>();

				//propDescCollection.Add(
				// new CustomEnableAttributePropertyDescriptor(
				//		"Field of View".Localize(),
				//		Schema.intervalCameraAnimType.fovAttribute,
				//		"Camera".Localize(),
				//		"Camera's Field of View".Localize(),
				//		false,
				//		new BoundedFloatEditor( 5, 150 )
				//		, new CustomEnableAttributePropertyDescriptorCallback( Schema.intervalCameraAnimType.fovOverrideAttribute, CustomEnableAttributePropertyDescriptorCallback.Condition.ReadOnlyIfSetToFalse )
				//		) );

				//propDescCollection.Add(
				// new CustomEnableAttributePropertyDescriptor(
				//		"Near Clip Plane".Localize(),
				//		Schema.intervalCameraAnimType.nearClipPlaneAttribute,
				//		"Camera".Localize(),
				//		"Camera's Near Clip Plane".Localize(),
				//		false,
				//		new BoundedFloatEditor( 0.01f, 10000.0f )
				//		, new CustomEnableAttributePropertyDescriptorCallback( Schema.intervalCameraAnimType.nearClipPlaneOverrideAttribute, CustomEnableAttributePropertyDescriptorCallback.Condition.ReadOnlyIfSetToFalse )
				//		) );

				//propDescCollection.Add(
				// new CustomEnableAttributePropertyDescriptor(
				//		"Far Clip Plane".Localize(),
				//		Schema.intervalCameraAnimType.farClipPlaneAttribute,
				//		"Camera".Localize(),
				//		"Camera's Far Clip Plane".Localize(),
				//		false,
				//		new BoundedFloatEditor( 0.01f, 10000.0f )
				//		, new CustomEnableAttributePropertyDescriptorCallback( Schema.intervalCameraAnimType.farClipPlaneOverrideAttribute, CustomEnableAttributePropertyDescriptorCallback.Condition.ReadOnlyIfSetToFalse )
				//		) );

				propDescCollection.Add(
					new AttributePropertyDescriptor(
						"Field of View".Localize(),
						Schema.intervalCameraAnimType.fovAttribute,
						"Camera".Localize(),
						"Camera's Field of View".Localize(),
						false,
						new BoundedFloatEditor( 5, 150 )
						) );

				propDescCollection.Add(
					new AttributePropertyDescriptor(
						"Near Clip Plane".Localize(),
						Schema.intervalCameraAnimType.nearClipPlaneAttribute,
						"Camera".Localize(),
						"Camera's Near Clip Plane".Localize(),
						false,
						new BoundedFloatEditor( 0.01f, 10000.0f )
						) );

				propDescCollection.Add(
					new AttributePropertyDescriptor(
						"Far Clip Plane".Localize(),
						Schema.intervalCameraAnimType.farClipPlaneAttribute,
						"Camera".Localize(),
						"Camera's Far Clip Plane".Localize(),
						false,
						new BoundedFloatEditor( 0.01f, 10000.0f )
						) );
			}

			{
				PropertyDescriptorCollection propDescCollection = Schema.intervalCameraAnimType.Type.GetTag<PropertyDescriptorCollection>();
				pico.Controls.PropertyEditing.CustomPropertyDescriptor<IntervalCameraAnim>.CreateDescriptors( propDescCollection );
			}
			{
				PropertyDescriptorCollection propDescCollection = Schema.intervalCharacterControllerAnimType.Type.GetTag<PropertyDescriptorCollection>();
				pico.Controls.PropertyEditing.CustomPropertyDescriptor<IntervalCharacterControllerAnim>.CreateDescriptors( propDescCollection );
			}
			{
				PropertyDescriptorCollection propDescCollection = Schema.intervalAnimControllerType.Type.GetTag<PropertyDescriptorCollection>();
				pico.Controls.PropertyEditing.CustomPropertyDescriptor<IntervalAnimController>.CreateDescriptors( propDescCollection );
			}

			{
				PropertyDescriptorCollection propDescCollection = Schema.intervalSoundType.Type.GetTag<PropertyDescriptorCollection>();

				var formatEditor = new DynamicEnumUITypeEditor( new IntervalSoundLister() );
				formatEditor.MaxDropDownItems = 16;
				var apd = new AttributePropertyDescriptor(
					"Sound".Localize(),
					Schema.intervalSoundType.soundAttribute,
					"Sound".Localize(),
					"Sound to be played from sound bank".Localize(),
					false,
					formatEditor
				);
				propDescCollection.Add( apd );


				var apd2 = new CustomEnableAttributePropertyDescriptor(
					"Positional".Localize(),
					Schema.intervalSoundType.positionalAttribute,
					"Sound".Localize(),
					"Whether sound is located in space relative to object".Localize(),
					false,
					new BoolEditor()
					, new IntervalAnimPositionalEnabledCallback()
				);
				propDescCollection.Add( apd2 );

				var formatEditor3 = new DynamicEnumUITypeEditor( new IntervalAnimSkelLister() );
				formatEditor3.MaxDropDownItems = 16;
				var apd3 = new CustomEnableAttributePropertyDescriptor(
					"Position".Localize(),
					Schema.intervalSoundType.positionAttribute,
					"Sound".Localize(),
					"Specifies joint on object where to attach sound source".Localize(),
					false,
					formatEditor3
						, new IntervalAnimPositionEnabledCallback()
				);
				propDescCollection.Add( apd3 );
			}

			{
				PropertyDescriptorCollection propDescCollection = Schema.keySoundType.Type.GetTag<PropertyDescriptorCollection>();

				var formatEditor = new DynamicEnumUITypeEditor( new KeySoundLister() );
				formatEditor.MaxDropDownItems = 16;
				var apd = new AttributePropertyDescriptor(
					"Sound".Localize(),
					Schema.keySoundType.soundAttribute,
					"Sound".Localize(),
					"Sound to be played from sound bank".Localize(),
					false,
					formatEditor
				);
				propDescCollection.Add( apd );


				var apd2 = new CustomEnableAttributePropertyDescriptor(
					"Positional".Localize(),
					Schema.keySoundType.positionalAttribute,
					"Sound".Localize(),
					"Whether sound is located in space relative to object".Localize(),
					false,
					new BoolEditor()
					, new KeyAnimPositionalEnabledCallback()
				);
				propDescCollection.Add( apd2 );


				var formatEditor3 = new DynamicEnumUITypeEditor( new KeyAnimSkelLister() );
				formatEditor3.MaxDropDownItems = 16;
				var apd3 = new CustomEnableAttributePropertyDescriptor(
					"Position".Localize(),
					Schema.keySoundType.positionAttribute,
					"Sound".Localize(),
					"Specifies joint on object where to attach sound source".Localize(),
					false,
					formatEditor3
						, new KeyAnimPositionEnabledCallback()
				);
				propDescCollection.Add( apd3 );
			}
		}

		public int[] GetEnumIntValues( Type type )
		{
			System.Array valuesArray = Enum.GetValues( type );
			int[] intArray = new int[valuesArray.Length];
			for ( int i = 0; i < valuesArray.Length; ++i )
			{
				intArray[i] = (int) valuesArray.GetValue( i );
			}		
			return intArray;
		}
    }
}
