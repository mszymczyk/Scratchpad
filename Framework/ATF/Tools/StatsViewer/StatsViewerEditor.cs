//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

//using StatsViewerEditorSample.DomNodeAdapters;
using Sce.Atf.Controls.Timelines.Direct2D;

namespace misz.StatsViewer
{
    /// <summary>
    /// Editor class that creates and saves timeline documents. 
    /// There is just one instance of this class in this application.
    /// It creates a D2dTimelineRenderer and SessionControl to render and display timelines.
    /// It registers this control with the hosting service so that the control appears in the Windows docking framework.
    /// This document client handles file operations, such as saving and closing a document, and
    /// handles application data persistence.</summary>
    [Export(typeof(StatsViewerEditor))]
    [Export(typeof(IDocumentClient))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StatsViewerEditor : IDocumentClient, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor that subscribes to document events and adds palette information</summary>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="documentRegistry">Document registry</param>
        /// <param name="documentService">Document service</param>
        /// <param name="paletteService">Palette service</param>
        /// <param name="settingsService">Settings service</param>
        [ImportingConstructor]
        public StatsViewerEditor(
            IControlHostService controlHostService,
            ICommandService commandService,
            IContextRegistry contextRegistry,
            IDocumentRegistry documentRegistry,
            IDocumentService documentService,
			//IPaletteService paletteService,
            ISettingsService settingsService)
        {
            s_schemaLoader = new StatsViewerSchemaLoader();

            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;
            m_controlHostService = controlHostService;
            m_documentService = documentService;
            m_settingsService = settingsService;
        }

		/// <summary>
		/// Gets the currently active SessionControl or null if there is none</summary>
		public SessionControl ActiveControl
		{
			get
			{
				SessionDocument document = (SessionDocument) m_documentRegistry.ActiveDocument;
				if (document != null)
					return document.SessionControl;
				return null;
			}
		}

        /// <summary>
        /// Gets the current timeline context associated with the current StatsViewerControl</summary>
        public StatsViewerContext ActiveContext
        {
            get { return m_contextRegistry.GetActiveContext<StatsViewerContext>(); }
        }

        /// <summary>
        /// Gets the current timeline document associated with the current StatsViewerControl</summary>
        public SessionDocument ActiveDocument
        {
            get { return m_documentRegistry.GetActiveDocument<SessionDocument>(); }
        }

		///// <summary>
		///// Checks whether the given timeline object's attribute is editable for the current
		///// context and document</summary>
		///// <param name="item">Timeline object that changed</param>
		///// <param name="attribute">Attribute on the timeline object that changed</param>
		///// <returns>True iff this timeline object attribute is editable for the current
		///// ActiveControl, ActiveContext, and ActiveDocument properties</returns>
		//public virtual bool IsEditable(ITimelineObject item, AttributeInfo attribute)
		//{
		//	if (attribute == Schema.groupType.expandedAttribute)
		//		return true;

		//	TimelinePath path = new TimelinePath(item);
		//	return ActiveControl.IsEditable(path);
		//}

        // scripting related members
        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;

        #region IInitializable

        /// <summary>
        /// Finishes initializing component by setting up scripting service, subscribing to document
        /// events, and creating PropertyDescriptors for settings</summary>
        void IInitializable.Initialize()
        {
            if (m_scriptingService != null)
            {
                // load this assembly into script domain.
                m_scriptingService.LoadAssembly(GetType().Assembly);
                m_scriptingService.ImportAllTypes("StatsViewerEditorSample");
                m_scriptingService.ImportAllTypes("StatsViewerEditorSample.DomNodeAdapters");
                m_scriptingService.SetVariable("editor", this);

                m_contextRegistry.ActiveContextChanged += delegate
                {
                    EditingContext editingContext = m_contextRegistry.GetActiveContext<EditingContext>();
                    IHistoryContext hist = m_contextRegistry.GetActiveContext<IHistoryContext>();
                    m_scriptingService.SetVariable("editingContext", editingContext);
                    m_scriptingService.SetVariable("hist", hist);
                };
            }

            if (m_fileWatcherService != null)
            {
                m_fileWatcherService.FileChanged += fileWatcherService_FileChanged;
            }

            var settings = new BoundPropertyDescriptor[] {
                new BoundPropertyDescriptor(typeof(D2dTimelineRenderer),
                    () => D2dTimelineRenderer.GlobalHeaderWidth,
                    "Header Width", "Appearance", "Width of Group/Track Header"),
                new BoundPropertyDescriptor(typeof(D2dTimelineRenderer),
                    () => D2dTimelineRenderer.GlobalKeySize, "Key Size", "Appearance", "Size of Keys"),
                new BoundPropertyDescriptor(typeof(D2dTimelineRenderer),
                    () => D2dTimelineRenderer.GlobalMajorTickSpacing, "Major Tick Spacing", "Appearance", "Pixels between major ticks"),
                new BoundPropertyDescriptor(typeof(D2dTimelineRenderer),
                    () => D2dTimelineRenderer.GlobalPickTolerance, "Pick Tolerance", "Behavior", "Picking tolerance, in pixels"),
                new BoundPropertyDescriptor(typeof(D2dTimelineRenderer),
                    () => D2dTimelineRenderer.GlobalTrackHeight, "Track Height", "Appearance", "Height of track, relative to units of time"),

                //manipulator settings
                new BoundPropertyDescriptor(typeof(D2dSnapManipulator), () => D2dSnapManipulator.SnapTolerance, "Snap Tolerance", "Behavior",
                    "The maximum number of pixels that a selected object will be snapped"),
                new BoundPropertyDescriptor(typeof(D2dSnapManipulator), () => D2dSnapManipulator.Color, "Snap Indicator Color", "Appearance",
                    "The color of the indicator to show that a snap will take place"),
                new BoundPropertyDescriptor(typeof(D2dScaleManipulator), () => D2dScaleManipulator.Color, "Scale Manipulator Color", "Appearance",
                    "The color of the scale manipulator")            };
            m_settingsService.RegisterUserSettings("Timeline Editor", settings);
            m_settingsService.RegisterSettings(this, settings);
        }

        #endregion

        #region IDocumentClient Members

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
        public bool CanOpen(Uri uri)
        {
            return s_info.IsCompatibleUri(uri);
        }

        /// <summary>
        /// Opens or creates a document at the given URI.
        /// Uses LoadOrCreateDocument() to create a D2dTimelineRenderer and SessionControl.</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>Document, or null if the document couldn't be opened or created</returns>
        public IDocument Open(Uri uri)
        {
            SessionDocument document = LoadOrCreateDocument(uri); //true: this is a master document

            if (document != null)
            {
				m_controlHostService.RegisterControl(
					document.SessionControl,
					document.Cast<StatsViewerContext>().ControlInfo,
					this );

				document.SessionControl.Frame();
            }

            return document;
        }

        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        public void Show(IDocument document)
        {
			SessionDocument timelineDocument = document as SessionDocument;
			if (timelineDocument != null)
				m_controlHostService.Show( timelineDocument.SessionControl );
        }

        /// <summary>
        /// Saves the document at the given URI. Persists document data.</summary>
        /// <param name="document">Document to save</param>
        /// <param name="uri">New document URI</param>
        public void Save(IDocument document, Uri uri)
        {
			//SessionDocument timelineDocument = document as SessionDocument;
			//if (timelineDocument == null)
			//	return;

			//if (m_fileWatcherService != null)
			//	m_fileWatcherService.Unregister(uri.LocalPath);

			//string filePath = uri.LocalPath;
			//FileMode fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate;
			//using (FileStream stream = new FileStream(filePath, fileMode))
			//{
			//	var writer = new TimelineXmlWriter(s_schemaLoader.TypeCollection);
			//	writer.Write(timelineDocument.DomNode, stream, uri);
			//}

			//if (m_fileWatcherService != null)
			//	m_fileWatcherService.Register(uri.LocalPath);

			//// mark all sub-context histories as clean);
			//foreach (EditingContext context in timelineDocument.EditingContexts)
			//	context.History.Dirty = false;
        }

        /// <summary>
        /// Closes the document and removes any views of it from the UI</summary>
        /// <param name="document">Document to close</param>
        public void Close(IDocument document)
        {
			// Close the root timeline's control.
			SessionDocument sessionDocument = document as SessionDocument;
			if (sessionDocument == null)
				return;

			m_controlHostService.UnregisterControl( sessionDocument.SessionControl );
			//if (m_reloading || NumReferences( sessionDocument ) == 1)
			//	s_repository.Remove( sessionDocument );
			m_contextRegistry.RemoveContext( sessionDocument );
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Activate(Control control)
        {
			SessionControl timelineControl = (SessionControl) control;
			SessionDocument timelineDocument = (SessionDocument) timelineControl.SessionDocument;
			m_contextRegistry.ActiveContext = timelineDocument;
			m_documentRegistry.ActiveDocument = timelineDocument;
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
			SessionControl timelineControl = (SessionControl) control;
			SessionDocument timelineDocument = (SessionDocument) timelineControl.SessionDocument;

			if (timelineDocument != null)
				return m_documentService.Close( timelineDocument );

            return true;
        }

        #endregion

		/// <summary>
		/// Creates the timeline renderer</summary>
		/// <returns>The renderer to use for one timeline control</returns>
		protected virtual SessionRenderer CreateTimelineRenderer()
		{
			return new DefaultSessionRenderer();
		}

        /// <summary>
        /// Loads the document at the given URI. Creates a D2dTimelineRenderer and SessionControl 
        /// (through SessionDocument's Renderer property) to render and display timelines. 
        /// If isMasterDocument is true and if the file doesn't exist, a new document is created.</summary>
        /// <param name="uri">URI of document to load</param>
        /// <param name="isMasterDocument">True iff is master document</param>
        /// <returns>SessionDocument loaded</returns>
        private SessionDocument LoadOrCreateDocument(Uri uri)
        {
			// Documents need to have a absolute Uri, so that the relative references to sub-documents
			//  are not ambiguous, and so that the FileWatcherService can be used.
			string filePath;
			if (uri.IsAbsoluteUri)
			{
				filePath = uri.LocalPath;
			}
			else
			{
				filePath = PathUtil.GetAbsolutePath( uri.OriginalString, Directory.GetCurrentDirectory() );
				uri = new Uri( filePath, UriKind.Absolute );
			}

			// Check if the repository contains this Uri. Remember that document Uris have to be absolute.
			bool isNewToThisEditor = true;
			DomNode node = null;
			SessionDocument document = null;

			if (File.Exists( filePath ))
			{
				// read existing document using standard XML reader
				using (FileStream stream = new FileStream( filePath, FileMode.Open, FileAccess.Read ))
				{
					DomXmlReader reader = new DomXmlReader( s_schemaLoader );
					node = reader.Read( stream, uri );
				}
			}
			else
			{
				// create new document by creating a Dom node of the root type defined by the schema
				node = new DomNode( StatsViewerSchema.sessionType.Type, StatsViewerSchema.sessionRootElement );
			}

			if (node != null)
			{
				if (document == null)
				{
					document = node.Cast<SessionDocument>();

					SessionRenderer renderer = CreateTimelineRenderer();
					document.Renderer = renderer;
					renderer.Init( document.SessionControl.D2dGraphics );

					string fileName = Path.GetFileName( filePath );
					ControlInfo controlInfo = new ControlInfo( fileName, filePath, StandardControlGroup.Center );

					//Set IsDocument to true to prevent exception in command service if two files with the
					//  same name, but in different directories, are opened.
					controlInfo.IsDocument = true;

					StatsViewerContext timelineContext = document.Cast<StatsViewerContext>();
					timelineContext.ControlInfo = controlInfo;

					document.Uri = uri;

					//if (isMasterDocument)
					//	s_repository.ActiveDocument = document;//adds 'document'
					//else
					//{
					//	// For sub-documents, we want ActiveDocument to remain the main document so that
					//	//  TimelineValidator can identify if a sub-document or master document is being
					//	//  modified.
					//	IDocument previous = s_repository.ActiveDocument;
					//	s_repository.ActiveDocument = document;//adds 'document'
					//	s_repository.ActiveDocument = previous;//restores master document
					//}
				}

				//IHierarchicalTimeline hierarchical = document.Timeline as IHierarchicalTimeline;
				//if (hierarchical != null)
				//{
				//	ResolveAll( hierarchical, new HashSet<IHierarchicalTimeline>() );
				//}

				// Listen to events if this is the first time we've seen this.
				if (isNewToThisEditor)
				{
					//// The master document/context needs to listen to events on any sub-document
					////  so that transactions can be cancelled correctly.
					//if (isMasterDocument)
					//{
						node.AttributeChanging += DomNode_AttributeChanging;
					//}
					//else
					//{
					//	DomNode masterNode = s_repository.ActiveDocument.As<DomNode>();
					//	node.SubscribeToEvents( masterNode );
					//}
				}

				// Initialize Dom extensions now that the data is complete
				node.InitializeExtensions();
			}

			return document;
        }

        private void observable_Reloaded(object sender, EventArgs e)
        {
            sender.Cast<SessionDocument>();
            InvalidateTimelineControls();
        }

        private void observable_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            sender.Cast<SessionDocument>();
            InvalidateTimelineControls();
        }

        private void observable_ItemChanged(object sender, ItemChangedEventArgs<object> e)
        {
            sender.Cast<SessionDocument>();
            InvalidateTimelineControls();
        }

        private void observable_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            InvalidateTimelineControls();
        }

        private void InvalidateTimelineControls()
        {
        }

        private void DomNode_AttributeChanging(object sender, AttributeEventArgs e)
        {
        }

		/// <summary>
        /// Performs custom actions when FileChanged event occurs. 
        /// Updates current document if necessary.</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">FileSystemEventArgs containing event data</param>
        void fileWatcherService_FileChanged(object sender, FileSystemEventArgs e)
        {
        }

		//private class TimelineXmlWriter : DomXmlWriter
		//{
		//	public TimelineXmlWriter(XmlSchemaTypeCollection typeCollection)
		//		: base(typeCollection)
		//	{
		//		// By default, attributes are not persisted if they have their default values.
		//		// Set PersistDefaultAttributes to true to persist these attributes. This might
		//		//  be useful if another app will consume the XML file without a schema file.
		//		//PersistDefaultAttributes = true;
		//	}

		//	// Persists relative references instead of absolute references
		//	protected override void WriteElement(DomNode node, System.Xml.XmlWriter writer)
		//	{
		//		TimelineReference reference = node.As<TimelineReference>();
		//		Uri originalUri = null;
		//		if (reference != null && reference.Uri != null && reference.Uri.IsAbsoluteUri)
		//		{
		//			originalUri = reference.Uri;
		//			reference.Uri = Uri.MakeRelativeUri(reference.Uri);
		//		}

		//		base.WriteElement(node, writer);

		//		if (originalUri != null)
		//		{
		//			reference.Uri = originalUri;
		//		}
		//	}
		//}

        private IControlHostService m_controlHostService;

        [Import(AllowDefault=true)]
        private IFileWatcherService m_fileWatcherService = null;

		//[Import(AllowDefault = true)] 
		//private MainForm m_mainForm = null;

        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;
        private IDocumentService m_documentService;
		private ISettingsService m_settingsService;

        private static StatsViewerSchemaLoader s_schemaLoader;
        private static readonly DocumentClientInfo s_info = new DocumentClientInfo(
			"StatsViewerSession".Localize(),
            new string[] { ".statv" },
            Sce.Atf.Resources.DocumentImage,
            Sce.Atf.Resources.FolderImage,
            true);
    }
}
