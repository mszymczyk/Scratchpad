using System;
using Sce.Atf;
using ZeroMQ;

namespace misz
{
    /// <summary>
    /// Component for communication with ZmqHub
    /// </summary>
	public class ZMQHubService : IHubServiceImplementation
	{
		public ZMQHubService()
		{
			// TODO: fix Dispose

			try
			{
				m_context = new ZContext();
				m_publisher = new ZSocket( m_context, ZSocketType.PUB );

				m_publisher.Bind( "tcp://*:6000" );
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine( "Exception while connecting to Hub (outbound): " + ex.Message );
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

		public void Send( HubMessageOut msg )
		{
			if (m_publisher == null)
			{
				Outputs.WriteLine( OutputMessageType.Error, "ZMQHubService is not connected to Hub" );
				return;
			}

			try
			{
				byte[] bytes = msg.getFinalByteStream();
				ZFrame zframe = new ZFrame( bytes );
				m_publisher.Send( zframe );
			}
			catch (Exception ex)
			{
				Outputs.WriteLine( OutputMessageType.Error, "Exception: send failed! " + ex.Message );
			}
		}

        public event EventHandler<MessagesReceivedEventArgs> MessageReceived;

        static ZContext m_context;
		static ZSocket m_publisher;
	}
}

