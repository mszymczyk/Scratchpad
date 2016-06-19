using System.Drawing;

using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Adaptation;
using pico.Controls.PropertyEditing;
using System.ComponentModel;

using pico.Timeline;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoAnimClipEditor.DomNodeAdapters
{
	class IntervalCharacterSoundLister : DynamicEnumUITypeEditorLister
	{
		public string[] GetNames( object instance )
		{
			IntervalCharacterSound intervalCharacterSound = instance.As<IntervalCharacterSound>();
			if ( intervalCharacterSound == null )
				// returning an non-empty string is necessary to avaid LongEnumEditorCrash
				//
				return new string[] { "#objectIsNotIntervalCharacterSound" };

			string[] soundNames = pico.ScreamInterop.GetBankSounds( intervalCharacterSound.SoundBank );
			if ( soundNames == null || soundNames.Length == 0 )
				return new string[] { "#noSoundsFound" };

			return soundNames;
		}
	}

    /// <summary>
    /// Adapts DomNode to a Key</summary>
	public class IntervalCharacterSound : Interval, ITimelineValidationCallback, ITimelineObjectCreator
    {
		#region IEvent Members

		///// <summary>
		///// Gets and sets the event's color</summary>
		//public override Color Color
		//{
		//	get { return Color.Aqua; }
		//	set { }
		//}

		/// <summary>
		/// Gets and sets the event's color</summary>
		public override Color Color
		{
			get { return Color.FromArgb( (int) DomNode.GetAttribute( Schema.intervalCharacterSoundType.colorAttribute ) ); }
			set { DomNode.SetAttribute( Schema.intervalCharacterSoundType.colorAttribute, value.ToArgb() ); }
		}

		#endregion

		#region ITimelineObjectCreator Members
		ITimelineObject ITimelineObjectCreator.Create()
		{
			DomNodeType type = Schema.intervalCharacterSoundType.Type;
			DomNode dn = new DomNode( type );
			IntervalCharacterSound i = dn.As<IntervalCharacterSound>();
			
			NodeTypePaletteItem paletteItem = type.GetTag<NodeTypePaletteItem>();
			AttributeInfo idAttribute = type.IdAttribute;
			if (paletteItem != null &&
                idAttribute != null)
			{
				i.DomNode.SetAttribute( idAttribute, paletteItem.Name );
			}

			return i;
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
			if ( e.AttributeInfo.Equivalent(Schema.intervalCharacterSoundType.soundBankAttribute) )
			{
				TimelineEditor.LastSoundBankFilename = SoundBank;
			}
		}

		/// <summary>
		/// Gets and sets the sound bank</summary>
		public string SoundBank
		{
			get { return (string) DomNode.GetAttribute( Schema.intervalCharacterSoundType.soundBankAttribute ); }
			set	{ DomNode.SetAttribute( Schema.intervalCharacterSoundType.soundBankAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the sound name</summary>
		public string Sound
		{
			get { return (string) DomNode.GetAttribute( Schema.intervalCharacterSoundType.soundAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCharacterSoundType.soundAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets whether sound is positional or not</summary>
		public bool Positional
		{
			get { return (bool) DomNode.GetAttribute( Schema.intervalCharacterSoundType.positionalAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCharacterSoundType.positionalAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the local position on character</summary>
		public string Position
		{
			get { return (string) DomNode.GetAttribute( Schema.intervalCharacterSoundType.positionAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalCharacterSoundType.positionAttribute, value ); }
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




