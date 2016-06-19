//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

using pico;
using pico.Timeline;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoTimelineEditor.DomNodeAdapters
{
	/// <summary>
	/// Adapts DomNode to a special purpose group of camera tracks</summary>
	public class GroupAnimController : Group, ITimelineValidationCallback, IFileChangedNotification
	{
		/// <summary>
		/// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
		/// Raises the DomNodeAdapter NodeSet event. Creates several curves, automatically added to the animation.</summary>
		protected override void OnNodeSet()
		{
			base.OnNodeSet();

			//ReloadSkelInfo();

			//DomNode.AttributeChanged += DomNode_AttributeChanged;

			IList<ITrack> tracks = Tracks;
			if ( tracks.Count > 0 )
				return;

			// add default track for camera animation
			//
			TrackAnimController track = (new DomNode( Schema.trackAnimControllerType.Type )).As<TrackAnimController>();
			track.Name = "TrackAnimController";
			tracks.Add( track );
		}

		/// <summary>
		/// Gets or sets the track root node</summary>
		public string RootNode
		{
			get { return (string) DomNode.GetAttribute( Schema.groupAnimControllerType.rootNodeAttribute ); }
			set { DomNode.SetAttribute( Schema.groupAnimControllerType.rootNodeAttribute, value ); }
		}

		/// <summary>
		/// Gets or sets the track skel filename</summary>
		public string SkelFilename
		{
			get { return (string) DomNode.GetAttribute( Schema.groupAnimControllerType.skelFileAttribute ); }
			set { DomNode.SetAttribute( Schema.groupAnimControllerType.skelFileAttribute, value ); }
		}

		//public pico.Anim.SkelFileInfo Skel { get { return m_sfi; } }

		//public void ReloadSkelInfo()
		//{
		//	if ( SkelFilename != null && SkelFilename.Length > 0 )
		//	{
		//		pico.Anim.SkelFileInfo sfi = pico.Anim.SkelFileInfo.ReadFromFile2( SkelFilename );
		//		if ( sfi != null )
		//			m_sfi = sfi;
		//	}
		//}

		//private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		//{
		//	if ( e.AttributeInfo.Equivalent( Schema.groupAnimControllerType.skelFileAttribute ) )
		//	{
		//		ReloadSkelInfo();
		//	}
		//}

		#region IFileChangedNotification

		void IFileChangedNotification.OnFileChanged( System.IO.FileSystemEventArgs e, string ext, string picoDemoPath )
		{
			//if ( ext != ".skel" )
			//	return;

			//if ( picoDemoPath != SkelFilename )
			//	return;

			//ReloadSkelInfo();
		}

		#endregion

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
			if ( !parent.Is<Timeline>() )
				return false;

			return true;
		}

		//private pico.Anim.SkelFileInfo m_sfi;
	}
}




