using System;
using System.Diagnostics;
using System.Linq;

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
            string SCRATCHPAD_DIR = System.Environment.GetEnvironmentVariable( "SCRATCHPAD_DIR" );
            if ( string.IsNullOrEmpty( SCRATCHPAD_DIR ) )
                throw new InvalidOperationException( "Couldn't read 'SCRATCHPAD_DIR' environment variable" );
//            string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
//            string exeDir = System.IO.Path.GetDirectoryName( exePath );
//#if DEBUG
//            string SCRATCHPAD_DIR = System.IO.Path.GetFullPath( exeDir + "..\\..\\..\\..\\..\\" );
//#else
//            string SCRATCHPAD_DIR = System.IO.Path.GetFullPath( exeDir + "..\\..\\..\\" );
//#endif

            LaunchHubIfNotRunning( SCRATCHPAD_DIR );

            // setup paths and hub
            misz.HubService.SetImpl(new misz.ZMQHubService());
            misz.Paths.SetupPaths( SCRATCHPAD_DIR, SCRATCHPAD_DIR );

            Program prog = new Program();

            prog.StartUpBase(args);

            prog.Run();

            prog.ShutDownBase();
        }

        public static void LaunchHubIfNotRunning( string SCRATCHPAD_DIR )
        {
            Process[] localAll = Process.GetProcesses();
            var HubList = localAll.Where( p => p.ProcessName == "ZMQHub.exe" ).ToList();
            if ( HubList.Count == 0 )
            {
                // launch ZmqHub.exe
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = SCRATCHPAD_DIR + "Framework\\Bin\\ZMQHub.exe";
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit( 1000 );
            }

        }
    }
}
