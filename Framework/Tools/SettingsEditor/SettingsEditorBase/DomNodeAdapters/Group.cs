using System.Collections.Generic;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.CurveEditing;
using System;

namespace SettingsEditor
{
    /// <summary>
    /// Adapts DomNode to a Group
    /// Name 'Group' relates directly to c++ struct that's being generated for this node.
    /// </summary>
    public class Group : DomNodeAdapter, ICurveSet
    {
        /// <summary>
        /// Performs one-time initialization when this adapter's DomNode property is set.
        /// The DomNode property is only ever set once for the lifetime of this adapter.</summary>
        protected override void OnNodeSet()
        {
            DomNode.ChildInserted += DomNode_ChildInserted;

            base.OnNodeSet();
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            // we have to fill Preset here because there's no 'ParentSet' event
            // this callback will be called for every group in the hierarchy
            // that's why we're checking if parent is actually this node
            if (!e.Parent.Equals(this.DomNode))
                return;

            Preset preset = e.Child.As<Preset>();
            Group group = e.Parent.Cast<Group>();
            if (preset != null && group != null)
                preset.GroupName = group.AbsoluteName;
        }

        /// <summary>
        /// Gets or sets the referenced UI object</summary>
        public Preset SelectedPresetRef
        {
            get { return GetReference<Preset>( Schema.groupType.selectedPresetRefAttribute ); }
            set { SetReference( Schema.groupType.selectedPresetRefAttribute, value ); }
        }

        /// <summary>
        /// Name of the group</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute( Schema.groupType.nameAttribute ); }
            set { DomNode.SetAttribute( Schema.groupType.nameAttribute, value ); }
        }

        /// <summary>
        /// Gets the list of all tracks in the group</summary>
        public IList<Group> NestedGroups
        {
            get { return GetChildList<Group>( Schema.groupType.groupChild ); }
        }

        /// <summary>
        /// Gets the list of all tracks in the group</summary>
        public IList<Preset> Presets
        {
            get { return GetChildList<Preset>( Schema.groupType.presetChild ); }
        }

        /// <summary>
        /// Returns list of instance parameters
        /// </summary>
        public IList<DynamicProperty> Properties
        {
            get { return GetChildList<DynamicProperty>( Schema.groupType.propChild ); }
        }

        /// <summary>
        /// This property is not stored within Dom. It's being setup each time group is recreated.
        /// </summary>
        public SettingGroup SettingGroup
        {
            get;
            set;
        }

        public string AbsoluteName
        {
            get
            {
                string name = Name;
                Group group = DomNode.Parent.As<Group>();
                while ( group != null )
                {
                    name = group.Name + "." + name;
                    group = group.DomNode.Parent.As<Group>();
                }

                return name;
            }
        }

        public IList<ICurve> Curves
        {
            get
            {
                List<ICurve> curves = new List<ICurve>();
                foreach( DynamicProperty dp in Properties )
                {
                    if ( dp.PropertyType == SettingType.AnimCurve )
                        curves.Add( dp.AnimCurveValue );
                }
                return curves;
            }
        }

        public System.ComponentModel.PropertyDescriptor[] PropertyDescriptors { get; set; }
    }
}



