#include "Util_pch.h"
#include "ZMQHubUtil.h"
#include <3rdParty/zeromq/include/zmq.h>

#pragma comment(lib, "libzmq.lib")

namespace spad
{
	namespace ZMQHubUtil
	{
		struct _Impl
		{
			int startUp()
			{
				zctx_ = zmq_ctx_new();

				subscriber_ = zmq_socket( zctx_, ZMQ_SUB );
				zmq_connect( subscriber_, "tcp://127.0.0.1:6001" );
				// subscribe to all messages
				//
				zmq_setsockopt( subscriber_, ZMQ_SUBSCRIBE, nullptr, 0 );

				BackgroundThread bgt( this );
				thread_ = std::thread( bgt );

				return 0;
			}

			void shutDown()
			{
				// subscriber socket closed in another thread
				//
				if (zctx_)
				{
					zmq_ctx_destroy( zctx_ );
					zctx_ = nullptr;
				}

				if (thread_.joinable())
					thread_.join();
			}

			void registerMessageHandler( MessageHandler funcHandler, void* userData )
			{
				SPAD_ASSERT( std::find_if( messageHandlers_.begin(), messageHandlers_.end(), [&]( _MsgHandler& h ) { return h.func == funcHandler; } ) == messageHandlers_.end() );

				_Impl::_MsgHandler h;
				h.func = funcHandler;
				h.userData = userData;
				messageHandlers_.push_back( h );
			}

			void unregisterMessageHandler( MessageHandler funcHandler )
			{
				std::remove_if( messageHandlers_.begin(), messageHandlers_.end(), [&]( _MsgHandler& h ) { return h.func == funcHandler;	} );
			}

			void processReceivedData( void* userData2 )
			{
				std::lock_guard<std::mutex> lck( messagesMutex_ );

				for (const auto& msg : messages_)
				{
					const u8* rawData = &msg[0];
					const size_t rawDataSize = msg.size();
					const char* tag = reinterpret_cast<const char*>( rawData + 4 );
					size_t headerSize = 4 + strlen( tag ) + 1;
					SPAD_ASSERT( headerSize <= rawDataSize );
					size_t userDataSize = rawDataSize - headerSize;
					IncommingMessage imsg( tag, rawData + headerSize, userDataSize );

					for (const auto& handler : messageHandlers_)
					{
						handler.func( imsg, handler.userData, userData2 );
					}
				}

				messages_.clear();
			}

			struct BackgroundThread
			{
				_Impl* impl_ = nullptr;

				BackgroundThread( _Impl* impl )
					: impl_( impl )
				{	}

				void operator()()
				{
					//std::unique_ptr<std::vector<u8>> msgCopy = std::make_unique<std::vector<u8>>();
					std::vector<u8> msgCopy;

					for ( ;; )
					{
						zmq_msg_t msg;
						int rc = zmq_msg_init( &msg );
						SPAD_ASSERT( rc == 0 );
						rc = zmq_msg_recv( &msg, impl_->subscriber_, 0 );
						if (rc == -1)
						{
							int err = zmq_errno();
							if (err == ETERM)
							{
								// ok, we're shutting down
								//
							}
							else
							{
								logError( "zmq_msg_recv failed. Err=%d", err );
							}

							// close socket here or we never exit from zmq_ctx_destroy in main thread
							//
							zmq_close( impl_->subscriber_ );
							impl_->subscriber_ = nullptr;

							zmq_msg_close( &msg );
							break;
						}

						size_t msgSize = zmq_msg_size( &msg );
						void* msgData = zmq_msg_data( &msg );

						//size_t oldSize = msgCopy->size();
						//msgCopy->resize( oldSize + msgSize );
						//memcpy( &(*msgCopy)[oldSize], msgData, msgSize );
						size_t oldSize = msgCopy.size();
						msgCopy.resize( oldSize + msgSize );
						memcpy( &msgCopy[oldSize], msgData, msgSize );

						if (zmq_msg_more( &msg ))
						{
							// more data to come
							// don't put message in the queue yet
						}
						else
						{
							{
								std::lock_guard<std::mutex> lck( impl_->messagesMutex_ );
								impl_->messages_.emplace_back( std::move(msgCopy) );
							}

							//msgCopy->clear();
							msgCopy.clear();
						}

						zmq_msg_close( &msg );
					}
				}
			};

			void* zctx_ = nullptr;
			void* subscriber_ = nullptr;
			std::thread thread_;
			std::mutex messagesMutex_;

			//std::vector<std::unique_ptr<std::vector<u8>>> messages_;
			std::vector<std::vector<u8>> messages_;

			struct _MsgHandler
			{
				MessageHandler func = nullptr;
				void* userData = nullptr;
			};

			std::vector<_MsgHandler> messageHandlers_;

		} *_gImpl;

		int startUp()
		{
			SPAD_ASSERT( !_gImpl );
			if (_gImpl)
				return -1;

			_gImpl = new _Impl();
			int ires = _gImpl->startUp();
			if (ires)
			{
				_gImpl->shutDown();
				delete _gImpl;
				_gImpl = nullptr;
				SPAD_ASSERT2( false, "ZMQHubUtil startUp failed!" );
				return ires;
			}

			return 0;
		}

		void shutDown()
		{
			if (_gImpl)
			{
				_gImpl->shutDown();
				delete _gImpl;
				_gImpl = nullptr;
			}
		}

		void registerMessageHandler( MessageHandler funcHandler, void* userData )
		{
			if (_gImpl)
				_gImpl->registerMessageHandler( funcHandler, userData );
		}

		void unregisterMessageHandler( MessageHandler funcHandler )
		{
			if (_gImpl)
				_gImpl->unregisterMessageHandler( funcHandler );
		}

		void processReceivedData( void* userData2 )
		{
			if (_gImpl)
				_gImpl->processReceivedData( userData2 );
		}

	} // namespace ZMQHubUtil
} // namespace spad
