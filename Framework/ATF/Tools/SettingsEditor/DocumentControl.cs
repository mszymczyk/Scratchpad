using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Controls;
using Sce.Atf.Applications;
using System.Drawing;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using System;
using static Sce.Atf.Controls.TreeControl;

namespace SettingsEditor
{
    public class DocumentControl : UserControl
    {
        public DocumentControl( Editor editor, ICommandService commandService )
        {
            m_editor = editor;
            m_commandService = commandService;

            m_filteredTreeControlEditor = new FilteredTreeControlEditor( editor.CommandService );
            m_filteredTreeControlEditor.Control.Dock = DockStyle.Fill;
            m_filteredTreeControlEditor.TreeControl.MouseDoubleClick += m_treeControl_MouseDoubleClick;
            m_filteredTreeControlEditor.SearchInputUI.Updated += SearchInputUI_Updated;

            m_filteredTreeControlEditor.TreeControlAdapter.LastHitChanged += treeControlAdapter_LastHitChanged;
            m_filteredTreeControlEditor.TreeControl.MouseUp += TreeControl_MouseUp;

            m_propertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid( PropertyGridMode.PropertySorting | PropertyGridMode.DisplayDescriptions | PropertyGridMode.AlwaysShowVerticalScrollBar );
            m_propertyGrid.PropertySorting = Sce.Atf.Controls.PropertyEditing.PropertySorting.Categorized;
            m_propertyGrid.Dock = DockStyle.Fill;

            m_splitContainer = new SplitContainer();
            m_splitContainer.Text = "Split Container";
            m_splitContainer.Panel1.Controls.Add( m_filteredTreeControlEditor.Control );
            m_splitContainer.Panel2.Controls.Add( m_propertyGrid );
            m_splitContainer.Dock = DockStyle.Fill;

            Dock = DockStyle.Fill;
            Controls.Add( m_splitContainer );
        }

        private void TreeControl_MouseUp( object sender, MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Right )
            {
                IEnumerable<object> commands =
                    m_editor.ContextMenuCommandProviders.GetCommands( m_filteredTreeControlEditor.TreeView, m_filteredTreeControlEditor.TreeControlAdapter.LastHit );

                Point screenPoint = m_filteredTreeControlEditor.TreeControl.PointToScreen( new Point( e.X, e.Y ) );
                m_commandService.RunContextMenu( commands, screenPoint );
            }
        }

        public void Setup( DomNode root )
        {
            RootNode = root;

            TreeView treeView = RootNode.As<TreeView>();

            m_filteredTreeControlEditor.TreeView = new FilteredTreeView( treeView, CustomFilter );

            m_propertyGrid.Bind( root.As<DocumentEditingContext>() );

            m_filteredTreeControlEditor.TreeControl.Name = ControlInfo.DisplayName;
        }

        private void SearchInputUI_Updated( object sender, System.EventArgs e )
        {
            // we're filling property editor's search bar while typing in tree's search bar
            // this is to further narrow search so user can quicker find what he's after
            // it's kind of hack, setting only m_propertyGrid.PropertyGridView.FilterPattern
            // will filter properly, but filling search bar is more intuitive
            // because user will see what's actually happening
            // to set this search text box we need to find it among other controls
            // PropertyGrid doesn't expose any API to do it directly
            //
            foreach ( ToolStripItem tsi in m_propertyGrid.ToolStrip.Items )
            {
                if ( tsi is ToolStripAutoFitTextBox )
                {
                    ToolStripAutoFitTextBox t = tsi as ToolStripAutoFitTextBox;
                    t.Text = m_filteredTreeControlEditor.SearchInputUI.SearchPattern;
                    m_propertyGrid.PropertyGridView.FilterPattern = m_filteredTreeControlEditor.SearchInputUI.SearchPattern;
                    break;
                }
            }
        }

        /// <summary>
        /// Callback to determine if an item in the tree is filtered in (return true) or out</summary>
        /// <param name="item">Item tested for filtering</param>
        /// <returns>True if filtered in, false if filtered out</returns>
        private bool CustomFilter( object item )
        {
            // filter first tries to match group name and if failed checks all settings
            //
            DomNode dn = item.As<DomNode>();
            StringSearchInputUI ssi = m_filteredTreeControlEditor.SearchInputUI;
            Group group = null;

            if ( dn.Type == Schema.presetType.Type )
            {
                Preset preset = dn.Cast<Preset>();
                if ( ssi.IsNullOrEmpty()
                    || ssi.Matches( preset.PresetName ) )
                    return true;

                group = preset.Cast<Group>();
            }
            else if ( dn.Type == Schema.groupType.Type )
            {
                group = dn.Cast<Group>();

                if ( ssi.IsNullOrEmpty()
                    || ssi.Matches( group.Name ) )
                    return true;
            }

            if ( group != null )
            {
                foreach ( DynamicProperty dp in group.Properties )
                {
                    if ( ssi.Matches( dp.Name )
                        || ssi.Matches( dp.DisplayName )
                        || ssi.Matches( dp.ExtraName )
                        )
                        return true;
                }

                return false;
            }

            return true; // Don't filter anything if the context doesn't implement IItemView
        }

        private void treeControlAdapter_LastHitChanged( object sender, System.EventArgs e )
        {
            DocumentEditingContext editingContext = RootNode.Cast<DocumentEditingContext>();
            Document document = editingContext.Cast<Document>();
            editingContext.SetInsertionParent( document.Control.GetLastHit() );
        }

        void m_treeControl_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            Preset preset = m_filteredTreeControlEditor.TreeControlAdapter.LastHit.As<Preset>();
            if ( preset != null )
            {
                Group group = preset.Group;
                Preset prevPreset = group.SelectedPresetRef;
                Preset newPreset = prevPreset == preset ? null : preset;

                ITransactionContext transactionContext = RootNode.As<ITransactionContext>();
                transactionContext.DoTransaction(
                    delegate
                    {
                        group.SelectedPresetRef = newPreset;
                    }, newPreset != null
                        ? "Select preset: " + newPreset.PresetName
                        : "Unselect preset" + prevPreset.PresetName
                        );
            }
        }

        public void Rebind()
        {
            System.Diagnostics.Debug.WriteLine( "DocumentControl.Rebind" );
            m_propertyGrid.Bind( null );
            m_propertyGrid.Bind( RootNode.As<DocumentEditingContext>() );
        }

        /// <summary>
        /// Selects DomNode in tree control. This is really hacky way to do it.
        /// We need to remap DomNode to TreeControl.Node, don't know any better way to do it.
        /// </summary>
        /// <param name="domNode">Group or Preset to select</param>
        public void SetSelectedDomNode( DomNode domNode )
        {
            var paths = m_filteredTreeControlEditor.TreeControlAdapter.GetPaths( domNode );
            var list = paths.ToList();
            if ( list.Count > 0 )
            {
                //TreeControl.Node n = m_treeControlAdapter.ExpandPath( list[0] );
                TreeControl.Node n = m_filteredTreeControlEditor.TreeControlAdapter.ExpandPath( list[0] );
                //m_treeControl.SetSelection( n );
                m_filteredTreeControlEditor.TreeControl.SetSelection( n );
            }
        }

        public void SetSearchPattern( string searchPattern )
        {
            m_filteredTreeControlEditor.SearchInputUI.SearchPattern = searchPattern;
        }

        public void ExpandGroup( Group group )
        {
            m_filteredTreeControlEditor.TreeControlAdapter.Expand( group );
        }

        public object GetLastHit()
        {
            return m_filteredTreeControlEditor.TreeControlAdapter.LastHit;
        }

        public TreeControl TreeControl
        {
            get { return m_filteredTreeControlEditor != null ? m_filteredTreeControlEditor.TreeControl : null; }
        }

        public DomNode RootNode { get; set; }
        public ControlInfo ControlInfo { get; set; }

        private FilteredTreeControlEditor m_filteredTreeControlEditor;

        private readonly SplitContainer m_splitContainer;
		private readonly Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyGrid;

        private Editor m_editor;
        private ICommandService m_commandService;
    }
}
