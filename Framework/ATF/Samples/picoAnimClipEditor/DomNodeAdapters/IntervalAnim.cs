//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

using pico.Timeline;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoAnimClipEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to an Interval</summary>
	public class IntervalAnim : Interval, ITimelineValidationCallback
    {
		public override bool CanParentTo( DomNode parent )
		{
			return true;
		}

		public override bool Validate( DomNode parent )
		{
			return true;
		}
    }
}





