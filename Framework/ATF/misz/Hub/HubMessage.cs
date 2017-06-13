using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace misz
{
    public class HubMessageOut
    {
        public HubMessageOut( string msgTag )
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

        public void appendBytes(byte[] bytes, int offset, int count)
        {
            writeBytes(m_memStream, bytes, offset, count);
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

        private void writeBytes(MemoryStream ms, byte[] bytes, int offset, int count)
        {
            ms.Write(bytes, offset, count);
        }

        MemoryStream m_memStream;
    }

    public class HubMessageIn
    {
        public string tag;
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
}

