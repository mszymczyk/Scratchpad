using Sce.Atf;
using Sce.Atf.Adaptation;

namespace misz.StatsViewer
{
    /// <summary>
    /// Interface for editable session documents</summary>
    public interface ISessionDocument : IDocument, IObservableContext, IAdaptable
    {
        /// <summary>
        /// Gets the session</summary>
        ISession Session { get; }
    }
}
