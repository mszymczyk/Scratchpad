////Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

//using System;
//using System.IO;

//using ZeroMQ;

//namespace SettingsEditor
//{
//	public class ZMQHubService
//	{
//		void ZMQHubService()
//		{
//			// The publisher sends random messages starting with A-J:
//			using (var context = new ZContext())
//			using (var publisher = new ZSocket( context, ZSocketType.PUB ))
//			{
//				publisher.Bind( "tcp://*:6000" );

//				ZError error;

//				while (true)
//				{
//					var bytes = new byte[5];

//					using (var hash = new RNGCryptoServiceProvider()) hash.GetBytes( bytes );

//					if (!publisher.Send( bytes, 0, bytes.Length, ZSocketFlags.None, out error ))
//					{
//						if (error == ZError.ETERM)
//							return;    // Interrupted
//						throw new ZException( error );
//					}

//					Thread.Sleep( 64 );
//				}
//			}
//		}

//		void Connect()
//		{
//		}

//		ZContext m_context;
//		ZSocket m_publisher;
//	}
//}
using System;
using System.ComponentModel.Composition;
using System.IO;

using Sce.Atf;

using ZeroMQ;

namespace SettingsEditor
{
    /// <summary>
    /// Component for communication with picoHub
    /// </summary>
    [Export( typeof( IInitializable ) )]
	[Export( typeof( ZMQHubService ) )]
	[PartCreationPolicy( CreationPolicy.Shared )]
	public class ZMQHubService : IInitializable
	{
		#region IInitializable Members

		/// <summary>
		/// Finishes initializing component by connecting to picoHub</summary>
		void IInitializable.Initialize()
		{
			BlockOutboundTraffic = false;
			BlockInboundTraffic = false;

			// TODO: fix Dispose

			try
			{
				m_context = new ZContext();
				m_publisher = new ZSocket( m_context, ZSocketType.PUB );

				m_publisher.Bind( "tcp://*:6000" );
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine( "Exception while connecting to picoHub (outbound): " + ex.Message );
				if (m_publisher != null)
				{
					m_publisher.Dispose();
					m_publisher = null;
				}
				if (m_context != null)
				{
					m_context.Dispose();
					m_context = null;
				}
			}
		}

		#endregion

		public static bool BlockOutboundTraffic { get; set; }
		public static bool BlockInboundTraffic { get; set; }

		public static void send( ZMQHubMessage msg )
		{
			if (m_publisher == null)
			{
				Outputs.WriteLine( OutputMessageType.Error, "ZMQHubService is not connected to picoHub" );
				return;
			}

			if (BlockOutboundTraffic)
				return;

			try
			{
				byte[] bytes = msg.getFinalByteStream();
				//m_publisher.Send( bytes, 0, bytes.Length );
				//ZError error;
				//if (!m_publisher.Send( bytes, 0, bytes.Length, ZSocketFlags.None, out error ))
				//{
				//	if (error == ZError.ETERM)
				//		return;    // Interrupted
				//	throw new ZException( error );
				//}

				//ZMessage zmessage = new ZMessage();
				//zmessage.
				//m_publisher.Send( )
				ZFrame zframe = new ZFrame( bytes );
				m_publisher.Send( zframe );
			}
			catch (Exception ex)
			{
				Outputs.WriteLine( OutputMessageType.Error, "Exception: send failed! " + ex.Message );
			}
		}

		public static void sendAlways( ZMQHubMessage msg )
		{
			if (m_publisher == null)
			{
				Outputs.WriteLine( OutputMessageType.Error, "ZMQHubService is not connected to picoHub" );
				return;
			}

			try
			{
				byte[] bytes = msg.getFinalByteStream();
				m_publisher.Send( bytes, 0, bytes.Length );
			}
			catch (Exception ex)
			{
				Outputs.WriteLine( OutputMessageType.Error, "Exception: send failed! " + ex.Message );
			}
		}

		// picoHub
		//
		public static string ZMQ_HUB_IP = "localhost";
		public static int ZMQ_HUB_PORT_OUTBOUND = 6666; // for sending data to picoHub
		public static int ZMQ_HUB_PORT_INBOUND = 6667; // for receiving data from picoHub

		//[Import( AllowDefault = true )]
		//private MainForm m_mainForm = null;

		static ZContext m_context;
		static ZSocket m_publisher;
	}

	public class ZMQHubMessage
	{
		public ZMQHubMessage( string msgTag )
		{
			m_memStream = new MemoryStream();
			byte[] fakeMsg = new byte[4];
			m_memStream.Write( fakeMsg, 0, 4 );
			writeBytes( m_memStream, toBytes( msgTag ) );
			appendByte( 0 );
		}

		public void appendString( string str )
		{
			writeBytes( m_memStream, toBytes( str.Length ) );
			writeBytes( m_memStream, toBytes( str ) );
		}

		public void appendInt( int val )
		{
			writeBytes( m_memStream, toBytes( val ) );
		}

		public void appendFloat( float val )
		{
			writeBytes( m_memStream, toBytes( val ) );
		}

		public void appendByte( byte val )
		{
			m_memStream.WriteByte( val );
		}

		public void appendBytes( byte[] bytes )
		{
			writeBytes( m_memStream, bytes );
		}

		public byte[] getFinalByteStream()
		{
			byte[] msgBytes = m_memStream.ToArray();
			byte[] msgSizeBytes = toBytes( msgBytes.Length - 4 );
			System.Buffer.BlockCopy( msgSizeBytes, 0, msgBytes, 0, 4 );
			return msgBytes;
		}

		private byte[] toBytes( string str )
		{
			byte[] bytes = System.Text.Encoding.ASCII.GetBytes( str );
			return bytes;
		}

		private byte[] toBytes( int ival )
		{
			byte[] bytes = BitConverter.GetBytes( ival );
			return bytes;
		}

		private byte[] toBytes( float fval )
		{
			byte[] bytes = BitConverter.GetBytes( fval );
			return bytes;
		}

		private void writeBytes( MemoryStream ms, byte[] bytes )
		{
			ms.Write( bytes, 0, bytes.Length );
		}

		MemoryStream m_memStream;
	}
}

