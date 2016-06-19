//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets; 

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;

using picoTimelineEditor.DomNodeAdapters;
using Sce.Atf.Controls.Timelines.Direct2D;
using Sce.Atf.Controls.SyntaxEditorControl;

using pico.Hub;
using pico.Timeline;

namespace picoTimelineEditor
{
	public enum EditMode
	{
		Standalone,
		Editing
	};

    /// <summary>
    /// Editor class that creates and saves timeline documents. 
    /// There is just one instance of this class in this application.
    /// It creates a D2dTimelineRenderer and D2dTimelineControl to render and display timelines.
    /// It registers this control with the hosting service so that the control appears in the Windows docking framework.
    /// This document client handles file operations, such as saving and closing a document, and
    /// handles application data persistence.</summary>
    [Export(typeof(TimelineEditor))]
    [Export(typeof(IDocumentClient))]
    [Export(typeof(IPaletteClient))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class TimelineEditor : IDocumentClient, IControlHostClient, IPaletteClient, IInitializable
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
        public TimelineEditor(
            IControlHostService controlHostService,
            ICommandService commandService,
            IContextRegistry contextRegistry,
            IDocumentRegistry documentRegistry,
            IDocumentService documentService,
            IPaletteService paletteService,
            ISettingsService settingsService)
        {
            s_schemaLoader = new SchemaLoader();
            s_repository.DocumentAdded += repository_DocumentAdded;
            s_repository.DocumentRemoved += repository_DocumentRemoved;

            paletteService.AddItem(Schema.markerType.Type, "Timelines", this);
            paletteService.AddItem(Schema.groupType.Type, "Timelines", this);
            paletteService.AddItem(Schema.trackType.Type, "Timelines", this);
			//paletteService.AddItem(Schema.intervalType.Type, "Timelines", this);
			//paletteService.AddItem(Schema.keyType.Type, "Timelines", this);
			//paletteService.AddItem(Schema.timelineRefType.Type, "Timelines", this);

			// pico
			//
			//paletteService.AddItem( Schema.trackFaderType.Type, "pico", this );
			//paletteService.AddItem( Schema.intervalCurveType.Type, "pico", this );

			// random stuff
			//
			paletteService.AddItem( Schema.keyLuaScriptType.Type, "pico", this );
			paletteService.AddItem( Schema.intervalTextType.Type, "pico", this );
			paletteService.AddItem( Schema.intervalNodeAnimationType.Type, "Anim", this );
			paletteService.AddItem( Schema.intervalBlendFactorType.Type, "pico", this );
			paletteService.AddItem( Schema.trackBlendFactorType.Type, "pico", this );

			// references
			//
			paletteService.AddItem( Schema.refChangeLevelType.Type, "pico", this );
			paletteService.AddItem( Schema.refPlayTimelineType.Type, "pico", this );

			// sound
			//
			paletteService.AddItem( Schema.keySoundType.Type, "Sound", this );
			//paletteService.AddItem( Schema.intervalCharacterSoundType.Type, "Sound", this );
			paletteService.AddItem( Schema.intervalSoundType.Type, "Sound", this );

			// anim controller
			//
			paletteService.AddItem( Schema.trackAnimControllerType.Type, "Anim", this );
			paletteService.AddItem( Schema.intervalAnimControllerType.Type, "Anim", this );

			// fader
			//
			paletteService.AddItem( Schema.intervalFaderType.Type, "pico", this );
			paletteService.AddItem( Schema.trackFaderType.Type, "pico", this );

			// Camera
			//
			paletteService.AddItem( Schema.groupCameraType.Type, "Camera", this );
			paletteService.AddItem( Schema.trackCameraAnimType.Type, "Camera", this );
			paletteService.AddItem( Schema.intervalCameraAnimType.Type, "Camera", this );

			// CharacterController
			//
			paletteService.AddItem( Schema.groupCharacterControllerType.Type, "CharacterController", this );
			paletteService.AddItem( Schema.trackCharacterControllerAnimType.Type, "CharacterController", this );
			paletteService.AddItem( Schema.intervalCharacterControllerAnimType.Type, "CharacterController", this );

			// settings
			//
			paletteService.AddItem( Schema.intervalSettingType.Type, "pico", this );

            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;
            m_controlHostService = controlHostService;
            m_documentService = documentService;
            m_settingsService = settingsService;

			Playing = true;
        }

        /// <summary>
        /// Gets the currently active D2dTimelineControl or null if there is none</summary>
        public D2dTimelineControl ActiveControl
        {
            get
            {
                TimelineDocument document = (TimelineDocument)m_documentRegistry.ActiveDocument;
                if (document != null)
                    return document.TimelineControl;
                return null;
            }
        }

        /// <summary>
        /// Gets the current timeline context associated with the current TimelineControl</summary>
        public TimelineContext ActiveContext
        {
            get { return m_contextRegistry.GetActiveContext<TimelineContext>(); }
        }

        /// <summary>
        /// Gets the current timeline document associated with the current TimelineControl</summary>
        public TimelineDocument ActiveDocument
        {
            get { return m_documentRegistry.GetActiveDocument<TimelineDocument>(); }
        }

        /// <summary>
        /// Checks whether the given timeline object's attribute is editable for the current
        /// context and document</summary>
        /// <param name="item">Timeline object that changed</param>
        /// <param name="attribute">Attribute on the timeline object that changed</param>
        /// <returns>True iff this timeline object attribute is editable for the current
        /// ActiveControl, ActiveContext, and ActiveDocument properties</returns>
        public virtual bool IsEditable(ITimelineObject item, AttributeInfo attribute)
        {
            if (attribute == Schema.groupType.expandedAttribute)
                return true;

			// when batch processing and converting cut to timeline, this might be null
			//
			if ( ActiveControl == null )
				return true;

            TimelinePath path = new TimelinePath(item);
            return ActiveControl.IsEditable(path);
        }

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
                m_scriptingService.ImportAllTypes("TimelineEditorSample");
                m_scriptingService.ImportAllTypes("TimelineEditorSample.DomNodeAdapters");
                m_scriptingService.SetVariable("editor", this);

                m_contextRegistry.ActiveContextChanged += delegate
                {
                    EditingContext editingContext = m_contextRegistry.GetActiveContext<EditingContext>();
                    IHistoryContext hist = m_contextRegistry.GetActiveContext<IHistoryContext>();
                    m_scriptingService.SetVariable("editingContext", editingContext);
                    m_scriptingService.SetVariable("hist", hist);

					ISelectionContext selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
					if ( selectionContext != null )
					{
						object lastSelected = selectionContext.LastSelected;
						_ChangeLuaScript( lastSelected );
					}
				};
			}

            if (m_fileWatcherService != null)
            {
                m_fileWatcherService.FileChanged += fileWatcherService_FileChanged;
            }

			if ( m_directoryWatcherService != null )
			{
				m_directoryWatcherService.Register( pico.Paths.PICO_DEMO_data, new string[] { "*.anim", "*.skel", "*.bnk" }, true );
				// FileChanged event won't be fired when file is created
				// there are issues with large files (file is created but not fully copied, etc...)
				//
				m_directoryWatcherService.FileChanged += directoryWatcherService_FileChanged;
			}

            m_documentService.DocumentOpened += documentService_DocumentOpened;
            if (m_liveConnectService != null)
            {
                m_liveConnectService.MessageReceived += liveConnectService_MessageReceived;
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
                    "The color of the scale manipulator"),
                new BoundPropertyDescriptor(typeof(picoTimelineControl), () => picoTimelineControl.CursorStep, "Cursor Step", "Behavior",
                    "Scrubber and cursor positions are rounded to this value (in milliseconds)")
			};

            m_settingsService.RegisterUserSettings("Timeline Editor", settings);
            m_settingsService.RegisterSettings(this, settings);

			var settings2 = new BoundPropertyDescriptor[] {
                new BoundPropertyDescriptor(typeof(TimelineEditor),
                    () => TimelineEditor.LastSoundBankFilename,
                    "LastSoundBankFilename", "Operation", "Last Sound Bank File selected by user")
			};
			m_settingsService.RegisterSettings( this, settings2 );

			if (m_curveEditor != null)
			{
				m_curveEditor.Control.AutoComputeCurveLimitsEnabled = false;
				m_curveEditor.Control.OnlyEditSelectedCurves = true;
				m_curveEditor.MultiSelectionOverlay = true;
				m_curveEditor.Control.CurvesChanged += ( sender, e ) => m_curveEditor.Control.FitAll();

				m_curveEditor.Control.SelectedCurveThickness = 4;
				m_curveEditor.Control.CurveThickness = 2;
			}

			m_luaEditorPanel = new Panel();
			m_luaEditorPanel.Name = "m_luaEditorPanel";
			m_luaEditorPanelControlInfo = new ControlInfo( "Lua Script", "Lua Script Editor", StandardControlGroup.Bottom );
			m_controlHostService.RegisterControl(
				m_luaEditorPanel,
				m_luaEditorPanelControlInfo,
				this );

			m_hubService.MessageReceived += hubService_receiveMessages;

			D2dScrubberManipulator.Moved += ( object sender, EventArgs e ) =>
			{
				D2dScrubberManipulator scrubber = sender as D2dScrubberManipulator;
				if ( scrubber == null )
					return;

				//TimelineHubCommunication hubComm = scrubber.Owner.TimelineDocument.Cast<TimelineHubCommunication>();
				////hubComm.sendScrubberPosition( scrubber.Position );
				//hubComm.sendScrubberPosition();
				hubService_sendScrubberPosition();
			};

			m_mainForm.DragEnter += mainForm_DragEnter;
			m_mainForm.DragDrop += mainForm_DragDrop;
		}

        #endregion

        #region IPaletteClient Members

        /// <summary>
        /// Gets display information for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Info object, which client can fill out</param>
        void IPaletteClient.GetInfo(object item, ItemInfo info)
        {
            DomNodeType nodeType = (DomNodeType)item;
            NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
            if (paletteItem != null)
            {
                info.Label = paletteItem.Name;
                info.Description = paletteItem.Description;
                info.ImageIndex = info.GetImageList().Images.IndexOfKey(paletteItem.ImageName);
                info.HoverText = paletteItem.Description;
            }
        }

        /// <summary>
        /// Converts the palette item into an object that can be inserted into an
        /// IInstancingContext</summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Object that can be inserted into an IInstancingContext</returns>
        object IPaletteClient.Convert(object item)
        {
            DomNodeType nodeType = (DomNodeType)item;
            DomNode node = new DomNode(nodeType);

            NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
            AttributeInfo idAttribute = nodeType.IdAttribute;
            if (paletteItem != null &&
                idAttribute != null)
            {
                node.SetAttribute(idAttribute, paletteItem.Name);
            }

            return node;
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
        /// Uses LoadOrCreateDocument() to create a D2dTimelineRenderer and D2dTimelineControl.</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>Document, or null if the document couldn't be opened or created</returns>
        public IDocument Open(Uri uri)
        {
			try
			{
				TimelineDocument document = LoadOrCreateDocument( uri, true ); //true: this is a master document
				if ( document != null )
				{
					m_controlHostService.RegisterControl(
						document.TimelineControl,
						document.Cast<TimelineContext>().ControlInfo,
						this );

					document.TimelineControl.Frame();
				}

				return document;
			}
			catch( Exception ex )
			{
				InvalidOperationException newEx = new InvalidOperationException( uri + ": " + ex.Message, ex );
				throw newEx;
			}
        }

        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        public void Show(IDocument document)
        {
            TimelineDocument timelineDocument = document as TimelineDocument;
            if (timelineDocument != null)
                m_controlHostService.Show(timelineDocument.TimelineControl);
        }

        /// <summary>
        /// Saves the document at the given URI. Persists document data.</summary>
        /// <param name="document">Document to save</param>
        /// <param name="uri">New document URI</param>
        public void Save(IDocument document, Uri uri)
        {
            TimelineDocument timelineDocument = document as TimelineDocument;
            if (timelineDocument == null)
                return;

            if (m_fileWatcherService != null)
                m_fileWatcherService.Unregister(uri.LocalPath);

            string filePath = uri.LocalPath;
            FileMode fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate;
            using (FileStream stream = new FileStream(filePath, fileMode))
            {
                var writer = new TimelineXmlWriter(s_schemaLoader.TypeCollection);
                writer.Write(timelineDocument.DomNode, stream, uri);
            }

            if (m_fileWatcherService != null)
                m_fileWatcherService.Register(uri.LocalPath);

            // mark all sub-context histories as clean);
            foreach (EditingContext context in timelineDocument.EditingContexts)
                context.History.Dirty = false;
        }

        /// <summary>
        /// Closes the document and removes any views of it from the UI</summary>
        /// <param name="document">Document to close</param>
        public void Close(IDocument document)
        {
            // Close the root timeline's control.
            TimelineDocument timelineDocument = document as TimelineDocument;
            if (timelineDocument == null)
                return;

            m_controlHostService.UnregisterControl(timelineDocument.TimelineControl);
            if (m_reloading || NumReferences(timelineDocument) == 1)
                s_repository.Remove(timelineDocument);
            m_contextRegistry.RemoveContext(timelineDocument);
        }

        #endregion

		private void mainForm_DragEnter( object sender, DragEventArgs e )
		{
			OnDragEnter( sender, e );
		}

		private void mainForm_DragDrop( object sender, DragEventArgs e )
		{
			OnDragDrop( sender, e );
		}

		public void OnDragEnter( object sender, DragEventArgs e )
		{
			e.Effect = DragDropEffects.None;
			if (e.Data.GetDataPresent( DataFormats.FileDrop ))
			{
				string[] files = (string[]) e.Data.GetData( DataFormats.FileDrop );
				foreach (string file in files)
				{
					string ext = Path.GetExtension( file );
					if (ext != ".timeline")
					{
						return;
					}

					string picoPath = pico.Paths.PathToPicoDemoPath( file );
					if (string.IsNullOrEmpty( picoPath ))
						return;
				}

				e.Effect = DragDropEffects.Copy;
			}
		}

		public void OnDragDrop( object sender, DragEventArgs e )
		{
			if (e.Data.GetDataPresent( DataFormats.FileDrop ))
			{
				string[] files = (string[]) e.Data.GetData( DataFormats.FileDrop );
				foreach (string file in files)
				{
					m_documentService.OpenExistingDocument( this, new Uri( file ) );
				}
			}
		}

        /// <summary>
        /// Gets the global collection of known TimelineDocuments. Contains master and sub-documents.</summary>
        public static DocumentRegistry TimelineDocumentRegistry
        {
            get { return s_repository; }
        }

        /// <summary>
        /// Attempts to unload a sub-document if only 1 or 0 references to it exist</summary>
        /// <param name="uri">URI of sub-document</param>
        public void UnloadSubDocument(Uri uri)
        {
            TimelineDocument doc = (TimelineDocument)s_repository.GetDocument(uri);
            if (doc != null && NumReferences(doc) <= 1)
                s_repository.Remove(doc);
        }

        /// <summary>
        /// Attempts to load a sub-document if it is not already loaded</summary>
        /// <param name="uri">URI of sub-document</param>
        public void LoadSubDocument(Uri uri)
        {
            //The URI can be null, for example, if the property editor's Reset Current command is used.
            //  http://tracker.ship.scea.com/jira/browse/ATFINTERNALJIRA-212
            if (uri != null)
            {
                TimelineDocument doc = (TimelineDocument)s_repository.GetDocument(uri);
                if (doc == null)
                {
                    LoadOrCreateDocument(uri, false);
                    InvalidateTimelineControls();
                }
            }
        }

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Activate(Control control)
        {
			D2dTimelineControl timelineControl = control as D2dTimelineControl;
			if ( timelineControl != null )
			{
				TimelineDocument timelineDocument = (TimelineDocument)timelineControl.TimelineDocument;
				m_contextRegistry.ActiveContext = timelineDocument;
				m_documentRegistry.ActiveDocument = timelineDocument;

				if ( m_domExplorer != null )
					m_domExplorer.Root = timelineDocument.DomNode;
			}
			else if ( object.ReferenceEquals( control, m_luaEditorPanel ) )
			{
				ISelectionContext selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
				if ( selectionContext != null )
				{
					if ( selectionContext.LastSelected.Is<LuaScript>() )
						m_contextRegistry.ActiveContext = m_luaEditorPanel;
				}
			}
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
			D2dTimelineControl timelineControl = control as D2dTimelineControl;
			if (timelineControl != null)
			{
				TimelineDocument timelineDocument = (TimelineDocument) timelineControl.TimelineDocument;

				if (timelineDocument != null)
					return m_documentService.Close( timelineDocument );
			}

            return true;
        }

        #endregion

        /// <summary>
        /// Creates the timeline renderer</summary>
        /// <returns>The renderer to use for one timeline control</returns>
		protected virtual picoD2dTimelineRenderer CreateTimelineRenderer()
        {
			//return new D2dDefaultTimelineRenderer();
			return new picoD2dTimelineRenderer();
        }

        /// <summary>
        /// Loads the document at the given URI. Creates a D2dTimelineRenderer and D2dTimelineControl 
        /// (through TimelineDocument's Renderer property) to render and display timelines. 
        /// If isMasterDocument is true and if the file doesn't exist, a new document is created.</summary>
        /// <param name="uri">URI of document to load</param>
        /// <param name="isMasterDocument">True iff is master document</param>
        /// <returns>TimelineDocument loaded</returns>
        private TimelineDocument LoadOrCreateDocument(Uri uri, bool isMasterDocument)
        {
            // Documents need to have a absolute Uri, so that the relative references to sub-documents
            //  are not ambiguous, and so that the FileWatcherService can be used.
            string filePath;
            if (uri.IsAbsoluteUri)
            {
                filePath = uri.LocalPath;
            }
            else if (!isMasterDocument)
            {
                filePath = PathUtil.GetAbsolutePath(uri.OriginalString,
                                                    Path.GetDirectoryName(s_repository.ActiveDocument.Uri.LocalPath));
                uri = new Uri(filePath, UriKind.Absolute);
            }
            else
            {
                filePath = PathUtil.GetAbsolutePath(uri.OriginalString, Directory.GetCurrentDirectory());
                uri = new Uri(filePath, UriKind.Absolute);
            }

            // Check if the repository contains this Uri. Remember that document Uris have to be absolute.
            bool isNewToThisEditor = true;
            DomNode node = null;
            TimelineDocument document = (TimelineDocument)s_repository.GetDocument(uri);

            if (document != null)
            {
                node = document.DomNode;
                isNewToThisEditor = false;
            }
            else if (File.Exists(filePath))
            {
				//if ( Path.GetExtension( filePath ).ToLower() == ".cut" )
				//{
				//	//picoCutToTimelineConverter converter = new picoCutToTimelineConverter( uri );
				//	//node = converter.Convert();
				//	node = new DomNode( Schema.timelineType.Type, Schema.timelineRootElement );
				//}
				//else
				//{
					// read existing document using standard XML reader
					using ( FileStream stream = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
					{
						DomXmlReader reader = new DomXmlReader( s_schemaLoader );
						node = reader.Read( stream, uri );
					}
				//}
            }
            else if (isMasterDocument)
            {
                // create new document by creating a Dom node of the root type defined by the schema
                node = new DomNode(Schema.timelineType.Type, Schema.timelineRootElement);
            }

            if (node != null)
            {
                if (document == null)
                {
                    document = node.Cast<TimelineDocument>();

					if ( document.Renderer == null )
					{
						picoD2dTimelineRenderer renderer = CreateTimelineRenderer();
						renderer.ContextRegistry = m_contextRegistry;
						document.Renderer = renderer;
						renderer.Init( document.TimelineControl.D2dGraphics );
					}

                    string fileName = Path.GetFileName(filePath);
                    ControlInfo controlInfo = new ControlInfo(fileName, filePath, StandardControlGroup.Center);

                    //Set IsDocument to true to prevent exception in command service if two files with the
                    //  same name, but in different directories, are opened.
                    controlInfo.IsDocument = true;

                    TimelineContext timelineContext = document.Cast<TimelineContext>();
                    timelineContext.ControlInfo = controlInfo;
					timelineContext.TimelineEditor = this;

                    document.Uri = uri;

                    if (isMasterDocument)
                        s_repository.ActiveDocument = document;//adds 'document'
                    else
                    {
                        // For sub-documents, we want ActiveDocument to remain the main document so that
                        //  TimelineValidator can identify if a sub-document or master document is being
                        //  modified.
                        IDocument previous = s_repository.ActiveDocument;
                        s_repository.ActiveDocument = document;//adds 'document'
                        s_repository.ActiveDocument = previous;//restores master document
                    }
                }

                IHierarchicalTimeline hierarchical = document.Timeline as IHierarchicalTimeline;
                if (hierarchical != null)
                {
                    ResolveAll(hierarchical, new HashSet<IHierarchicalTimeline>());
                }

                // Listen to events if this is the first time we've seen this.
                if (isNewToThisEditor)
                {
                    // The master document/context needs to listen to events on any sub-document
                    //  so that transactions can be cancelled correctly.
                    if (isMasterDocument)
                    {
                        node.AttributeChanging += DomNode_AttributeChanging;
                    }
                    else
                    {
                        DomNode masterNode = s_repository.ActiveDocument.As<DomNode>();
                        node.SubscribeToEvents(masterNode);
                    }
                }

				// select this document initially, so timeline properties are visible
				//
				ISelectionContext selectionContext = document.Cast<ISelectionContext>();
				selectionContext.Set( node );

				selectionContext.SelectionChanged += delegate
				{
					object lastSelected = selectionContext.LastSelected;
					_ChangeLuaScript( lastSelected );
				};

                // Initialize Dom extensions now that the data is complete
                node.InitializeExtensions();
            }

			//if ( Path.GetExtension( filePath ).ToLower() == ".cut" )
			//{
			//	picoCutToTimelineConverter converter = new picoCutToTimelineConverter( filePath );
			//	//node = converter.Convert();
			//	converter.Convert( node );
			//	//node = new DomNode( Schema.timelineType.Type, Schema.timelineRootElement );
			//}

            return document;
        }

        /// <summary>
        /// Counts the number of references to this document by all TimelineDocuments</summary>
        /// <param name="document">Document</param>
        /// <returns>Number references to document</returns>
        private int NumReferences(ITimelineDocument document)
        {
            int count = 0;
            foreach (IDocument topDoc in m_documentRegistry.Documents)
            {
                ITimelineDocument topTimelineDoc = topDoc as ITimelineDocument;
                if (topTimelineDoc != null)
                {
                    if (document == topTimelineDoc)
                        count++;

                    foreach (TimelinePath path in D2dTimelineControl.GetHierarchy(topTimelineDoc.Timeline))
                    {
                        IHierarchicalTimeline target = ((ITimelineReference)path.Last).Target;
                        if (document == target.As<ITimelineDocument>())
                            count++;
                    }
                }
            }
            return count;
        }

        private void repository_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
        {
            IObservableContext observable = e.Item.As<IObservableContext>();
            if (observable != null)
            {
                observable.ItemChanged += observable_ItemChanged;
                observable.ItemInserted += observable_ItemInserted;
                observable.ItemRemoved += observable_ItemRemoved;
                observable.Reloaded += observable_Reloaded;
            }

            if (m_fileWatcherService != null && !m_reloading)
            {
                m_fileWatcherService.Register(e.Item.Uri.LocalPath);
            }
        }

        private void repository_DocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
        {
            IObservableContext observable = e.Item.As<IObservableContext>();
            if (observable != null)
            {
                observable.ItemChanged -= observable_ItemChanged;
                observable.ItemInserted -= observable_ItemInserted;
                observable.ItemRemoved -= observable_ItemRemoved;
                observable.Reloaded -= observable_Reloaded;
            }

            if (m_fileWatcherService != null && !m_reloading)
            {
                m_fileWatcherService.Unregister(e.Item.Uri.LocalPath);
            }
        }

        private void observable_Reloaded(object sender, EventArgs e)
        {
            sender.Cast<TimelineDocument>();
            InvalidateTimelineControls();
        }

        private void observable_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            sender.Cast<TimelineDocument>();
            InvalidateTimelineControls();
        }

        private void observable_ItemChanged(object sender, ItemChangedEventArgs<object> e)
        {
            sender.Cast<TimelineDocument>();
            InvalidateTimelineControls();
        }

        private void observable_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            InvalidateTimelineControls();
        }

        private void InvalidateTimelineControls()
        {
            foreach (TimelineDocument doc in s_repository.Documents.OfType<TimelineDocument>())
                doc.TimelineControl.Invalidate();
        }

        private void DomNode_AttributeChanging(object sender, AttributeEventArgs e)
        {
            ITimelineObject item = e.DomNode.As<ITimelineObject>();
            if (item != null)
            {
                if (!IsEditable(item, e.AttributeInfo))
                {
                    if (ActiveDocument.Cast<ITransactionContext>().InTransaction)
                        throw new InvalidTransactionException("timeline object can't be edited");
                    else
                        return;
                }

				//// Check if a URI on a timeline reference has changed, so we can unload
				////  old document and load new document.
				//if (e.AttributeInfo.Equivalent(Schema.timelineRefType.refAttribute))
				//{
				//	UnloadSubDocument((Uri)e.OldValue);
				//	LoadSubDocument((Uri)e.NewValue);
				//}

				// Check if a URI on a timeline reference has changed, so we can unload
				//  old document and load new document.
				if ( e.AttributeInfo.Equivalent( Schema.timelineRefType.timelineFilenameAttribute ) )
				{
					Uri oldValue = new Uri( pico.Paths.LocalPathToPicoDataAbsolutePath( (string)e.OldValue ) );
					Uri newValue = new Uri( pico.Paths.LocalPathToPicoDataAbsolutePath( (string)e.NewValue ) );

					UnloadSubDocument( oldValue );
					LoadSubDocument( newValue );
				}
			}
        }

        /// <summary>
        /// Does a depth-first traversal, resolving all referenced sub-documents. Does not traverse 
        /// any node twice. This effectively treats a directed graph as a tree.</summary>
        /// <param name="root">Root timeline</param>
        /// <param name="resolved">Resolved hash set to add to</param>
        private void ResolveAll(IHierarchicalTimeline root, HashSet<IHierarchicalTimeline> resolved)
        {
            resolved.Add(root);
            foreach (ITimelineReference refInterface in root.References)
            {
                TimelineReference reference = (TimelineReference)refInterface;
                //reference.Resolve();

                IDocument referencedDocument = null;
                Uri refUri = reference.Uri;
                if (refUri != null)
                    referencedDocument = LoadOrCreateDocument(refUri, false); //this is not a master document

                if (referencedDocument != null)
                {
                    // Switch from a possible relative path to an absolute path
                    reference.Uri = referencedDocument.Uri;

                    IHierarchicalTimeline child = reference.Target;
                    if (child != null &&
                        !resolved.Contains(child))
                    {
                        ResolveAll(child, resolved);
                    }
                }
            }
        }

        private static void liveConnectService_MessageReceived(object sender, LiveConnectService.LiveConnectMessageArgs e)
        {
            Outputs.WriteLine(OutputMessageType.Info, "{0} : {1}", e.SenderName, e.MessageString);
        }

        private void documentService_DocumentOpened(object sender, DocumentEventArgs e)
        {
            if (m_liveConnectService != null)
                m_liveConnectService.Send("Timeline Editor opened: " + e.Document.Uri.OriginalString);
        }

        /// <summary>
        /// Performs custom actions when FileChanged event occurs. 
        /// Updates current document if necessary.</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">FileSystemEventArgs containing event data</param>
        void fileWatcherService_FileChanged(object sender, FileSystemEventArgs e)
        {
            if (m_mainForm == null || m_reloading)
                return;
            
            Uri uri = new Uri(e.FullPath);
            FileInfo fileInfo = new FileInfo(uri.LocalPath);
            DateTime lastWriteTime = fileInfo.LastWriteTime;
            DateTime loadedWriteTime;
            if (m_loadedWriteTimes.TryGetValue(uri, out loadedWriteTime) && loadedWriteTime >= lastWriteTime)
                return; // Already loaded most recent version of the file

            TimelineDocument doc = null;
            
            // First test if this document is a "master" document
            doc = m_documentRegistry.GetDocument(uri) as TimelineDocument;
            if (doc == null || doc.TimelineControl == null)
            {
                // It's not a master document. Check if it's already loaded. Since sub-documents are read-only,
                //  we don't need to ask the user if they want to reload. Just do it.
                doc = s_repository.GetDocument(uri) as TimelineDocument;
                if (doc != null)
                {
                    s_repository.Remove(doc);
                    WinFormsUtil.InvokeIfRequired(m_mainForm,
                        delegate
                        {
                            LoadOrCreateDocument(uri, false);
                            InvalidateTimelineControls();
                        });
                }
                return;
            }

            // This is a master document. Query the user and if it's OK, then close it and reopen it, and this
            //  will update other open master documents that might reference this one as a sub-document.            
            WinFormsUtil.InvokeIfRequired(m_mainForm,
                delegate
                    {
                        DialogResult result = MessageBox.Show(
                            m_mainForm,
                            doc.Uri.LocalPath + Environment.NewLine + Environment.NewLine +
                            "The file above was changed outside the editor.".Localize() + Environment.NewLine +
                            "Reload, and lose all changes made in the editor?".Localize(),
                            "File Changed, Reload?".Localize(),
                            MessageBoxButtons.YesNo);

                    // Update last write time, as the file may have been saved again while the
                    // message box was displayed
                    fileInfo = new FileInfo(uri.LocalPath);
                    lastWriteTime = fileInfo.LastWriteTime;

                    if (result == DialogResult.Yes)
                    {                    
                        // Reload the document
                        m_reloading = true;
                        doc.Dirty = false;
                        Close(doc.TimelineControl);
                        Open(doc.Uri);
                        m_loadedWriteTimes[uri] = lastWriteTime;
                        m_reloading = false;
                        InvalidateTimelineControls();
                    }
                });
        }

		void directoryWatcherService_FileChanged( object sender, FileSystemEventArgs e )
		{
			string picoDemoPath = pico.Paths.PathToPicoDemoPath( e.FullPath );
			if ( string.IsNullOrEmpty( picoDemoPath ) )
				return;

			string ext = Path.GetExtension( picoDemoPath );

			if ( ext == ".anim" )
			{
				pico.Anim.AnimCache.OnFileChanged( e, ext, picoDemoPath );

				m_hubServiceCommands.ReloadResource( picoDemoPath );

				foreach( IDocument document in m_documentRegistry.Documents )
				{
					ITimeline timeline = document.As<ITimeline>();
					timeline.NotifyFileChange( e, ext, picoDemoPath );
				}
			}
			else if ( ext == ".skel" )
			{
				pico.Anim.AnimCache.OnFileChanged( e, ext, picoDemoPath );
			}
			else if ( ext == ".bnk" )
			{
				pico.ScreamInterop.RefreshAllBanks();
			}
		}

        public class TimelineXmlWriter : DomXmlWriter
        {
            public TimelineXmlWriter(XmlSchemaTypeCollection typeCollection)
                : base(typeCollection)
            {
                // By default, attributes are not persisted if they have their default values.
                // Set PersistDefaultAttributes to true to persist these attributes. This might
                //  be useful if another app will consume the XML file without a schema file.
                PersistDefaultAttributes = true;
            }

			//// Persists relative references instead of absolute references
			//protected override void WriteElement(DomNode node, System.Xml.XmlWriter writer)
			//{
			//	TimelineReference reference = node.As<TimelineReference>();
			//	Uri originalUri = null;
			//	if (reference != null && reference.Uri != null && reference.Uri.IsAbsoluteUri)
			//	{
			//		originalUri = reference.Uri;
			//		reference.Uri = Uri.MakeRelativeUri(reference.Uri);
			//	}

			//	base.WriteElement(node, writer);

			//	if (originalUri != null)
			//	{
			//		reference.Uri = originalUri;
			//	}
			//}
        }

		private void _ChangeLuaScript( object lastSelected )
		{
			Path<object> path = lastSelected as Path<object>;
			object selected = path != null ? path.Last : lastSelected;

			LuaScript luaScript = selected.As<LuaScript>();
			if ( luaScript != null )
			{
				if ( m_luaEditorCurScript != null )
					m_luaEditorCurScript.SourceCodeChanged -= luaScript_SourceCodeChanged;

				m_luaEditorCurScript = luaScript;
				m_luaEditorCurScript.SourceCodeChanged += luaScript_SourceCodeChanged;

				m_luaEditorPanel.Controls.Clear();
				m_luaEditorPanel.Controls.Add( luaScript.LuaEditorControl );
				m_luaEditorPanelControlInfo.Name = "Lua Script: " + luaScript.Name;
			}
			else
			{
				if ( m_luaEditorCurScript != null )
					m_luaEditorCurScript.SourceCodeChanged -= luaScript_SourceCodeChanged;

				m_luaEditorCurScript = null;

				m_luaEditorPanel.Controls.Clear();
				m_luaEditorPanelControlInfo.Name = "Lua Script";
			}
		}
		private void luaScript_SourceCodeChanged( object sender, EventArgs e )
		{
			DomNode luaScript = sender.As<DomNode>();
			Sce.Atf.Dom.DomNode root = luaScript.GetRoot();
			TimelineDocument document = root.Cast<TimelineDocument>();
			hubService_sendReloadTimeline( document, true );
		}

		public EditMode EditMode { get { return m_editMode; } }
		public void setEditMode( string editMode )
		{
			Enum.TryParse<EditMode>( editMode, out m_editMode );
		}

		private EditMode m_editMode;
		public static bool Playing { get; set; }

        /// <summary>
        /// A collection of all ITimelineDocuments that have been loaded. This is necessary so that we can 
        /// track if the same ITimelineDocument has been loaded as a main document and as a sub-document, 
        /// so that we have only one copy in memory. The ActiveDocument property of this DocumentRegistry
        /// is always a "master" ITimelineDocument.</summary>
        private static DocumentRegistry s_repository = new DocumentRegistry();

        private IControlHostService m_controlHostService;

        [Import(AllowDefault=true)]
        private IFileWatcherService m_fileWatcherService = null;

		[Import( AllowDefault=true )]
		private IDirectoryWatcherService m_directoryWatcherService = null;

        [Import(AllowDefault = true)] 
        private MainForm m_mainForm = null;

        [Import(AllowDefault = true)]
        private LiveConnectService m_liveConnectService = null;

		[Import( AllowDefault = true )]
		private DomExplorer m_domExplorer = null;

		[Import( AllowDefault = true )]
		private CurveEditor m_curveEditor = null;

		[Import( AllowDefault = true )]
		private HubService m_hubService = null;

		[Import( AllowDefault = true )]
		private HubServiceCommands m_hubServiceCommands = null;

        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;
        private IDocumentService m_documentService;
        private ISettingsService m_settingsService;

        private bool m_reloading; // true while reloading the document in response to a FileWatcher.FileChanged event
        private readonly Dictionary<Uri, DateTime> m_loadedWriteTimes = new Dictionary<Uri, DateTime>();

        public static SchemaLoader s_schemaLoader;
        private static readonly DocumentClientInfo s_info = new DocumentClientInfo(
            "Timeline".Localize(),
			//new string[] { ".timeline", ".cut" },
			new string[] { ".timeline" },
			Sce.Atf.Resources.DocumentImage,
            Sce.Atf.Resources.FolderImage,
            true);

		private Panel m_luaEditorPanel;
		private ControlInfo m_luaEditorPanelControlInfo;
		private LuaScript m_luaEditorCurScript;

		//// picoHub
		////
		//static string PICO_HUB_IP = "localhost";
		//static int PICO_HUB_PORT = 6666;

		////Client socket stuff 
		//System.Net.Sockets.Socket m_picoHubClientSocket;
		public static string LastSoundBankFilename { get { return ms_LastSoundBankFilename; } set { ms_LastSoundBankFilename = value; } }
		private static string ms_LastSoundBankFilename = "";
    }
}
