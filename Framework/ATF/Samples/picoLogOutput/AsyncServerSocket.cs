using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace pico.LogOutput
{
	// State object for reading client data asynchronously
	public class StateObject
	{
		// Client  socket.
		public Socket workSocket = null;
		// Size of receive buffer.
		public const int BufferSize = 512 * 1024;
		// Receive buffer.
		public byte[] buffer = new byte[BufferSize];
		//// Received data string.
		//public StringBuilder sb = new StringBuilder();
		public int nBytesInDataBuffer;
	}

	public class AsynchronousSocketListener
	{
		// Thread signal.
		public ManualResetEvent allDone = new ManualResetEvent( false );
		private picoLogOutputEditor m_editor;

		public AsynchronousSocketListener( picoLogOutputEditor editor )
		{
			m_editor = editor;
		}

		public void StartListening()
		{
			//// Data buffer for incoming data.
			//byte[] bytes = new Byte[1024];

			// Establish the local endpoint for the socket.
			// The DNS name of the computer
			// running the listener is "host.contoso.com".
			//IPHostEntry ipHostInfo = Dns.Resolve( Dns.GetHostName() );
			//IPHostEntry ipHostInfo = Dns.GetHostEntry( Dns.GetHostName() );
			//IPHostEntry ipHostInfo = Dns.GetHostEntry( "localhost" );
			//IPHostEntry ipHostInfo = Dns.GetHostEntry( "175.50.1.249" );
			IPHostEntry ipHostInfo = Dns.GetHostEntry( string.Empty );
			IPAddress ipAddress = null;
			foreach (var addr in Dns.GetHostEntry(string.Empty).AddressList)
			{
				if ( addr.AddressFamily == AddressFamily.InterNetwork )
					ipAddress = addr;
			}

			//IPAddress ipAddress = ipHostInfo.AddressList[2];
			IPEndPoint localEndPoint = new IPEndPoint( ipAddress, 6668 );

			// Create a TCP/IP socket.
			Socket listener = new Socket( AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp );

			// Bind the socket to the local endpoint and listen for incoming connections.
			try
			{
				listener.Bind( localEndPoint );
				listener.Listen( 100 );

				while (true)
				{
					// Set the event to nonsignaled state.
					allDone.Reset();

					// Start an asynchronous socket to listen for connections.
					Console.WriteLine( "Waiting for a connection..." );
					listener.BeginAccept(
						new AsyncCallback( AcceptCallback ),
						listener );
					//listener.BeginAccept(
					//	AcceptCallback,
					//	listener );

					// Wait until a connection is made before continuing.
					allDone.WaitOne();
				}

			}
			catch (Exception e)
			{
				Console.WriteLine( e.ToString() );
			}

			//Console.WriteLine( "\nPress ENTER to continue..." );
			//Console.Read();

		}

		public void AcceptCallback( IAsyncResult ar )
		{
			// Signal the main thread to continue.
			allDone.Set();

			// Get the socket that handles the client request.
			Socket listener = (Socket) ar.AsyncState;
			Socket handler = listener.EndAccept( ar );

			// Create the state object.
			StateObject state = new StateObject();
			state.workSocket = handler;
			handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
				new AsyncCallback( ReadCallback ), state );
		}

		public void ReadCallback( IAsyncResult ar )
		{
			String content = String.Empty;

			// Retrieve the state object and the handler socket
			// from the asynchronous state object.
			StateObject state = (StateObject) ar.AsyncState;
			Socket handler = state.workSocket;

			// Read data from the client socket. 
			int bytesReadInThisCall = 0;
			try
			{
				bytesReadInThisCall = handler.EndReceive( ar );
			}
			catch ( SocketException sex )
			{
				System.Diagnostics.Debug.WriteLine( "SocketException: " + sex.Message );
				handler.Close();
				return;
			}

			int bytesRead = bytesReadInThisCall + state.nBytesInDataBuffer;

			if (bytesRead > 0)
			{
				List<_Msg> messages = new List<_Msg>();

				int nBytesReceivedTmp = bytesRead;
				byte[] receivedDataTmp = state.buffer;
				int receivedDataTmpOffset = 0;

				while ( nBytesReceivedTmp > 0 )
				{
					_Msg lastMessage = m_messageBeingProcessed;

					bool startNewMessage = false;

					if ( lastMessage != null )
					{
						if ( lastMessage.payloadSizeReceived_ < lastMessage.payloadSize_ )
						{
							// incomplete message
							//
							int neededDataSize = lastMessage.payloadSize_ - lastMessage.payloadSizeReceived_;
							int newDataSize = Math.Min( nBytesReceivedTmp, neededDataSize );
							//memcpy( lastMessage->payload_ + lastMessage->payloadSizeReceived_, receivedDataTmp, newDataSize );
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
							//memcpy( tmpBuf, receivedDataTmp, nBytesReceivedTmp );
							//memcpy( dstDataBuffer, tmpBuf, nBytesReceivedTmp );
							//nBytesInDataBuffer = nBytesReceivedTmp;
							System.Buffer.BlockCopy( receivedDataTmp, receivedDataTmpOffset, tmpBuf, 0, nBytesReceivedTmp );
							System.Buffer.BlockCopy( tmpBuf, 0, state.buffer, 0, nBytesReceivedTmp );
							break;
						}

						_Msg newMessage = new _Msg();
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

				foreach ( _Msg msg in messages )
				{
					string messageType = msg.UnpackString();
					if ( messageType == "cmd" )
					{
						string cmd = msg.UnpackString();
						if ( cmd == "clear" )
						{
							string channel = msg.UnpackString();
							m_editor.clearChannel( channel );
						}
						//else if ( cmd == "clearAll" )
						//{
						//	m_editor.clearAll();
						//}
						else if ( cmd == "rename" )
						{
							string oldChannelName = msg.UnpackString();
							string newChannelName = msg.UnpackString();
							m_editor.renameChannel( oldChannelName, newChannelName );
						}
						else if ( cmd == "close" )
						{
							string channel = msg.UnpackString();
							m_editor.closeChannel( channel );
						}
					}
					else if ( messageType == "msg" )
					{
						DataItem di = new DataItem();

						string channel = msg.UnpackString();
						di.Type = msg.UnpackInt();
						di.Description = msg.UnpackString();
						di.Tag = msg.UnpackString();
						di.File = msg.UnpackString();
						di.Line = msg.UnpackInt();

						m_editor.addDataItem( di, channel );
					}
				}

				//int messageSize = BitConverter.ToInt32( state.buffer, 0 );
				//int messageType = BitConverter.ToInt32( state.buffer, 4 );
				//// There  might be more data, so store the data received so far.
				//state.sb.Append( Encoding.ASCII.GetString(
				//	state.buffer, 0, bytesRead ) );

				//// Check for end-of-file tag. If it is not there, read 
				//// more data.
				//content = state.sb.ToString();
				//if (content.IndexOf( "<EOF>" ) > -1)
				//{
				//	// All the data has been read from the 
				//	// client. Display it on the console.
				//	Console.WriteLine( "Read {0} bytes from socket. \n Data : {1}",
				//		content.Length, content );
				//	// Echo the data back to the client.
				//	Send( handler, content );
				//}
				//else
				//{
				// Not all data received. Get more.
				handler.BeginReceive( state.buffer, state.nBytesInDataBuffer, StateObject.BufferSize, 0,
				new AsyncCallback( ReadCallback ), state );
				//}
			}
			else
			{
				// client has closed connection
				//
				handler.Close();
			}
		}

		//private static void Send( Socket handler, String data )
		//{
		//	// Convert the string data to byte data using ASCII encoding.
		//	byte[] byteData = Encoding.ASCII.GetBytes( data );

		//	// Begin sending the data to the remote device.
		//	handler.BeginSend( byteData, 0, byteData.Length, 0,
		//		new AsyncCallback( SendCallback ), handler );
		//}

		//private static void SendCallback( IAsyncResult ar )
		//{
		//	try
		//	{
		//		// Retrieve the socket from the state object.
		//		Socket handler = (Socket) ar.AsyncState;

		//		// Complete sending the data to the remote device.
		//		int bytesSent = handler.EndSend( ar );
		//		Console.WriteLine( "Sent {0} bytes to client.", bytesSent );

		//		handler.Shutdown( SocketShutdown.Both );
		//		handler.Close();

		//	}
		//	catch (Exception e)
		//	{
		//		Console.WriteLine( e.ToString() );
		//	}
		//}


		//public static int Main( String[] args )
		//{
		//	StartListening();
		//	return 0;
		//}

		class _Msg
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
					readOffset_ += strLen + 1; // null-terminating char
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
		};

		private _Msg m_messageBeingProcessed;
	}

}