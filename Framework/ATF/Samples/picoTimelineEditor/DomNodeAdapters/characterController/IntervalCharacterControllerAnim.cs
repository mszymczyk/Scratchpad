//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.VectorMath;

using pico;
using pico.Timeline;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to an Interval</summary>
	public class IntervalCharacterControllerAnim : Interval, ITimelineObjectCreator, IFileChangedNotification, ITimelineValidationCallback
    {

		///// <summary>
		///// Gets and sets the event's name</summary>
		//public override string Name
		//{
		//	get { return (string) DomNode.GetAttribute( Schema.intervalCharacterControllerAnimType.nameAttribute ); }
		//	set { DomNode.SetAttribute( Schema.intervalCharacterControllerAnimType.nameAttribute, value ); }
		//}

		///// <summary>
		///// Gets and sets the event's color</summary>
		//public override Color Color
		//{
		//	get { return Color.FromArgb( (int)DomNode.GetAttribute( Schema.intervalCharacterControllerAnimType.colorAttribute ) ); }
		//	set { DomNode.SetAttribute( Schema.intervalCharacterControllerAnimType.colorAttribute, value.ToArgb() ); }
		//}

		/// <summary>
		/// Gets and sets the camera's animation file</summary>
		public string AnimFile
		{
			get { return (string)DomNode.GetAttribute( Schema.intervalCharacterControllerAnimType.animFileAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCharacterControllerAnimType.animFileAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the camera's Anim Offset</summary>
		public float AnimOffset
		{
			get { return (float)DomNode.GetAttribute( Schema.intervalCharacterControllerAnimType.animOffsetAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCharacterControllerAnimType.animOffsetAttribute, value ); }
		}

		/// <summary>
		/// Gets animation's duration in milliseconds
		/// </summary>
		[pico.Controls.PropertyEditing.CutomPropertyEditingAttribute( true, "AnimDuration", "AnimFileInfo", "Animation duration in milliseconds" )]
		public float AnimDuration
		{
			get { return ( m_afh != null ) ? m_afh.durationMilliseconds : 0; }
		}

		/// <summary>
		/// Gets animation's number of frames
		/// </summary>
		[pico.Controls.PropertyEditing.CutomPropertyEditingAttribute( true, "AnimNumFrames", "AnimFileInfo", "Animation frame count" )]
		public int AnimNumFrames
		{
			get { return ( m_afh != null ) ? m_afh.nFrames : 0; }
		}

		/// <summary>
		/// Gets animation's framerate (frames per second)
		/// </summary>
		[pico.Controls.PropertyEditing.CutomPropertyEditingAttribute( true, "AnimFramerate", "AnimFileInfo", "Animation frames per second" )]
		public float AnimFramerate
		{
			get { return ( m_afh != null ) ? m_afh.framerate : 0; }
		}

		/// <summary>
		/// Performs initialization when the adapter is connected to the DomNode.
		/// </summary>
		protected override void OnNodeSet()
		{
			base.OnNodeSet();

			DomNode.AttributeChanged += DomNode_AttributeChanged;

			ReloadAnimFile();
		}

		private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		{
			if ( e.AttributeInfo.Equivalent( Schema.intervalCharacterControllerAnimType.animFileAttribute ) )
			{
				ReloadAnimFile();
			}
		}

		#region ITimelineObjectCreator Members
		ITimelineObject ITimelineObjectCreator.Create()
		{
			DomNodeType nodeType = Schema.intervalCharacterControllerAnimType.Type;
			DomNode dn = new DomNode( nodeType );

			NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
			if (paletteItem != null)
				dn.SetAttribute( nodeType.IdAttribute, paletteItem.Name );
			else
				dn.SetAttribute( nodeType.IdAttribute, "CharacterControllerAnim" );

			return dn.Cast<ITimelineObject>();
		}
		#endregion

		#region IFileChangedNotification

		void IFileChangedNotification.OnFileChanged( System.IO.FileSystemEventArgs e, string ext, string picoDemoPath )
		{
			if ( ext != ".anim" )
				return;

			if ( picoDemoPath != AnimFile )
				return;

			ReloadAnimFile();
		}

		#endregion

		public void ReloadAnimFile()
		{
			if ( AnimFile != null && AnimFile.Length > 0 )
			{
				pico.Anim.AnimFileHeader afh = pico.Anim.AnimFileHeader.ReadFromFile2( AnimFile );
				if ( afh != null )
					m_afh = afh;
			}
		}

		public bool CanParentTo( DomNode parent )
		{
			return ValidateImpl( parent, 0 );
		}

		public bool Validate( DomNode parent )
		{
			return ValidateImpl( parent, 1 );
		}

		private bool ValidateImpl( DomNode parent, int validating )
		{
			if ( !parent.Is<TrackCharacterControllerAnim>() )
				return false;

			return true;
		}

		private pico.Anim.AnimFileHeader m_afh;
	}
}





