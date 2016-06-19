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
            Preset preset = presetNode.Cast<Preset>();
            presetNode.SetAttribute( Schema.presetType.Type.IdAttribute, group.Name + "Preset" );
            group.Presets.Add( preset );

            // preset is also a Group
            Group presetGroup = preset.Cast<Group>();

            foreach ( DynamicProperty dp in group.Properties )
            {
                DomNode dpNodeCopy = DomNode.Copy( dp.DomNode );
                dpNodeCopy.InitializeExtensions();
                DynamicProperty dpCopy = dpNodeCopy.Cast<DynamicProperty>();
                presetGroup.Properties.Add( dpCopy );
            }

            return preset;
        }

        /// <summary>
        /// Performs one-time initialization when this adapter's DomNode property is set.
        /// The DomNode property is only ever set once for the lifetime of this adapter.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
        }

        /// <summary>
        /// Gets preset's parent</summary>
        public Group Group
        {
            get { return DomNode.Parent.As<Group>(); }
        }

        /// <summary>
        /// Gets and sets the Preset's name</summary>
        public string PresetName
        {
            get { return (string)DomNode.GetAttribute( Schema.presetType.presetNameAttribute ); }
            set { DomNode.SetAttribute( Schema.presetType.presetNameAttribute, value ); }
        }
    }
}



