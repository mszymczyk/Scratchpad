using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace pico
{
    /// <summary>
    /// Used for checking if required pico services are running and launches missing ones.
	/// </summary>
    public interface ILogger
    {
		void LogInfo( string str );
		void LogWarning( string str );
		void LogError( string str );
	}
}
