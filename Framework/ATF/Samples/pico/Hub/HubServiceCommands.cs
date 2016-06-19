using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

namespace pico.Hub
{
	/// <summary>
	/// Component for sending common commands to pico via HubService
	/// </summary>
	[Export( typeof( IInitializable ) )]
	[Export( typeof( HubServiceCommands ) )]
	[PartCreationPolicy( CreationPolicy.Shared )]
	public class HubServiceCommands : IInitializable
	{
		#region IInitializable Members

		/// <summary>
		/// Finishes initializing component by connecting to picoHub</summary>
		void IInitializable.Initialize()
		{
		}

		#endregion

		public void ReloadResource( string picoDemoPath )
		{
			HubMessage hubMsg = new HubMessage( RESMGRV2_TAG );
			hubMsg.appendString( "reloadResource" );
			hubMsg.appendString( picoDemoPath );
			m_hubService.sendAlways( hubMsg );
		}

		[Import( AllowDefault = true )]
		private HubService m_hubService = null;

		public static readonly string RESMGRV2_TAG = "resMgrV2";
	}
}
