﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FxCompiler
{
    /// <summary>
    /// Used for checking if required pico services are running and launches missing ones.
	/// </summary>
    public static class FxCompilerInterop
    {
        public const string FxCompilerDll = "FxCompilerDll.dll";

		[UnmanagedFunctionPointer( CallingConvention.StdCall, CharSet = CharSet.Ansi )]
		public delegate void LogCallbackType( int messageType, string text );
		private static LogCallbackType s_logInstance;

		[DllImport( "kernel32", CharSet = CharSet.Auto, SetLastError = true )]
		public static extern IntPtr LoadLibrary( string fileName );

		//[ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
		[DllImport( "kernel32", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		private static extern bool FreeLibrary( IntPtr hModule );

		[DllImportAttribute( FxCompilerDll, EntryPoint = "FxCompilerDll_Initialize", CallingConvention = CallingConvention.StdCall )]
		private static extern int NativeStartUp( LogCallbackType logCallback );

		[DllImportAttribute( FxCompilerDll, EntryPoint = "FxCompilerDll_Shutdown", CallingConvention = CallingConvention.StdCall )]
		private static extern int NativeShutDown();

        [DllImportAttribute( FxCompilerDll, EntryPoint = "FxCompilerDll_CompileFile", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi )]
        public static extern void CompileFile( string fileFullPath, string outDir, string intDir );


        public static bool StartUp( LogCallbackType logCallback )
		{
            s_libHandle = LoadLibrary( FxCompilerDll );
            if ( s_libHandle == IntPtr.Zero )
                return false;

            s_logInstance = logCallback;

            NativeStartUp( s_logInstance );

            return true;
		}

		public static void ShutDown()
		{
			if ( s_libHandle != IntPtr.Zero )
			{
                NativeShutDown();
                FreeLibrary( s_libHandle );
			}
		}

		private static IntPtr s_libHandle;
		private static Dictionary<string, string[]> m_bankCache = new Dictionary<string, string[]>();
	}
}
