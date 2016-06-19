using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Editor providing a tree control, listing material's instances.</summary>
    [Export(typeof (IInitializable))]
    [Export(typeof( MaterialInstancesLister ) )]
    [Export(typeof (IContextMenuCommandProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MaterialInstancesLister : FilteredTreeControlEditor, IControlHostClient, IInitializable,
                                     IContextMenuCommandProvider, ICommandClient
                                    , ITreeView, IItemView
                                    //, ISelectionContext
                                    , IAdaptable
    {
        [ImportingConstructor]
        public MaterialInstancesLister(
            ICommandService commandService,
            IControlHostService controlHostService,
            IContextRegistry contextRegistry)
            : base(commandService)
        {
            m_controlHostService = controlHostService;
            m_contextRegistry = contextRegistry;                        
            m_contextRegistry.ActiveContextChanged += ContextRegistry_ActiveContextChanged;
        }

       

        /// <summary>
        /// Refreshes project tree.
        /// </summary>
        public void Refresh()
        {
            //if ( m_selectedDomNode != null )
            //{
            //    TreeControlAdapter.Refresh( TreeView.Root );
            //}
        }
        
        #region IInitializable Members

        public void Initialize()
        {

            //string addNewSubGame = "Add new SubGame".Localize();
            //CommandService.RegisterCommand(
            //   Command.CreateNewSubGame,
            //   StandardMenu.File,
            //   StandardCommandGroup.FileNew,
            //   addNewSubGame,
            //   addNewSubGame,
            //   Keys.None,
            //   null,
            //   CommandVisibility.ContextMenu,
            //   this);

            //string addExistingEubGame = "Add existing SubGame".Localize();
            //CommandService.RegisterCommand(
            //    Command.AddSubGame,
            //   StandardMenu.File,
            //   StandardCommandGroup.FileNew,
            //    addExistingEubGame,
            //    addExistingEubGame,
            //    Keys.None,
            //    null,
            //    CommandVisibility.ContextMenu,
            //    this);

            //CommandService.RegisterCommand(
            //   Command.Exclude,
            //  StandardMenu.File,
            //  StandardCommandGroup.FileNew,
            //   "Exclude SubGame".Localize(),
            //   "Exlude from master level".Localize(),
            //   Keys.None,
            //   null,
            //   CommandVisibility.ContextMenu,
            //   this);


            //string resolveSubGame = "Resolve SubGame".Localize();
            //CommandService.RegisterCommand(
            //   Command.Resolve,
            //  StandardMenu.File,
            //  StandardCommandGroup.FileNew,
            //   resolveSubGame,
            //   resolveSubGame,
            //   Keys.None,
            //   null,
            //   CommandVisibility.ContextMenu,
            //   this);


            //string unresolveSubGame = "Unresolve SubGame".Localize();
            //CommandService.RegisterCommand(
            //   Command.Unresolve,
            //  StandardMenu.File,
            //  StandardCommandGroup.FileNew,
            //   unresolveSubGame,
            //   unresolveSubGame,
            //   Keys.None,
            //   null,
            //   CommandVisibility.ContextMenu,
            //   this);


            string createInstance = "Create Instance".Localize();
            CommandService.RegisterCommand(
               Command.CreateInstance,
               StandardMenu.File,
               StandardCommandGroup.FileNew,
               createInstance,
               createInstance,
               Keys.None,
               null,
               CommandVisibility.ContextMenu,
               this );

            // on initialization, register our tree control with the hosting service
            m_controlHostService.RegisterControl(
                Control,
                new ControlInfo(
                    "Material Instances".Localize(),
                    "Lists instances of current material".Localize(),
                    StandardControlGroup.Left),
                this);

            //string ext = m_gameEditor.Info.Extensions[0];
            //m_fileFilter = string.Format("SubGame (*{0})|*{0}", ext);

            //m_treeView = new FilteredTreeView( (ITreeView)this, DefaultFilter );
            //m_treeView = this;

            if (m_scriptingService != null)
                m_scriptingService.SetVariable("matInstLister", this);
        }

        #endregion

        #region ITreeView Members

        public object Root
        {
            //get { return m_selectedDomNode; }
            get { return m_circuitEditingContext.DomNode; }
        }

        public IEnumerable<object> GetChildren( object parent )
        {
            CircuitDocument document = parent.As<CircuitDocument>();
            if ( document != null )
                return document.MaterialInstances;

            return new object[] { };
        }


        #endregion

        #region IItemView Members

        public void GetInfo( object item, ItemInfo info )
        {
            //IListable listable = item.As<IListable>();
            //if ( listable != null )
            //{
            //    listable.GetInfo( info );
            //    return;
            //}

            //IResource resource = item.As<IResource>();
            //if ( resource != null && !item.Is<Game>() )
            //{
            //    info.Label = Path.GetFileName( resource.Uri.LocalPath );
            //    info.ImageIndex = info.GetImageList().Images.IndexOfKey( Sce.Atf.Resources.ResourceImage );
            //    return;
            //}

            //// If the object has a name use it as the label (overriding whatever may have been set previously)
            //INameable nameable = Adapters.As<INameable>( item );
            //if ( nameable != null )
            //    info.Label = nameable.Name;
            //info.Label = "Instance0";
            MaterialInstance materialInstance = item.As<MaterialInstance>();
            if ( materialInstance != null )
                info.Label = materialInstance.Name;
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        void IControlHostClient.Activate(Control control)
        {
            //if (TreeView != null)
            //    m_contextRegistry.ActiveContext = TreeView;
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

        #region IContextMenuCommandProvider Members

        IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object target)
        {
            ICommandClient cmdclient = (ICommandClient)this;

            //if ( context == this.TreeView &&
            //    (Adapters.Is<CircuitEditingContext>( target ) ) )
            if ( context == this.TreeView )
            {
                foreach ( Command command in Enum.GetValues( typeof( Command ) ) )
                {
                    if ( cmdclient.CanDoCommand( command ) )
                    {
                        yield return command;
                    }
                }
            }

            yield return null;
        }

        #endregion

        #region ICommandClient Members

        bool ICommandClient.CanDoCommand(object commandTag)
        {
            bool cando = false;
            switch ( (Command)commandTag )
            {
                //    case Command.CreateNewSubGame:
                //    case Command.AddSubGame:
                //        {
                //            IGame game = TreeControlAdapter.LastHit.As<IGame>();
                //            if (game == null)
                //            {
                //                GameReference gameRef = TreeControlAdapter.LastHit.As<GameReference>();
                //                game = (gameRef != null) ? gameRef.Target : null;
                //            }
                //            cando = game != null;
                //        }
                //        break;
                //    case Command.Exclude:
                //        cando = TreeControlAdapter.LastHit.Is<GameReference>();
                //        break;
                //    case Command.Resolve:
                //        {
                //            GameReference gameRef = TreeControlAdapter.LastHit.As<GameReference>();
                //            cando = gameRef != null && gameRef.Target == null;
                //        }                    
                //        break;

                //    case Command.Unresolve:
                //        {
                //            GameReference gameRef = TreeControlAdapter.LastHit.As<GameReference>();
                //            cando = gameRef != null && gameRef.Target != null;
                //        }                    
                //        break;                    
                case Command.CreateInstance:
                    {
                        Circuit circuit = m_circuitEditingContext.As<Circuit>(); // m_circuitEditingContext can be null
                        if ( circuit != null )
                        {
                            MaterialModule materialModule = MaterialGraphUtil.GetMaterialModule( circuit );
                            if ( materialModule != null )
                            {
                                IList<IMaterialParameterModule> materialParameterModules = MaterialGraphUtil.GetMaterialParameterModules( materialModule );
                                if ( materialParameterModules.Count > 0 )
                                    cando = true;
                            }
                        }
                    }
                    break;
            }
            return cando;
        }

        void ICommandClient.DoCommand(object commandTag)
        {
            //IGame game = TreeControlAdapter.LastHit.As<IGame>();
            //if (game == null)
            //{
            //    GameReference gameRef = TreeControlAdapter.LastHit.As<GameReference>();
            //    game = gameRef.Target;
            //}
            //IDocument gameDocument = game.As<IDocument>();
            //string filePath = null;

            switch ( (Command)commandTag )
            {
                //    case Command.CreateNewSubGame:
                //        filePath = Util.GetFilePath(m_fileFilter,
                //            System.IO.Path.GetDirectoryName(gameDocument.Uri.LocalPath), true);
                //        if (!string.IsNullOrEmpty(filePath))
                //        {
                //            try
                //            {
                //                if (!m_gameEditor.Info.IsCompatiblePath(filePath))
                //                    throw new Exception("Incompatible file type " + filePath);

                //                Uri ur = new Uri(filePath);
                //                if (m_gameDocumentRegistry.FindDocument(ur) != null)
                //                    throw new Exception(filePath + " is already open");
                //                GameDocument subGame = GameDocument.OpenOrCreate(ur, m_schemaLoader);
                //                subGame.Dirty = true;
                //                GameReference gameRef = GameReference.CreateNew(subGame);
                //                IHierarchical parent = game.As<IHierarchical>();
                //                parent.AddChild(gameRef);
                //                // because we performing this operation outside of TransactionContext
                //                // we must set Document Dirty flag.
                //                gameDocument.Dirty = true;

                //            }
                //            catch (Exception ex)
                //            {
                //                MessageBox.Show(m_mainWindow.DialogOwner, ex.Message);
                //            }
                //        }
                //        break;
                //    case Command.AddSubGame:

                //        filePath = Util.GetFilePath(m_fileFilter, 
                //            System.IO.Path.GetDirectoryName(gameDocument.Uri.LocalPath), 
                //            false);

                //        if (!string.IsNullOrEmpty(filePath))
                //        {
                //            try
                //            {
                //                if (!m_gameEditor.Info.IsCompatiblePath(filePath))
                //                    throw new Exception("Incompatible file type " + filePath);

                //                Uri ur = new Uri(filePath);
                //                if (m_gameDocumentRegistry.FindDocument(ur) != null)
                //                    throw new Exception(filePath + " is already open");

                //                GameReference gameRef = GameReference.CreateNew(ur);
                //                gameRef.Resolve();
                //                IHierarchical parent = game.As<IHierarchical>();
                //                parent.AddChild(gameRef);

                //                // because we performing this operation outside of TransactionContext
                //                // we must set Document Dirty flag.
                //                gameDocument.Dirty = true;
                //                RefreshLayerContext();
                //            }
                //            catch (Exception ex)
                //            {
                //                MessageBox.Show(m_mainWindow.DialogOwner, ex.Message);
                //            }
                //        }
                //        break;
                //    case Command.Exclude:
                //        {
                //            GameReference gameRef = TreeControlAdapter.LastHit.As<GameReference>();
                //            gameDocument = gameRef.DomNode.Parent.Cast<IDocument>();
                //            GameDocument subDoc = gameRef.Target.Cast<GameDocument>();

                //            bool exclue = true;
                //            bool save = false;
                //            if (subDoc.Dirty)
                //            {
                //                string msg = "Save changes\r\n" + subDoc.Uri.LocalPath;
                //                DialogResult dlgResult =
                //                    MessageBox.Show(m_mainWindow.DialogOwner, msg, m_mainWindow.Text
                //                    , MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                //                save = dlgResult == DialogResult.Yes;
                //                exclue = dlgResult != DialogResult.Cancel;
                //            }

                //            if (save)
                //                subDoc.Save(subDoc.Uri, m_schemaLoader);

                //            if (exclue)
                //            {
                //                gameRef.DomNode.RemoveFromParent();
                //                // because we performing this operation outside of TransactionContext
                //                // we must set Document Dirty flag.                        
                //                gameDocument.Dirty = true;
                //                UpdateGameObjectReferences();
                //                RefreshLayerContext();
                //            }
                //        }
                //        break;
                //    case Command.Resolve:
                //        {
                //            GameReference gameRef = TreeControlAdapter.LastHit.As<GameReference>();
                //            gameRef.Resolve();
                //            TreeControlAdapter.Refresh(gameRef);
                //            RefreshLayerContext();
                //        }
                //        break;
                //    case Command.Unresolve:
                //        {
                //            try
                //            {
                //                GameReference gameRef = TreeControlAdapter.LastHit.As<GameReference>();
                //                GameDocument subDoc = gameRef.Target.Cast<GameDocument>();
                //                bool unresolve = true;
                //                bool save = false;
                //                if (subDoc.Dirty)
                //                {
                //                    string msg = "Save changes\r\n" + subDoc.Uri.LocalPath;                                
                //                    DialogResult dlgResult =
                //                        MessageBox.Show(m_mainWindow.DialogOwner, msg, m_mainWindow.Text
                //                        , MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                //                    save = dlgResult == DialogResult.Yes;
                //                    unresolve = dlgResult != DialogResult.Cancel;
                //                }
                //                //cando = gameRef != null && gameRef.Target != null;
                //                if (save) 
                //                    subDoc.Save(subDoc.Uri, m_schemaLoader);
                //                if (unresolve)
                //                {
                //                    gameRef.Unresolve();
                //                    UpdateGameObjectReferences();
                //                    RefreshLayerContext();

                //                }
                //                TreeControlAdapter.Refresh(gameRef);
                //            }
                //            catch (Exception ex)
                //            {
                //                MessageBox.Show(m_mainWindow.DialogOwner, ex.Message);
                //            }                             
                //        }
                //        break;
                //    default:
                //        throw new ArgumentOutOfRangeException("commandTag");
                case Command.CreateInstance:
                    {
                        MaterialModule materialModule = MaterialGraphUtil.GetMaterialModule( m_circuitEditingContext.Cast<Circuit>() );
                        IList<IMaterialParameterModule> materialParameterModules = MaterialGraphUtil.GetMaterialParameterModules( materialModule );

                        if ( materialParameterModules.Count > 0 )
                        {
                            CircuitDocument circuitDocument = m_circuitEditingContext.Cast<CircuitDocument>();
                            ITransactionContext transactionContext = m_circuitEditingContext.Cast<ITransactionContext>();

                            transactionContext.DoTransaction( delegate
                            {
                                MaterialInstance materialInstance = MaterialInstance.CreateMaterialInstance();
                                circuitDocument.MaterialInstances.Add( materialInstance );

                                foreach ( IMaterialParameterModule parameterModule in materialParameterModules )
                                {
                                    MaterialInstanceParameter mip = MaterialInstanceParameter.CreateFromParameterModule( parameterModule );
                                    materialInstance.Parameters.Add( mip );
                                }
                            },
                            "Create material instance" );
                        }
                    }
                    break;
            }
            //m_designView.InvalidateViews();
            //Refresh();
        }

        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
            
        }

        #endregion

        protected override void Configure(out Sce.Atf.Controls.TreeControl treeControl,
                                          out TreeControlAdapter treeControlAdapter)
        {
            base.Configure(out treeControl, out treeControlAdapter);
            treeControl.ShowRoot = false;
            treeControl.AllowDrop = true;
            treeControl.Name = "MaterialInstancesLister";
            // to support child reordering implement IOrderedInsertionContext on GameContext
            // and uncomment following line
            //
            // treeControl.ShowDragBetweenCue = true;
        }

        /// <summary>
        /// Raises the LastHitChanged event and performs custom processing</summary>
        /// <param name="e">Event args</param>
        protected override void OnLastHitChanged(EventArgs e)
        {
            //// forward "last hit" information to the GameContext which needs to know
            ////  where to insert objects during copy/paste and drag/drop. The base tracks
            ////  the last clicked and last dragged over tree objects.
            //GameContext context = m_selectionContext.As<GameContext>();
            //if ( context != null )
            //    context.SetActiveItem( LastHit );

            base.OnLastHitChanged(e);
        }

        private void ContextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            CircuitEditingContext ctx = m_contextRegistry.GetActiveContext<CircuitEditingContext>();
            if ( ctx == null )
            {
                TreeView = null;
                m_circuitEditingContext = null;
            }
            else
            {
                m_circuitEditingContext = ctx;
                ITreeView treeView = new FilteredTreeView( (ITreeView)this, DefaultFilter );
                TreeView = treeView;
            }
            //if ( m_selectionContext != null )
            //{
            //    m_selectionContext.SelectionChanged -= selectionContext_SelectionChanged;
            //}
            //m_selectionContext = ctx.As<ISelectionContext>();
            //if ( m_selectionContext != null )
            //{
            //    m_selectionContext.SelectionChanged += selectionContext_SelectionChanged;
            //}

            //if ( m_observationContext != null )
            //{
            //    m_observationContext.ItemRemoved -= m_observationContext_ItemRemoved;
            //}
            //m_observationContext = ctx.As<IObservableContext>();
            //if ( m_observationContext != null )
            //{
            //    m_observationContext.ItemRemoved += m_observationContext_ItemRemoved;
            //}

            //m_editingContext = m_selectionContext.As<EditingContext>();

            //TreeView = null;

            if ( m_validationContext != null )
            {
                m_validationContext.Ended -= ValidationContext_Ended;
            }
            m_validationContext = (IValidationContext)ctx;
            if ( m_validationContext != null )
            {
                m_validationContext.Ended += ValidationContext_Ended;
            }
        }

        //private void selectionContext_SelectionChanging( object sender, EventArgs e )
        //{
        //    SelectionChanging.Raise( sender, e );
        //}

        private void selectionContext_SelectionChanged( object sender, EventArgs e )
        {
            //if ( m_selectionContext.LastSelected != null )
            //{
            //    DomNode domNode = m_selectionContext.LastSelected.As<DomNode>();
            //    if ( domNode.Is<IGameObject>() )
            //    {
            //        m_selectedDomNode = domNode;
            //        TreeView = m_treeView;
            //        TreeControl.ExpandAll();
            //    }
            //}

            // setting TreeView to null when m_selectionContext.LastSelected == null causes null pointer exceptions in TreeControl...
            // this can have something to do with changing selection (and removing event handlers) while "selection changed" is being processed
            //
        }

        private void m_observationContext_ItemRemoved( object sender, ItemRemovedEventArgs<object> e )
        {
            //if ( e.Item.Equals(m_selectedDomNode) )
            //{
            //    m_selectedDomNode = null;
            //    TreeView = null;
            //}
        }

        private void ValidationContext_Ended(object sender, EventArgs e)
        {
            Refresh();
        }

        public object GetAdapter( Type type )
        {
            if ( type.IsAssignableFrom( typeof( CircuitEditingContext ) ) )
                return m_circuitEditingContext;

            return null;
        }

        //private void RefreshLayerContext()
        //{
        //    var layerContext = m_gameDocumentRegistry.MasterDocument.As<LayeringContext>();
        //    if (layerContext != null)
        //        layerContext.RefreshRoot();
        //}
        ///// <summary>
        ///// Unresolve all the GameObjectReferences, 
        ///// if the target object is belong to the removed documents</summary>
        //private void UpdateGameObjectReferences()
        //{

        //    // Refresh LayerListers, after the following subgame operations.
        //    // adding 
        //    // unresolving
        //    // excluding  
        //    // for all Layer Lister need to be refreshed.

        //    foreach (var subDoc in m_gameDocumentRegistry.Documents)
        //    {
        //        var rootNode = subDoc.Cast<DomNode>();
        //        foreach (DomNode childNode in rootNode.Subtree)
        //        {
        //            var gameObjectReference = childNode.As<GameObjectReference>();
        //            if (gameObjectReference == null) continue;
        //            var targetNode = Adapters.As<DomNode>(gameObjectReference.Target);
        //            if(targetNode == null) continue;
        //            var targetDoc = targetNode.GetRoot().As<IGameDocument>();
        //            if(!m_gameDocumentRegistry.Contains(targetDoc))
        //                gameObjectReference.UnResolve();
        //        }
        //    }
        //}

        private readonly IControlHostService m_controlHostService;
        private readonly IContextRegistry m_contextRegistry;
                
        //[Import(AllowDefault = false)]
        //private IDesignView m_designView = null;

        //[Import(AllowDefault = false)]
        //private GameEditor m_gameEditor = null;

        //[Import(AllowDefault = false)]
        //private IMainWindow m_mainWindow = null;

        //[Import(AllowDefault = false)]
        //private SchemaLoader m_schemaLoader = null;

        //[Import(AllowDefault = false)]
        //private IGameDocumentRegistry m_gameDocumentRegistry = null;

        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;

        private IValidationContext m_validationContext;
        //private ISelectionContext m_selectionContext;
        //private IObservableContext m_observationContext; // for tracking node deletions
        private CircuitEditingContext m_circuitEditingContext;

        //DomNode m_selectedDomNode;
        //ITreeView m_treeView;

        //public event EventHandler SelectionChanging;
        //public event EventHandler SelectionChanged;

        //private string m_fileFilter;
        private enum Command
        {
            //CreateNewSubGame,
            //AddSubGame,
            //Exclude,
            //Resolve,
            //Unresolve
            CreateInstance
        }
    }
}
