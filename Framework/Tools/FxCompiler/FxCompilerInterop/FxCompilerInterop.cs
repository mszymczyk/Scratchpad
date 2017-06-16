using System;
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
        public static extern int CompileFile( string filePathWithinDataRoot );

        [DllImportAttribute( FxCompilerDll, EntryPoint = "FxCompilerDll_CompileFiles", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi )]
        public static extern int CompileFiles( string[] filePathsWithinDataRoot, int nFilePaths, LogCallbackType logCallback );

        public static bool StartUp( LogCallbackType logCallback )
        {
            string dllPath = System.Environment.GetEnvironmentVariable("SCRATCHPAD_DIR");
            if (dllPath == null)
                dllPath = FxCompilerDll;
            else
                dllPath = dllPath + "\\Framework\\Bin\\" + FxCompilerDll;

            s_libHandle = LoadLibrary( dllPath );
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
    }
}
