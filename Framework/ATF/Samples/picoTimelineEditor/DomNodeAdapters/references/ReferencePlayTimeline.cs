//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;

using pico.Timeline;

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a ReferencePlayTimeline
	/// </summary>
	public class ReferencePlayTimeline : TimelineReference, ITimelineValidationCallback
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
			if ( !parent.Is<Timeline>() )
				return false;

			return true;
		}
	}
}




