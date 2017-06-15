using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

namespace SettingsEditor
{
    /// <summary>
    /// Adapts DomNode to a Preset</summary>
    public class Preset : DomNodeAdapter
    {
        /// <summary>
        /// Creates given group's preset.
        /// </summary>
        public static Preset CreatePreset( Group group )
        {
            DomNode presetNode = new DomNode( Schema.presetType.Type );
            Preset preset = presetNode.Cast<Preset>(); // OnNodeSet is called at this point with DomNode.Parent == null
            //preset.Group = group;
            preset.m_groupName = group.AbsoluteName;
            presetNode.SetAttribute( Schema.presetType.Type.IdAttribute, group.Name + "Preset" );

            // preset is also a Group
            Group presetGroup = preset.Cast<Group>();

            foreach ( DynamicProperty dp in group.Properties )
            {
                DomNode dpNodeCopy = DomNode.Copy( dp.DomNode );
                dpNodeCopy.InitializeExtensions();
                DynamicProperty dpCopy = dpNodeCopy.Cast<DynamicProperty>();
                presetGroup.Properties.Add( dpCopy );
            }

            foreach ( var ch in group.DomNode.GetChildList(Schema.groupType.groupChild) )
            {
                DomNode chCopy = DomNode.Copy(ch);
                chCopy.InitializeExtensions();
                preset.DomNode.GetChildList(Schema.groupType.groupChild).Add(chCopy);
            }

			// add preset to group, child added gets called with all preset props ready
            // preset.DomNode.Parent is set at this point
            group.Presets.Add( preset );

			// don't change selected preset, if it's null, then group properties will be used
			// user might have done it on purpose
            //// if this is first preset, then select it
            //if ( group.Presets.Count == 1 || group.SelectedPresetRef == null )
            //    group.SelectedPresetRef = preset;

            return preset;
        }

        /// <summary>
        /// Performs one-time initialization when this adapter's DomNode property is set.
        /// The DomNode property is only ever set once for the lifetime of this adapter.</summary>
        protected override void OnNodeSet()
        {
            // when creating new preset with CreatePreset, DomNode.Parent will be null
            // it won't be null when reading from file, for instance
            if (DomNode.Parent != null)
                //Group = DomNode.Parent.Cast<Group>();
                m_groupName = DomNode.Parent.Cast<Group>().AbsoluteName;

            base.OnNodeSet();
        }

        /// <summary>
        /// Gets preset's parent</summary>
        public Group Group
        {
            // can't use "get { return DomNode.Parent.As<Group>(); }" because PresetUniqueIdValidator uses Preset's Group as a category
            // when Preset is deleted, then it's parent is cleared and validator crashes with null pointer exception
            // update: now using GroupName as a category in PresetUniqueIdValidator
            //get;
            //private set;
            get { return DomNode.Parent.As<Group>(); }
        }

        /// <summary>
        /// GroupName is used as a category in PresetUniqueIdValidator
        /// </summary>
        public string GroupName
        {
            get { return m_groupName; }
            set { m_groupName = value; }
        }

        /// <summary>
        /// Gets and sets the Preset's name</summary>
        public string PresetName
        {
            //get { return (string)DomNode.GetAttribute( Schema.presetType.presetNameAttribute ); }
            //set { DomNode.SetAttribute( Schema.presetType.presetNameAttribute, value ); }
            get { return (string)DomNode.GetAttribute( Schema.groupType.nameAttribute ); }
            set { DomNode.SetAttribute( Schema.groupType.nameAttribute, value ); }
        }

        private string m_groupName;
    }
}



