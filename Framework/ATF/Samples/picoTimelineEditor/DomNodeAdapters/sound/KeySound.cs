//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;

using pico.Anim;
using pico.Controls.PropertyEditing;

namespace picoTimelineEditor.DomNodeAdapters
{
	class KeySoundLister : DynamicEnumUITypeEditorLister
	{
		public string[] GetNames( object instance )
		{
			KeySound key = instance.As<KeySound>();
			if ( key == null )
				// returning an non-empty string is necessary to avaid LongEnumEditorCrash
				//
				return new string[] { "#objectIsNotKeySound" };

			string[] soundNames = pico.ScreamInterop.GetBankSounds( key.SoundBank );
			if ( soundNames == null || soundNames.Length == 0 )
				return new string[] { "#noSoundsFound" };

			return soundNames;
		}
	}

	class KeyAnimSkelLister : DynamicEnumUITypeEditorLister
	{
		public string[] GetNames( object instance )
		{
			KeySound key = instance.As<KeySound>();
			if ( key == null )
				// returning an non-empty string is necessary to avaid LongEnumEditorCrash
				//
				return new string[] { "#objectIsNotIntervalSound" };

			// return joints based on where this interval is rooted at
			//

			// root is GroupAnimController
			//
			GroupAnimController groupAC = key.GroupAC;
			if ( groupAC != null )
			{
				SkelFileInfo sfi = AnimCache.GetSkelInfo( groupAC.SkelFilename );
				if ( sfi != null )
					return sfi.JointNames;
			}

			// root is GroupCharacterController
			//
			GroupCharacterController groupCC = key.GroupCC;
			if ( groupCC != null )
			{
				SkelFileInfo sfi = AnimCache.GetSkelInfo( groupCC.SkelFilename );
				if ( sfi != null )
					return sfi.JointNames;
			}

			return new string[] { "#noJointsFound" };
		}
	}

	class KeyAnimPositionalEnabledCallback : ICustomEnableAttributePropertyDescriptorCallback
	{
		public bool IsReadOnly( DomNode domNode, CustomEnableAttributePropertyDescriptor descriptor )
		{
			KeySound key = domNode.As<KeySound>();
			if ( key == null )
				return true;

			if ( key.GroupAC != null )
				return false;

			if ( key.GroupCC != null )
				return false;

			return true;
		}
	};

	class KeyAnimPositionEnabledCallback : ICustomEnableAttributePropertyDescriptorCallback
	{
		public bool IsReadOnly( DomNode domNode, CustomEnableAttributePropertyDescriptor descriptor )
		{
			KeySound key = domNode.As<KeySound>();
			if ( key == null )
				return true;

			if ( !key.Positional )
				return true;

			if ( key.GroupAC != null )
				return false;

			if ( key.GroupCC != null )
				return false;

			return true;
		}
	};

    /// <summary>
    /// Adapts DomNode to a Key</summary>
    public class KeySound : Key
    {
		/// <summary>
		/// Performs initialization when the adapter is connected to the DomNode.
		/// </summary>
		protected override void OnNodeSet()
		{
			base.OnNodeSet();

			if ( string.IsNullOrEmpty( SoundBank ) )
			{
				SoundBank = TimelineEditor.LastSoundBankFilename;
			}

			DomNode.AttributeChanged += DomNode_AttributeChanged;
		}

		private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		{
			if ( e.AttributeInfo.Equivalent( Schema.keySoundType.soundBankAttribute ) )
			{
				TimelineEditor.LastSoundBankFilename = SoundBank;
			}
		}

		/// <summary>
		/// Gets and sets the sound bank</summary>
		public string SoundBank
		{
			get { return (string)DomNode.GetAttribute( Schema.keySoundType.soundBankAttribute); }
			set { DomNode.SetAttribute( Schema.keySoundType.soundBankAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the sound name</summary>
		public string Sound
		{
			get { return (string)DomNode.GetAttribute( Schema.keySoundType.soundAttribute ); }
			set { DomNode.SetAttribute( Schema.keySoundType.soundAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets whether sound is positional or not</summary>
		public bool Positional
		{
			get { return (bool) DomNode.GetAttribute( Schema.keySoundType.positionalAttribute ); }
			set { DomNode.SetAttribute( Schema.keySoundType.positionalAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the local position on character</summary>
		public string Position
		{
			get { return (string) DomNode.GetAttribute( Schema.keySoundType.positionAttribute ); }
			set { DomNode.SetAttribute( Schema.keySoundType.positionAttribute, value ); }
		}

		public GroupAnimController GroupAC
		{
			get
			{
				foreach ( DomNode pp in DomNode.Lineage )
				{
					GroupAnimController group = pp.As<GroupAnimController>();
					if ( group != null )
					{
						return group;
					}
				}

				return null;
			}
		}

		public GroupCharacterController GroupCC
		{
			get
			{
				foreach ( DomNode pp in DomNode.Lineage )
				{
					GroupCharacterController group = pp.As<GroupCharacterController>();
					if ( group != null )
					{
						return group;
					}
				}

				return null;
			}
		}
	}
}




