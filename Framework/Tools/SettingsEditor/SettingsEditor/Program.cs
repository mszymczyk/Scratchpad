//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using System.Windows.Forms;


using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;

using HelpAboutCommand = Sce.Atf.Applications.HelpAboutCommand;

namespace SettingsEditor
{
    /// <summary>
    /// The purpose of this sample is to:
    /// 1- Show how to decorate a DomNode with property descriptors so it can be edited 
    ///    using the PropertyEditor component.
    /// 2- List most of the available property descriptors and type-editors.
    /// The most important file in this sample is SchemaLoader.
    /// SchemaLoader decorates the DomNodeTypes with descriptors and editors depending on 
    /// property type.
    /// For more information, see https://github.com/SonyWWS/ATF/wiki/ATF-DOM-Property-Editor-Sample. </summary>
    class Program : ProgramBase
    {
        /// <summary>
        /// The main entry point for the application.</summary>
        [STAThread]
        static void Main( string[] args )
        {
            Program prog = new Program();

            // setup paths and hub

            prog.StartUpBase(args);

            prog.Run();

            prog.ShutDownBase();
        }
    }
}
