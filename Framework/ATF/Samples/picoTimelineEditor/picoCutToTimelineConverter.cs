//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Xml;
using System.IO;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.Timelines.Direct2D;

using picoTimelineEditor.DomNodeAdapters;
using System.Text;

namespace picoTimelineEditor
{
    /// <summary>
    /// </summary>
    public class picoCutToTimelineConverter
    {
		public picoCutToTimelineConverter( string filePath )
		{
			m_filePath = filePath;
			//m_uri = filePath;
		}

		public bool Convert( DomNode root )
		{
			//DomNode root = null;

			string xmlFileString = string.Empty;

			using ( StreamReader sr = new StreamReader( m_filePath ) )
			{
				StringBuilder sb = new StringBuilder();

				while ( sr.Peek() >= 0 )
				{
					bool copyLine = true;
					string line = sr.ReadLine();
					if ( line.StartsWith( "<?xml" ) )
					{
						if ( !line.Contains( "version=\"1.0\"" ) )
						{
							copyLine = false;
							sb.AppendLine( "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" );
						}
					}

					if ( copyLine )
						sb.AppendLine( line );
				}

				xmlFileString = sb.ToString();
			}

			if ( xmlFileString.Length == 0 )
				return false;

			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.LoadXml( xmlFileString );
			}
			catch ( Exception ex )
			{
				string msg = ex.Message;
				return false;
			}

			m_xmlDoc = xmlDoc;
			m_xmlDocElement = xmlDoc.DocumentElement;

			{
				// read cutsceneDir used for resolving filenames
				//
				XmlElement cutsceneDirElement = (XmlElement)m_xmlDocElement.SelectSingleNode( "cutsceneDir" );
				if ( cutsceneDirElement != null )
				{
					string dir = cutsceneDirElement.GetAttribute( "dir" );
					if ( string.IsNullOrEmpty( dir ) )
						throw new Exception( "cutsceneDir dir attribute is empty or not present" );

					if ( dir[dir.Length-1] != '/' && dir[dir.Length-1] != '\\' )
						dir += "\\";

					string dirPico = pico.Paths.CanonicalizePathSimple( dir );
					m_cutsceneDir = dirPico;
				}
			}

			{
				// read cutsceneNodeContainer used for resolving node names
				//
				XmlElement cutsceneNodeContainerElement = (XmlElement)m_xmlDocElement.SelectSingleNode( "cutsceneNodeContainer" );
				if ( cutsceneNodeContainerElement != null )
				{
					string cont = cutsceneNodeContainerElement.GetAttribute( "cont" );
					if ( string.IsNullOrEmpty(cont) )
						throw new Exception( "cutsceneNodeContainer cont attribute is empty or not present" );

					m_cutsceneNodeContainer = cont;
				}
			}

			try
			{
				_ParseCutsceneFile( xmlDoc, root );
			}
			catch ( Exception ex )
			{
				string str = ex.Message;
				return false;
			}

			return true;
		}

		private void _ParseCutsceneFile( XmlDocument xmlDocument, DomNode root )
		{
			//m_rootNode = new DomNode( Schema.timelineType.Type, Schema.timelineRootElement );
			m_rootNode = root;
			m_timelineDocument = m_rootNode.Cast<TimelineDocument>();
			m_timeline = m_rootNode.Cast<Timeline>();

			//D2dTimelineRenderer renderer = new picoD2dTimelineRenderer();
			//m_timelineDocument.Renderer = renderer;
			//renderer.Init( m_timelineDocument.TimelineControl.D2dGraphics );

			//m_rootNode.InitializeExtensions();

			{
				// calculate whole cutscene duration
				//
				XmlElement sequence = (XmlElement)m_xmlDocElement.SelectSingleNode( "sequence" );
				XmlNodeList nextList = sequence.SelectNodes( "next" );
				foreach ( XmlNode nextNode in nextList )
				{
					XmlElement next = (XmlElement)nextNode;

					int duration;
					_ReadTime( next, "duration", out duration, true );

					m_cutsceneDuration += duration;
				}
			}

			TimelineContext timelineContext = m_rootNode.As<TimelineContext>();
			//string fileName = Path.GetFileName( m_filePath );
			//ControlInfo controlInfo = new ControlInfo( fileName, m_filePath, StandardControlGroup.Center );
			//timelineContext.ControlInfo = controlInfo;

			//m_timelineDocument.Uri = m_uri;

			ITransactionContext transactionContext = timelineContext.As<ITransactionContext>();
			transactionContext.DoTransaction(
				delegate
				{

					_CreateCameraGroup();
					_ReadFaderSequence();
					_ReadLuaSequence();
					_ReadTextSequence();
					_ReadSoundSequence();
					_ReadAnimControlerSequence();
					_ReadKsiezniczkaCharacterSequence();
					_ReadBeachPrincessCharacterSequence();
					_ReadQueenCharacterSequence();
					_ReadMonsterCharacterSequence();
					_FinalizeCameraSequence();

					_ReadChangeLevelSequence();

				}, "_ParseCutsceneFile" );


			//return m_rootNode;
		}

		private void _CreateCameraGroup()
		{
			DomNode node = _CreateNode( Schema.groupCameraType.Type );

			m_groupCamera = node.As<GroupCamera>();
			m_timeline.Groups.Add( m_groupCamera );
			m_groupCamera.Name = "Camera";

			m_trackCameraAnim = m_groupCamera.Tracks[0].Cast<TrackCameraAnim>();
			m_trackCameraAnim.Name = "CameraAnim";

			XmlElement cameraBlender = (XmlElement) m_xmlDocElement.SelectSingleNode( "cameraBlender" );
			if ( cameraBlender != null )
			{
				XmlElement blendInElement = (XmlElement) cameraBlender.SelectSingleNode( "blendIn" );
				if ( blendInElement != null )
				{
					int duration;
					_ReadTime( blendInElement, "duration", out duration, false );
					m_groupCamera.BlendInDuration = duration;
				}

				XmlElement blendOutElement = (XmlElement)cameraBlender.SelectSingleNode( "blendOut" );
				if ( blendOutElement != null )
				{
					int duration;
					_ReadTime( blendOutElement, "duration", out duration, false );
					m_groupCamera.BlendOutDuration = duration;
				}
			}

			_ReadWholeCameraSequence();
		}

		private IntervalCameraAnim _CreateCameraAnim()
		{
			DomNode node = _CreateNode( Schema.intervalCameraAnimType.Type );
			return node.As<IntervalCameraAnim>();
		}

		private void _ReadWholeCameraSequence()
		{
			XmlElement sequence = (XmlElement) m_xmlDocElement.SelectSingleNode( "sequence" );
			XmlNodeList nextList = sequence.SelectNodes( "next" );

			int playTime = 0;

			foreach ( XmlNode nextNode in nextList )
			{
				XmlElement next = (XmlElement)nextNode;

				IntervalCameraAnim cameraAnim = _CreateCameraAnim();
				m_trackCameraAnim.Intervals.Add( cameraAnim );

				cameraAnim.Start = playTime;

				int startTime;
				if ( _ReadTime( next, "startTime", out startTime, false ) )
				{
					cameraAnim.AnimOffset = startTime;
				}
				//if ( startTime != 0 )
				//{
				//	throw new Exception( "startTime attribute must be 0" );
				//}

				int duration;
				_ReadTime( next, "duration", out duration, true );

				cameraAnim.Length = duration;

				float fov;
				if ( _ReadFloat( next, "cameraFOV", out fov, false ) )
				{
					cameraAnim.FieldOfView = fov;
				}

				float nearClipPlane;
				if ( _ReadFloat( next, "cameraNearPlane", out nearClipPlane, false ) )
				{
					cameraAnim.NearClipPlane = nearClipPlane;
				}

				float farClipPlane;
				if ( _ReadFloat( next, "cameraFarPlane", out farClipPlane, false ) )
				{
					cameraAnim.FarClipPlane = farClipPlane;
				}

				string cameraView = null;
				if ( _Read_nodeName( next, "cameraView", out cameraView, false ) )
				{
					cameraAnim.CameraView = cameraView;
				}

				string animFile = null;
				if ( _Read_animFile( next, "cameraAnim", out animFile, false ) )
				{
					cameraAnim.AnimFile = animFile;
				}

				playTime += duration;
			}
		}

		private void _FinalizeCameraSequence()
		{
			//if ( m_groupKsiezniczkaAnimController != null )
			//{
			//	m_groupCamera.PreCutsceneCamera = "ksiezniczka:characterCameraShape";
			//	m_groupCamera.PostCutsceneCamera = m_groupCamera.PreCutsceneCamera;
			//}
			//else if ( m_groupBeachPrincessAnimController != null )
			//{
			//	m_groupCamera.PreCutsceneCamera = "beachPrincess:beachPrincessCameraShape";
			//	m_groupCamera.PostCutsceneCamera = m_groupCamera.PreCutsceneCamera;
			//}
			//else
			//{
			//}
		}


		private void _ReadFaderSequence()
		{
			XmlElement sequence = (XmlElement)m_xmlDocElement.SelectSingleNode( "sequence" );
			XmlNodeList nextList = sequence.SelectNodes( "next" );

			int playTime = 0;
			int faderMaxStartTime = 0;

			{
				// find fader's max starttime
				//
				foreach ( XmlNode nextNode in nextList )
				{
					XmlElement next = (XmlElement)nextNode;

					int duration;
					_ReadTime( next, "duration", out duration, true );

					XmlNodeList faderList = next.SelectNodes( "fader" );
					foreach ( XmlNode faderNode in faderList )
					{
						XmlElement fader = (XmlElement)faderNode;

						int startTime;
						_ReadTime( fader, "startTime", out startTime, true );

						int absoluteStartTime = playTime + startTime;
						faderMaxStartTime = Math.Max( absoluteStartTime, faderMaxStartTime );
					}

					playTime += duration;
				}
			}

			faderMaxStartTime = Math.Min( faderMaxStartTime, m_cutsceneDuration );

			playTime = 0;

			foreach ( XmlNode nextNode in nextList )
			{
				XmlElement next = (XmlElement)nextNode;

				int duration;
				_ReadTime( next, "duration", out duration, true );

				XmlNodeList faderList = next.SelectNodes( "fader" );
				foreach ( XmlNode faderNode in faderList )
				{
					XmlElement fader = (XmlElement)faderNode;

					int startTime;
					_ReadTime( fader, "startTime", out startTime, true );

					if ( m_groupFader == null )
					{
						m_groupFader = m_timeline.CreateGroup().Cast<Group>();
						m_timeline.Groups.Add( m_groupFader );
						m_groupFader.Name = "Fader";

						{
							DomNode node = _CreateNode( Schema.trackFaderType.Type );
							m_trackFader = node.As<TrackFader>();
							m_groupFader.Tracks.Add( m_trackFader );
						}

						int absoluteStartTime = playTime + startTime;
						if ( absoluteStartTime > m_cutsceneDuration )
							return;

						{
							DomNode node = _CreateNode( Schema.intervalFaderType.Type );
							m_intervalFader = node.As<IntervalFader>();
							m_trackFader.Intervals.Add( m_intervalFader );
						}

						m_intervalFader.Start = absoluteStartTime;
						m_intervalFader.Length = faderMaxStartTime - absoluteStartTime;

						float r = 0, g = 0, b = 0;
						_ReadFloat( fader, "r", out r, false );
						_ReadFloat( fader, "g", out g, false );
						_ReadFloat( fader, "b", out b, false );

						m_intervalFader.Color = pico.picoColor.fromRGB( r, g, b );

						m_faderCurve = m_intervalFader.Curves[0].Cast<Curve>();
						m_faderCurve.Clear();
						m_faderCurve.PreInfinity = CurveLoopTypes.Constant;
						m_faderCurve.PostInfinity = CurveLoopTypes.Constant;
						m_faderCurve.CurveInterpolation = InterpolationTypes.Linear;
					}

					float fval;
					_ReadFloat( fader, "fval", out fval, true );
					fval = MathUtil.Clamp<float>( fval, 0, 1 );

					float localCurveTime = (float)startTime + (float)playTime - m_intervalFader.Start;
					localCurveTime = Math.Min( localCurveTime, m_intervalFader.Length );

					IControlPoint cp = m_faderCurve.CreateControlPoint();
					cp.X = localCurveTime * 0.001f;
					cp.Y = fval;
					cp.TangentInType = CurveTangentTypes.Linear;
					cp.TangentOutType = CurveTangentTypes.Linear;
					m_faderCurve.AddControlPoint( cp );
				}

				playTime += duration;
			}

			if ( m_faderCurve != null )
				CurveUtils.ComputeTangent( m_faderCurve );
		}

		//private void _ReadFaderSequence()
		//{
		//	XmlElement sequence = (XmlElement)m_xmlDocElement.SelectSingleNode( "sequence" );
		//	XmlNodeList nextList = sequence.SelectNodes( "next" );

		//	int playTime = 0;

		//	foreach ( XmlNode nextNode in nextList )
		//	{
		//		XmlElement next = (XmlElement)nextNode;

		//		int duration;
		//		_ReadTime( next, "duration", out duration, true );

		//		IntervalFader intervalFader = null;
		//		Curve faderCurve = null;

		//		XmlNodeList faderList = next.SelectNodes( "fader" );
		//		foreach ( XmlNode faderNode in faderList )
		//		{
		//			XmlElement fader = (XmlElement)faderNode;

		//			int startTime;
		//			_ReadTime( fader, "startTime", out startTime, true );

		//			if ( m_groupFader == null )
		//			{
		//				m_groupFader = m_timeline.CreateGroup().Cast<Group>();
		//				m_timeline.Groups.Add( m_groupFader );
		//				m_groupFader.Name = "Fader";

		//				{
		//					DomNode node = _CreateNode( Schema.trackFaderType.Type );
		//					m_trackFader = node.As<TrackFader>();
		//					m_groupFader.Tracks.Add( m_trackFader );
		//				}
		//			}

		//			if ( intervalFader == null )
		//			{
		//				DomNode node = _CreateNode( Schema.intervalFaderType.Type );
		//				intervalFader = node.As<IntervalFader>();
		//				m_trackFader.Intervals.Add( intervalFader );

		//				int absoluteStartTime = playTime + startTime;

		//				intervalFader.Start = absoluteStartTime;
		//				intervalFader.Length = Math.Max( duration - (float)absoluteStartTime, 0.0f );

		//				float r = 0, g = 0, b = 0;
		//				_ReadFloat( fader, "r", out r, false );
		//				_ReadFloat( fader, "g", out g, false );
		//				_ReadFloat( fader, "b", out b, false );

		//				intervalFader.Color = pico.picoColor.fromRGB( r, g, b );

		//				faderCurve = intervalFader.Curves[0].Cast<Curve>();
		//				faderCurve.Clear();
		//				faderCurve.PreInfinity = CurveLoopTypes.Constant;
		//				faderCurve.PostInfinity = CurveLoopTypes.Constant;
		//				faderCurve.CurveInterpolation = InterpolationTypes.Linear;
		//			}

		//			float fval;
		//			_ReadFloat( fader, "fval", out fval, true );
		//			fval = MathUtil.Clamp<float>( fval, 0, 1 );

		//			//float localCurveTime = (float)startTime + (float)playTime - m_intervalFader.Start;
		//			float localCurveTime = startTime;
		//			localCurveTime = Math.Min( localCurveTime, intervalFader.Length );

		//			IControlPoint cp = faderCurve.CreateControlPoint();
		//			cp.X = localCurveTime * 0.001f;
		//			cp.Y = fval;
		//			cp.TangentInType = CurveTangentTypes.Linear;
		//			cp.TangentOutType = CurveTangentTypes.Linear;
		//			faderCurve.AddControlPoint( cp );
		//		}

		//		playTime += duration;

		//		if ( faderCurve != null )
		//			CurveUtils.ComputeTangent( faderCurve );
		//	}
		//}

		private void _ReadLuaSequence()
		{
			XmlElement sequence = (XmlElement)m_xmlDocElement.SelectSingleNode( "sequence" );
			XmlNodeList nextList = sequence.SelectNodes( "next" );

			int playTime = 0;

			foreach ( XmlNode nextNode in nextList )
			{
				XmlElement next = (XmlElement)nextNode;

				int duration;
				_ReadTime( next, "duration", out duration, true );

				XmlNodeList luaList = next.SelectNodes( "lua" );
				foreach ( XmlNode luaNode in luaList )
				{
					XmlElement lua = (XmlElement)luaNode;

					int startTime;
					_ReadTime( lua, "startTime", out startTime, false );

					if ( m_groupLua == null )
					{
						m_groupLua = m_timeline.CreateGroup().Cast<Group>();
						m_timeline.Groups.Add( m_groupLua );
						m_groupLua.Name = "Lua";

						//m_trackLua = m_groupLua.CreateTrack().Cast<Track>();
						//m_groupLua.Tracks.Add( m_trackLua );
						//m_trackLua.Name = "LuaTrack";
					}

					DomNode node = _CreateNode( Schema.keyLuaScriptType.Type );
					LuaScript luaScript = node.As<LuaScript>();
					//m_trackLua.Keys.Add( luaScript );

					XmlCDataSection luaCData = (XmlCDataSection)lua.FirstChild;
					string sourceCodeRaw = luaCData.Data;
					string sourceCode = sourceCodeRaw.Trim();

					luaScript.Start = playTime + startTime;
					luaScript.SourceCode = sourceCode;

					// select track on which to place this script
					//
					_PlaceKeyOnTrack( m_groupLua, luaScript, "LuaTrack" );
				}

				playTime += duration;
			}
		}



		private void _ReadChangeLevelSequence()
		{
			XmlElement sequence = (XmlElement)m_xmlDocElement.SelectSingleNode( "sequence" );
			XmlNode changeLevelNode = sequence.SelectSingleNode( "changeLevel" );

			if ( changeLevelNode != null )
			{
				XmlElement changeLevel = (XmlElement)changeLevelNode;

				//m_groupChangeLevel = m_timeline.CreateGroup().Cast<Group>();
				//m_timeline.Groups.Add( m_groupChangeLevel );
				//m_groupChangeLevel.Name = "ChangeLevel";

				DomNode node = _CreateNode( Schema.refChangeLevelType.Type );
				ReferenceChangeLevel keyChangeLevel = node.Cast<ReferenceChangeLevel>();

				keyChangeLevel.Start = m_cutsceneDuration;

				string levelName = null;
				_ReadString( changeLevel, "levelName", out levelName, true );
				keyChangeLevel.LevelName = levelName;

				int unloadCurrentLevel;
				_ReadInt( changeLevel, "unloadCurrentLevel", out unloadCurrentLevel, true );
				keyChangeLevel.UnloadCurrentLevel = unloadCurrentLevel != 0;

				XmlElement cutsceneChild = (XmlElement) changeLevel.FirstChild;
				if ( cutsceneChild != null )
				{
					string cutsceneFile = null;
					_ReadString( cutsceneChild, "filename", out cutsceneFile, true );
					//keyChangeLevel.CutsceneFile = cutsceneFile;
					string absFile = pico.Paths.LocalPathToPicoDataAbsolutePath( cutsceneFile );
					keyChangeLevel.Uri = new Uri( absFile );
				}

				//_PlaceKeyOnTrack( m_groupChangeLevel, keyChangeLevel, "ChangeLevelTrack" );
				//ITrack newTrack = m_groupChangeLevel.CreateTrack();
				//newTrack.Name = "ChangeLevelTrack";
				//m_groupChangeLevel.Tracks.Add( newTrack );
				m_timeline.References.Add( keyChangeLevel );
			}
		}


		private void _ReadTextSequence()
		{
			XmlElement sequence = (XmlElement)m_xmlDocElement.SelectSingleNode( "sequence" );
			XmlNodeList nextList = sequence.SelectNodes( "next" );

			int playTime = 0;

			foreach ( XmlNode nextNode in nextList )
			{
				XmlElement next = (XmlElement)nextNode;

				int duration;
				_ReadTime( next, "duration", out duration, true );

				XmlNodeList textList = next.SelectNodes( "text" );
				foreach ( XmlNode textNode in textList )
				{
					XmlElement text = (XmlElement)textNode;

					int startTime;
					_ReadTime( text, "startTime", out startTime, true );

					if ( m_groupText == null )
					{
						m_groupText = m_timeline.CreateGroup().Cast<Group>();
						m_timeline.Groups.Add( m_groupText );
						m_groupText.Name = "Text";

						m_trackText = m_groupText.CreateTrack().Cast<Track>();
						m_groupText.Tracks.Add( m_trackText );
						m_trackText.Name = "TextTrack";
					}

					DomNode node = _CreateNode( Schema.intervalTextType.Type );

					IntervalText intervalText = node.As<IntervalText>();
					m_trackText.Intervals.Add( intervalText );

					int textDuration;
					_ReadTime( text, "duration", out textDuration, true );

					intervalText.Start = playTime + startTime;
					intervalText.Length = textDuration;

					string textNodeName = null;
					_Read_nodeName( text, "node", out textNodeName, true );
					intervalText.TextNodeName = textNodeName;

					string textTag = null;
					_ReadString( text, "tag", out textTag, true );
					intervalText.TextTag = textTag;
				}

				playTime += duration;
			}
		}


		private void _ReadSoundSequence()
		{
			XmlElement sequence = (XmlElement)m_xmlDocElement.SelectSingleNode( "sequence" );
			XmlNodeList nextList = sequence.SelectNodes( "next" );

			int playTime = 0;

			foreach ( XmlNode nextNode in nextList )
			{
				XmlElement next = (XmlElement)nextNode;

				int duration;
				_ReadTime( next, "duration", out duration, true );

				XmlNodeList soundList = next.SelectNodes( "sound" );
				foreach ( XmlNode soundNode in soundList )
				{
					XmlElement sound = (XmlElement)soundNode;

					int startTime;
					_ReadTime( sound, "startTime", out startTime, true );

					if ( m_groupSound == null )
					{
						m_groupSound = m_timeline.CreateGroup().Cast<Group>();
						m_timeline.Groups.Add( m_groupSound );
						m_groupSound.Name = "Sound";

						//Track trackSound = m_groupSound.CreateTrack().Cast<Track>();
						//m_groupSound.Tracks.Add( trackSound );
						//trackSound.Name = "SoundTrack";
					}

					DomNode node = _CreateNode( Schema.keySoundType.Type );
					KeySound keySound = node.As<KeySound>();

					keySound.Start = playTime + startTime;

					string soundBank = null;
					_ReadString( sound, "soundBank", out soundBank, true );
					keySound.SoundBank = soundBank;

					string soundName = null;
					_ReadString( sound, "sound", out soundName, true );
					keySound.Sound = soundName;

					// select track on which to place this sound
					//
					_PlaceKeyOnTrack( m_groupSound, keySound, "SoundTrack" );
				}

				playTime += duration;
			}
		}


		private void _ReadAnimControlerSequence()
		{
			XmlElement sequence = (XmlElement)m_xmlDocElement.SelectSingleNode( "sequence" );
			XmlNodeList nextList = sequence.SelectNodes( "next" );

			int playTime = 0;

			foreach ( XmlNode nextNode in nextList )
			{
				XmlElement next = (XmlElement)nextNode;

				int duration;
				_ReadTime( next, "duration", out duration, true );

				int startTime;
				_ReadTime( next, "startTime", out startTime, false );

				XmlNodeList animControllerList = next.SelectNodes( "animController" );
				foreach ( XmlNode animControllerNode in animControllerList )
				{
					XmlElement animController = (XmlElement)animControllerNode;

					if ( m_groupAnimControllers == null )
					{
						m_groupAnimControllers = m_timeline.CreateGroup().Cast<Group>();
						m_timeline.Groups.Add( m_groupAnimControllers );
						m_groupAnimControllers.Name = "AnimControllers";
					}

					// create new interval
					//
					DomNode node = _CreateNode( Schema.intervalAnimControllerType.Type );
					IntervalAnimController intervalAnimController = node.As<IntervalAnimController>();

					intervalAnimController.Start = playTime;
					intervalAnimController.Length = duration;
					intervalAnimController.AnimOffset = startTime;

					string animFile = null;
					_Read_animFile( animController, "anim", out animFile, true );
					intervalAnimController.AnimFile = animFile;

					string skelFile = null;
					_Read_animFile( animController, "skel", out skelFile, true );

					XmlElement rootNode = (XmlElement) animController.FirstChild;
					string rootNodeName = null;
					_Read_nodeName( rootNode, "node", out rootNodeName, true );

					_PlaceAnimControllerOnTrack( intervalAnimController, skelFile, rootNodeName );
				}

				playTime += duration;
			}
		}


		private void _ReadCharacterControllerSequence( GroupCharacterController group, TrackCharacterControllerAnim track, string name, string xmlElementName, string nodeName, string description )
		{
			XmlElement sequence = (XmlElement)m_xmlDocElement.SelectSingleNode( "sequence" );
			XmlNodeList nextList = sequence.SelectNodes( "next" );

			int playTime = 0;

			{
				// find fader's max starttime
				//
				foreach ( XmlNode nextNode in nextList )
				{
					XmlElement next = (XmlElement)nextNode;

					int duration;
					_ReadTime( next, "duration", out duration, true );

					int startTime;
					_ReadTime( next, "startTime", out startTime, true );

					XmlNodeList animControllerList = next.SelectNodes( xmlElementName );
					foreach ( XmlNode animControllerNode in animControllerList )
					{
						XmlElement animController = (XmlElement)animControllerNode;

						if ( group == null )
						{
							_CreateCharacterControllerGroup( nodeName, description, out group, out track );
							group.Name = name;
							group.Name = name + "Anim";
						}

						// create new interval
						//
						DomNode node = _CreateNode( Schema.intervalCharacterControllerAnimType.Type );
						IntervalCharacterControllerAnim intervalCharacterController = node.As<IntervalCharacterControllerAnim>();
						track.Intervals.Add( intervalCharacterController );

						intervalCharacterController.Start = playTime;
						intervalCharacterController.Length = duration;

						string animFile = null;
						_Read_animFile( animController, "anim", out animFile, true );

						intervalCharacterController.AnimFile = animFile;
						intervalCharacterController.AnimOffset = startTime;
					}

					playTime += duration;
				}
			}
		}


		private void _ReadKsiezniczkaCharacterSequence()
		{
			//XmlElement sequence = (XmlElement)m_xmlDocElement.SelectSingleNode( "sequence" );
			//XmlNodeList nextList = sequence.SelectNodes( "next" );

			//int playTime = 0;

			//{
			//	// find fader's max starttime
			//	//
			//	foreach ( XmlNode nextNode in nextList )
			//	{
			//		XmlElement next = (XmlElement)nextNode;

			//		int duration;
			//		_ReadTime( next, "duration", out duration, true );

			//		int startTime;
			//		_ReadTime( next, "startTime", out startTime, true );

			//		XmlNodeList animControllerKsiezniczkaList = next.SelectNodes( "animControllerKsiezniczka" );
			//		foreach ( XmlNode animControllerKsiezniczkaNode in animControllerKsiezniczkaList )
			//		{
			//			XmlElement animControllerKsiezniczka = (XmlElement)animControllerKsiezniczkaNode;

			//			if ( m_groupKsiezniczkaAnimController == null )
			//			{
			//				_CreateCharacterControllerGroup( "ksiezniczka:picoCharacterControllerShape1", "", out m_groupKsiezniczkaAnimController, out m_trackKsiezniczkaAnimController );
			//				m_groupKsiezniczkaAnimController.Name = "ksiezniczka";
			//				m_trackKsiezniczkaAnimController.Name = "ksiezniczkaAnim";
			//			}

			//			// create new interval
			//			//
			//			DomNode node = _CreateNode( Schema.intervalCharacterControllerAnimType.Type );
			//			IntervalCharacterControllerAnim intervalCharacterController = node.As<IntervalCharacterControllerAnim>();
			//			m_trackKsiezniczkaAnimController.Intervals.Add( intervalCharacterController );

			//			intervalCharacterController.Start = playTime;
			//			intervalCharacterController.Length = duration;

			//			string animFile = null;
			//			_Read_animFile( animControllerKsiezniczka, "anim", out animFile );

			//			intervalCharacterController.AnimFile = animFile;
			//			intervalCharacterController.AnimOffset = startTime;
			//		}

			//		playTime += duration;
			//	}
			//}

			//if ( m_groupKsiezniczkaAnimController != null )
			//{
			//	XmlElement meta = (XmlElement)m_xmlDocElement.SelectSingleNode( "meta" );
			//	if ( meta != null )
			//	{
			//		XmlElement characterBlendIn = (XmlElement)meta.SelectSingleNode( "characterBlendIn" );
			//		if ( characterBlendIn != null )
			//		{
			//			int duration;
			//			_ReadTime( characterBlendIn, "duration", out duration, false );
			//			m_groupKsiezniczkaAnimController.BlendInDuration = duration;
			//		}
			//		XmlElement characterBlendOut = (XmlElement)meta.SelectSingleNode( "characterBlendOut" );
			//		if ( characterBlendOut != null )
			//		{
			//			int duration;
			//			_ReadTime( characterBlendOut, "duration", out duration, false );
			//			m_groupKsiezniczkaAnimController.BlendOutDuration = duration;
			//		}
			//	}
			//}

			_ReadCharacterControllerSequence( m_groupKsiezniczkaAnimController, m_trackKsiezniczkaAnimController, "ksiezniczka", "animControllerKsiezniczka", "ksiezniczka:picoCharacterControllerShape1", "" );
			_ReadCharacterControllerBlends( m_groupKsiezniczkaAnimController );
		}


		private void _ReadBeachPrincessCharacterSequence()
		{
			_ReadCharacterControllerSequence( m_groupBeachPrincessAnimController, m_trackBeachPrincessAnimController, "beachPrincess", "animControllerBeachPrincess", "beachPrincess:picoBeachCharacterControllerShape1", "" );
			_ReadCharacterControllerSequence( m_groupDiaryAnimController, m_trackDiaryAnimController, "diary", "animControllerDiary", "beachPrincess:picoBeachCharacterControllerShape1", "diary" );
		}


		private void _ReadQueenCharacterSequence()
		{
			_ReadCharacterControllerSequence( m_groupQueenAnimController, m_trackQueenAnimController, "queen", "animControllerQueen", "QueensChamber:picoKsiezniczkaQueenControllerShape1", "" );
		}


		private void _ReadMonsterCharacterSequence()
		{
			_ReadCharacterControllerSequence( m_groupMonsterAnimController, m_trackMonsterAnimController, "monster", "animControllerMonster", "monster:picoMonsterControllerShape1", "" );
		}


		private void _ReadCharacterControllerBlends( GroupCharacterController group )
		{
			if ( group != null )
			{
				XmlElement meta = (XmlElement)m_xmlDocElement.SelectSingleNode( "meta" );
				if ( meta != null )
				{
					XmlElement characterBlendIn = (XmlElement)meta.SelectSingleNode( "characterBlendIn" );
					if ( characterBlendIn != null )
					{
						int duration;
						_ReadTime( characterBlendIn, "duration", out duration, false );
						group.BlendInDuration = duration;
					}
					XmlElement characterBlendOut = (XmlElement)meta.SelectSingleNode( "characterBlendOut" );
					if ( characterBlendOut != null )
					{
						int duration;
						_ReadTime( characterBlendOut, "duration", out duration, false );
						group.BlendOutDuration = duration;
					}
				}
			}
		}

		//private void _CreateCharacterControllerGroup<G,T>( string nodeName, string description, out G group, out T track )
		//	where G : class
		//	where T : class
		//{
		//	DomNodeType nodeType = Schema.groupCharacterControllerType.Type;
		//	DomNode node = new DomNode( nodeType );

		//	NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
		//	AttributeInfo idAttribute = nodeType.IdAttribute;
		//	if ( paletteItem != null && idAttribute != null )
		//	{
		//		node.SetAttribute( idAttribute, paletteItem.Name );
		//	}

		//	group = node.Cast<G>();
		//	IGroup igroup = group.Cast<IGroup>();
		//	m_timeline.Groups.Add( igroup );
		//	track = igroup.Tracks[0].Cast<T>();
		//}

		private DomNode _CreateNode( DomNodeType nodeType )
		{
			DomNode node = new DomNode( nodeType );

			NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
			AttributeInfo idAttribute = nodeType.IdAttribute;
			if ( paletteItem != null && idAttribute != null )
			{
				node.SetAttribute( idAttribute, paletteItem.Name );
			}

			return node;
		}

		private void _CreateCharacterControllerGroup( string nodeName, string description, out GroupCharacterController group, out TrackCharacterControllerAnim track )
		{
			DomNode node = _CreateNode( Schema.groupCharacterControllerType.Type );

			group = node.Cast<GroupCharacterController>();
			m_timeline.Groups.Add( group );
			track = group.Tracks[0].Cast<TrackCharacterControllerAnim>();

			group.NodeName = nodeName;
			group.Description = description;
		}

		private void _PlaceKeyOnTrack( IGroup group, IKey key, string newTrackName )
		{
			foreach ( ITrack track in group.Tracks )
			{
				if ( track.Keys.Count == 0 )
				{
					track.Keys.Add( key );
					return;
				}
				else
				{
					foreach ( IKey existingKey in track.Keys )
					{
						float distance = Math.Abs( existingKey.Start - key.Start );
						if ( distance > 1000 )
						{
							track.Keys.Add( key );
							return;
						}
					}
				}
			}

			// no suitable track was found
			// create new one
			//
			ITrack newTrack = group.CreateTrack();
			newTrack.Name = newTrackName;
			group.Tracks.Add( newTrack );
			newTrack.Keys.Add( key );
		}

		private void _PlaceAnimControllerOnTrack( IntervalAnimController animController, string skel, string rootNode )
		{
			foreach ( ITrack track in m_groupAnimControllers.Tracks )
			{
				TrackAnimController trackAnimController = track.Cast<TrackAnimController>();
				if ( trackAnimController.SkelFilename == skel && trackAnimController.RootNode == rootNode )
				{
					trackAnimController.Intervals.Add( animController );
					return;
				}
			}

			//ITrack newTrack = m_groupAnimControllers.CreateTrack();
			DomNode newTrackNode = _CreateNode( Schema.trackAnimControllerType.Type );
			TrackAnimController newTrack = newTrackNode.Cast<TrackAnimController>();
			newTrack.Name = "Track " + rootNode;
			newTrack.SkelFilename = skel;
			newTrack.RootNode = rootNode;
			m_groupAnimControllers.Tracks.Add( newTrack );
			newTrack.Intervals.Add( animController );
		}

		private bool _ReadTime( XmlElement xmlElem, string attribName, out int dstTimeMS, bool errorWhenMissing )
		{
			dstTimeMS = 0;

			//bool tValid = false;
			//double t = 0;

			//string attribNameValue = xmlElem.GetAttribute( attribName );
			//if ( !string.IsNullOrEmpty( attribNameValue ) )
			//{
			//	if ( !double.TryParse( attribNameValue, out t ) )
			//	{
			//		throw new Exception( string.Format( "can't parse {0} as double", attribName ) );
			//	}

			//	tValid = true;
			//}
			float t;
			bool tValid = _ReadFloat( xmlElem, attribName, out t, false );

			//bool tMSValid = false;
			//int tMS = 0;

			string attribNameMS = attribName + "MS";
			//string attribNameMSValue = xmlElem.GetAttribute( attribNameMS );
			//if ( ! string.IsNullOrEmpty(attribNameMSValue) )
			//{
			//	if ( ! int.TryParse( attribNameMSValue, out tMS ) )
			//	{
			//		throw new Exception( string.Format( "can't parse {0}:{1} as int", xmlElem.Name, attribNameMS ) );
			//	}

			//	tMSValid = true;
			//}

			int tMS;
			bool tMSValid = _ReadInt( xmlElem, attribNameMS, out tMS, false );

			if ( tValid && tMSValid )
			{
				dstTimeMS = tMS;

				throw new Exception( string.Format( "{0} has both {1} and {2} attributes", xmlElem.Name, attribName, attribNameMS ) );
			}
			else if ( tMSValid )
			{
				dstTimeMS = tMS;
			}
			else if ( tValid )
			{
				dstTimeMS = (int)( t * 1000 );
			}
			else
			{
				if ( errorWhenMissing )
				{
					throw new Exception( string.Format( "can't read element's {0} attribute {1} (or {2})", xmlElem.Name, attribName, attribNameMS ) );
				}

				return false;
			}

			return true;
		}

		private bool _Read_nodeName( XmlElement xmlElem, string attribName, out string dst, bool required )
		{
			dst = string.Empty;

			string attribNameValue = xmlElem.GetAttribute( attribName );
			if ( string.IsNullOrEmpty(attribNameValue) )
			{
				if ( required )
				{
					throw new Exception( string.Format( "can't read element's {0} node name {1}", xmlElem.Name, attribName ) );
				}

				return false;
			}

			if ( ! attribNameValue.Contains( ':' ) )
			{
				if ( string.IsNullOrEmpty(m_cutsceneNodeContainer) )
				{
					throw new Exception( string.Format( "{0} must have format <contName>:<nodeName> or cutsceneNodeContainer must be defined!", attribName ) );
				}
				else
				{
					dst = m_cutsceneNodeContainer;
					dst += ':';
					dst += attribNameValue;
					return true;
				}
			}

			dst = attribNameValue;
			return true;
		}

		private bool _Read_animFile( XmlElement xmlElem, string attribName, out string dstFilename, bool required )
		{
			dstFilename = string.Empty;

			string attribNameValue = xmlElem.GetAttribute( attribName );
			if ( string.IsNullOrEmpty( attribNameValue ) )
			{
				if ( required )
					throw new Exception( string.Format( "can't read animFile {0}", attribName ) );

				return false;
			}

			string animFile = attribNameValue;
			if ( attribNameValue[0] != '\\' && attribNameValue[0] != '/' )
			{
				animFile = m_cutsceneDir + animFile;
			}

			dstFilename = pico.Paths.CanonicalizePathSimple( animFile );

			return true;
		}

		private bool _ReadFloat( XmlElement xmlElem, string attribName, out float fval, bool required )
		{
			string attribNameValue = xmlElem.GetAttribute( attribName );
			if ( !string.IsNullOrEmpty( attribNameValue ) )
			{
				double t;

				if ( !double.TryParse( attribNameValue, out t ) )
				{
					throw new Exception( string.Format( "can't parse {0} as double", attribName ) );
				}

				fval = (float)t;
				return true;
			}

			if ( required )
			{
				throw new Exception( string.Format( "element's {0} attrib {1} is required", xmlElem.Name, attribName ) );
			}

			fval = 0;
			return false;
		}

		private bool _ReadInt( XmlElement xmlElem, string attribName, out int ival, bool required )
		{
			string attribNameValue = xmlElem.GetAttribute( attribName );
			if ( !string.IsNullOrEmpty( attribNameValue ) )
			{
				int t;

				if ( !int.TryParse( attribNameValue, out t ) )
				{
					throw new Exception( string.Format( "can't parse {0} as int", attribName ) );
				}

				ival = t;
				return true;
			}

			if ( required )
			{
				throw new Exception( string.Format( "element's {0} attrib {1} is required", xmlElem.Name, attribName ) );
			}

			ival = 0;
			return false;
		}

		private bool _ReadString( XmlElement xmlElem, string attribName, out string sval, bool required )
		{
			string attribNameValue = xmlElem.GetAttribute( attribName );
			if ( string.IsNullOrEmpty( attribNameValue ) )
			{
				if ( required )
					throw new Exception( string.Format( "can't read element's {0} string attribute {1}", xmlElem.Name, attribName ) );

				sval = string.Empty;
				return false;
			}

			sval = attribNameValue;
			return true;
		}

		private string m_filePath;
		//private Uri m_uri;
		private XmlDocument m_xmlDoc;
		private XmlElement m_xmlDocElement;
		private string m_cutsceneDir;
		private string m_cutsceneNodeContainer;
		private int m_cutsceneDuration;

		private DomNode m_rootNode;
		private TimelineDocument m_timelineDocument;
		private Timeline m_timeline;
		// camera
		//
		private GroupCamera m_groupCamera;
		private TrackCameraAnim m_trackCameraAnim;
		// fader
		//
		private Group m_groupFader;
		private TrackFader m_trackFader;
		private IntervalFader m_intervalFader;
		private Curve m_faderCurve;
		// lua
		//
		private Group m_groupLua;
		// change level
		//
		//private Group m_groupChangeLevel;
		// text
		//
		private Group m_groupText;
		private Track m_trackText;
		// sound
		//
		private Group m_groupSound;
		// animControllers
		//
		private Group m_groupAnimControllers;
		// animControllerKsiezniczka 
		//
		private GroupCharacterController m_groupKsiezniczkaAnimController;
		private TrackCharacterControllerAnim m_trackKsiezniczkaAnimController;
		// beach princess
		//
		private GroupCharacterController m_groupBeachPrincessAnimController;
		private TrackCharacterControllerAnim m_trackBeachPrincessAnimController;
		// beach princess diary
		//
		private GroupCharacterController m_groupDiaryAnimController;
		private TrackCharacterControllerAnim m_trackDiaryAnimController;
		// animControllerQueen 
		//
		private GroupCharacterController m_groupQueenAnimController;
		private TrackCharacterControllerAnim m_trackQueenAnimController;
		// monster
		//
		private GroupCharacterController m_groupMonsterAnimController;
		private TrackCharacterControllerAnim m_trackMonsterAnimController;

	}
}
