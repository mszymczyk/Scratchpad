using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace pico
{
    /// <summary>
    /// Used for checking if required pico services are running and launches missing ones.
	/// </summary>
    public static class ServicesLauncher
    {
		public static void LaunchServices()
		{
			string PICO_ROOT = Environment.GetEnvironmentVariable( "PICO_ROOT" );

			Process[] localAll = Process.GetProcesses();
			var picoHubList = localAll.Where( p => p.ProcessName.Contains( "picoHub" ) ).ToList();
			if ( picoHubList.Count == 0 )
			{
				// launch picoHub.exe
				//
				StartAndWaitSome( Paths.picoHub_exe );
			}

			var picoLogOutputList = localAll.Where( p => p.ProcessName.Contains( "picoLogOutput" ) ).ToList();
			if ( picoLogOutputList.Count == 0 )
			{
				// launch picoLogOutput.exe
				//
				StartAndWaitSome( Paths.picoLogOutput_exe );
			}
		}

		private static void StartAndWaitSome( string fileName )
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.FileName = fileName;
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit( 1000 );
		}
	}
}
