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
	/// Component for communication with picoHub
	/// </summary>
	[Export( typeof( IInitializable ) )]
	[Export( typeof( HubService ) )]
	[PartCreationPolicy( CreationPolicy.Shared )]
	public class HubService : IInitializable
    {
		#region IInitializable Members

		/// <summary>
		/// Finishes initializing component by connecting to picoHub</summary>
		void IInitializable.Initialize()
		{
			BlockOutboundTraffic = false;
			BlockInboundTraffic = false;

			try
			{
				IPHostEntry ipHostInfo = Dns.GetHostEntry( PICO_HUB_IP );
				IPAddress serverAddr = null;
				foreach ( var addr in Dns.GetHostEntry( string.Empty ).AddressList )
				{
					if ( addr.AddressFamily == AddressFamily.InterNetwork )
						serverAddr = addr;
				}
				var clientEndPoint = new IPEndPoint( serverAddr, PICO_HUB_PORT_OUTBOUND );

				// Create a client socket and connect it to the endpoint 
				m_picoHubClientSocketOutbound = new System.Net.Sockets.Socket( System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp );
				m_picoHubClientSocketOutbound.NoDelay = true;
				m_picoHubClientSocketOutbound.Connect( clientEndPoint );
			}
			catch( SocketException sex )
			{
				System.Diagnostics.Debug.WriteLine( "SocketException while connecting to picoHub (outbound): " + sex.Message );
				m_picoHubClientSocketOutbound.Dispose();
				m_picoHubClientSocketOutbound = null;
			}
			catch( Exception ex )
			{
				System.Diagnostics.Debug.WriteLine( "Exception while connecting to picoHub (outbound): " + ex.Message );
				m_picoHubClientSocketOutbound.Dispose();
				m_picoHubClientSocketOutbound = null;
			}

			m_inputThread = new InputThread( this );

			Instance = this;
		}

		#endregion

		public static HubService Instance { get; set; }

		public bool BlockOutboundTraffic { get; set; }
		public bool BlockInboundTraffic { get; set; }

		public void send( HubMessage msg )
		{
			//if ( ! CanSendData )
			//	return;
			if ( m_picoHubClientSocketOutbound == null )
			{
				Outputs.WriteLine( OutputMessageType.Error, "HubService is not connected to picoHub" );
				return;
			}

			if ( BlockOutboundTraffic )
				return;

			try
			{
				byte[] bytes = msg.getFinalByteStream();

				m_picoHubClientSocketOutbound.Send( bytes );
			}
			catch ( SocketException sex )
			{
				Outputs.WriteLine( OutputMessageType.Error, "SocketException: send failed! " + sex.Message );
			}
			catch ( Exception ex )
			{
				Outputs.WriteLine( OutputMessageType.Error, "Exception: send failed! " + ex.Message );
			}
		}

		public void sendAlways( HubMessage msg )
		{
			if ( m_picoHubClientSocketOutbound == null )
			{
				Outputs.WriteLine( OutputMessageType.Error, "HubService is not connected to picoHub" );
				return;
			}

			try
			{
				byte[] bytes = msg.getFinalByteStream();
				m_picoHubClientSocketOutbound.Send( bytes );
			}
			catch ( SocketException sex )
			{
				Outputs.WriteLine( OutputMessageType.Error, "SocketException: send failed! " + sex.Message );
			}
			catch ( Exception ex )
			{
				Outputs.WriteLine( OutputMessageType.Error, "Exception: send failed! " + ex.Message );
			}
		}

		public static void sendS( HubMessage msg )
		{
			if (Instance != null)
				Instance.send( msg );
		}

		public static void sendAlwaysS( HubMessage msg )
		{
			if (Instance != null)
				Instance.sendAlways( msg );
		}

		public EventHandler< MessagesReceivedEventArgs > MessageReceived;

		public void ProcessMessages( IList<HubMessageIn> messages )
		{
			if ( !BlockInboundTraffic && MessageReceived != null && m_mainForm != null )
			{
				m_mainForm.BeginInvoke( new MethodInvoker( () => ProcessMessagesThreadSafe( messages ) ) );
			}
		}

		private void ProcessMessagesThreadSafe( IList<HubMessageIn> messages )
		{
			if ( MessageReceived != null )
			{
				MessageReceived( this, new MessagesReceivedEventArgs( messages ) );
			}
		}

		// picoHub
		//
		public static string PICO_HUB_IP = "localhost";
		public static int PICO_HUB_PORT_OUTBOUND = 6666; // for sending data to picoHub
		public static int PICO_HUB_PORT_INBOUND = 6667; // for receiving data from picoHub

		// Client socket stuff 
		System.Net.Sockets.Socket m_picoHubClientSocketOutbound;
		private InputThread m_inputThread;

		[Import( AllowDefault = true )]
		private MainForm m_mainForm = null;
	}

	/// <summary>
	/// Arguments for "item inserted" event</summary>
	/// <typeparam name="T">Type of inserted item</typeparam>
	public class MessagesReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Constructor using index, inserted item and parent</summary>
		/// <param name="index">Index of insertion</param>
		/// <param name="item">Inserted item</param>
		/// <param name="parent">Parent item</param>
		public MessagesReceivedEventArgs( IList<HubMessageIn> messages )
		{
			m_messages = messages;
		}

		public IList<HubMessageIn> Messages
		{
			get { return m_messages; }
		}

		IList<HubMessageIn> m_messages;
	}

	public class HubMessage
	{
		public HubMessage( string msgTag )
		{
			m_memStream = new MemoryStream();
			byte[] fakeMsg = new byte[4];
			m_memStream.Write( fakeMsg, 0, 4 );
			writeBytes( m_memStream, toBytes(msgTag) );
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

	public class HubMessageIn
	{
		public byte[] payload_;
		public int payloadSize_;
		public int payloadSizeReceived_;
		public int readOffset_;

		public string UnpackString()
		{
			int strLen = BitConverter.ToInt32( payload_, readOffset_ );
			readOffset_ += 4;
			if ( strLen > 0 )
			{
				string str = Encoding.ASCII.GetString( payload_, readOffset_, strLen );
				readOffset_ += strLen;
				return str;
			}
			else
			{
				return string.Empty;
			}
		}

		public string UnpackString( int strLen )
		{
			if ( strLen > 0 )
			{
				string str = Encoding.ASCII.GetString( payload_, readOffset_, strLen );
				readOffset_ += strLen;
				return str;
			}
			else
			{
				return string.Empty;
			}
		}

		public int UnpackInt()
		{
			int ival = BitConverter.ToInt32( payload_, readOffset_ );
			readOffset_ += 4;
			return ival;
		}

		public float UnpackFloat()
		{
			float fval = BitConverter.ToSingle( payload_, readOffset_ );
			readOffset_ += 4;
			return fval;
		}
	};

	class InputThread
	{
		public InputThread( HubService hubService )
		{
			m_hubService = hubService;

			m_thread = new Thread( Run );
			m_thread.Name = "HubInputClient";
			m_thread.IsBackground = true; //so that the thread can be killed if app dies.
			m_thread.SetApartmentState( ApartmentState.STA );
			m_thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			m_thread.Start();
		}

		private void Run()
		{
			try
			{
				// server never ends...
				//
				m_client = new AsynchronousClient( m_hubService );
				m_client.StartClient( HubService.PICO_HUB_IP, HubService.PICO_HUB_PORT_INBOUND );
			}
			catch ( Exception ex )
			{
				System.Diagnostics.Debug.WriteLine( "AsynchronousSocketListener exception: " + ex.Message );
			}
		}

		private HubService m_hubService;
		private Thread m_thread;
		private AsynchronousClient m_client;
	}

	// State object for receiving data from remote device.
	public class StateObject
	{
		// Client socket.
		public Socket workSocket = null;
		// Size of receive buffer.
		public const int BufferSize = 512 * 1024;
		// Receive buffer.
		public byte[] buffer = new byte[BufferSize];
		// Received data string.
		public int nBytesInDataBuffer;
	}

	class AsynchronousClient
	{
		// ManualResetEvent instances signal completion.
		private static ManualResetEvent connectDone = 
        new ManualResetEvent( false );
		private static ManualResetEvent receiveDone = 
        new ManualResetEvent( false );

		private HubService m_hubService;
		public AsynchronousClient( HubService hubService )
		{
			m_hubService = hubService;
		}

		public void StartClient( string hubIp, int portNo )
		{
			// Connect to a remote device.
			try
			{
				// Establish the remote endpoint for the socket.
				// The name of the 
				// remote device is "host.contoso.com".
				IPHostEntry ipHostInfo = Dns.GetHostEntry( hubIp );
				IPAddress ipAddress = null;
				foreach ( var addr in Dns.GetHostEntry( string.Empty ).AddressList )
				{
					if ( addr.AddressFamily == AddressFamily.InterNetwork )
						ipAddress = addr;
				}

				IPEndPoint remoteEP = new IPEndPoint( ipAddress, portNo );

				// Create a TCP/IP socket.
				Socket client = new Socket( AddressFamily.InterNetwork,
					SocketType.Stream, ProtocolType.Tcp );

				// Connect to the remote endpoint.
				client.BeginConnect( remoteEP,
					new AsyncCallback( ConnectCallback ), client );
				connectDone.WaitOne();

				// Receive the response from the remote device.
				Receive( client );
				receiveDone.WaitOne();

				// Release the socket.
				client.Shutdown( SocketShutdown.Both );
				client.Close();
			}
			catch ( Exception e )
			{
				Console.WriteLine( e.ToString() );
			}
		}

		private void ConnectCallback( IAsyncResult ar )
		{
			try
			{
				// Retrieve the socket from the state object.
				Socket client = (Socket) ar.AsyncState;

				// Complete the connection.
				client.EndConnect( ar );

				Console.WriteLine( "Socket connected to {0}",
					client.RemoteEndPoint.ToString() );

				// Signal that the connection has been made.
				connectDone.Set();
			}
			catch ( Exception e )
			{
				Console.WriteLine( e.ToString() );
			}
		}

		private void Receive( Socket client )
		{
			try
			{
				// Create the state object.
				StateObject state = new StateObject();
				state.workSocket = client;

				// Begin receiving the data from the remote device.
				client.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
					new AsyncCallback( ReceiveCallback ), state );
			}
			catch ( Exception e )
			{
				Console.WriteLine( e.ToString() );
			}
		}

		private void ReceiveCallback( IAsyncResult ar )
		{
			// Retrieve the state object and the client socket 
			// from the asynchronous state object.
			StateObject state = (StateObject) ar.AsyncState;
			Socket client = state.workSocket;

			try
			{
				// Read data from the client socket. 
				int bytesReadInThisCall = client.EndReceive( ar );
				int bytesRead = bytesReadInThisCall + state.nBytesInDataBuffer;

				if ( bytesRead > 0 )
				{
					List<HubMessageIn> messages = new List<HubMessageIn>();

					int nBytesReceivedTmp = bytesRead;
					byte[] receivedDataTmp = state.buffer;
					int receivedDataTmpOffset = 0;

					while ( nBytesReceivedTmp > 0 )
					{
						HubMessageIn lastMessage = m_messageBeingProcessed;

						bool startNewMessage = false;

						if ( lastMessage != null )
						{
							if ( lastMessage.payloadSizeReceived_ < lastMessage.payloadSize_ )
							{
								// incomplete message
								//
								int neededDataSize = lastMessage.payloadSize_ - lastMessage.payloadSizeReceived_;
								int newDataSize = Math.Min( nBytesReceivedTmp, neededDataSize );
								System.Buffer.BlockCopy( receivedDataTmp, receivedDataTmpOffset, lastMessage.payload_, lastMessage.payloadSizeReceived_, newDataSize );
								receivedDataTmpOffset += newDataSize;
								nBytesReceivedTmp -= newDataSize;
								lastMessage.payloadSizeReceived_ += newDataSize;

								if ( lastMessage.payloadSize_ == lastMessage.payloadSizeReceived_ )
								{
									messages.Add( lastMessage );
									m_messageBeingProcessed = null;
								}

								continue;
							}
							else
							{
								messages.Add( lastMessage );
								m_messageBeingProcessed = null;

								// new message
								//
								startNewMessage = true;
							}
						}
						else
						{
							startNewMessage = true;
						}

						if ( startNewMessage )
						{
							if ( nBytesReceivedTmp < sizeof( int ) )
							{
								// wait for more data
								//
								byte[] tmpBuf = new byte[4];
								System.Buffer.BlockCopy( receivedDataTmp, receivedDataTmpOffset, tmpBuf, 0, nBytesReceivedTmp );
								System.Buffer.BlockCopy( tmpBuf, 0, state.buffer, 0, nBytesReceivedTmp );
								break;
							}

							HubMessageIn newMessage = new HubMessageIn();
							m_messageBeingProcessed = newMessage;

							// message size
							//
							int messageSize = BitConverter.ToInt32( receivedDataTmp, receivedDataTmpOffset );
							receivedDataTmpOffset += 4;
							nBytesReceivedTmp -= 4;

							newMessage.payloadSize_ = messageSize;
							newMessage.payloadSizeReceived_ = 0;
							newMessage.payload_ = new byte[messageSize];
						}

					}

					state.nBytesInDataBuffer = nBytesReceivedTmp;

					if ( messages.Count > 0 )
						m_hubService.ProcessMessages( messages );

					// Not all data received. Get more.
					client.BeginReceive( state.buffer, state.nBytesInDataBuffer, StateObject.BufferSize, 0,
					new AsyncCallback( ReceiveCallback ), state );
				}
				else
				{
					// client has closed connection
					//
					client.Close();

					receiveDone.Set();
				}
			}
			catch ( Exception e )
			{
				Console.WriteLine( e.ToString() );
				client.Close();

				receiveDone.Set();
			}
		}

		private HubMessageIn m_messageBeingProcessed;
	}
}
