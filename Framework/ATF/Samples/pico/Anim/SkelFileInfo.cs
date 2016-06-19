using System;
using System.IO;
using System.Text;

namespace pico.Anim
{
	public class SkelFileInfo
	{
		public string magic; // 8 character long, in file, these are 8 bytes
		public byte endianess;
		//public byte padding;
		public short nJoints;
		//public int nFrames;
		//public int framerate;
		private short[] jointsParents;
		private string[] m_jointNames;

		public string[] JointNames { get { return m_jointNames; } }

		//// not present in file
		//public float durationSeconds;
		//public float durationMilliseconds;

		public static SkelFileInfo ReadFromFile( string absFilePath )
		{
			SkelFileInfo sfi = new SkelFileInfo();
			BinaryReader b = null;

			try
			{
				b = new BinaryReader( File.Open( absFilePath, FileMode.Open, FileAccess.Read, FileShare.Read ) );
				int length = (int)b.BaseStream.Length;
				if ( length <= 16 )
					throw new Exception( "Not a pico skel file: " + absFilePath );

				byte[] magic = b.ReadBytes( 8 );
				string magicStr = System.Text.Encoding.ASCII.GetString( magic );
				if ( magicStr != "pskel101" )
					throw new Exception( "Not a pico skel file: " + absFilePath );

				sfi.magic = magicStr;
				sfi.endianess = b.ReadByte();
				byte padding = b.ReadByte();
				sfi.nJoints = b.ReadInt16();
				int padding2 = b.ReadInt32();

				sfi.m_jointNames = new string[sfi.nJoints];
				sfi.jointsParents = new short[sfi.nJoints];

				// skip pointers: jointsParents, jointHashes, jointNames
				// all should be 0, patched at runtime
				//
				b.ReadInt64();
				b.ReadInt64();
				b.ReadInt64();

				// jointsParents
				//
				for ( short ijoint = 0; ijoint < sfi.nJoints; ++ijoint )
					sfi.jointsParents[ijoint] = b.ReadInt16();
				
				// skip jointHashes
				//
				for ( short ijoint = 0; ijoint < sfi.nJoints; ++ijoint )
					b.ReadInt32();

				// skip joint offsets
				//
				for ( short ijoint = 0; ijoint < sfi.nJoints; ++ijoint )
					b.ReadInt32();

				// joint names
				//
				for ( short ijoint = 0; ijoint < sfi.nJoints; ++ijoint )
				{
					sfi.m_jointNames[ijoint] = ReadJointName( b );
				}
			}
			catch
			{
				return null;
			}
			finally
			{
				if ( b != null )
					b.Close();
			}

			return sfi;
		}

		public static SkelFileInfo ReadFromFile2( string picoDemoPath )
		{
			string absFilePath = pico.Paths.LocalPathToPicoDataAbsolutePath( picoDemoPath );
			return ReadFromFile( absFilePath );
		}

		private static string ReadJointName( BinaryReader b )
		{
			StringBuilder sb = new StringBuilder();
			byte[] bytes = new byte[1];
			char[] chars = new char[1];
			while(true)
			{
				byte c = b.ReadByte();
				if ( c == 0 )
					break;

				bytes[0] = c;
				Encoding.ASCII.GetChars(bytes, 0, 1, chars, 0);

				sb.Append( chars[0] );
			}

			return sb.ToString();
		}
	}
}
