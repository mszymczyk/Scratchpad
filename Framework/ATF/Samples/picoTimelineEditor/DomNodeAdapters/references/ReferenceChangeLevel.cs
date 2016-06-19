//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;

using pico.Timeline;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Key</summary>
	public class ReferenceChangeLevel : TimelineReference, ITimelineValidationCallback
    {
		/// <summary>
		/// Gets and sets the sound bank</summary>
		public string LevelName
		{
			get { return (string)DomNode.GetAttribute( Schema.refChangeLevelType.levelNameAttribute); }
			set { DomNode.SetAttribute( Schema.refChangeLevelType.levelNameAttribute, value ); }
		}

		///// <summary>
		///// Gets and sets the sound name</summary>
		//public string CutsceneFile
		//{
		//	//get { return (string)DomNode.GetAttribute( Schema.keyChangeLevelType.cutsceneFileAttribute ); }
		//	//set { DomNode.SetAttribute( Schema.keyChangeLevelType.cutsceneFileAttribute, value ); }
		//	get { return (string)DomNode.GetAttribute( Schema.timelineRefType.filenameAttribute); }
		//	set { DomNode.SetAttribute( Schema.timelineRefType.filenameAttribute, value ); }
		//}

		/// <summary>
		/// Gets and sets the unloadCurrentlevel</summary>
		public bool UnloadCurrentLevel
		{
			get { return (bool)DomNode.GetAttribute( Schema.refChangeLevelType.unloadCurrentlevelAttribute ); }
			set { DomNode.SetAttribute( Schema.refChangeLevelType.unloadCurrentlevelAttribute, value ); }
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

			//if ( picoTimelineDomValidator.ParentHasChildOfType<GroupCamera>( parent ) )
			//	return false;

			//IList<DomNode> childList = parent.GetChildList( Schema.timelineType.groupChild );
			//int childCount = childList.Count;
			int childCount = picoTimelineDomValidator.CountChildrenOfType<ReferenceChangeLevel>( parent );
			if ( childCount >= ( 1 + validating ) )
				return false;

			return true;
		}
	}
}




