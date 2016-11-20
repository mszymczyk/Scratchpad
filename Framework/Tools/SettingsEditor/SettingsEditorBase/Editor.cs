using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using System.ComponentModel;
using Sce.Atf.Controls;

namespace SettingsEditor
{
    /// <summary>
    /// There's just one instance of this class in this application. Manages creation of documents.</summary>
    [Export( typeof( IInitializable ) )]
    [Export( typeof( IDocumentClient ) )]
    [Export( typeof( Editor ) )]
    [PartCreationPolicy( CreationPolicy.Shared )]
    public class Editor : IDocumentClient, IControlHostClient, IInitializable, IPartImportsSatisfiedNotification
    {
        #region IInitializable Members

        /// <summary>
        /// Finish MEF initialization for the component by creating DomNode tree for application data.</summary>
        void IInitializable.Initialize()
        {
            {
                string descr = "Root path for settings description files".Localize();
                var codeDir =
                    new BoundPropertyDescriptor( typeof( Globals ), () => Globals.CodeDirectory,
                        "SettingsDescRoot".Localize(),
                        null,
                        descr,
                        new FolderBrowserDialogUITypeEditor( descr ), null );

                m_settingsService.RegisterSettings( this, codeDir );
                m_settingsService.RegisterUserSettings( "SettingsDescRoot".Localize(), codeDir );
            }
            {
                string descr = "Root path for settings files".Localize();
                var dataDir =
                    new BoundPropertyDescriptor( typeof( Globals ), () => Globals.DataDirectory,
                        "SettingsRoot".Localize(),
                        null,
                        descr,
                        new FolderBrowserDialogUITypeEditor( descr ), null );

                m_settingsService.RegisterSettings( this, dataDir );
                m_settingsService.RegisterUserSettings( "SettingsRoot".Localize(), dataDir );
            }

            m_settingsService.RegisterSettings( this, new BoundPropertyDescriptor( this, () => GroupExpandedInfo, "GroupExpandedInfo", null, null ) );

            m_mainForm.AllowDrop = true;
            m_mainForm.DragEnter += mainForm_DragEnter;
            m_mainForm.DragDrop += mainForm_DragDrop;

            m_fileWatcherService.FileChanged += fileWatcherService_FileChanged;

            m_documentRegistry.DocumentRemoved += documentRegistry_DocumentRemoved;

            m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
        }

        private void contextRegistry_ActiveContextChanged( object sender, EventArgs e )
        {
            if ( m_selectionContext != null )
                m_selectionContext.SelectionChanged -= selectionContext_SelectionChanged;

            m_selectionContext = m_contextRegistry.ActiveContext.As<ISelectionContext>();
            
            if ( m_selectionContext != null )
                m_selectionContext.SelectionChanged += selectionContext_SelectionChanged;
        }

        private void selectionContext_SelectionChanged( object sender, EventArgs e )
        {
            Group g = m_selectionContext.LastSelected.As<Group>();
            if ( g != null )
            {
                IDocument doc = g.DomNode.GetRoot().Cast<IDocument>();
                DocumentPersistedInfo dpi = GetDocumentPersistedInfo( doc.Uri );
                dpi.m_selectedGroup = g.Name;
            }
        }

        private void mainForm_Closing( object sender, CancelEventArgs e )
        {
            m_mainFormClosing = true;
        }

        private void documentRegistry_DocumentRemoved( object sender, ItemRemovedEventArgs<IDocument> e )
        {
            if ( m_reloadInfo != null )
                // we're in the middle of reloading documents, don't remove group info
                return;

            if ( m_mainFormClosing )
                // program is closing, don't remove group info, it will be persisted by SettingsService
                return;

            m_groupExpandedInfo.Remove( e.Item.Uri );
        }

        private string DataDirectory
        {
            get
            {
                return Globals.DataDirectory;
            }
            set
            {
                Globals.DataDirectory = value;
            }
        }

        #endregion


        #region IPartImportsSatisfiedNotification Members

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            // for persisting opened document's group info with SettingsService
            // we need to be notified about main form closing before ControlHostService
            // the order of events is: main form closing, closing all documents (documentRegistry_DocumentRemoved), persisting settings
            // the only purpose of mainForm_Closing is to freeze DocumentPersistedInfo state
            // so it's not modified by documentRegistry_DocumentRemoved
            // and can be safely persisted after all document's have been closed
            m_mainForm.Closing += mainForm_Closing;
        }

        #endregion

        Document CreateNewParamFile( Uri settingsFile, string descFile, string shaderOutputPath, bool createNew )
        {
            string descFullPath = Path.GetFullPath( descFile );

            SettingsCompiler compiler;
            if ( m_reloadInfo != null )
            {
                compiler = m_reloadInfo.m_compiler;
            }
            else
            {
                compiler = new SettingsCompiler();
                compiler.ReflectSettings( descFullPath );
            }

            compiler.GenerateHeaderIfChanged( descFullPath, shaderOutputPath );

            DynamicSchema dynamicSchema = new DynamicSchema( compiler );
            DomNode rootNode = null;
            if ( createNew )
            {
                rootNode = new DomNode( Schema.settingsFileType.Type, Schema.settingsFileRootElement );

                dynamicSchema.CreateNodes( rootNode );
            }
            else
            {
                // read existing document using standard XML reader
                using ( FileStream stream = new FileStream( settingsFile.LocalPath, FileMode.Open, FileAccess.Read ) )
                {
                    // don't need to use tweaked xml reader anymore
                    //SettingsEditor.DomXmlReader reader = new SettingsEditor.DomXmlReader( SchemaLoader.s_schemaLoader );
                    DomXmlReader reader = new DomXmlReader( SchemaLoader.s_schemaLoader );
                    rootNode = reader.Read( stream, settingsFile );
                }

                dynamicSchema.CreateNodes( rootNode );
            }

            string filePath = settingsFile.LocalPath;
            string fileName = Path.GetFileName( filePath );

            FileInfo fileInfo = new FileInfo( descFullPath );

            Document document = rootNode.Cast<Document>();

            DocumentControl control;
            ControlInfo controlInfo;
            if ( m_reloadInfo != null )
            {
                control = m_reloadInfo.m_documentControl;
                control.Setup( rootNode );
                controlInfo = m_reloadInfo.m_documentControl.ControlInfo;
            }
            else
            {
                control = new DocumentControl( this, m_commandService );
                controlInfo = new ControlInfo( fileName, filePath, StandardControlGroup.Center );
                control.ControlInfo = controlInfo;
                control.Setup( rootNode );
            }

            // IsDocument needs to be set to persist layout and placement of document
            // after editor is closed
            //
            controlInfo.IsDocument = true;

            document.ControlInfo = controlInfo;
            document.Control = control;

            document.DescFilePath = descFullPath;
            document.PathRelativeToData = Globals.GetPathRelativeToData( filePath );
            string descFileRelativePath = Globals.GetPathRelativeToCode( descFullPath );
            document.DescFileRelativePath = descFileRelativePath;

            document.Uri = settingsFile;

            if ( !createNew )
                document.ExplicitlySavedByUser = true;

            rootNode.InitializeExtensions();

            var edContext = rootNode.Cast<DocumentEditingContext>();
            edContext.Set( rootNode );

            m_contextRegistry.ActiveContext = rootNode;

            if ( m_reloadInfo == null )
                m_controlHostService.RegisterControl( control, controlInfo, this );

            m_fileWatcherService.Register( descFullPath );

            DocumentPersistedInfo dgei = GetDocumentPersistedInfo( document.Uri );
            HashSet<string> expandedGroups = dgei.m_expandedGroups;
            dgei.m_expandedGroups = new HashSet<string>();

            foreach ( Group group in rootNode.Subtree.AsIEnumerable<Group>() )
            {
                string absName = group.AbsoluteName;
                if ( expandedGroups.Contains( absName ) )
                {
                    dgei.m_expandedGroups.Add( absName );
                    control.ExpandGroup( group );
                }
            }

            // if file is being reloaded, try setting last valid selection
            // this might be not possible, because group names might have changed
            //
            string groupToSelect = null;

            if ( m_reloadInfo != null && m_reloadInfo.m_selectedGroupName != null )
                groupToSelect = m_reloadInfo.m_selectedGroupName;
            else if ( dgei.m_selectedGroup != null )
                groupToSelect = dgei.m_selectedGroup;


            if ( groupToSelect != null )
            {
                foreach ( Group s in rootNode.Subtree.AsIEnumerable<Group>() )
                {
                    if ( s.Name == groupToSelect )
                        control.SetSelectedDomNode( s.DomNode );
                }
            }

            // save it so new groups\settings get written to disk
            //
            document.SaveImpl();

            document.LoadTime = DateTime.Now;
            m_fileWatcherService.Register( settingsFile.LocalPath );

            document.Control.TreeControl.NodeExpandedChanged += documentControl_treeControl_NodeExpandedChanged;

            return document;
        }

        #region IDocumentClient Members

        /// <summary>
        /// Gets information about the document client, such as the file type and file
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
            return s_info.IsCompatibleUri( uri );
        }

        /// <summary>
        /// Info describing our document type</summary>
        private static DocumentClientInfo s_info =
            new DocumentClientInfo(
                "SettingsFile".Localize(),   // file type
                ".settings",                      // file extension
                null,                       // "new document" icon
                null );                      // "open document" icon

        /// <summary>
        /// Opens or creates a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>Document, or null if the document couldn't be opened or created</returns>
        public IDocument Open( Uri uri )
        {
            //DomNode node = null;
            string filePath = uri.LocalPath;

            if ( File.Exists( filePath ) )
            {
                string pathRelativeToData = Globals.GetPathRelativeToData( uri.LocalPath );
                if ( string.IsNullOrEmpty( pathRelativeToData ) )
                    throw new InvalidSettingsPathException( uri );

                string descFullPath = ExtractDescFile( uri );
                string shaderOutputPath = ExtractShaderFile( uri );
                if ( !string.IsNullOrEmpty( descFullPath ) )
                {
                    Document document = CreateNewParamFile( uri, descFullPath, shaderOutputPath, false );
                    return document;
                }
            }
            else if ( m_descFileToUseWhenCreatingNewDocument != null )
            {
                Document document = CreateNewParamFile( uri, m_descFileToUseWhenCreatingNewDocument, "", true );
                return document;
            }

            return null;
        }

        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        public void Show( IDocument document )
        {
            // set the active document and context; as there is only one editing context in
            //  a document, the document is also a context.
            m_contextRegistry.ActiveContext = document;
            m_documentRegistry.ActiveDocument = document;
        }

        /// <summary>
        /// Saves the document at the given URI</summary>
        /// <param name="document">Document to save</param>
        /// <param name="uri">New document URI</param>
        public void Save( IDocument document, Uri uri )
        {
            string pathRelativeToData = Globals.GetPathRelativeToData( uri.LocalPath );
            if ( string.IsNullOrEmpty( pathRelativeToData ) )
                throw new Exception( "Path is not relative to SettingsRoot" );

            Document doc = document as Document;
            doc.PathRelativeToData = pathRelativeToData;

            doc.ExplicitlySavedByUser = true;
            doc.SaveImpl();
        }

        /// <summary>
        /// Closes the document and removes any views of it from the UI</summary>
        /// <param name="document">Document to close</param>
        public void Close( IDocument document )
        {
            Document doc = document.Cast<Document>();

            if ( m_reloadInfo == null )
                m_controlHostService.UnregisterControl( doc.Control );
            m_contextRegistry.RemoveContext( document );
            m_documentRegistry.Remove( document );

            m_fileWatcherService.Unregister( doc.DescFilePath );
            m_fileWatcherService.Unregister( doc.Uri.LocalPath );
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Activate( Control control )
        {
            DocumentControl pfControl = (DocumentControl)control;
            if ( pfControl != null )
            {
                Document pfDocument = pfControl.RootNode.Cast<Document>();
                m_contextRegistry.ActiveContext = pfDocument;
                m_documentRegistry.ActiveDocument = pfDocument;
            }
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Deactivate( Control control )
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
        public bool Close( Control control )
        {
            DocumentControl pfControl = (DocumentControl)control;
            if ( pfControl != null )
            {
                Document pfDocument = pfControl.RootNode.Cast<Document>();
                m_documentService.Close( pfDocument );
            }

            return true;
        }

        #endregion

        private string ExtractDescFile( Uri uri )
        {
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load( uri.LocalPath );
            }
            catch ( Exception ex )
            {
                string msg = ex.Message;
                return string.Empty;
            }

            XmlElement documentElement = xmlDoc.DocumentElement;
            string descFile = documentElement.GetAttribute( "settingsDescFile" );
            if ( string.IsNullOrEmpty( descFile ) )
                return string.Empty;

            string descFileFull = Globals.GetCodeFullPath( descFile );
            return descFileFull;
        }
        private string ExtractShaderFile( Uri uri )
        {
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load( uri.LocalPath );
            }
            catch ( Exception ex )
            {
                string msg = ex.Message;
                return string.Empty;
            }

            XmlElement documentElement = xmlDoc.DocumentElement;
            string shaderFile = documentElement.GetAttribute( "shaderOutputFile" );
            if ( string.IsNullOrEmpty( shaderFile ) )
                return string.Empty;
            string shaderFileFull = Globals.GetCodeFullPath( shaderFile );
            return shaderFileFull;
        }

        private class ReloadInfo
        {
            public SettingsCompiler m_compiler = null;
            // we use the same control when reloading description file
            // this way we can keep it's layout and placement in the editor
            // without this, old control is closed and new one is docked in center
            public DocumentControl m_documentControl = null;
            public string m_selectedGroupName = null;
        }

        public void Reload( Document document )
        {
            Uri settingsFile = document.Uri;
            string descFilePath = document.DescFilePath;

            SettingsCompiler compiler = null;
            try
            {
                compiler = new SettingsCompiler();
                compiler.ReflectSettings( descFilePath );
            }
            catch ( Exception ex )
            {
                Outputs.WriteLine( OutputMessageType.Error, string.Format( "Reload failed! Exception while processing '{0}': {1}", descFilePath, ex.Message ) );
                return;
            }

            Outputs.WriteLine( OutputMessageType.Info, "Reloading: " + document.Uri.LocalPath );

            m_reloadInfo = new ReloadInfo();
            m_reloadInfo.m_compiler = compiler;
            m_reloadInfo.m_documentControl = document.Control;

            m_documentService.Close( document );
            m_descFileToUseWhenCreatingNewDocument = descFilePath;
            ISelectionContext selectionContext = document.As<ISelectionContext>();
            if ( selectionContext != null )
            {
                Group group = selectionContext.LastSelected.As<Group>();
                if ( group != null )
                    m_reloadInfo.m_selectedGroupName = group.Name;
            }

            m_documentService.OpenExistingDocument( this, settingsFile );
            m_descFileToUseWhenCreatingNewDocument = null;
            m_reloadInfo = null;
        }

        private void mainForm_DragEnter( object sender, DragEventArgs e )
        {
            e.Effect = DragDropEffects.None;
            if ( e.Data.GetDataPresent( DataFormats.FileDrop ) )
            {
                string[] files = (string[])e.Data.GetData( DataFormats.FileDrop );
                foreach ( string file in files )
                {
                    string ext = Path.GetExtension( file );
                    if ( ext != ".cs" )
                        return;

                    string picoPath = Globals.GetPathRelativeToCode( file );
                    if ( string.IsNullOrEmpty( picoPath ) )
                        return;
                }

                e.Effect = DragDropEffects.Copy;
            }
        }

        private void mainForm_DragDrop( object sender, DragEventArgs e )
        {
            if ( e.Data.GetDataPresent( DataFormats.FileDrop ) )
            {
                string[] files = (string[])e.Data.GetData( DataFormats.FileDrop );
                foreach ( string file in files )
                {
                    m_descFileToUseWhenCreatingNewDocument = file;
                    m_documentService.OpenNewDocument( this );
                    m_descFileToUseWhenCreatingNewDocument = null;
                }

                e.Effect = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// Performs custom actions when FileChanged event occurs. 
        /// Updates current document if necessary.</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">FileSystemEventArgs containing event data</param>
        void fileWatcherService_FileChanged( object sender, FileSystemEventArgs e )
        {
            Uri fileUri = new Uri( e.FullPath );

            foreach ( Document doc in m_documentRegistry.Documents )
            {
                if ( doc.DescFilePath == fileUri.LocalPath )
                {
                    FileInfo fileInfo = new FileInfo( fileUri.LocalPath );
                    DateTime lastWriteTime = fileInfo.LastWriteTime;
                    if ( lastWriteTime > doc.LoadTime )
                    {
                        Reload( doc );
                    }

                    break;
                }
                else if ( doc.Uri == fileUri )
                {
                    FileInfo fileInfo = new FileInfo( fileUri.LocalPath );
                    DateTime lastWriteTime = fileInfo.LastWriteTime;
                    if ( lastWriteTime > doc.SaveTime )
                    {
                        DialogResult dr = MessageBox.Show( m_mainForm.DialogOwner, string.Format( "File\n'{0}'\n has been modified externally. Reload?", fileUri.LocalPath ), "External modification detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning );
                        if ( dr == DialogResult.Yes )
                        {
                            Reload( doc );
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Gets all context menu command providers</summary>
        public IEnumerable<IContextMenuCommandProvider> ContextMenuCommandProviders
        {
            get
            {
                return
                    m_contextMenuCommandProviders == null
                        ? EmptyEnumerable<IContextMenuCommandProvider>.Instance
                        : m_contextMenuCommandProviders.GetValues();
            }
        }

        public ICommandService CommandService
        {
            get { return m_commandService; }
        }


        /// <summary>
        /// Gets or sets the info about group expansion as xml string</summary>
        private string GroupExpandedInfo
        {
            get
            {
                // Save group expansion info in the XML format
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild( xmlDoc.CreateXmlDeclaration( "1.0", System.Text.Encoding.UTF8.WebName, "yes" ) );
                XmlElement root = xmlDoc.CreateElement( "GroupExpandedSettings" );
                xmlDoc.AppendChild( root );

                foreach ( KeyValuePair<Uri, DocumentPersistedInfo> p in m_groupExpandedInfo )
                {
                    XmlElement docElement = xmlDoc.CreateElement( "doc" );
                    root.AppendChild( docElement );

                    docElement.SetAttribute( "uri", p.Key.LocalPath );

                    DocumentPersistedInfo dgei = p.Value;
                    if ( dgei.m_selectedGroup != null )
                        docElement.SetAttribute( "selectedGroup", dgei.m_selectedGroup );

                    foreach ( string absGroupName in dgei.m_expandedGroups )
                    {
                        XmlElement gElement = xmlDoc.CreateElement( "group" );
                        docElement.AppendChild( gElement );
                        gElement.SetAttribute( "absName", absGroupName );
                    }
                }

                return xmlDoc.InnerXml;
            }
            set
            {
                // Attempt to read the settings in the XML format.
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml( value );
                    XmlElement root = xmlDoc.DocumentElement;

                    // Add the new info
                    foreach ( XmlNode info in root.GetElementsByTagName( "doc" ) )
                    {
                        string uriString = info.Attributes["uri"].Value;
                        DocumentPersistedInfo dgei = GetDocumentPersistedInfo( new Uri( uriString ) );
                        XmlAttribute selectedGroup = info.Attributes["selectedGroup"];
                        if ( selectedGroup != null )
                            dgei.m_selectedGroup = selectedGroup.Value;

                        XmlElement docElement = (XmlElement)info;
                        foreach ( XmlNode group in docElement.GetElementsByTagName( "group" ) )
                        {
                            SetGroupExpandedOrColapsed( dgei, group.Attributes["absName"].Value, true );
                        }
                    }

                }
                catch ( XmlException )
                {
                    Outputs.WriteLine( OutputMessageType.Error, "Reading group settings failed" );
                }
            }
        }

        private class DocumentPersistedInfo
        {
            public HashSet<string> m_expandedGroups = new HashSet<string>();
            public string m_selectedGroup; // or preset
        }

        private static void SetGroupExpandedOrColapsed( DocumentPersistedInfo dpi, string groupAbsName, bool expanded )
        {
            if ( expanded )
                dpi.m_expandedGroups.Add( groupAbsName );
            else
                dpi.m_expandedGroups.Remove( groupAbsName );
        }

        private void SetGroupExpandedOrColapsed2( Uri documentUri, Group group, bool expanded )
        {
            DocumentPersistedInfo dpi = GetDocumentPersistedInfo( documentUri );
            SetGroupExpandedOrColapsed( dpi, group.AbsoluteName, expanded );
        }

        private DocumentPersistedInfo GetDocumentPersistedInfo( Uri documentUri )
        {
            DocumentPersistedInfo dpi;
            if ( !m_groupExpandedInfo.TryGetValue( documentUri, out dpi ) )
            {
                dpi = new Editor.DocumentPersistedInfo();
                m_groupExpandedInfo.Add( documentUri, dpi );
            }

            return dpi;
        }

        private void documentControl_treeControl_NodeExpandedChanged( object sender, TreeControl.NodeEventArgs e )
        {
            Group group = e.Node.Tag.As<Group>();
            if ( group != null )
            {
                IDocument document = group.DomNode.GetRoot().Cast<IDocument>();
                SetGroupExpandedOrColapsed2( document.Uri, group, e.Node.Expanded );
            }
        }

        private Dictionary<Uri, DocumentPersistedInfo> m_groupExpandedInfo = new Dictionary<Uri, DocumentPersistedInfo>();

        [Import( AllowDefault = false )]
        private MainForm m_mainForm = null; //initialize to null to avoid incorrect compiler warning

        [Import(AllowDefault = false)]
        private IContextRegistry m_contextRegistry = null; //initialize to null to avoid incorrect compiler warning

        [Import( AllowDefault = false )]
        private IDocumentRegistry m_documentRegistry = null;

        [Import( AllowDefault = false )]
        private IDocumentService m_documentService = null;

        [Import( AllowDefault = false )]
        private IControlHostService m_controlHostService = null;

        [Import( AllowDefault=true )]
        private IFileWatcherService m_fileWatcherService = null;

        [Import( AllowDefault = false )]
        private SettingsService m_settingsService = null;

        private string m_descFileToUseWhenCreatingNewDocument = null;
        private ReloadInfo m_reloadInfo = null;
        private bool m_mainFormClosing = false;
        private ISelectionContext m_selectionContext = null;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders = null;

        [Import( AllowDefault = false )]
        private ICommandService m_commandService = null;
    }
}
