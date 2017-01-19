using Sce.Atf;

namespace SettingsEditor
{
    /// <summary>
    /// Filenames for custom embedded image resources. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class Resources
    {
        /// <summary>
        /// Default program icon</summary>
        [ImageResource( "ProgramIcon.ico" )]
        public static readonly string ProgramIcon;

        /// <summary>
        /// Group image resource filename</summary>
        [ImageResource( "group.png" )]
        public static readonly string Group;
        /// <summary>
        /// Preset image resource filename</summary>
        [ImageResource( "preset.png" )]
        public static readonly string Preset;
        /// <summary>
        /// SelectedPreset image resource filename</summary>
        [ImageResource( "SelectedPreset.png" )]
        public static readonly string SelectedPreset;
    }
}
