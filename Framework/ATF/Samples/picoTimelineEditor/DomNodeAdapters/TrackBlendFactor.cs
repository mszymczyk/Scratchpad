using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

using pico.Timeline;

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a TrackBlendFactor</summary>
	public class TrackBlendFactor : Track, ITimelineValidationCallback
    {
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
			if ( parent.As<Group>() == null && parent.As<Track>() == null )
				return false;

			// count all tracks among given group
			// only one blend factor track is allowed per group
			//
			IGroup group = parent.As<IGroup>();
			if ( group == null )
			{
				ITrack track = parent.As<ITrack>();
				group = track.Group;
			}

			if ( group != null )
			{
				int nTracks = 0;
				foreach ( ITrack track in group.Tracks )
				{
					if ( track.As<TrackBlendFactor>() != null )
					{
						++nTracks;
						if ( nTracks >= (1 + validating) )
							return false;
					}
				}
			}

			return true;
		}
	}
}



