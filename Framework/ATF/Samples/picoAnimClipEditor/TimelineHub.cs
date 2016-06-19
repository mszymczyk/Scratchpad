//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.Timelines.Direct2D;

using pico.Hub;

namespace picoAnimClipEditor
{
	class TimelineHubCommunication : DomNodeAdapter
	{
		/// <summary>
		/// Performs initialization when the adapter is connected to the editing context's DomNode.
		/// Raises the EditingContext NodeSet event and performs custom processing to adapt objects
		/// and subscribe to DomNode change events.</summary>
		protected override void OnNodeSet()
		{
			m_timelineDocument = DomNode.Cast<TimelineDocument>();
			m_timelineControl = m_timelineDocument.TimelineControl;
			//m_editMode = "Standalone";

			DomNode.AttributeChanged += DomNode_AttributeChanged;
			DomNode.ChildInserted += DomNode_ChildInserted;
			DomNode.ChildRemoved += DomNode_ChildRemoved;

			base.OnNodeSet();
		}

		private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		{
			// TODO: there's a crash when undoing reference additions
			sendReloadTimeline();
		}

		private void DomNode_ChildInserted( object sender, ChildEventArgs e )
		{
			// TODO: there's a crash when undoing reference additions
			sendReloadTimeline();
		}

		private void DomNode_ChildRemoved( object sender, ChildEventArgs e )
		{
			// TODO: there's a crash when undoing reference additions
			sendReloadTimeline();
		}

		public bool Connected { get; set; }

		public void setup( HubService hubService, SchemaLoader schemaLoader )
		{
			m_schemaLoader = schemaLoader;
			m_hubService = hubService;
			Connected = m_hubService.Connected;
		}

		private bool validate( out string docUri )
		{
			docUri = "";

			if ( m_hubService == null )
				return false;

			if ( !m_hubService.Connected )
			{
				Outputs.WriteLine( OutputMessageType.Error, "Editor is not connected to picoHub" );
				return false;
			}

			if ( m_hubService.BlockOutboundTraffic )
			{
				return false;
			}

			//if ( m_editMode != "Editing" )
			//	return false;

			docUri = pico.Paths.UriToPicoDemoPath( m_timelineDocument.Uri );
			if ( docUri.Length == 0 )
			{
				Outputs.WriteLine( OutputMessageType.Error, "Timeline document {0} is not located within PICO_DEMO\\data folder!", m_timelineDocument.Uri.LocalPath );
				return false;
			}

			return true;
		}

		//public void sendScrubberPosition( float position )
		//{
		//	string docUri;
		//	if ( !validate(out docUri) )
		//		return;

		//	HubMessage hubMsg = new HubMessage( TIMELINE_TAG );
		//	hubMsg.appendString( "scrubberPos" ); // command
		//	hubMsg.appendString( docUri ); // what timeline
		//	hubMsg.appendFloat( position );
		//	m_hubService.send( hubMsg );
		//}
		public void sendScrubberPosition()
		{
			string docUri;
			if ( !validate( out docUri ) )
				return;

			HubMessage hubMsg = new HubMessage( TIMELINE_TAG );
			hubMsg.appendString( "scrubberPos" ); // command
			hubMsg.appendString( docUri ); // what timeline
			TimelineDocument document = this.As<TimelineDocument>();
			hubMsg.appendFloat( document.ScrubberManipulator.Position );
			m_hubService.send( hubMsg );
		}

		//public void setEditMode( string editMode )
		//{
		//	m_editMode = editMode;

		//	//string docUri;
		//	//if ( !validate( out docUri ) )
		//	//	return;

		//	//HubMessage hubMsg = new HubMessage( TIMELINE_TAG );
		//	//hubMsg.appendString( "editMode" ); // command
		//	//hubMsg.appendString( docUri ); // what timeline
		//	//hubMsg.appendString( editMode );
		//	//m_hubService.send( hubMsg );
		//}

		//public string getEditMode()	{ return m_editMode; }

		private void sendReloadTimeline()
		{
			if ( m_isWriting )
				return;

			string docUri;
			if ( !validate( out docUri ) )
				return;

			m_isWriting = true;

			MemoryStream stream = new MemoryStream();
			var writer = new TimelineEditor.TimelineXmlWriter( m_schemaLoader.TypeCollection );
			//writer.PersistDefaultAttributes = true;

			writer.Write( DomNode, stream, m_timelineDocument.Uri );

			HubMessage hubMessage = new HubMessage( TIMELINE_TAG );
			hubMessage.appendString( "reloadTimeline" );
			hubMessage.appendString( docUri );
			hubMessage.appendInt( (int)stream.Length );
			hubMessage.appendBytes( stream.ToArray() );
			TimelineDocument document = this.As<TimelineDocument>();
			hubMessage.appendFloat( document.ScrubberManipulator.Position );

			m_hubService.send( hubMessage );

			m_isWriting = false;
		}

        private TimelineDocument m_timelineDocument;
        private D2dTimelineControl m_timelineControl;
		private SchemaLoader m_schemaLoader;
		private HubService m_hubService;
		private bool m_isWriting; // to prevent endless recursion while serializing DOM with TimelineXmlWriter
		public static readonly string TIMELINE_TAG = "timeline";
		//private string m_editMode;
	};
}
