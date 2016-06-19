//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.Timelines.Direct2D;

namespace picoTimelineEditor
{
	class picoCutToTimelineBatchConverter
	{
		public picoCutToTimelineBatchConverter()
		{

		}

		public void batchConvert()
		{
			// find all cutscene files
			//
			string[] filesFound = System.IO.Directory.GetFiles( pico.Paths.PICO_DEMO_data, "*.cut", SearchOption.AllDirectories );
			string[] files = filesFound.Where( name => ( !name.Contains("Kopia") && !name.Contains("Copy") ) ).ToArray();
			Outputs.WriteLine( OutputMessageType.Info, string.Format( "Found {0} cutscene files.", files.Length ) );

			foreach( string cutsceneFilename in files )
			{
				string timelineFilename = Path.ChangeExtension( cutsceneFilename, ".timeline" );

				Outputs.WriteLine( OutputMessageType.Info, string.Format( "Converting {0} to {1}", cutsceneFilename, timelineFilename ) );

				DomNode domNode = _CreateTimelineDocument( cutsceneFilename );

				picoCutToTimelineConverter converter = new picoCutToTimelineConverter( cutsceneFilename );
				if ( !converter.Convert( domNode ) )
				{
					Outputs.WriteLine( OutputMessageType.Error, "Conversion FAILED!" );
					domNode = null;
					continue;
				}

				TimelineDocument timelineDocument = domNode.Cast<TimelineDocument>();

				string filePath = timelineFilename;
				FileMode fileMode = File.Exists( filePath ) ? FileMode.Truncate : FileMode.OpenOrCreate;
				using ( FileStream stream = new FileStream( filePath, fileMode ) )
				{
					var writer = new TimelineEditor.TimelineXmlWriter( TimelineEditor.s_schemaLoader.TypeCollection );
					writer.Write( timelineDocument.DomNode, stream, new System.Uri(filePath) );
				}

				domNode = null;
			}

			Outputs.WriteLine( OutputMessageType.Info, string.Format( "Converting {0} cutscene files completed successfully", files.Length ) );
		}

		private DomNode _CreateTimelineDocument( string filePath )
		{
			DomNode node = new DomNode( Schema.timelineType.Type, Schema.timelineRootElement );
			TimelineDocument document = node.Cast<TimelineDocument>();

			picoD2dTimelineRenderer renderer = new picoD2dTimelineRenderer();
			document.Renderer = renderer;
			renderer.Init( document.TimelineControl.D2dGraphics );

			string fileName = Path.GetFileName( filePath );
			ControlInfo controlInfo = new ControlInfo( fileName, filePath, StandardControlGroup.Center );

			//Set IsDocument to true to prevent exception in command service if two files with the
			//  same name, but in different directories, are opened.
			controlInfo.IsDocument = true;

			TimelineContext timelineContext = document.Cast<TimelineContext>();
			timelineContext.ControlInfo = controlInfo;

			document.Uri = new System.Uri( filePath );

			//if ( isMasterDocument )
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

			//IHierarchicalTimeline hierarchical = document.Timeline as IHierarchicalTimeline;
			//if ( hierarchical != null )
			//{
			//	ResolveAll( hierarchical, new HashSet<IHierarchicalTimeline>() );
			//}

			//// Listen to events if this is the first time we've seen this.
			//if ( isNewToThisEditor )
			//{
			//	// The master document/context needs to listen to events on any sub-document
			//	//  so that transactions can be cancelled correctly.
			//	if ( isMasterDocument )
			//	{
			//		node.AttributeChanging += DomNode_AttributeChanging;
			//	}
			//	else
			//	{
			//		DomNode masterNode = s_repository.ActiveDocument.As<DomNode>();
			//		node.SubscribeToEvents( masterNode );
			//	}
			//}

			//TimelineHubCommunication hubComm = node.Cast<TimelineHubCommunication>();
			//hubComm.setup( m_hubService, s_schemaLoader );

			//// select this document initially, so timeline properties are visible
			////
			//ISelectionContext selectionContext = document.Cast<ISelectionContext>();
			//selectionContext.Set( node );

			// Initialize Dom extensions now that the data is complete
			node.InitializeExtensions();

			return node;
		}

		//private List<string> DirSearch()
		//{
		//	try
		//	{
		//		foreach ( string d in Directory.GetDirectories( sDir ) )
		//		{
		//			foreach ( string f in Directory.GetFiles( d, txtFile.Text ) )
		//			{
		//				lstFilesFound.Items.Add( f );
		//			}
		//			DirSearch( d );
		//		}
		//	}
		//	catch ( System.Exception excpt )
		//	{
		//		Console.WriteLine( excpt.Message );
		//	}
		//}
	}
}
