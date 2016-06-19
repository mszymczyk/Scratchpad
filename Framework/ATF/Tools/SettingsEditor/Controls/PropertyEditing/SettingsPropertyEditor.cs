//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

using PropertyGrid = Sce.Atf.Controls.PropertyEditing.PropertyGrid;

namespace SettingsEditor
{
    [Export( typeof( IInitializable ) )]
    [Export( typeof( IControlHostClient ) )]
    [Export( typeof( SettingsPropertyEditor ) )]
    [PartCreationPolicy( CreationPolicy.Any )]
    // Demonstrates using tooltips instead of an embedded Control to display property descriptions.
    public class SettingsPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="commandService">ICommandService</param>
        /// <param name="controlHostService">IControlHostService</param>
        /// <param name="contextRegistry">IContextRegistry</param>
        [ImportingConstructor]
        public SettingsPropertyEditor(
            ICommandService commandService,
            IControlHostService controlHostService,
            IContextRegistry contextRegistry )
            : base( commandService, controlHostService, contextRegistry )
        {
        }

        protected override void Configure( out PropertyGrid propertyGrid, out ControlInfo controlInfo )
        {
            // Test that DisplayTooltips works instead of the usual DisplayDescriptions.
            propertyGrid = new PropertyGrid( PropertyGridMode.DisplayTooltips | PropertyGridMode.DisplayDescriptions | PropertyGridMode.PropertySorting | PropertyGridMode.AlwaysShowVerticalScrollBar, new PropertyGridView() );
            propertyGrid.PropertySorting = PropertySorting.Categorized;
            controlInfo = new ControlInfo(
                "Property Editor".Localize(),
                "Edits selected object properties".Localize(),
                StandardControlGroup.Right, null,
                "https://github.com/SonyWWS/ATF/wiki/Property-Editing-in-ATF".Localize() );
        }
    }
}
