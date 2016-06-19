//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace picoTimelineEditor
{
    /// <summary>
    /// Filenames for standard game icons. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class Resources
    {      
		/// <summary>
		/// Show alpha channel icon</summary>
		[ImageResource( "ProgramIcon.png" )]
		public static readonly string ProgramIcon;

		private const string ResourcePath = "picoTimelineEditor.Resources.";

        static Resources()
        {
            ResourceUtil.Register(typeof(Resources), ResourcePath);
        }
    }
}
