//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace pico.LogOutput
{
    /// <summary>
    /// Tree list view editor for hierarchical file system component</summary>
    [Export(typeof(IInitializable))]
    public class picoLogOutputEditor : IInitializable, IControlHostClient, ICommandClient, IDocumentClient
    {
        /// <summary>
        /// Constructor with parameters. Creates and registers UserControl and adds buttons to it.
        /// Creates a TreeListView to contain tree data.</summary>
        /// <param name="mainForm">Main form</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="settingsService">Settings service</param>
        /// <param name="controlHostService">Control host service</param>
        [ImportingConstructor]
		public picoLogOutputEditor(
            MainForm mainForm,
            IContextRegistry contextRegistry,
            ISettingsService settingsService,
            IControlHostService controlHostService,
			ICommandService commandService
			)
        {
            m_mainForm = mainForm;
            m_contextRegistry = contextRegistry;
			m_settingsService = settingsService;
			m_controlHostService = controlHostService;
			m_commandService = commandService;
        }

        #region IInitialize Interface

        /// <summary>
        /// Initialize component so it is displayed</summary>
        void IInitializable.Initialize()
        {
            // So the GUI will show up since nothing else imports it...
			//m_commandService.RegisterCommand(
			//   Command.ClearAll,
			//   StandardMenu.View,
			//   "LogCommands",
			//   "Clear All",
			//   "Clears All Outputs",
			//   Keys.None,
			//   null,
			//   CommandVisibility.ApplicationMenu,
			//   this );

			m_icons = new Icons();

			m_inputThread = new InputThread( this );
			//m_server = new AsynchronousSocketListener( this );
			//m_server.StartListening();

			//picoLogOutputForm3 form_All = new picoLogOutputForm3();
			//m_logForms.Add( "All", form_All );
			//picoLogDataTable data = new picoLogDataTable();
			//form_All.setup( data, m_icons );

			//var info =
			//		new ControlInfo(
			//		"All",
			//		"All",
			//		StandardControlGroup.CenterPermanent );

			//m_controlHostService.RegisterControl(
			//	form_All,
			//	info,
			//	this );

			//clearAllThreadSafe();

			picoLogOutputForm3 form = _AddNewForm( "StandaloneWin", false );
			form.LogDataTable.MaxRows = 10000;
		}

        #endregion

        #region IControlHostClient Interface

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Activate(Control control)
        {
			//if (ReferenceEquals(control, m_host))
			//	m_contextRegistry.ActiveContext = this;
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
			lock ( this )
			{
				foreach ( ChannelInstance chInst in m_logForms.Values )
				{
					if ( chInst.form == control )
					{
						m_logForms.Remove( chInst.channelName );
						break;
					}
				}
			}
            return true;
        }

        #endregion

		#region ICommandClient Members

		/// <summary>
		/// Logger commands
		/// </summary>
		protected enum Command
		{
			ClearAll
		};

		/// <summary>
		/// Can the client do the command?</summary>
		/// <param name="commandTag">Command</param>
		/// <returns>True iff client can do the command</returns>
		public bool CanDoCommand( object commandTag )
		{
			if (!(commandTag is Command))
				return false;

			switch ((Command) commandTag)
			{
				case Command.ClearAll:
					return true;
			}

			return false;
		}

		/// <summary>
		/// Do a command</summary>
		/// <param name="commandTag">Command</param>
		public void DoCommand( object commandTag )
		{
			//if (commandTag is Command)
			//{
			//	switch ((Command) commandTag)
			//	{
			//		case Command.ClearAll:
			//			clearAll();
			//			break;
			//	}
			//}
		}

		/// <summary>
		/// Updates command state for given command</summary>
		/// <param name="commandTag">Command</param>
		/// <param name="state">Command state to update</param>
		public void UpdateCommand( object commandTag, Sce.Atf.Applications.CommandState state )
		{
		}

		#endregion

		#region IDocumentClient Members

		private static readonly DocumentClientInfo s_info = new DocumentClientInfo(
			"LogChannel".Localize(),
			new string[] { ".log" },
			Sce.Atf.Resources.DocumentImage,
			Sce.Atf.Resources.FolderImage,
			true );

		/// <summary>
		/// Gets editor's information about the document client, such as the file type and file
		/// extensions it supports, whether or not it allows multiple documents to be open, etc.</summary>
		public DocumentClientInfo Info
		{
			get { return s_info; }
		}

		/// <summary>
		/// Returns whether the client can open or create a document at the given URI</summary>
		/// <param name="uri">Document URI</param>
		/// <returns>True iff the client can open or create a document at the given URI</returns>
		public bool CanOpen( Uri uri )
		{
			return true;
		}

		/// <summary>
		/// Opens or creates a document at the given URI.
		/// Uses LoadOrCreateDocument() to create a D2dTimelineRenderer and D2dTimelineControl.</summary>
		/// <param name="uri">Document URI</param>
		/// <returns>Document, or null if the document couldn't be opened or created</returns>
		public IDocument Open( Uri uri )
		{
			return null;
		}

		/// <summary>
		/// Makes the document visible to the user</summary>
		/// <param name="document">Document to show</param>
		public void Show( IDocument document )
		{
		}

		/// <summary>
		/// Saves the document at the given URI. Persists document data.</summary>
		/// <param name="document">Document to save</param>
		/// <param name="uri">New document URI</param>
		public void Save( IDocument document, Uri uri )
		{
		}

		/// <summary>
		/// Closes the document and removes any views of it from the UI</summary>
		/// <param name="document">Document to close</param>
		public void Close( IDocument document )
		{
		}

		#endregion

		public void addDataItem( DataItem dataItem, string channel )
		{
			//lock( this )
			//{
			//	picoLogOutputForm3 form;
			//	if ( m_logForms.TryGetValue( channel, out form ) )
			//	{
			//	}
			//	else
			//	{
			//		form = _AddNewForm( channel );
			//	}

			//	if (form.IsHandleCreated)
			//		form.BeginInvoke( new MethodInvoker( () => form.addDataItem( dataItem ) ) );

			//	//if ( channel != "All" )
			//	if ( !object.ReferenceEquals(form, m_logForm_All) )
			//	{
			//		if ( m_logForm_All.IsHandleCreated )
			//			m_logForm_All.BeginInvoke( new MethodInvoker( () => m_logForm_All.addDataItem( dataItem ) ) );
			//	}
			//}

			//lock ( this )
			//{
			//	picoLogDataTable dataTable;
			//	if ( m_logForms.TryGetValue( channel, out dataTable ) )
			//	{
			//	}
			//	else
			//	{
			//		dataTable = _AddNewForm( channel );
			//	}

			//	picoLogOutputForm3 form = dataTable.Form;

			//	if ( form.IsHandleCreated )
			//		form.BeginInvoke( new MethodInvoker( () => form.addDataItem( dataItem ) ) );

			//	//if ( channel != "All" )
			//	if ( !object.ReferenceEquals( form, m_logForm_All ) )
			//	{
			//		if ( m_logForm_All.IsHandleCreated )
			//			m_logForm_All.BeginInvoke( new MethodInvoker( () => m_logForm_All.addDataItem( dataItem ) ) );
			//	}
			//}

			m_mainForm.BeginInvoke( new MethodInvoker( () => addDataItemThreadSafe( dataItem, channel ) ) );
		}

		public void addDataItemThreadSafe( DataItem dataItem, string channel )
		{
			lock ( this )
			{
				ChannelInstance chInst;
				picoLogOutputForm3 form;
				if ( m_logForms.TryGetValue( channel, out chInst ) )
				{
					form = chInst.form;
				}
				else
				{
					form = _AddNewForm( channel, true );
				}

				//if ( form.IsHandleCreated )
				//	form.BeginInvoke( new MethodInvoker( () => form.addDataItem( dataItem ) ) );

				////if ( channel != "All" )
				//if ( !object.ReferenceEquals( form, m_logForm_All ) )
				//{
				//	if ( m_logForm_All.IsHandleCreated )
				//		m_logForm_All.BeginInvoke( new MethodInvoker( () => m_logForm_All.addDataItem( dataItem ) ) );
				//}

				form.addDataItem( dataItem );
				//if ( !object.ReferenceEquals(form, m_logForm_All) )
				//	m_logForm_All.addDataItem( dataItem );
			}
		}

		public void clearChannel( string channel )
		{
			if ( m_mainForm.IsHandleCreated )
				m_mainForm.BeginInvoke( new MethodInvoker( () => clearChannelThreadSafe(channel) ) );
		}

		public void clearChannelThreadSafe( string channel )
		{
			lock ( this )
			{
				ChannelInstance chInst;
				if ( m_logForms.TryGetValue( channel, out chInst ) )
				{
					picoLogOutputForm3 form = chInst.form;
					form.clearLog();
				}
			}
		}

		public void renameChannel( string oldName, string newName )
		{
			if ( m_mainForm.IsHandleCreated )
				m_mainForm.BeginInvoke( new MethodInvoker( () => renameChannelThreadSafe( oldName, newName ) ) );
		}

		public void renameChannelThreadSafe( string oldName, string newName )
		{
			lock ( this )
			{
				ChannelInstance chInst;
				if ( m_logForms.TryGetValue( oldName, out chInst ) )
				{
					m_logForms.Remove( oldName );

					ChannelInstance chInstNewName;
					if ( m_logForms.TryGetValue( newName, out chInstNewName ) )
					{
						// unregister old control
						// control with newName already exists
						//
						m_controlHostService.UnregisterControl( chInst.form );
					}
					else
					{
						m_logForms.Add( newName, chInst );

						chInst.channelName = newName;
						chInst.controlInfo.Name = newName;
						chInst.controlInfo.Description = newName;
					}
				}
			}
		}

		public void closeChannel( string channel )
		{
			if ( m_mainForm.IsHandleCreated )
				m_mainForm.BeginInvoke( new MethodInvoker( () => closeChannelThreadSafe( channel ) ) );
		}

		public void closeChannelThreadSafe( string channel )
		{
			lock ( this )
			{
				ChannelInstance chInst;
				if ( m_logForms.TryGetValue( channel, out chInst ) )
				{
					m_logForms.Remove( channel );
					m_controlHostService.UnregisterControl( chInst.form );
					// dispose this form, to free all resources
					// without this number of user objects when viewed in taskmanager increases to thousands
					//
					chInst.form.Dispose();
				}
			}
		}
		//public void clearAll()
		//{
		//	if ( m_mainForm.IsHandleCreated )
		//		m_mainForm.BeginInvoke( new MethodInvoker( () => clearAllThreadSafe() ) );
		//}

		//public void clearAllThreadSafe()
		//{
		//	lock ( this )
		//	{
		//		foreach ( picoLogOutputForm3 form in m_logForms.Values )
		//		{
		//			m_controlHostService.UnregisterControl( form );
		//		}

		//		m_logForms.Clear();

		//		//m_logForm_All = _AddNewForm( "All" );
		//		//m_logForm_All.LogDataTable.MaxRows = 10000;
		//	}
		//}

		private picoLogOutputForm3 _AddNewForm( string channel, bool canBeClosed )
		{
			lock ( this )
			{
				ChannelInstance chInst = new ChannelInstance();
				m_logForms.Add( channel, chInst );

				chInst.channelName = channel;
				picoLogOutputForm3 form = new picoLogOutputForm3();
				chInst.form = form;
				form.setup( m_icons );

				var info =
						new ControlInfo(
						channel,
						channel,
						canBeClosed ? StandardControlGroup.Center : StandardControlGroup.CenterPermanent );
				info.IsDocument = true;

				chInst.controlInfo = info;

				m_controlHostService.RegisterControl(
					form,
					info,
					this );
				return form;
			}
		}

        private readonly MainForm m_mainForm;
        private readonly IContextRegistry m_contextRegistry;
		private readonly ISettingsService m_settingsService;
		private readonly IControlHostService m_controlHostService;
		private readonly ICommandService m_commandService;

		private class ChannelInstance
		{
			public string channelName;
			public picoLogOutputForm3 form;
			public ControlInfo controlInfo;
		}

		private Dictionary<string, ChannelInstance> m_logForms = new Dictionary<string, ChannelInstance>();
		//private picoLogOutputForm3 m_logForm_All;
		//private Dictionary<string, picoLogDataTable> m_logForms = new Dictionary<string, picoLogDataTable>();
		private Icons m_icons;
		private InputThread m_inputThread;
		//private AsynchronousSocketListener m_server;
    }
}