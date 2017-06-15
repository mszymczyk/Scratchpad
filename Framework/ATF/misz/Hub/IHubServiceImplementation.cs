using System;

namespace misz
{
    public interface IHubServiceImplementation
    {
        void Send( HubMessageOut msg );

        event EventHandler<MessagesReceivedEventArgs> MessageReceived;
    }
}

