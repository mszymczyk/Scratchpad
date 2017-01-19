//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

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
    public class ProgramBase
    {
        ///// <summary>
        ///// The main entry point for the application.</summary>
        //[STAThread]
        protected void StartUpBase( string[] args )
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Set up localization support early on, so that user-readable strings will be localized
            //  during the initialization phase below. Use XML files that are embedded resources.
            //Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
            //Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CurrentCulture;

            Localizer.SetStringLocalizer(new EmbeddedResourceStringLocalizer());

            // Register the embedded image resources so that they will be available for all users of ResourceUtil,
            //
            ResourceUtil.Register( typeof( Resources ) );

            // Enable metadata driven property editing for the DOM
            DomNodeType.BaseOfAllTypes.AddAdapterCreator(new AdapterCreator<CustomTypeDescriptorNodeAdapter>());

            // Create a type catalog with the types of components we want in the application
            var catalog = new TypeCatalog(

                typeof(CommandService),                 // handles commands in menus and toolbars
                typeof(ControlHostService),             // docking control host
                typeof(AtfUsageLogger),                 // logs computer info to an ATF server
                typeof(CrashLogger),                    // logs unhandled exceptions to an ATF server
                typeof(UnhandledExceptionService),      // catches unhandled exceptions, displays info, and gives user a chance to save
                typeof(FileDialogService),
                typeof(SkinService),
                typeof(StandardFileExitCommand),        // standard File exit menu command
                typeof(StandardEditHistoryCommands),    // tracks document changes and updates main form title
                typeof(HelpAboutCommand),               // Help -> About command
                typeof(ContextRegistry),                // central context registry with change notification
                typeof(SettingsPropertyEditor),                 // property grid for editing selected objects
                typeof(GridPropertyEditor),             // grid control for editing selected objects
                typeof(PropertyEditingCommands),        // commands for PropertyEditor and GridPropertyEditor
                typeof(SettingsService),
                typeof(PythonService),                  // scripting service for automated tests
                typeof(ScriptConsole),                  // provides a dockable command console for entering Python commands
                typeof(AtfScriptVariables),             // exposes common ATF services as script variables
                typeof(AutomationService),              // provides facilities to run an automated script using the .NET remoting service

                typeof( StandardEditCommands ),           // standard Edit menu commands for copy/paste

                typeof( DocumentRegistry ),               // central document registry with change notification
                typeof( AutoDocumentService ),            // opens documents from last session, or creates a new document, on startup
                typeof( RecentDocumentCommands ),         // standard recent document commands in File menu
                typeof( StandardFileCommands ),           // standard File menu commands for New, Open, Save, SaveAs, Close
                typeof( MainWindowTitleService ),         // tracks document changes and updates main form title
                typeof( TabbedControlSelector ),          // enable ctrl-tab selection of documents and controls within the app
                typeof( Outputs ),                        // passes messages to all IOutputWriter components
                typeof( OutputService ),                  // rich text box for displaying error and warning messages. Implements IOutputWriter
                typeof( DefaultTabCommands ),             // provides the default commands related to document tab Controls

                typeof( HistoryLister ),                  // visual list of undo/redo stack
                typeof( FileWatcherService ),                // service to watch for changes to files
                typeof( CurveEditor ),

                //typeof( misz.HubService ),

                typeof( SchemaLoader ),                   // component that loads XML schema and sets up types
                typeof(Editor),                          // component that manages UI documents
                typeof(Commands),
                typeof(SettingsLister),

                typeof(D3DManipulator)
                );

            container = new CompositionContainer(catalog);

            // Configure the main Form

            // extract program icon from dll resource
            //System.Reflection.Assembly thisExe = System.Reflection.Assembly.GetExecutingAssembly();
            //System.IO.Stream file = thisExe.GetManifestResourceStream( "SettingsEditor.Resources.ProgramIcon.ico" );
            //Icon icon = new Icon( file );

            // Configure the main Form with a ToolStripContainer so the CommandService can
            //  generate toolbars.
            var toolStripContainer = new ToolStripContainer();
            toolStripContainer.Dock = DockStyle.Fill;
            mainForm = new MainForm(toolStripContainer)
            {
                Text = "Settings Editor".Localize(),
                Icon = GdiUtil.CreateIcon( ResourceUtil.GetImage( Resources.ProgramIcon ) )
            };

            // Add the main Form instance to the container
            var batch = new CompositionBatch();
            batch.AddPart(mainForm);
            // batch.AddPart(new WebHelpCommands("https://github.com/SonyWWS/ATF/wiki/ATF-DOM-Tree-Editor-Sample".Localize()));
            container.Compose(batch);

            // this will change available file commands
            // must be called prior to initialization
            //
            StandardFileCommands standardFileCommands = container.GetExportedValue<StandardFileCommands>();
            standardFileCommands.RegisterCommands = 
                  StandardFileCommands.CommandRegister.FileNew
                | StandardFileCommands.CommandRegister.FileOpen
                //| FileSave
                | StandardFileCommands.CommandRegister.FileSaveAs
                //| FileSaveAll
                | StandardFileCommands.CommandRegister.FileClose
                ;

            // Initialize components that require it. Initialization often can't be done in the constructor,
            //  or even after imports have been satisfied by MEF, since we allow circular dependencies between
            //  components, via the System.Lazy class. IInitializable allows components to defer some operations
            //  until all MEF composition has been completed.
            container.InitializeAll();

            CurveEditor curveEditor = container.GetExportedValue<CurveEditor>();
            curveEditor.Control.CurvesChanged += ( sender, e ) => curveEditor.Control.FitAll();
        }

        protected void Run()
        {
            Application.Run( mainForm );
        }

        protected void ShutDownBase()
        {
            // Give components a chance to clean up.
            container.Dispose();
        }

        // Set up the MEF container with these components
        protected CompositionContainer container;
        protected MainForm mainForm;
    }
}
