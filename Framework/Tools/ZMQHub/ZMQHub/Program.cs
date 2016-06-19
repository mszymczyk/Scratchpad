//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Threading;
//using System.Security.Cryptography;
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Security.Cryptography;

using ZeroMQ;

namespace ZMQSink
{
	class Program
	{
		//static void Main( string[] args )
		//{
		//	Program p = new Program();
		//	p.DoIt( args );
		//}

		//void DoIt( string[] args )
		//{
		//	//
		//	// Task sink
		//	// Binds PULL socket to tcp://localhost:5558
		//	// Collects results from workers via that socket
		//	//
		//	// Author: metadings
		//	//

		//	// Prepare our context and socket
		//	using (var context = new ZContext())
		//	using (var sink = new ZSocket( context, ZSocketType.PULL ))
		//	{
		//		sink.Bind( "tcp://*:5558" );

		//		//// Wait for start of batch
		//		//sink.ReceiveFrame();

		//		//// Start our clock now
		//		//var stopwatch = new Stopwatch();
		//		//stopwatch.Start();

		//		// Process 100 confirmations
		//		//for (int i = 0; i < 100; ++i)
		//		//{
		//		//	sink.ReceiveFrame();

		//		//	if ((i / 10) * 10 == i)
		//		//		Console.Write( ":" );
		//		//	else
		//		//		Console.Write( "." );
		//		//}

		//		Encoding e = new ASCIIEncoding();

		//		while( true )
		//		{
		//			//ZFrame frame = sink.ReceiveFrame();
		//			ZMessage msg = sink.ReceiveMessage();
		//			string s = msg.PopString( e );
		//			Console.WriteLine( "Msg: " + s );
		//		}

		//		//// Calculate and report duration of batch
		//		//stopwatch.Stop();
		//		//Console.WriteLine( "Total elapsed time: {0} ms", stopwatch.ElapsedMilliseconds );
		//	}
		//}
		//public static void Main( string[] args )
		//{
		//	//
		//	// Simple message queuing broker
		//	// Same as request-reply broker but using QUEUE device
		//	//
		//	// Author: metadings
		//	//

		//	// Socket facing clients and
		//	// Socket facing services
		//	using (var context = new ZContext())
		//	using (var frontend = new ZSocket( context, ZSocketType.XPUB ))
		//	using (var backend = new ZSocket( context, ZSocketType.XSUB ))
		//	{
		//		frontend.Bind( "tcp://*:5559" );
		//		backend.Bind( "tcp://*:5560" );

		//		// Start the proxy
		//		ZContext.Proxy( frontend, backend );
		//	}
		//}

		// http://zguide.zeromq.org/cs:espresso
		//

		public static void Main( string[] args )
		{
            Console.Title = "ZMQHUB";

			Espresso( args );
		}

		public static void Espresso( string[] args )
		{
			//
			// Espresso Pattern
			// This shows how to capture data using a pub-sub proxy
			//
			// Author: metadings
			//

			using (var context = new ZContext())
			using (var subscriber = new ZSocket( context, ZSocketType.XSUB ))
			using (var publisher = new ZSocket( context, ZSocketType.XPUB ))
			using (var listener = new ZSocket( context, ZSocketType.PAIR ))
			{
				//new Thread( () => Espresso_Publisher( context ) ).Start();
				//new Thread( () => Espresso_Subscriber( context ) ).Start();
				new Thread( () => Espresso_Listener( context ) ).Start();

				subscriber.Connect( "tcp://127.0.0.1:6000" );
				publisher.Bind( "tcp://*:6001" );
				listener.Bind( "inproc://listener" );

				ZError error;
				if (!ZContext.Proxy( subscriber, publisher, listener, out error ))
				{
					if (error == ZError.ETERM)
						return;    // Interrupted
					throw new ZException( error );
				}
			}
		}

		static void Espresso_Publisher( ZContext context )
		{
			// The publisher sends random messages starting with A-J:

			using (var publisher = new ZSocket( context, ZSocketType.PUB ))
			{
				publisher.Bind( "tcp://*:6000" );

				ZError error;

				while (true)
				{
					var bytes = new byte[5];

					using (var hash = new RNGCryptoServiceProvider()) hash.GetBytes( bytes );

					if (!publisher.Send( bytes, 0, bytes.Length, ZSocketFlags.None, out error ))
					{
						if (error == ZError.ETERM)
							return;    // Interrupted
						throw new ZException( error );
					}

					Thread.Sleep( 64 );
				}
			}
		}

		static void Espresso_Subscriber( ZContext context )
		{
			// The subscriber thread requests messages starting with
			// A and B, then reads and counts incoming messages.

			using (var subscriber = new ZSocket( context, ZSocketType.SUB ))
			{
				subscriber.Connect( "tcp://127.0.0.1:6001" );
				//subscriber.Subscribe( "A" );
				//subscriber.Subscribe( "B" );
				subscriber.SubscribeAll();

				ZError error;
				int count = 0;
				while (count < 5)
				{
					var bytes = new byte[10];
					if (-1 == subscriber.ReceiveBytes( bytes, 0, bytes.Length, ZSocketFlags.None, out error ))
					{
						if (error == ZError.ETERM)
							return;    // Interrupted
						throw new ZException( error );
					}
					++count;
				}

				Console.WriteLine( "I: subscriber counted {0}", count );
			}
		}

		static void Espresso_Listener( ZContext context )
		{
			// The listener receives all messages flowing through the proxy, on its
			// pipe. In CZMQ, the pipe is a pair of ZMQ_PAIR sockets that connect
			// attached child threads. In other languages your mileage may vary:

			using (var listener = new ZSocket( context, ZSocketType.PAIR ))
			{
				listener.Connect( "inproc://listener" );

				ZError error;
				ZFrame frame;
				while (true)
				{
					if (null != (frame = listener.ReceiveFrame( out error )))
					{
						using (frame)
						{
							string message = "";
							
							if (frame.Length >= 4)
							{
								message += frame.ReadUInt32();
								message += " bytes total. First 64:\n";

								// print first 64 bytes of message
								//
								int len = Math.Min( (int)(frame.Length - frame.Position), 64 );

								for (int ichar = 0; ichar < len; ++ichar)
								{
									int b = frame.ReadByte();
									char c = (char) b;
                                    if ( c > 32 && c < 127 ) // 32 is space, 127 is DEL
									{
										message += c;
									}
									else
									{
										if (b == 0)
											message += '.';
										else
											message += '?';
									}
								}

								Console.WriteLine( message );
							}
						}
					}
					else
					{
						if (error == ZError.ETERM)
							return;    // Interrupted
						throw new ZException( error );
					}
				}
			}
		}
	}


	public static class Ext
	{

		public static string ToHexString( this byte[] hex )
		{
			if (hex == null)
			{
				return null;
			}
			if (hex.Length == 0)
			{
				return string.Empty;
			}
			var s = new StringBuilder();
			foreach (byte b in hex)
			{
				s.Append( b.ToString( "x2" ) );
			}
			return s.ToString();
		}

		public static byte[] ToHexBytes( this string hex )
		{
			if (hex == null)
			{
				return null;
			}
			if (hex.Length == 0)
			{
				return new byte[0];
			}
			int l = hex.Length / 2;
			var b = new byte[l];
			for (int i = 0; i < l; ++i)
			{
				b[i] = Convert.ToByte( hex.Substring( i * 2, 2 ), 16 );
			}
			return b;
		}
	}
}
