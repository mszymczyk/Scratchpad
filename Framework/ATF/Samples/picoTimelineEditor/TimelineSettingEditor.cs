//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

using picoTimelineEditor.DomNodeAdapters;

namespace picoTimelineEditor
{
    /// <summary>
    /// Component to implement a DOM explorer, which is a tree view and property grid for exploring a DOM.
    /// It is useful as a raw view on DOM data for diagnosing DOM problems.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(TimelineSettingEditor))]
	//[Export(typeof(IContextMenuCommandProvider))]
	[PartCreationPolicy( CreationPolicy.Any )]
	public class TimelineSettingEditor : IControlHostClient, IInitializable//, ICommandClient, IContextMenuCommandProvider
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service</param>
        [ImportingConstructor]
        public TimelineSettingEditor(IControlHostService controlHostService, IContextRegistry contextRegistry, ICommandService commandService)
		{
			m_controlHostService = controlHostService;
			m_contextRegistry = contextRegistry;
			m_commandService = commandService;

			m_treeControl = new TreeControl();
			m_treeControl.Dock = DockStyle.Fill;
			m_treeControl.AllowDrop = true;
			m_treeControl.SelectionMode = SelectionMode.MultiExtended;
			m_treeControl.ImageList = ResourceUtil.GetImageList16();
			m_treeControl.NodeSelectedChanged += treeControl_NodeSelectedChanged;
			m_treeControl.SelectionChanged += treeControl_SelectionChanged;
			m_treeControl.MouseUp += treeControl_MouseUp;

			m_treeControlAdapter = new TreeControlAdapter( m_treeControl );
			m_treeView = new TreeView();

			//m_propertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
			//m_propertyGrid.Dock = DockStyle.Fill;

			m_splitContainer = new SplitContainer();
			m_splitContainer.Text = "Timeline Setting Editor";
			//m_splitContainer.Panel1.Controls.Add(m_treeControl);
			//m_splitContainer.Panel2.Controls.Add(m_propertyGrid);

			//m_uberControl = new UserControl { Dock = DockStyle.Fill };

			int x = 2, y = 2;
			var buttonHeight = -1;

			{
				string[] names = Enum.GetNames( typeof(Command) );
				m_addButton = CreateSplitButton( names, ref x, ref y, ref buttonHeight );
				m_addButton.ContextMenuStrip.ItemClicked += splitButtonContextMenuStrip_ItemClicked;
				m_addButton.Click += splitButton_Click;
				m_splitContainer.Panel1.Controls.Add( m_addButton );
			}

			{
				m_treeControl.Location = new Point( 0, buttonHeight + 2 );
				m_treeControl.Anchor =
                        AnchorStyles.Left | AnchorStyles.Top |
                        AnchorStyles.Right | AnchorStyles.Bottom;

				m_treeControl.Width = m_splitContainer.Panel1.Width;
				m_treeControl.Height = m_splitContainer.Panel1.Height - buttonHeight - 2;

				m_splitContainer.Panel1.Controls.Add( m_treeControl );
			}

		}

		private Sce.Atf.Controls.SplitButton CreateSplitButton( string[] items, ref int x, ref int y, ref int height )
		{
			//var btn = new Button { Text = text };
			var splitButton = new Sce.Atf.Controls.SplitButton();

			splitButton.ShowSplit = true;
			splitButton.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
			int maxWidth = 0;
			foreach( string item in items )
			{
				splitButton.ContextMenuStrip.Items.Add( item );
				var size = TextRenderer.MeasureText( item, splitButton.Font );
				maxWidth = MathUtil.Max<int>( maxWidth, size.Width );
			}

			splitButton.Text = items[0];

			splitButton.Width = maxWidth + 20;

			splitButton.Location = new Point( x, y );
			splitButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;

			x += splitButton.Width + 2;

			if ( height == -1 )
				height = splitButton.Height;

			return splitButton;
		}

		void splitButtonContextMenuStrip_ItemClicked( object sender, ToolStripItemClickedEventArgs e )
		{
			string text = e.ClickedItem.ToString();
			m_addButton.Text = text;

			splitButton_Click( sender, e );
		}

		void splitButton_Click( object sender, EventArgs e )
		{
			Command command;
			if ( Enum.TryParse<Command>( m_addButton.Text, out command ) )
			{
				DoCommandImpl( command );
			}
		}

        /// <summary>
        /// Gets or sets the root DomNode to explore</summary>
        public DomNode Root
        {
            get { return m_treeView.RootNode; }
            set
            {
                if (value != null)
                {
                    m_treeView.RootNode = value;
                    m_treeControlAdapter.TreeView = m_treeView;
                }
                else
                {
                    m_treeControlAdapter.TreeView = null;
                    m_treeView.RootNode = null;
                }
            }
        }

		///// <summary>
		///// Gets or sets whether DomNode adapters are displayed in the tree view</summary>
		//public bool ShowAdapters
		//{
		//	get { return m_treeView.ShowAdapters; }
		//	set { m_treeView.ShowAdapters = value; }
		//}

        /// <summary>
        /// Gets the TreeControl</summary>
        public TreeControl TreeControl
        {
            get { return m_treeControl; }
        }

        /// <summary>
        /// Gets the TreeControlAdapter</summary>
        public TreeControlAdapter TreeControlAdapter
        {
            get { return m_treeControlAdapter; }
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering control</summary>
        public virtual void Initialize()
        {
			m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;

			//m_commandService.RegisterCommand( Add_CResLod_CommandInfo, this );

            m_controlHostService.RegisterControl(m_splitContainer,
                "Timeline Setting Editor".Localize(),
                "Hierarchical setting editor".Localize(),
                StandardControlGroup.Bottom,
                this);
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Activates the client control</summary>
        /// <param name="control">Client control to be activated</param>
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
        /// <returns>True if the control can close, or false to cancel</returns>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

		//#region IContextMenuCommandProvider Members

		///// <summary>
		///// Gets tags for context menu (right click) commands</summary>
		///// <param name="context">Context containing target object</param>
		///// <param name="clicked">Right clicked object, or null if none</param>
		//IEnumerable<object> IContextMenuCommandProvider.GetCommands( object context, object clicked )
		//{
		//	return EmptyEnumerable<object>.Instance;
		//}

		//#endregion

		private void contextRegistry_ActiveContextChanged( object sender, EventArgs e )
		{
			//m_curveEditorControl.Context = m_contextRegistry.ActiveContext;

			if (m_selectionContext != null)
				m_selectionContext.SelectionChanged -= selectionContext_SelectionChanged;
			m_selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
			if (m_selectionContext != null)
				m_selectionContext.SelectionChanged += selectionContext_SelectionChanged;

			//if (m_validationContext != null)
			//{
			//	m_validationContext.Ended -= RefreshCurveControl;
			//	m_validationContext.Cancelled -= RefreshCurveControl;
			//}

			//m_validationContext = m_contextRegistry.GetActiveContext<IValidationContext>();
			//if (m_validationContext != null)
			//{
			//	m_validationContext.Ended += RefreshCurveControl;
			//	m_validationContext.Cancelled += RefreshCurveControl;
			//}

			//if (m_observableContext != null)
			//	m_observableContext.ItemChanged -= ObservableContextItemChanged;
			//m_observableContext = m_contextRegistry.GetActiveContext<IObservableContext>();
			//if (m_observableContext != null)
			//	m_observableContext.ItemChanged += ObservableContextItemChanged;

			if ( m_observableContext != null )
				m_observableContext.ItemRemoved -= ObservableContextItemRemoved;
			m_observableContext = m_contextRegistry.GetActiveContext<IObservableContext>();
			if ( m_observableContext != null )
				m_observableContext.ItemRemoved += ObservableContextItemRemoved;

			m_transactionContext = m_contextRegistry.GetActiveContext<ITransactionContext>();
		}

		/// <summary>
		/// Performs custom actions on SelectionChanged events</summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Event args</param>
		void selectionContext_SelectionChanged( object sender, EventArgs e )
		{
			//IList<ICurve> curves;
			//if (MultiSelectionOverlay)
			//{
			//	// Restore original curve colors
			//	foreach (KeyValuePair<ICurve, Color> pair in m_originalCurveColors)
			//		pair.Key.CurveColor = pair.Value;
			//	m_originalCurveColors.Clear();

			//	// Merge curves from all selected objects
			//	List<ICurve> curveList = new List<ICurve>();
			//	foreach (object obj in m_selectionContext.Selection)
			//		curveList.AddRange( GetCurves( obj ) );

			//	// Auto-assign curve colors (equally spaced in the color spectrum)
			//	if (m_selectionContext.SelectionCount > 1)
			//	{
			//		for (int i = 0; i < curveList.Count; i++)
			//		{
			//			ICurve curve = curveList[i];
			//			float hue = (i * 360.0f) / (float) curveList.Count;
			//			m_originalCurveColors[curve] = curve.CurveColor; // Remember original curve color
			//			curve.CurveColor = ColorUtil.FromAhsb( 255, hue, 1.0f, 0.5f );
			//		}
			//	}

			//	curves = curveList;
			//}
			//else
			//	curves = GetCurves( m_selectionContext.LastSelected );

			//foreach (ICurve curve in curves)
			//	CurveUtils.ComputeTangent( curve );

			//m_curveEditorControl.Curves = new ReadOnlyCollection<ICurve>( curves );

			object selectedObject = m_selectionContext.LastSelected;
			Path<object> path = selectedObject as Path<object>;
			object selected = path != null ? path.Last : selectedObject;

			//ICurveSet curveSet = selected.As<ICurveSet>();
			//if (curveSet != null && curveSet.Curves != null)
			//	return curveSet.Curves;

			//ICurve curve = selected.As<ICurve>();
			//if (curve != null)
			//	return new List<ICurve> { curve };

			//// Return empty list if object is incompatible
			//return new List<ICurve>();

			IntervalSetting intervalSetting = selected.As<IntervalSetting>();
			if ( intervalSetting != null )
			{
				Root = intervalSetting.DomNode;
				m_splitContainer.Enabled = true;
			}
			else
			{
				TimelineSetting tisett = selected.As<TimelineSetting>();
				if ( tisett == null )
				{
					m_splitContainer.Enabled = false;
					Root = null;
				}
			}
		}

        private void treeControl_NodeSelectedChanged(object sender, TreeControl.NodeEventArgs e)
        {
            if (e.Node.Selected)
            {
				//object item = e.Node.Tag;
				//{
				//	DomNode node = item as DomNode;
				//	if (node != null)
				//	{
				//		// Build property descriptors for node's attributes
				//		List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();
				//		foreach (AttributeInfo attributeInfo in node.Type.Attributes)
				//		{
				//			descriptors.Add(
				//				new AttributePropertyDescriptor(
				//					attributeInfo.Name,
				//					attributeInfo,
				//					"Attributes",
				//					null,
				//					true));
				//		}

				//		// use property collection wrapper to expose the descriptors to the property grid
				//		m_propertyGrid.Bind(new PropertyCollectionWrapper(descriptors.ToArray(), node));
				//	}
				//	else // for NodeAdapters
				//	{
				//		// Treat NodeAdapters like normal .NET objects and expose directly to the property grid
				//		DomNodeAdapter adapter = item as DomNodeAdapter;
				//		m_propertyGrid.Bind(adapter);
				//	}
				//}
            }
        }

		private void treeControl_SelectionChanged( object sender, EventArgs e )
		{
			if ( m_selectionContext != null )
			{
				List<object> newSelection = new List<object>();
				foreach ( TreeControl.Node node in m_treeControl.SelectedNodes )
					//newSelection.Add( MakePath( node ) );
					newSelection.Add( node.Tag );
				m_selectionContext.SetRange( newSelection );
			}
		}

		void treeControl_MouseUp( object sender, MouseEventArgs e )
		{
			if ( m_transactionContext == null )
				return;

			if ( e.Button == MouseButtons.Right )
			{
				TreeControl.Node node = m_treeControl.GetNodeAt( e.Location );
				if ( node == null )
					return;

				IntervalSetting intervalSetting = node.Tag.As<IntervalSetting>();
				if ( intervalSetting != null )
				{
					//IEnumerable<object> commands = EmptyEnumerable<object>.Instance;
					IEnumerable<object> commands = new object[]
                    {
                        Command.Add_CResLod
                    };

					System.Drawing.Point pointOnScreen = m_treeControl.PointToScreen( e.Location );
					m_commandService.RunContextMenu( commands, pointOnScreen );
				}
			}
		}


		/// <summary>
		/// Performs custom actions on ItemRemoved events</summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Event args</param>
		void ObservableContextItemRemoved( object sender, ItemRemovedEventArgs<object> e )
		{
			if ( ReferenceEquals(e.Item, Root) )
			{
				Root = null;
			}
		}

        private readonly IControlHostService m_controlHostService;
		private readonly IContextRegistry m_contextRegistry;
		private readonly ICommandService m_commandService;
		private readonly TreeControl m_treeControl;
        private readonly SplitContainer m_splitContainer;
        private readonly TreeControlAdapter m_treeControlAdapter;
		//private readonly Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyGrid;
        private readonly TreeView m_treeView;

		private ISelectionContext m_selectionContext;
		private IObservableContext m_observableContext;
		private ITransactionContext m_transactionContext;

		private Sce.Atf.Controls.SplitButton m_addButton;

        private class TreeView : ITreeView, IItemView, IObservableContext
        {
            public DomNode RootNode
            {
                get { return m_root; }
                set
                {
                    if (m_root != null)
                    {
                        m_root.AttributeChanged -= root_AttributeChanged;
                        m_root.ChildInserted -= root_ChildInserted;
                        m_root.ChildRemoving -= root_ChildRemoving;
                        m_root.ChildRemoved -= root_ChildRemoved;
                    }

                    m_root = value;

                    if (m_root != null)
                    {
                        m_root.AttributeChanged += root_AttributeChanged;
                        m_root.ChildInserted += root_ChildInserted;
                        m_root.ChildRemoving += root_ChildRemoving;
                        m_root.ChildRemoved += root_ChildRemoved;
                    }

                    Reloaded.Raise(this, EventArgs.Empty);
                }
            }

			//public bool ShowAdapters
			//{
			//	get { return m_showAdapters; }
			//	set { m_showAdapters = value; }
			//}

            #region ITreeView Members

            public object Root
            {
                get { return m_root; }
            }

            public IEnumerable<object> GetChildren(object parent)
            {
                DomNode node = parent as DomNode;
                if (node != null)
                {
					//if (m_showAdapters)
					//{
					//	// get all adapters, and wrap so that the TreeControlAdapter doesn't confuse
					//	//  them with their parent DomNode; remember that the DomNode and its adapters
					//	//  are logically Equal.
					//	IEnumerable<DomNodeAdapter> adapters = node.AsAll<DomNodeAdapter>();
					//	foreach (DomNodeAdapter adapter in adapters)
					//		yield return new Adapter(adapter);
					//}
                    // get child Dom objects
                    foreach (DomNode child in node.Children)
                        yield return child;
                }
            }

            #endregion

            #region IItemView Members

            public void GetInfo(object item, ItemInfo info)
            {
                info.IsLeaf = !HasChildren(item);

				IntervalSetting intervalSetting = item.As<IntervalSetting>();
				if ( intervalSetting != null )
				{
					info.Label = intervalSetting.Name;
				}
				else
				{
					TimelineSetting setting = item.Cast<TimelineSetting>();
					info.Label = setting.Label;
				}

				//DomNode node = item as DomNode;
				//if (node != null && node.ChildInfo != null)
				//{
				//	info.Label = node.ChildInfo.Name;
				//	//info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.DomObjectImage);
				//	return;
				//}

				//Adapter adapter = item as Adapter;
				//if (adapter != null)
				//{
				//	DomNodeAdapter nodeAdapter = adapter.Adaptee as DomNodeAdapter;
				//	StringBuilder sb = new StringBuilder();

				//	Type type = nodeAdapter.GetType();
				//	sb.Append(type.Name);
				//	sb.Append(" (");
				//	foreach (Type interfaceType in type.GetInterfaces())
				//	{
				//		sb.Append(interfaceType.Name);
				//		sb.Append(",");
				//	}
				//	sb[sb.Length - 1] = ')'; // remove trailing comma

				//	info.Label = sb.ToString();
				//	//info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.DomObjectInterfaceImage);

				//	return;
				//}
            }

            #endregion

            #region IObservableContext Members

            /// <summary>
            /// Event that is raised when a tree item is inserted</summary>
            public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

            /// <summary>
            /// Event that is raised when a tree item is removed</summary>
            public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

            /// <summary>
            /// Event that is raised when a tree item is changed</summary>
            public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

            /// <summary>
            /// Event that is raised when the tree is reloaded</summary>
            public event EventHandler Reloaded;

            #endregion

            public bool HasChildren(object item)
            {
                foreach (object child in (this).GetChildren(item))
                    return true;
                return false;
            }

            private void root_AttributeChanged(object sender, AttributeEventArgs e)
            {
                ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
            }

            private void root_ChildInserted(object sender, ChildEventArgs e)
            {
                int index = GetChildIndex(e.Child, e.Parent);
                if (index >= 0)
                {
                    ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(index, e.Child, e.Parent));
                }
            }

            private void root_ChildRemoving(object sender, ChildEventArgs e)
            {
                m_lastRemoveIndex = GetChildIndex(e.Child, e.Parent);
            }

            private void root_ChildRemoved(object sender, ChildEventArgs e)
            {
                if (m_lastRemoveIndex >= 0)
                {
                    ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(m_lastRemoveIndex, e.Child, e.Parent));
                }
            }

            private int GetChildIndex(object child, object parent)
            {
                // get child index by re-constructing what we'd give the tree control
                IEnumerable<object> treeChildren = GetChildren(parent);
                int i = 0;
                foreach (object treeChild in treeChildren)
                {
                    if (treeChild.Equals(child))
                        return i;
                    i++;
                }
                return -1;
            }

            private DomNode m_root;
            private int m_lastRemoveIndex;
			//private bool m_showAdapters = true;
        }

		private enum Command
		{
			Add_CResLod,
			Add_Test
		}

		private void DoCommandImpl( object commandTag )
		{
			if ( !(commandTag is Command) )
				return;

			if ( m_treeControl.Root == null )
				return;

			IntervalSetting intervalSetting = m_treeControl.Root.Tag.As<IntervalSetting>();
			if ( intervalSetting == null )
				return;

			if ( m_transactionContext == null )
				return;

			switch ( (Command) commandTag )
			{
				case Command.Add_CResLod:
					{
						m_transactionContext.DoTransaction( delegate
						{
							intervalSetting.CreateSetting( Schema.cresLodSettingType.Type, "CResLod" );
						},
						"Add CResLod" );
					}
					break;
			}
		}

		///// <summary>
		///// Standard Edit/Cut command</summary>
		//public static CommandInfo Add_CResLod_CommandInfo =
		//	new CommandInfo(
		//		Command.Add_CResLod,
		//		null,
		//		null,
		//		"Add CResLod Setting",
		//		"Creates new CResLod setting and adds it to selected Setting Interval",
		//		Sce.Atf.Input.Keys.None,
		//		null );

		//#region ICommandClient Members

		///// <summary>
		///// Checks whether the client can do the command, if it handles it</summary>
		///// <param name="commandTag">Command to be done</param>
		///// <returns>True iff client can do the command</returns>
		//bool ICommandClient.CanDoCommand( object commandTag )
		//{
		//	if ( !(commandTag is Command) )
		//		return false;

		//	switch ( (Command) commandTag )
		//	{
		//		case Command.Add_CResLod:
		//			return true;
		//	}

		//	return false;
		//}

		///// <summary>
		///// Does the command</summary>
		///// <param name="commandTag">Command to be done</param>
		//void ICommandClient.DoCommand( object commandTag )
		//{
		//	DoCommandImpl( commandTag );
		//}

		///// <summary>
		///// Updates command state for given command</summary>
		///// <param name="commandTag">Command</param>
		///// <param name="commandState">Command info to update</param>
		//void ICommandClient.UpdateCommand( object commandTag, CommandState commandState )
		//{
		//}

		//#endregion
    }
}
