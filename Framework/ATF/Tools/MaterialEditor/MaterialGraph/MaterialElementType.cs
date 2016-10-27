//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Controls.Adaptable.Graphs;

namespace CircuitEditorSample
{
    /// <summary>
    /// Simple implementation of ICircuitElementType</summary>
    public class MaterialElementType : ElementType
    {
        /// <summary>
        /// Constructor specifying circuit element type's attributes</summary>
        /// <param name="name">Element type's name</param>
        /// <param name="isConnector">Whether the element type is a connector</param>
        /// <param name="size">Element type's size</param>
        /// <param name="image">Element type's image</param>
        /// <param name="inputPins">Array of input pins</param>
        /// <param name="outputPins">Array of output pins</param>
        public MaterialElementType(
            string name,
            bool isConnector,
            Size size,
            Image image,
            ICircuitPin[] inputPins,
            ICircuitPin[] outputPins)
            : base( name, isConnector, image, inputPins, outputPins )
        {
            
        }

        /// <summary>
        /// Gets desired size of element type's interior, in pixels</summary>
        public Size InteriorSize
        {
            get { return (m_image != null) ? new Size(32, 32) : new Size(); }
        }

        //private Size m_size;
    }
}
