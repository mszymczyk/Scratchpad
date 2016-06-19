using System;
using System.IO;
using System.Text;

namespace pico.Anim
{
	public class AnimFileHeader
	{
		public string magic; // 8 character long, in file, these are 8 bytes
		public byte endianess;
		//public byte padding;
		public short nJoints;
		public int nFrames;
		public int framerate;

		// not present in file
		public float durationSeconds;
		public float durationMilliseconds;

		public static AnimFileHeader ReadFromFile( string absFilePath )
		{
			AnimFileHeader afh = new AnimFileHeader();
			BinaryReader b = null;

			try
			{
				b = new BinaryReader( File.Open( absFilePath, FileMode.Open ) );
				int length = (int)b.BaseStream.Length;
				if ( length <= 20 )
					throw new Exception( "Not a pico anim file: " + absFilePath );

				byte[] magic = b.ReadBytes( 8 );
				string magicStr = System.Text.Encoding.ASCII.GetString( magic );
				if ( magicStr != "panim100" )
					throw new Exception( "Not a pico anim file: " + absFilePath );

				afh.magic = magicStr;
				afh.endianess = b.ReadByte();
				byte padding = b.ReadByte();
				afh.nJoints = b.ReadInt16();
				afh.nFrames = b.ReadInt32();
				afh.framerate = b.ReadInt32();
				double duration = (double)( afh.nFrames * 1000 ) / afh.framerate;
				afh.durationSeconds = (float)(duration * 0.001);
				afh.durationMilliseconds = (float)Math.Floor( duration );
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

			return afh;
		}

		public static AnimFileHeader ReadFromFile2( string picoDemoPath )
		{
			string absFilePath = pico.Paths.LocalPathToPicoDataAbsolutePath( picoDemoPath );
			return ReadFromFile( absFilePath );
		}
	}
}
