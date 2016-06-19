using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace SettingsEditor
{
    /// <summary>
    /// Editor providing a tree control, listing material's instances.</summary>
    [Export(typeof (IInitializable))]
    [Export(typeof( SettingsLister ) )]
    [Export(typeof (IContextMenuCommandProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SettingsLister : FilteredTreeControlEditor, IControlHostClient, IInitializable
                                  , ITreeView, IItemView
    {
        [ImportingConstructor]
        public SettingsLister(
            ICommandService commandService,
            IControlHostService controlHostService,
            IDocumentRegistry documentRegistry
            )
            : base(commandService)
        {
            m_controlHostService = controlHostService;
            m_documentRegistry = documentRegistry;
        }

        private void documentRegistry_DocumentAdded( object sender, ItemInsertedEventArgs<IDocument> e )
        {
            if ( TreeView != m_filteredTreeView )
                TreeView = m_filteredTreeView;
            else
                TreeControlAdapter.Refresh( TreeView.Root );
        }

        private void documentRegistry_DocumentRemoved( object sender, ItemRemovedEventArgs<IDocument> e )
        {
            if ( TreeView != m_filteredTreeView )
                TreeView = m_filteredTreeView;
            else
                TreeControlAdapter.Refresh( TreeView.Root );
        }

        #region IInitializable Members

        public void Initialize()
        {
            // on initialization, register our tree control with the hosting service
            m_controlHostService.RegisterControl(
                Control,
                new ControlInfo(
                    "Settings Lister".Localize(),
                    "Lists all opened settings files in a single tree".Localize(),
                    StandardControlGroup.Left),
                this);

            m_documentRegistry.DocumentAdded += documentRegistry_DocumentAdded;
            m_documentRegistry.DocumentRemoved += documentRegistry_DocumentRemoved;

            m_filteredTreeView = new FilteredTreeView( this, CustomFilter );
        }

        #endregion

        #region ITreeView Members

        public object Root
        {
            get { return this; }
        }

        public IEnumerable<object> GetChildren( object parent )
        {
            if ( parent == this )
            {
                foreach ( IDocument document in m_documentRegistry.Documents )
                {
                    yield return document;
                }
            }

            DomNode node = parent.As<DomNode>();
            if ( node != null )
            {
                // get child Dom nodes and empty reference "slots"
                foreach ( ChildInfo childInfo in node.Type.Children )
                {
                    // uncomment following to hide properties
                    //if ( childInfo.Equals( Schema.groupType.propChild ) || childInfo.Equals( Schema.presetType.propChild ) )
                    //    continue;

                    foreach ( DomNode child in node.GetChildList( childInfo ) )
                        yield return child;
                }
            }

        }


        #endregion

        #region IItemView Members

        public void GetInfo( object item, ItemInfo info )
        {
            if ( item == this )
            {
                info.Label = "Root";
                info.IsLeaf = false;
                return;
            }

            if ( item.Is<IDocument>() )
            {
                IDocument document = item.Cast<IDocument>();
                info.Label = document.Uri.ToString();
                info.IsLeaf = false;
                return;
            }

            DomNode node = item as DomNode;
            info.Label = node.Type.Name;
            info.AllowLabelEdit = false;

            if ( node.Is<Preset>() )
            {
                Preset p = node.Cast<Preset>();
                Group g = node.Cast<Group>();
                info.Label = p.PresetName;
                info.IsLeaf =  g.Properties.Count == 0;
                info.AllowLabelEdit = true;
                info.ImageIndex = info.GetImageList().Images.IndexOfKey( Resources.Preset );
                if ( p.Group != null && p.Group.SelectedPresetRef == p )
                {
                    info.FontStyle = System.Drawing.FontStyle.Underline;
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey( Resources.SelectedPreset );
                }
            }
            else if ( node.Is<Group>() )
            {
                Group s = node.As<Group>();
                info.Label = s.Name;
                info.IsLeaf = s.NestedGroups.Count == 0 && s.Presets.Count == 0 && s.Properties.Count == 0;
                info.ImageIndex = info.GetImageList().Images.IndexOfKey( Resources.Group );
            }
            else if ( node.Type == Schema.dynamicPropertyType.Type )
            {
                DynamicProperty dp = node.Cast<DynamicProperty>();
                info.Label = string.IsNullOrEmpty( dp.ExtraName ) ? dp.DisplayName : string.Format( "{0}({1})", dp.DisplayName, dp.ExtraName );
                info.IsLeaf = true;
                info.AllowSelect = false;
            }
            else if ( node.Type == Schema.settingsFileType.Type )
            {
                info.Label = "Settings";
            }
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        void IControlHostClient.Activate(Control control)
        {
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another control or "host" control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        void IControlHostClient.Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control.</summary>
        /// <param name="control">Client control to be closed</param>
        /// <returns>true if the control can close, or false to cancel.</returns>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

        protected override void Configure(out Sce.Atf.Controls.TreeControl treeControl,
                                          out TreeControlAdapter treeControlAdapter)
        {
            base.Configure(out treeControl, out treeControlAdapter);
            treeControl.ShowRoot = false;
            treeControl.AllowDrop = false;
            treeControl.Name = "SettingsLister";
            treeControl.SelectionMode = SelectionMode.One;
        }

        /// <summary>
        /// Raises the LastHitChanged event and performs custom processing</summary>
        /// <param name="e">Event args</param>
        protected override void OnLastHitChanged(EventArgs e)
        {
            DomNode domNode = TreeControlAdapter.LastHit.As<DomNode>();
            if ( domNode != null )
            {
                DomNode root = domNode.GetRoot();
                Document document = root.As<Document>();
                if ( document != null )
                {
                    m_controlHostService.Show( document.Control );

                    if ( domNode.Is<DynamicProperty>() )
                    {
                        DynamicProperty dp = domNode.Cast<DynamicProperty>();
                        document.Control.SetSearchPattern( dp.DisplayName );
                        document.Control.SetSelectedDomNode( domNode.Parent );
                    }
                    else
                    {
                        if ( !string.IsNullOrEmpty( SearchInputUI.SearchPattern ) )
                            document.Control.SetSearchPattern( SearchInputUI.SearchPattern );
                        else
                            document.Control.SetSearchPattern( "" );

                        document.Control.SetSelectedDomNode( domNode );
                    }
                }
            }

            base.OnLastHitChanged(e);
        }

        /// <summary>
        /// Callback to determine if an item in the tree is filtered in (return true) or out</summary>
        /// <param name="item">Item tested for filtering</param>
        /// <returns>True if filtered in, false if filtered out</returns>
        private bool CustomFilter( object item )
        {
            if ( item == this )
                return true;

            // filter first tries to match group name and if failed checks all settings
            //
            DomNode dn = item.As<DomNode>();
            Group group = null;

            if ( dn.Type == Schema.presetType.Type )
            {
                Preset preset = dn.Cast<Preset>();

                if ( SearchInputUI.IsNullOrEmpty()
                    || SearchInputUI.Matches( preset.PresetName ) )
                    return true;

                group = preset.Cast<Group>();
            }
            else if ( dn.Type == Schema.groupType.Type )
            {
                group = dn.Cast<Group>();

                if ( SearchInputUI.IsNullOrEmpty()
                    || SearchInputUI.Matches( group.Name ) )
                    return true;
            }
            else if ( dn.Type == Schema.dynamicPropertyType.Type )
            {
                DynamicProperty dp = dn.Cast<DynamicProperty>();
                if (   SearchInputUI.IsNullOrEmpty()
                    || SearchInputUI.Matches( dp.Name )
                    || SearchInputUI.Matches( dp.DisplayName )
                    || SearchInputUI.Matches( dp.ExtraName )
                    )
                    return true;

                return false;
            }

            if ( group != null )
            {
                foreach ( DynamicProperty dp in group.Properties )
                {
                    if ( SearchInputUI.Matches( dp.Name )
                        || SearchInputUI.Matches( dp.DisplayName )
                        || SearchInputUI.Matches( dp.ExtraName )
                        )
                        return true;
                }

                return false;
            }

            return true; // Don't filter anything if the context doesn't implement IItemView
        }

        private readonly IControlHostService m_controlHostService;
        private readonly IDocumentRegistry m_documentRegistry;

        private FilteredTreeView m_filteredTreeView;
    }
}
