//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace pico.LogOutput
{
    /// <summary>
    /// Filenames for standard game icons. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class Resources
    {      
		/// <summary>
		/// Default program icon</summary>
		[ImageResource( "ProgramIcon.ico" )]
		public static readonly string ProgramIcon;

		private const string ResourcePath = "pico.LogOutput.Resources.";

        static Resources()
        {
            ResourceUtil.Register(typeof(Resources), ResourcePath);
        }
    }
}
