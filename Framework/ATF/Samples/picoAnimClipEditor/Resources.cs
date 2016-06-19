//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace picoAnimClipEditor
{
    /// <summary>
    /// Filenames for custom embedded image resources. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class Resources
    {
        /// <summary>
        /// Group image resource resource filename</summary>
        [ImageResource("group.png")]
        public static readonly string GroupImage;

        /// <summary>
        /// Interval image resource resource filename</summary>
        [ImageResource("interval.png")]
        public static readonly string IntervalImage;

        /// <summary>
        /// Key image resource resource filename</summary>
        [ImageResource("key.png")]
        public static readonly string KeyImage;

        /// <summary>
        /// Marker image resource resource filename</summary>
        [ImageResource("marker.png")]
        public static readonly string MarkerImage;

        /// <summary>
        /// Track image resource resource filename</summary>
        [ImageResource("track.png")]
        public static readonly string TrackImage;

		/// <summary>
		/// LuaScript image resource resource filename</summary>
		[ImageResource( "luaScript.png" )]
		public static readonly string LuaScriptImage;

		/// <summary>
		/// Default program icon</summary>
		[ImageResource( "ProgramIcon.ico" )]
		public static readonly string ProgramIcon;

		/// <summary>
		/// Animdata Ok image resource filename</summary>
		[ImageResource( "animdataOk.png" )]
		public static readonly string AnimdataOk;

		/// <summary>
		/// Missing animdata image resource filename</summary>
		[ImageResource( "animdataMissing.png" )]
		public static readonly string AnimdataMissing;

		/// <summary>
		/// Outdated animdata image resource filename</summary>
		[ImageResource( "animdataOutdated.png" )]
		public static readonly string AnimdataOutdated;
	}
}
