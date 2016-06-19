using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls;

using pico.Hub;

namespace pico.Controls
{	/// <summary>
	/// Component for communication with picoHub
	/// </summary>
	[Export( typeof( IInitializable ) )]
	[Export( typeof( TouchPad ) )]
	[PartCreationPolicy( CreationPolicy.Shared )]
	public partial class TouchPad : AdaptableControl, IInitializable
	{
		public TouchPad()
		{
			InitializeComponent();
			//Controls.Add( buttonFrameSelection );
			labelHint.Text =
@"Hold left mouse button to rotate camera
Use WASDQZ to move
Hold left SHIFT to move quicker";
			Controls.Add( labelHint );

			splitButtonFrameSelection.ShowSplit = true;
			splitButtonFrameSelection.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
			splitButtonFrameSelection.ContextMenuStrip.Items.Add( "+XYZ" );
			splitButtonFrameSelection.ContextMenuStrip.Items.Add( "-XYZ" );

			splitButtonFrameSelection.ContextMenuStrip.Items.Add( "+X" );
			splitButtonFrameSelection.ContextMenuStrip.Items.Add( "-X" );
			splitButtonFrameSelection.ContextMenuStrip.Items.Add( "+Y" );
			splitButtonFrameSelection.ContextMenuStrip.Items.Add( "-Y" );
			splitButtonFrameSelection.ContextMenuStrip.Items.Add( "+Z" );
			splitButtonFrameSelection.ContextMenuStrip.Items.Add( "-Z" );

			splitButtonFrameSelection.ContextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked;
			splitButtonFrameSelection.Click += splitButtonFrameSelection_Click;
			splitButtonFrameSelection.Text = splitButtonFrameSelection.ContextMenuStrip.Items[0].ToString();
			Controls.Add( splitButtonFrameSelection );

			Controls.Add( checkBoxFollowSelection );
			checkBoxFollowSelection.CheckedChanged += checkBoxFollowSelection_CheckedChanged;

			//wyDay.Controls.SplitButton sb = new wyDay.Controls.SplitButton();
			//sb.Text = "SplitButton";
			//sb.Name = "SplitButton";
			//sb.Location = new System.Drawing.Point( 0, 100 );
			//Controls.Add( sb );

			MouseDown += TouchPad_MouseDown;
			MouseUp += TouchPad_MouseUp;
			MouseMove += TouchPad_MouseMove;
			GotFocus += TouchPad_GotFocus;
			LostFocus += TouchPad_LostFocus;
			KeyDown += TouchPad_KeyDown;
			KeyUp += TouchPad_KeyUp;
			//buttonFrameSelection.Click += buttonFrameSelection_Click;
		}

		#region IInitializable

		/// <summary>
		/// Finishes initializing component by setting up scripting service, subscribing to document
		/// events, and creating PropertyDescriptors for settings</summary>
		void IInitializable.Initialize()
		{
			ControlInfo controlInfo = new ControlInfo( "CameraController", "Camera Controller", StandardControlGroup.Bottom );
			m_controlHostService.RegisterControl(
				this,
				controlInfo,
				null );
		}

		#endregion

		void TouchPad_MouseDown( object sender, MouseEventArgs e )
		{
			if ( e.Button == System.Windows.Forms.MouseButtons.Left )
			{
				m_mouseDown = true;
				m_mousePosition = e.Location;

				SendMouseDown();
			}
		}

		void TouchPad_MouseUp( object sender, MouseEventArgs e )
		{
			m_mouseDown = false;
			SendMouseUp();
		}

		void TouchPad_MouseMove( object sender, MouseEventArgs e )
		{
			if ( m_mouseDown )
			{
				m_mousePosition = e.Location;

				SendMouseMove();
			}
		}

		void TouchPad_GotFocus( object sender, EventArgs e )
		{
			HasKeyboardFocus = true;
		}

		void TouchPad_LostFocus( object sender, EventArgs e )
		{
			m_mouseDown = false;
			HasKeyboardFocus = false;
		}

		void TouchPad_KeyDown( object sender, KeyEventArgs e )
		{
			int keyVal = e.KeyValue;
			if ( keyVal < 256 )
			{
				HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
				hubMsg.appendString( "touchPad" );
				hubMsg.appendString( "key" );
				hubMsg.appendInt( keyVal );
				hubMsg.appendInt( 1 );
				m_hubService.send( hubMsg );
			}
		}

		void TouchPad_KeyUp( object sender, KeyEventArgs e )
		{
			int keyVal = e.KeyValue;
			if ( keyVal < 256 )
			{
				HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
				hubMsg.appendString( "touchPad" );
				hubMsg.appendString( "key" );
				hubMsg.appendInt( keyVal );
				hubMsg.appendInt( 0 );
				m_hubService.send( hubMsg );
			}
		}

		//void buttonFrameSelection_Click( object sender, EventArgs e )
		//{
		//	HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
		//	hubMsg.appendString( "touchPad" );
		//	hubMsg.appendString( "frameSelection" );
		//	m_hubService.send( hubMsg );
		//}

		void ContextMenuStrip_ItemClicked( object sender, ToolStripItemClickedEventArgs e )
		{
			string text = e.ClickedItem.ToString();
			splitButtonFrameSelection.Text = text;

			splitButtonFrameSelection_Click( sender, e );
		}

		void splitButtonFrameSelection_Click( object sender, EventArgs e )
		{
			HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
			hubMsg.appendString( "touchPad" );
			hubMsg.appendString( "frameSelection" );
			hubMsg.appendString( splitButtonFrameSelection.Text );
			m_hubService.send( hubMsg );
		}

		void checkBoxFollowSelection_CheckedChanged( object sender, EventArgs e )
		{
			HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
			hubMsg.appendString( "touchPad" );
			hubMsg.appendString( "followSelection" );
			if ( checkBoxFollowSelection.Checked )
				hubMsg.appendInt( 1 );
			else
				hubMsg.appendInt( 0 );
			m_hubService.send( hubMsg );
		}

		private void SendMouseDown()
		{
			HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
			hubMsg.appendString( "touchPad" );
			hubMsg.appendString( "mouseDown" );
			FillButtons( hubMsg );
			m_hubService.send( hubMsg );
		}

		private void SendMouseUp()
		{
			HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
			hubMsg.appendString( "touchPad" );
			hubMsg.appendString( "mouseUp" );
			m_hubService.send( hubMsg );
		}

		private void SendMouseMove()
		{
			HubMessage hubMsg = new HubMessage( APPPARAM_TAG );
			hubMsg.appendString( "touchPad" );
			hubMsg.appendString( "mouseMove" );
			FillButtons( hubMsg );
			m_hubService.send( hubMsg );
		}

		private void FillButtons( HubMessage msg )
		{
			msg.appendInt( m_mousePosition.X );
			msg.appendInt( m_mousePosition.Y );
		}

		[Import( AllowDefault = true )]
		private HubService m_hubService = null;

		[Import( AllowDefault = true )]
		private IControlHostService m_controlHostService = null;

		private bool m_mouseDown;
		private Point m_mousePosition;

		public static readonly string APPPARAM_TAG = "appparam";

		private bool[] m_keysDown = new bool[127];
	}
}
