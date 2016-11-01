#pragma once

#include "Def.h"

namespace spad
{
	namespace ZMQHubUtil
	{
		class IncommingMessage
		{
		public:
			IncommingMessage( const char* tag, const u8* data, size_t dataSize )
				:tag_( tag )
				, data_( data )
				, dataSize_( dataSize )
				, dataPtr_( 0 )
			{
			}

			bool doTagsEqual( const char* otherTag ) const
			{
				return strcmp( tag_, otherTag ) == 0;
			}

			const u8* getData() const
			{
				return data_;
			}

			size_t getDataSize() const
			{
				return dataSize_;
			}

			int readInt( bool& ok ) const
			{
				if (dataPtr_ + 4 <= dataSize_)
				{
					// dataPtr_ may point to unaligned memory
					//
					int dst;
					memcpy( &dst, data_ + dataPtr_, 4 );
					dataPtr_ += 4;
					ok = true;
					return dst;
				}

				ok = false;
				return 0;
			}

			float readFloat( bool& ok ) const
			{
				if (dataPtr_ + 4 <= dataSize_)
				{
					// dataPtr_ may point to unaligned memory
					//
					float dst;
					memcpy( &dst, data_ + dataPtr_, 4 );
					dataPtr_ += 4;
					ok = true;
					return dst;
				}

				ok = false;
				return 0;
			}

			std::string readString( bool& ok ) const
			{
				std::string str;

				if (dataPtr_ + 4 <= dataSize_)
				{
					// first 4 bytes is string length
					//
					int strLen;
					memcpy( &strLen, data_ + dataPtr_, 4 );
					dataPtr_ += 4;
					const size_t maxStrLen = 4 * 1024 * 1024;
					SPAD_ASSERT2( strLen <= maxStrLen && strLen + dataPtr_ <= dataSize_, "string is longer than whole the message..." );
					if (strLen <= maxStrLen)
					{
						str.resize( strLen );
						memcpy( &str[0], data_ + dataPtr_, strLen );
						dataPtr_ += strLen;
						ok = true;
						return str;
					}
				}

				ok = false;
				return str;
			}

		private:
			const char* tag_;
			const u8* data_;
			size_t dataSize_;
			mutable size_t dataPtr_;
		};

		typedef void( *MessageHandler ) ( const IncommingMessage& msg, void* userData, void* userData2 );

		int startUp();
		void shutDown();
		void registerMessageHandler( MessageHandler funcHandler, void* userData );
		void unregisterMessageHandler( MessageHandler funcHandler );
		void processReceivedData( void* userData2 );

	} // namespace SettingsEditor
} // namespace spad
