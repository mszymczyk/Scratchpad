//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;

namespace picoAnimClipEditor.DomNodeAdapters
{
	/// <summary>
	/// Interface used for validation of anim clip editor's elements
	/// </summary>
	public interface AnimClipElementValidationInterface
	{
		bool CanParentTo( DomNode parent );
		bool Validate( DomNode parent );
	}
}



