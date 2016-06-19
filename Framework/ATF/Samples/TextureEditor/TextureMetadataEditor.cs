//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.IO;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Applications;
using PropertyGrid = Sce.Atf.Controls.PropertyEditing.PropertyGrid;
using Sce.Atf.Adaptation;
using System.Windows.Forms;
using System.Reflection;

namespace TextureEditor
{
    /// <summary>
    /// Component to edit resource meta-data.
    /// </summary>
	[Export( typeof( IInitializable ) )]    
    [PartCreationPolicy(CreationPolicy.Shared)]
	public class TextureMetadataEditor : IInitializable
    {
        public TextureMetadataEditor()
        {
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
			//if (m_resourceLister == null || m_resourceMetadataService == null)
			//	return;

			m_previewWindow = new TexturePreviewWindowSharpDX(m_contextRegistry);
			m_textureViewCommands = new TextureViewCommands( m_commandService, m_previewWindow, m_mainForm, m_schemaLoader );

			ControlInfo cinfo = new ControlInfo("Texture Preview", "texture viewer", StandardControlGroup.CenterPermanent);
			m_controlHostService.RegisterControl(m_previewWindow, cinfo, null);

			m_resourceLister.SetRootFolder( new CustomFileSystemResourceFolder( pico.Paths.PICO_DEMO ) );

			//m_resourceLister.SelectionChanged += resourceLister_SelectionChanged;
			m_resourceLister.ThumbnailControl.SelectionChanged += resourceLister_SelectionChanged_ThumbnailView;
			//m_resourceLister.ListView.MultiSelect = false;
			// SelectedIndexChanged is called every time item is added to list
			// in case of multiselection, items are added one at a time
			// and performance of resourceLister_SelectionChanged_ListView drops considerably due to many slection changes
			//
			//m_resourceLister.ListView.SelectedIndexChanged += resourceLister_SelectionChanged_ListView;
			m_resourceLister.ListView.MouseUp += resourceLister_SelectionChanged_ListView;

			m_propertyGrid = new PropertyGrid();
			m_controlInfo = new ControlInfo(
				"Source texture Information".Localize(),
				"Displays information about source texture".Localize(),
				StandardControlGroup.Hidden);
			m_controlHostService.RegisterControl(m_propertyGrid, m_controlInfo, null);

			m_helpTextBox = new RichTextBox();
			string aboutFilePath = "TextureEditor.Resources.Help.rtf";
			Stream textFileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream( aboutFilePath );
			if ( textFileStream != null )
				m_helpTextBox.LoadFile( textFileStream, RichTextBoxStreamType.RichText );

			ControlInfo helpTextBoxInfo = new ControlInfo( "Help", "help", StandardControlGroup.Bottom );
			m_controlHostService.RegisterControl( m_helpTextBox, helpTextBoxInfo, null );

			DomNode rootNode = new DomNode(Schema.textureMetadataEditorType.Type, Schema.textureMetadataEditorRootElement);
			rootNode.InitializeExtensions();

			m_editorRootNode = rootNode;

			if (m_domExplorer != null)
				m_domExplorer.Root = m_editorRootNode;

			//m_editingContext = new ResourceMetadataEditingContext();
			m_contextRegistry.ActiveContext = rootNode;
        }

        #endregion

		//private void resourceLister_SelectionChanged( object sender, EventArgs e )
		//{
		//	////var edContext = m_editorRootNode.Cast<ResourceMetadataEditingContext>();
		//	////edContext.SetRange(mdatadata);
		//	//IEnumerable<Uri> resourceUris = null;// m_resourceLister.Selection;
		//	//IEnumerable<Uri> resourceUris = m_resourceLister.ThumbnailControl.Selection;
		//	List<Uri> resourceUris = new List<Uri>();
		//	foreach( var t in m_resourceLister.ThumbnailControl.Selection )
		//	{
		//		Uri path = t.Tag as Uri;
		//		resourceUris.Add( path );
		//	}
		//}

		private void resourceLister_SelectionChanged_ThumbnailView( object sender, EventArgs e )
		{
			List<Uri> resourceUris = new List<Uri>();
			foreach ( var t in m_resourceLister.ThumbnailControl.Selection )
			{
				Uri path = t.Tag as Uri;
				resourceUris.Add( path );
			}

			SelectionChangedImpl( resourceUris );
		}
		private void resourceLister_SelectionChanged_ListView( object sender, EventArgs e )
		{
			List<Uri> resourceUris = new List<Uri>();
			foreach ( ListViewItem t in m_resourceLister.ListView.SelectedItems )
			{
				Uri path = t.Tag as Uri;
				resourceUris.Add( path );
			}

			SelectionChangedImpl( resourceUris );
		}

		private void SelectionChangedImpl( List<Uri> resourceUris )
		{
			List<DomNode> rootNodes = new List<DomNode>();
			foreach ( Uri resourceUri in resourceUris )
			{
				string reExt = System.IO.Path.GetExtension( resourceUri.LocalPath ).ToLower();

				string metadataFilePath = resourceUri.LocalPath + ".metadata";
				Uri metadataUri = new Uri( metadataFilePath );
				DomNode rootNode = null;

				if ( m_loadedNodes.TryGetValue( metadataUri, out rootNode ) )
				{
				}
				else
				{
					if ( File.Exists( metadataFilePath ) )
					{
						// read existing metadata
						using ( FileStream stream = File.OpenRead( metadataFilePath ) )
						{
							var reader = new DomXmlReader( m_schemaLoader );
							rootNode = reader.Read( stream, metadataUri );
						}
					}
					else
					{
						rootNode = new DomNode( Schema.textureMetadataType.Type, Schema.textureMetadataRootElement );
						rootNode.SetAttribute( Schema.resourceMetadataType.uriAttribute, resourceUri );
					}

					m_loadedNodes.Add( metadataUri, rootNode );

					rootNode.InitializeExtensions();

					ResourceMetadataDocument document = rootNode.Cast<ResourceMetadataDocument>();
					document.Uri = metadataUri;

					TextureMetadata md = rootNode.Cast<TextureMetadata>();
					md.LocalPath = resourceUri.LocalPath;

					// this node must be added to root in order for history to work
					//
					m_editorRootNode.GetChildList( Schema.textureMetadataEditorType.textureMetadataChild ).Add( rootNode );
				}

				rootNodes.Add( rootNode );
			}

			if ( rootNodes.Count > 0 )
			{
				DomNode lastNode = rootNodes.Last();
				TextureMetadata md = lastNode.Cast<TextureMetadata>();

				TextureProperties tp = m_previewWindow.showResource( md );
				if ( tp != null )
				{
					m_propertyGrid.Bind( new[] { tp } );
					m_textureViewCommands.onShowResource( tp );
				}
			}

			//System.Diagnostics.Debug.WriteLine( "Nb nodes" + rootNodes.Count );

			var edContext = m_editorRootNode.Cast<ResourceMetadataEditingContext>();
			edContext.SetRange( rootNodes );
		}

		[Import( AllowDefault = true )]
		private ResourceLister m_resourceLister = null;

        [Import(AllowDefault = false)]
        private ControlHostService m_controlHostService = null;

		[Import(AllowDefault = false)]
		private IContextRegistry m_contextRegistry = null;

		[Import(AllowDefault = false)]
		private ICommandService m_commandService = null;

		[Import(AllowDefault = true)]
		private DomExplorer m_domExplorer = null;

		[Import(AllowDefault = false)]
		private SchemaLoader m_schemaLoader = null;

		[Import( AllowDefault = false )]
		private MainForm m_mainForm = null;

		private ControlInfo m_controlInfo;
		private PropertyGrid m_propertyGrid;
		private RichTextBox m_helpTextBox;

		DomNode m_editorRootNode;

		private Dictionary<Uri, DomNode> m_loadedNodes = new Dictionary<Uri, DomNode>();

		private TexturePreviewWindowSharpDX m_previewWindow;
		private TextureViewCommands m_textureViewCommands;
	}
}
