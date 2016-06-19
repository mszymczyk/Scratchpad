//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using Sce.Atf.Controls.Timelines;

namespace pico.Timeline
{
	/// <summary>
	/// Scrubber manipulator: a vertical bar that can slide left and right by grabbing the handle on
	/// the top
	/// pico version just makes sure the position is >= 0
	/// </summary>
	public class picoD2dScrubberManipulator : Sce.Atf.Controls.Timelines.Direct2D.D2dScrubberManipulator
    {
        /// <summary>
        /// Constructor that permanently attaches to the given timeline control by subscribing to its
        /// events</summary>
        /// <param name="owner">The timeline control whose events we permanently listen to</param>
		public picoD2dScrubberManipulator( Sce.Atf.Controls.Timelines.Direct2D.D2dTimelineControl owner )
			: base( owner )
        {
        }

		/// <summary>
		/// Validates and corrects the proposed new position of the scrubber manipulator</summary>
		/// <param name="position">The proposed new value of the Position property</param>
		/// <returns>The corrected value that the Position property becomes</returns>
		protected override float ValidatePosition( float position )
		{
			float newPosition = Owner.ConstrainFrameOffset( position );
			float newPositionClamped = Math.Max( newPosition, 0.0f );
			return newPositionClamped;
		}
	}
}
