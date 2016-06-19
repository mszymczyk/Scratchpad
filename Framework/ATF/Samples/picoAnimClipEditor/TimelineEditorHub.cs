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

using picoAnimClipEditor.DomNodeAdapters;
using Sce.Atf.Controls.Timelines.Direct2D;
using Sce.Atf.Controls.SyntaxEditorControl;

using pico.Hub;

namespace picoAnimClipEditor
{
    /// <summary>
    /// Part of editor that handles interaction with picoHub
	/// </summary>
    public partial class TimelineEditor //: IDocumentClient, IControlHostClient, IPaletteClient, IInitializable
    {
		public static readonly string ANIMCLIPEDITOR_TAG = "animClipEditor";

		private void hubService_MessageReceived( object sender, pico.Hub.MessagesReceivedEventArgs args )
		{
			// callback on main thread
			//
			foreach ( HubMessageIn msg in args.Messages )
			{
				if ( msg.payloadSize_ <= ANIMCLIPEDITOR_TAG.Length )
					continue;

				string tag = msg.UnpackString( ANIMCLIPEDITOR_TAG.Length );
				if ( tag != ANIMCLIPEDITOR_TAG )
					return;

				string cmd = msg.UnpackString();
				if ( cmd == "clearAnimList" )
				{
					m_animListEditor.RemoveAllItems();
				}
				else if ( cmd == "animList" )
				{
					string category = msg.UnpackString();
					int nAnims = msg.UnpackInt();
					for ( int ianim = 0; ianim < nAnims; ++ianim )
					{
						string userName = msg.UnpackString();
						string fileName = msg.UnpackString();

						picoAnimListEditorElement ale = new picoAnimListEditorElement( category, userName, fileName );
						ale.updateIcon();
						m_animListEditor.AddItem( ale, category, this );
					}
				}
				else if ( cmd == "requestPlaybackInfo" )
				{
					string editMode = m_editMode.ToString();
					changeEditMode( editMode );
				}
				else if ( cmd == "scrubberPos" )
				{
					TimelineDocument document = ActiveDocument;
					if ( document == null )
						continue;

					float scrubberPos = msg.UnpackFloat();
					m_receivingScrubberPos = true;
					document.ScrubberManipulator.Position = scrubberPos;
					m_receivingScrubberPos = false;
				}
			}
		}

		private void hubService_sendRefreshList()
		{
			HubMessage hubMsg = new HubMessage( ANIMCLIPEDITOR_TAG );
			hubMsg.appendString( "refreshAnimList" );
			m_hubService.send( hubMsg );
		}

		private void hubService_sendScrubberPosition()
		{
			if ( m_editMode != EditMode.Editing || m_receivingScrubberPos )
				return;

			TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
			if ( context == null )
				return;

			float scrubberPosition = 0;
			TimelineDocument document = context.As<TimelineDocument>();
			if ( document != null )
				scrubberPosition = document.ScrubberManipulator.Position;

			HubMessage hubMsg = new HubMessage( ANIMCLIPEDITOR_TAG );
			hubMsg.appendString( "scrubberPos" );
			hubMsg.appendFloat( scrubberPosition );
			m_hubService.send( hubMsg );
		}

		public void changeEditMode( string editMode )
		{
			TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
			if ( context == null )
				return;

			setEditMode( editMode );

			m_hubService.BlockOutboundTraffic = false;

			HubMessage hubMsg = new HubMessage( ANIMCLIPEDITOR_TAG );
			hubMsg.appendString( "editMode" ); // command

			float scrubberPosition = 0;
			TimelineDocument document = context.As<TimelineDocument>();
			if ( document != null )
				scrubberPosition = document.ScrubberManipulator.Position;

			hubMsg.appendString( editMode ); // what mode
			hubMsg.appendFloat( scrubberPosition );
			m_hubService.send( hubMsg );

			if ( EditMode == picoAnimClipEditor.EditMode.Standalone )
			{
				m_hubService.BlockOutboundTraffic = true;
			}
			else
			{
				Timeline tim = context.Cast<Timeline>();
				changePreview( tim.AnimCategory, tim.AnimUserName );
			}
		}

		public void changePreview( string category, string userName )
		{
			if ( EditMode == picoAnimClipEditor.EditMode.Standalone )
				return;

			HubMessage hubMsg = new HubMessage( ANIMCLIPEDITOR_TAG );
			hubMsg.appendString( "preview" );
			hubMsg.appendString( category );
			hubMsg.appendString( userName );
			m_hubService.send( hubMsg );
		}

		void AnimListEditorTreeControl_MouseDoubleClick( object sender, MouseEventArgs e )
		{
			TreeControl tc = m_animListEditor.TreeControl;
			//TreeControl tc = (TreeControl) sender;
			//Point localPos = tc.PointToClient( new Point(e.X, e.Y) );
			//Point localPos2 = tc.PointToClient( e.Location );
			//TreeControl.Node nod = tc.GetNodeAt( localPos );
			TreeControl.Node nod = tc.GetNodeAt( e.Location );
			if ( nod != null )
			{
				picoAnimListEditorElement ale = nod.Tag as picoAnimListEditorElement;
				if ( ale != null )
				{
					changePreview( ale.Category, ale.UserName );

					string metadataPathTmp = pico.Paths.PICO_DEMO_data + ale.FileName;
					string metadataPath = Path.ChangeExtension( metadataPathTmp, ".animdata" );
					Uri uri = new Uri( Path.GetFullPath(metadataPath) );
					m_animListEditorElementToUseWhenCreatingNewDocument = ale;
					IDocument document = m_documentService.OpenExistingDocument( this, uri );
					m_animListEditorElementToUseWhenCreatingNewDocument = null;
					if ( document.Dirty )
						m_documentService.Save( document );
				}
			}
		}

		private bool m_receivingScrubberPos;
	}
}
