using System;
using System.Collections.Generic;

using pico;

namespace pico.Anim
{
	public static class AnimCache
	{
		//private static AnimCache s_instance = new AnimCache();

		public static AnimFileHeader GetAnimInfo( string picoDemoPathOrig )
		{
			string picoDemoPath = pico.Paths.CanonicalizePathSimple( picoDemoPathOrig );

			AnimFileHeader afh = null;
			if ( m_animCache.TryGetValue( picoDemoPath, out afh ) )
				return afh;

			AnimFileHeader newAfh = AnimFileHeader.ReadFromFile2( picoDemoPath );
			m_animCache.Add( picoDemoPath, newAfh );

			return newAfh;
		}

		public static SkelFileInfo GetSkelInfo( string picoDemoPathOrig )
		{
			string picoDemoPath = pico.Paths.CanonicalizePathSimple( picoDemoPathOrig );

			SkelFileInfo sfi = null;
			if ( m_skelCache.TryGetValue( picoDemoPath, out sfi ) )
				return sfi;

			SkelFileInfo newSfi = SkelFileInfo.ReadFromFile2( picoDemoPath );
			m_skelCache.Add( picoDemoPath, newSfi );

			return newSfi;
		}

		public static void OnFileChanged( System.IO.FileSystemEventArgs e, string ext, string picoDemoPath )
		{
			if ( ext == ".anim" )
			{
				AnimFileHeader afh = null;
				if ( m_animCache.TryGetValue( picoDemoPath, out afh ) )
				{
					AnimFileHeader newAfh = AnimFileHeader.ReadFromFile2( picoDemoPath );
					m_animCache[picoDemoPath] = newAfh;
				}

				return;
			}

			if ( ext == ".skel" )
			{
				SkelFileInfo sfi = null;
				if ( m_skelCache.TryGetValue( picoDemoPath, out sfi ) )
				{
					SkelFileInfo newSfi = SkelFileInfo.ReadFromFile2( picoDemoPath );
					m_skelCache[picoDemoPath ] = newSfi;
				}

				return;
			}
		}

		private static Dictionary<string, AnimFileHeader> m_animCache = new Dictionary<string,AnimFileHeader>();
		private static Dictionary<string, SkelFileInfo> m_skelCache = new Dictionary<string,SkelFileInfo>();
	}
}
