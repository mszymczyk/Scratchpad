using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Interface representing nodes/modules of a material graph.
    ///</summary>
    public interface IMaterialGraphModule
    {
        bool DoesRequireRecompile( AttributeInfo attr );
        bool DoesRequireRefresh( AttributeInfo attr );
    }
}
