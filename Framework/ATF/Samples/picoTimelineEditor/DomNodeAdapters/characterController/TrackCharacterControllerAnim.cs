//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

using pico.Timeline;

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Track</summary>
	public class TrackCharacterControllerAnim : Track, ITimelineValidationCallback
    {
        #region ITrack Members

        /// <summary>
        /// Gets or sets the track name</summary>
        public override string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.trackCharacterControllerAnimType.nameAttribute); }
			set { DomNode.SetAttribute( Schema.trackCharacterControllerAnimType.nameAttribute, value ); }
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
			if ( !parent.Is<GroupCharacterController>() )
				return false;

			IList<DomNode> childList = parent.GetChildList( Schema.groupCharacterControllerType.trackChild );
			if ( childList.Count >= ( 1 + validating ) )
				return false;

			return true;
		}
	}
}



