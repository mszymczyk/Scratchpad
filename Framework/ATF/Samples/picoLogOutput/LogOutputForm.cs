//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace pico.LogOutput
{
    /// <summary>
    /// Common data item editor derived from TreeListViewEditor used for all the tree view editors
    /// except RawTreeListView.</summary>
    class picoLogOutputForm : TreeListViewEditor, IControlHostClient
    {
        /// <summary>
        /// Constructor that configures TreeListView. Creates and registers control it populates with desired buttons
        /// that have the handler method BtnClick().</summary>
        /// <param name="name">Name of editor</param>
        /// <param name="style">TreeListView style</param>
        /// <param name="flags">Flags indicating which buttons appear for this editor</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="settingsService">Settings service</param>
        /// <param name="controlHostService">Control host service</param>
		public picoLogOutputForm(
            string name,
			//TreeListView.Style style,
			//Buttons flags,
			IContextRegistry contextRegistry,
            ISettingsService settingsService,
			IControlHostService controlHostService
			)
			//: base(TreeListView.Style.VirtualList)
			: base(TreeListView.Style.List)
        {
			m_contextRegistry = contextRegistry;

            TreeListView.Name = name;

			m_data = new LogDataContainer();

			View = m_data;

			//m_data.GenerateVirtual();
			m_data.GenerateFlat( null );

			TreeListViewAdapter.RetrieveVirtualItem += TreeListViewAdapterRetrieveVirtualItem;

            {
                var owner =
                    string.Format(
                        "{0}-{1}-TreeListView",
                        this,
                        TreeListView.Name);

                settingsService.RegisterSettings(
                    owner,
                    new BoundPropertyDescriptor(
                        TreeListView,
                        () => TreeListView.PersistedSettings,
                        SettingsDisplayName,
                        SettingsCategory,
                        SettingsDescription));
            }

            {
                //
                // Create custom control to contain any
                // data creation buttons + TreeListView
                //

                m_uberControl = new UserControl {Dock = DockStyle.Fill};

				//int x = 2, y = 2;
				var buttonHeight = -1;

				//if ((flags & Buttons.AddFlat) == Buttons.AddFlat)
				//{
				//	var btn = CreateButton(AddFlatText, ref x, ref y, ref buttonHeight);
				//	btn.Tag = Buttons.AddFlat;
				//	btn.Click += BtnClick;
				//	m_uberControl.Controls.Add(btn);
				//}

				//if ((flags & Buttons.AddHierarchical) == Buttons.AddHierarchical)
				//{
				//	var btn = CreateButton(AddHierarchicalText, ref x, ref y, ref buttonHeight);
				//	btn.Tag = Buttons.AddHierarchical;
				//	btn.Click += BtnClick;
				//	m_uberControl.Controls.Add(btn);
				//}

				//if ((flags & Buttons.AddVirtual) == Buttons.AddVirtual)
				//{
				//	var btn = CreateButton(AddVirtualText, ref x, ref y, ref buttonHeight);
				//	btn.Tag = Buttons.AddVirtual;
				//	btn.Click += BtnClick;
				//	m_uberControl.Controls.Add(btn);
				//}

				//if ((flags & Buttons.Reload) == Buttons.Reload)
				//{
				//	var btn = CreateButton(ReloadText, ref x, ref y, ref buttonHeight);
				//	btn.Tag = Buttons.Reload;
				//	btn.Click += BtnClick;
				//	m_uberControl.Controls.Add(btn);
				//}

				//if ((flags & Buttons.ExpandAll) == Buttons.ExpandAll)
				//{
				//	var btn = CreateButton(ExpandAllText, ref x, ref y, ref buttonHeight);
				//	btn.Tag = Buttons.ExpandAll;
				//	btn.Click += BtnClick;
				//	m_uberControl.Controls.Add(btn);
				//}

				//if ((flags & Buttons.CollapseAll) == Buttons.CollapseAll)
				//{
				//	var btn = CreateButton(CollapseAllText, ref x, ref y, ref buttonHeight);
				//	btn.Tag = Buttons.CollapseAll;
				//	btn.Click += BtnClick;
				//	m_uberControl.Controls.Add(btn);
				//}

				//if ((flags & Buttons.RemoveItem) == Buttons.RemoveItem)
				//{
				//	var btn = CreateButton(RemoveItemText, ref x, ref y, ref buttonHeight);
				//	btn.Tag = Buttons.RemoveItem;
				//	btn.Click += BtnClick;
				//	m_uberControl.Controls.Add(btn);
				//}

				//if ((flags & Buttons.ModifySelected) == Buttons.ModifySelected)
				//{
				//	var btn = CreateButton(ModifySelectedText, ref x, ref y, ref buttonHeight);
				//	btn.Tag = Buttons.ModifySelected;
				//	btn.Click += BtnClick;
				//	m_uberControl.Controls.Add(btn);
				//}

				//if ((flags & Buttons.SelectAll) == Buttons.SelectAll)
				//{
				//	var btn = CreateButton(SelectAllText, ref x, ref y, ref buttonHeight);
				//	btn.Tag = Buttons.SelectAll;
				//	btn.Click += BtnClick;
				//	m_uberControl.Controls.Add(btn);
				//}

				//if ((flags & Buttons.RecursiveCheckBoxes) == Buttons.RecursiveCheckBoxes)
				//{
				//	var btn = CreateButton(RecursiveCheckBoxesOffText, ref x, ref y, ref buttonHeight);
				//	btn.Tag = Buttons.RecursiveCheckBoxes;
				//	btn.Click += BtnClick;
				//	m_uberControl.Controls.Add(btn);
				//}

                {
                    TreeListView.Control.Location = new Point(0, buttonHeight + 2);
                    TreeListView.Control.Anchor =
                        AnchorStyles.Left | AnchorStyles.Top |
                        AnchorStyles.Right | AnchorStyles.Bottom;

                    TreeListView.Control.Width = m_uberControl.Width;
                    TreeListView.Control.Height = m_uberControl.Height - buttonHeight - 2;

                    m_uberControl.Controls.Add(TreeListView);
                }

                var info =
                    new ControlInfo(
                        TreeListView.Name,
                        TreeListView.Name + " - TreeListView",
                        StandardControlGroup.CenterPermanent);

                controlHostService.RegisterControl(
                    m_uberControl,
                    info,
                    this);
            }
        }

		///// <summary>
		///// Initialize</summary>
		//void IInitializable.Initialize()
		//{
		//	// So the GUI will show up since nothing else imports it...
		//}

        #region IControlHostClient Interface

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Activate(Control control)
        {
			//if (ReferenceEquals(control, m_uberControl) && View != null)
			m_contextRegistry.ActiveContext = View;
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        public bool Close(Control control)
        {
            return true;
        }

        #endregion

		//private static Button CreateButton(string text, ref int x, ref int y, ref int height)
		//{
		//	var btn = new Button {Text = text};

		//	var size = TextRenderer.MeasureText(btn.Text, btn.Font);
		//	btn.Width = size.Width + 20;

		//	btn.Location = new Point(x, y);
		//	btn.Anchor = AnchorStyles.Left | AnchorStyles.Top;

		//	x += btn.Width + 2;

		//	if (height == -1)
		//		height = btn.Height;

		//	return btn;
		//}

		private void TreeListViewAdapterRetrieveVirtualItem( object sender, TreeListViewAdapter.RetrieveVirtualNodeAdapter e )
		{
			e.Item = m_data[e.ItemIndex];
		}

		public UserControl UberControl
		{
			get { return m_uberControl; }
		}

		private readonly LogDataContainer m_data;

        private UserControl m_uberControl;
		private readonly IContextRegistry m_contextRegistry;

        private const string AddFlatText = "Add Flat";
        private const string AddHierarchicalText = "Add Hierarchical";
        private const string AddVirtualText = "Add Virtual";
        private const string ReloadText = "Reload";
        private const string ExpandAllText = "Expand All";
        private const string CollapseAllText = "Collapse All";
        private const string RemoveItemText = "Remove Item";
        private const string ModifySelectedText = "Modify Selected";
        private const string SelectAllText = "Select All";
        private const string RecursiveCheckBoxesOnText = "Recursive CheckBoxes: on";
        private const string RecursiveCheckBoxesOffText = "Recursive CheckBoxes: off";
        private const string SettingsDisplayName = "TreeListView Persisted Settings";
        private const string SettingsCategory = "TreeListView";
        private const string SettingsDescription = "TreeListView Persisted Settings";
    }
}