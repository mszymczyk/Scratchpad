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
    /// Adapts DomNode to a TrackFader</summary>
	public class TrackFader : Track, ITimelineValidationCallback
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
			if ( parent.Type != Schema.groupType.Type )
				return false;

			// count all fader tracks among all groups
			// only one fader track is allowed GLOBALLY
			//
			int nTracks = 0;
			DomNode rootNode = parent.GetRoot();
			foreach( DomNode sub in rootNode.Subtree )
			{
				if ( sub.Is<TrackFader>() )
				{
					++nTracks;

					if ( nTracks >= (1 + validating) )
						return false;
				}
			}

			return true;
		}
	}
}



