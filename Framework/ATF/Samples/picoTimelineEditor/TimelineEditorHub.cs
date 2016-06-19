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
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;

using picoTimelineEditor.DomNodeAdapters;
using Sce.Atf.Controls.Timelines.Direct2D;
using Sce.Atf.Controls.SyntaxEditorControl;

using pico.Hub;

namespace picoTimelineEditor
{
    /// <summary>
    /// Part of editor that handles interaction with picoHub
	/// </summary>
    public partial class TimelineEditor
    {
		public static readonly string TIMELINEEDITOR_TAG = "timeline";

		private void hubService_receiveMessages( object sender, pico.Hub.MessagesReceivedEventArgs args )
		{
			// callback on main thread
			//
			foreach ( HubMessageIn msg in args.Messages )
			{
				if ( msg.payloadSize_ <= TIMELINEEDITOR_TAG.Length )
					continue;

				string tag = msg.UnpackString( TIMELINEEDITOR_TAG.Length );
				if ( tag != TIMELINEEDITOR_TAG )
					return;

				string cmd = msg.UnpackString();
				if ( cmd == "levelChanged" )
				{
					// commands must be send exactly in the order it is below
					//

					foreach ( IDocument document in m_documentRegistry.Documents )
					{
						if ( !document.Dirty )
							continue;

						hubService_sendReloadTimeline( document.Cast<TimelineDocument>(), false );
					}

					hubService_sendSelectTimeline( !Playing );
					hubService_sendPlayPause();
				}
				else if ( cmd == "scrubberPosPico" )
				{
					TimelineDocument document = ActiveDocument;
					if ( document == null )
						continue;

					string filename = msg.UnpackString();
					string absFilename = pico.Paths.LocalPathToPicoDataAbsolutePath( filename );
					if ( document.Uri.LocalPath != absFilename )
					{
						Outputs.WriteLine( OutputMessageType.Warning, "{0} doesn't match active document", filename );
						continue;
					}

					float scrubberPos = msg.UnpackFloat();
					m_receivingScrubberPos = true;
					document.ScrubberManipulator.Position = scrubberPos;
					m_receivingScrubberPos = false;
				}
			}
		}

		public void hubService_sendSelectTimeline( bool sendScrubberPosition )
		{
			string docUri = null;

			TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
			if ( context != null )
			{
				TimelineDocument document = context.As<TimelineDocument>();
				if ( document != null )
				{
					docUri = pico.Paths.UriToPicoDemoPath( document.Uri );
				}
			}

			if ( string.IsNullOrEmpty( docUri ) )
				docUri = ".*";

			HubMessage hubMsg = new HubMessage( TIMELINEEDITOR_TAG );
			hubMsg.appendString( "selectTimeline" ); // command
			hubMsg.appendString( docUri ); // what timeline
			m_hubService.send( hubMsg );

			if ( sendScrubberPosition && docUri != ".*" )
				hubService_sendScrubberPosition();
		}

		public void hubService_sendPlayPause()
		{
			HubMessage hubMsg = new HubMessage( TIMELINEEDITOR_TAG );
			hubMsg.appendString( "playPause" ); // command
			hubMsg.appendInt( Playing ? 1 : 0 );

			//float scrubberPosition = 0;
			//TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
			//if ( context != null )
			//{
			//	TimelineDocument document = context.As<TimelineDocument>();
			//	scrubberPosition = document.ScrubberManipulator.Position;
			//}
			//hubMsg.appendFloat( scrubberPosition );

			m_hubService.send( hubMsg );
		}

		public void hubService_sendScrubberPosition()
		{
			if ( m_receivingScrubberPos )
				return;

			HubMessage hubMsg = new HubMessage( TIMELINEEDITOR_TAG );
			hubMsg.appendString( "scrubberPos" );

			string docUri = null;
			float scrubberPosition = 0;

			TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
			if ( context != null )
			{
				TimelineDocument document = context.As<TimelineDocument>();
				if ( document != null )
				{
					docUri = pico.Paths.UriToPicoDemoPath( document.Uri );
					scrubberPosition = document.ScrubberManipulator.Position;
				}
			}

			if ( string.IsNullOrEmpty( docUri ) )
				docUri = ".*";

			hubMsg.appendString( docUri );
			hubMsg.appendFloat( scrubberPosition );

			m_hubService.send( hubMsg );
		}

		public void hubService_sendReloadTimeline( TimelineDocument document, bool scrollToTime )
		{
			if (m_isWriting)
				return;

			string docUri = pico.Paths.UriToPicoDemoPath( document.Uri );
			if ( string.IsNullOrEmpty( docUri ) )
			{
				Outputs.WriteLine( OutputMessageType.Error, "Document's path is not within PICO_DEMO directory!" );
				return;
			}

			m_isWriting = true;

			MemoryStream stream = new MemoryStream();
			var writer = new TimelineXmlWriter( s_schemaLoader.TypeCollection );
			writer.Write( document.DomNode, stream, document.Uri );

			HubMessage hubMessage = new HubMessage( TIMELINEEDITOR_TAG );
			hubMessage.appendString( "reloadTimeline" );
			hubMessage.appendString( docUri );
			hubMessage.appendInt( scrollToTime ? 1 : 0 );
			hubMessage.appendInt( (int) stream.Length );
			hubMessage.appendBytes( stream.ToArray() );
			hubMessage.appendFloat( document.ScrubberManipulator.Position );

			m_hubService.send( hubMessage );

			m_isWriting = false;
		}

		private bool m_receivingScrubberPos;
		private bool m_isWriting; // to prevent endless recursion while serializing DOM with TimelineXmlWriter
	}
}
