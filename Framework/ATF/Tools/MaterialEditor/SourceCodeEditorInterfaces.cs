namespace CircuitEditorSample
{
    /// <summary>
    /// Source code interface</summary>
    public interface ISourceCode
    {
        /// <summary>
        /// Gets or sets source code name</summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets display name or null</summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets or sets source code</summary>
        string Text { get; set; }
    }
}
