using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace pico
{
    /// <summary>
    /// Used for checking if required pico services are running and launches missing ones.
	/// </summary>
    public interface ITextureExporter
    {
		ILogger Logger { get; set; }

		bool IsExportReqiured( string inputFile, DateTime inputTimestamp );
		int Export( string inputFile );
		void Delete( string inputFile );
	}
}
