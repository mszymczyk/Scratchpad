using System.Drawing;

using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Adaptation;
using pico.Controls.PropertyEditing;

using pico.Timeline;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoAnimClipEditor.DomNodeAdapters
{
	class KeySoundLister : DynamicEnumUITypeEditorLister
	{
		public string[] GetNames( object instance )
		{
			KeySound keySound = instance.As<KeySound>();
			if ( keySound == null )
				// returning an non-empty string is necessary to avaid LongEnumEditorCrash
				//
				return new string[] { "#objectIsNotKeySound" };

			string[] soundNames = pico.ScreamInterop.GetBankSounds( keySound.SoundBank );
			if ( soundNames == null || soundNames.Length == 0 )
				return new string[] { "#noSoundsFound" };

			return soundNames;
		}
	}

    /// <summary>
    /// Adapts DomNode to a Key</summary>
	public class KeySound : Key
    {
		#region IEvent Members

		/// <summary>
		/// Gets and sets the event's color</summary>
		public override Color Color
		{
			get { return Color.Aqua; }
			set { }
		}

		#endregion

		/// <summary>
		/// Performs initialization when the adapter is connected to the DomNode.
		/// Raises the DomNodeAdapter NodeSet event. Creates read only data for animdata
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
			if ( e.AttributeInfo.Equivalent(Schema.keySoundType.soundBankAttribute) )
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

		public override bool CanParentTo( DomNode parent )
		{
			return ValidateImpl( parent, 0 );
		}

		public override bool Validate( DomNode parent )
		{
			return ValidateImpl( parent, 1 );
		}

		private bool ValidateImpl( DomNode parent, int validating )
		{
			if ( parent.Type != Schema.trackType.Type )
				return false;

			return true;
		}
	}
}




