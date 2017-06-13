//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;

namespace SettingsEditor
{
    /// <summary>
    /// Force control points to be within curve limits</summary>
    public class PresetUniqueIdValidator : CategoryUniqueIdValidator
    {
        /// <summary>
        /// Get the id category of the given node</summary>
        /// <param name="node">Node</param>
        /// <returns>Id category</returns>
        protected override object GetIdCategory( DomNode node )
        {
            Preset preset = node.As<Preset>();
            if ( preset != null )
                //return preset.Group.DomNode;
                return preset.GroupName;

            return null;
        }
    }
}
