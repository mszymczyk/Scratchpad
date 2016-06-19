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
	public class GroupCharacterController : Group, ITimelineValidationCallback
	{
		/// <summary>
		/// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
		/// Raises the DomNodeAdapter NodeSet event. Creates several curves, automatically added to the animation.</summary>
		protected override void OnNodeSet()
		{
			base.OnNodeSet();

			ReloadSkelInfo();

			IList<ITrack> tracks = Tracks;
			if ( tracks.Count > 0 )
				return;

			// add default track for camera animation
			//
			TrackCharacterControllerAnim track = ( new DomNode( Schema.trackCharacterControllerAnimType.Type ) ).As<TrackCharacterControllerAnim>();
			track.Name = "TrackCharacterAnim";
			tracks.Add( track );
		}

		/// <summary>
		/// Gets and sets the camera's animation file</summary>
		public string NodeName
		{
			get { return (string)DomNode.GetAttribute( Schema.groupCharacterControllerType.nodeNameAttribute ); }
			set { DomNode.SetAttribute( Schema.groupCharacterControllerType.nodeNameAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets group blend in duration</summary>
		public float BlendInDuration
		{
			get { return (float)DomNode.GetAttribute( Schema.groupCharacterControllerType.blendInDurationAttribute ); }
			set { DomNode.SetAttribute( Schema.groupCharacterControllerType.blendInDurationAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets group blend out duration</summary>
		public float BlendOutDuration
		{
			get { return (float)DomNode.GetAttribute( Schema.groupCharacterControllerType.blendOutDurationAttribute ); }
			set { DomNode.SetAttribute( Schema.groupCharacterControllerType.blendOutDurationAttribute, value ); }
		}

		/// <summary>
		/// Gets the track skel filename. Hack!</summary>
		public string SkelFilename
		{
			get { return m_skelFilename; }
		}

		public void ReloadSkelInfo()
		{
			// this is ugly hack but there is no other way of supporting legacy ksiezniczka character nodes
			//
			string nodeName = NodeName;
			if ( nodeName == "ksiezniczka:picoCharacterControllerShape1" )
			{
				m_skelFilename = "assets\\ksiezniczkaCharacter\\anim\\princess.skel";
			}
			else if ( nodeName == "QueensChamber:picoKsiezniczkaQueenControllerShape1" )
			{
				m_skelFilename = "assets\\queen\\anim\\queen.skel";
			}
			else if ( nodeName == "monster:picoMonsterControllerShape1" )
			{
				m_skelFilename = "assets\\monster\\anim\\monster.skel";
			}
			else if ( nodeName == "monster:brotherShape" )
			{
				m_skelFilename = "assets\\monster_brother\\anim\\monster_brother.skel";
			}

			else if ( nodeName == "beachPrincess:picoBeachCharacterControllerShape1" )
			{
				m_skelFilename = "assets\\beachPrincessCharacter\\anim\\beachPrincess.skel";
			}

			else
			{
				m_skelFilename = null;
			}
		}

		private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		{
			if ( e.AttributeInfo.Equivalent( Schema.groupCharacterControllerType.nodeNameAttribute ) )
			{
				ReloadSkelInfo();
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
			if ( !parent.Is<Timeline>() )
				return false;

			return true;
		}

		private string m_skelFilename;
	}
}




