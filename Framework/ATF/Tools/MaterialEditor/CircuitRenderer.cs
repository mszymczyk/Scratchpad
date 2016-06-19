//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Direct2D;
using Sce.Atf.Adaptation;

namespace CircuitEditorSample
{
    /// <summary>
    /// A custom circuit renderer, demonstrating how to use a particular theme (D2dDiagramTheme)
    /// for particular types of circuit elements.</summary>
    public class CircuitRenderer : D2dCircuitRenderer<Module, Connection, ICircuitPin>
    {
        /// <summary>
        /// Initializes a new instance of this class</summary>
        /// <param name="theme">Diagram theme for rendering graph</param>
        /// <param name="documentRegistry">An optional document registry, used to clear the internal
        /// element type cache when a document is removed</param>
        public CircuitRenderer(D2dDiagramTheme theme, IDocumentRegistry documentRegistry) :
            base(theme, documentRegistry)
        {
            m_disabledTheme = new D2dDiagramTheme();
            m_disabledTheme.FillBrush = D2dFactory.CreateSolidBrush(SystemColors.ControlDark);
            m_disabledTheme.TextBrush = D2dFactory.CreateSolidBrush(SystemColors.InactiveCaption);
            D2dGradientStop[] gradstops = 
                { 
                    new D2dGradientStop(Color.DarkGray, 0),
                    new D2dGradientStop(Color.DimGray, 1.0f),
                };
            m_disabledTheme.FillGradientBrush = D2dFactory.CreateLinearGradientBrush(gradstops);

            // Set the pin colors
            m_disabledTheme.RegisterCustomBrush("boolean", D2dFactory.CreateSolidBrush(Color.LightGray));
        }

        /// <summary>
        /// Draws a graph node</summary>
        /// <param name="element">Element to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">Graphics object</param>
        public override void Draw(Module element, DiagramDrawingStyle style, D2dGraphics g)
        {
            // Use the "disabled" theme when drawing disabled circuit elements.
            if (!((ModuleElementInfo)element.ElementInfo).Enabled)
            {
                D2dDiagramTheme defaultTheme = Theme;
                Theme = m_disabledTheme;
                base.Draw(element, style, g);
                Theme = defaultTheme;
                return;
            }

            base.Draw(element, style, g);
        }

        /// <summary>
        /// Draws a graph edge</summary>
        /// <param name="edge">Edge to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">Graphics object</param>
        public override void Draw( Connection edge, DiagramDrawingStyle style, D2dGraphics g )
        {
            // this func is copied directly from void Draw(TWire edge, DiagramDrawingStyle style, D2dGraphics g)
            // the only difference is that pen is taken from outputPin (source) instead from inputPin (destination)
            // unfortunately I had to make DrawEdgeUsingStyleInfo and DrawWire methods public
            // probably better idea would be to implement (or copy implementation) here
            //
            ICircuitPin inputPin = edge.InputPin;
            ICircuitPin outputPin = edge.OutputPin;
            if ( RectangleSelectsWires && style == DiagramDrawingStyle.LastSelected ) // last selected is not well defined in multi-edge selection   
                style = DiagramDrawingStyle.Selected;

            D2dBrush pen = (style == DiagramDrawingStyle.Normal) ? GetPen( outputPin ) : Theme.GetOutLineBrush( style );
            if ( edge.Is<IEdgeStyleProvider>() )
                DrawEdgeUsingStyleInfo( edge.Cast<IEdgeStyleProvider>(), pen, g );
            else
                DrawWire( edge.OutputElement.As<Module>(), outputPin, edge.InputElement.As<Module>(), inputPin, g, pen );
        }

        /// <summary>
        /// Get pen for drawing pin</summary>
        /// <param name="pin">Pin to draw</param>
        /// <returns>Pen for drawing pin</returns>
        protected override D2dBrush GetPen( ICircuitPin pin )
        {
            // overridden this to select color based on pin channel
            //
            D2dBrush pen = null;
            MaterialGraphPin p = pin as MaterialGraphPin;

            if ( p != null )
                pen = Theme.GetCustomBrush( p.Component );

            if ( pen == null )
                pen = Theme.GetCustomBrush( pin.TypeName );
            if ( pen == null )
                pen = Theme.GhostBrush;

            return pen;
        }

        private readonly D2dDiagramTheme m_disabledTheme;
    }
}
