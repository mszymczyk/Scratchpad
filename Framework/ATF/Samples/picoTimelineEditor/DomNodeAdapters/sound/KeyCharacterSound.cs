//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Adaptation;
using pico.Controls.PropertyEditing;

using pico.Timeline;

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoTimelineEditor.DomNodeAdapters
{
	/// <summary>
	/// Adapts DomNode to a Key</summary>
	public class KeyCharacterSound : Key, ITimelineValidationCallback
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

			//if ( string.IsNullOrEmpty( SoundBank ) )
			//{
			//	SoundBank = TimelineEditor.LastSoundBankFilename;
			//}

			DomNode.AttributeChanged += DomNode_AttributeChanged;
		}

		private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		{
			//if ( e.AttributeInfo.Equivalent( Schema.keyCharacterSoundType.soundBankAttribute ) )
			//{
			//	TimelineEditor.LastSoundBankFilename = SoundBank;
			//}
		}

		/// <summary>
		/// Gets and sets whether sound is positional or not</summary>
		public bool Positional
		{
			get { return (bool) DomNode.GetAttribute( Schema.keyCharacterSoundType.positionalAttribute ); }
			set { DomNode.SetAttribute( Schema.keyCharacterSoundType.positionalAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the local position on character</summary>
		public string Position
		{
			get { return (string) DomNode.GetAttribute( Schema.keyCharacterSoundType.positionAttribute ); }
			set { DomNode.SetAttribute( Schema.keyCharacterSoundType.positionAttribute, value ); }
		}

		bool ITimelineValidationCallback.CanParentTo( DomNode parent )
		{
			return ValidateImpl( parent, 0 );
		}

		bool ITimelineValidationCallback.Validate( DomNode parent )
		{
			return ValidateImpl( parent, 1 );
		}

		private bool ValidateImpl( DomNode parent, int validating )
		{
			if ( parent.Type == Schema.trackAnimControllerType.Type
				|| parent.Type == Schema.trackCharacterControllerAnimType.Type
				|| parent.Type == Schema.intervalAnimControllerType.Type
				|| parent.Type == Schema.intervalCharacterControllerAnimType.Type
				)
				return true;

			return false;
		}
	}
}




