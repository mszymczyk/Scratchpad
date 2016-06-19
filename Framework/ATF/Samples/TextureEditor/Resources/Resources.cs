//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace TextureEditor
{
    /// <summary>
    /// Filenames for standard game icons. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class Resources
    {      
        /// <summary>
        /// Source icon name</summary>
		[ImageResource( "Source16.png", "Source24.png", "Source32.png" )]
        public static readonly string SourceImage;

		/// <summary>
		/// Exported icon name</summary>
		[ImageResource( "Exported16.png", "Exported24.png", "Exported32.png" )]
		public static readonly string ExportedImage;

		/// <summary>
		/// Difference icon name</summary>
		[ImageResource( "Difference16.png", "Difference24.png", "Difference32.png" )]
		public static readonly string DifferenceImage;

		/// <summary>
		/// Show red channel icon</summary>
		[ImageResource( "ShowRed.png" )]
		public static readonly string ShowRedChannelImage;

		/// <summary>
		/// Show green channel icon</summary>
		[ImageResource( "ShowGreen.png" )]
		public static readonly string ShowGreenChannelImage;

		/// <summary>
		/// Show blue channel icon</summary>
		[ImageResource( "ShowBlue.png" )]
		public static readonly string ShowBlueChannelImage;

		/// <summary>
		/// Show alpha channel icon</summary>
		[ImageResource( "ShowAlpha.png" )]
		public static readonly string ShowAlphaChannelImage;

		/// <summary>
		/// Default program icon</summary>
		[ImageResource( "ProgramIcon.ico" )]
		public static readonly string ProgramIcon;

        private const string ResourcePath = "TextureEditor.Resources.";

        static Resources()
        {
            ResourceUtil.Register(typeof(Resources), ResourcePath);
        }
    }
}
