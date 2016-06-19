using Sce.Atf.Dom;

namespace pico.Timeline
{
	/// <summary>
	/// Interface used for validation of timeline elements
	/// </summary>
	public interface ITimelineValidationCallback
	{
		bool CanParentTo( DomNode parent );
		bool Validate( DomNode parent );
	}
}
