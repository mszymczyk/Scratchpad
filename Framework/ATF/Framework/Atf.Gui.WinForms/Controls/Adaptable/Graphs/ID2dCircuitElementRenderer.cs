// misz

using System.Drawing;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for circuit element types, which define the appearance, inputs,
    /// and outputs of the element. Consider using the Element and Group classes
    /// in the Sce.Atf.Controls.Adaptable.Graphs namespace.</summary>
    public interface ID2dCircuitElementRenderer
    {
        void DrawInterior( ICircuitElement element, D2dGraphics g, RectangleF bounds );
    }
}
