using System;
using System.ComponentModel.Composition;

using Sce.Atf;

namespace misz
{
    /// <summary>
    /// Component for communication with Hub
    /// </summary>
    //[Export( typeof( IInitializable ) )]
    //[Export( typeof( HubService ) )]
    //[PartCreationPolicy( CreationPolicy.Shared )]
    public static class HubService //: IInitializable
    {
        //#region IInitializable Members

        ///// <summary>
        ///// Finishes initializing component by connecting to Hub</summary>
        //void IInitializable.Initialize()
        //{
        //    BlockOutboundTraffic = false;
        //    BlockInboundTraffic = false;
        //}

        //#endregion

        public static bool BlockOutboundTraffic { get; set; }
        public static bool BlockInboundTraffic { get; set; }

        public static void Send( HubMessageOut msg )
        {
            if (m_impl == null)
            {
                Outputs.WriteLine( OutputMessageType.Error, "HubService is not initialized" );
                return;
            }

            if (BlockOutboundTraffic)
                return;

            try
            {
                m_impl.Send( msg );
            }
            catch (Exception ex)
            {
                Outputs.WriteLine( OutputMessageType.Error, "Send failed! " + ex.Message );
            }
        }

        public static void SendAlways( HubMessageOut msg )
        {
            if ( m_impl == null )
            {
                Outputs.WriteLine( OutputMessageType.Error, "HubService is not initialized" );
                return;
            }

            try
            {
                m_impl.Send( msg );
            }
            catch (Exception ex)
            {
                Outputs.WriteLine( OutputMessageType.Error, "Send failed! " + ex.Message );
            }
        }

        public static EventHandler<MessagesReceivedEventArgs> MessageReceived;

        public static void SetImpl( IHubServiceImplementation impl )
        {
            m_impl = impl;
        }

        static IHubServiceImplementation m_impl;
    }
}

