//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;

using pico.Timeline;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoAnimClipEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a KeyTag</summary>
	public class KeyTag : Key
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




