using System.Drawing;

using Sce.Atf.Dom;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Adaptation;
using pico.Controls.PropertyEditing;
using System.ComponentModel;

using pico.Anim;
using pico.Timeline;

namespace picoTimelineEditor.DomNodeAdapters
{
	class IntervalSoundLister : DynamicEnumUITypeEditorLister
	{
		public string[] GetNames( object instance )
		{
			IntervalSound interval = instance.As<IntervalSound>();
			if ( interval == null )
				// returning an non-empty string is necessary to avaid LongEnumEditorCrash
				//
				return new string[] { "#objectIsNotIntervalSound" };

			string[] soundNames = pico.ScreamInterop.GetBankSounds( interval.SoundBank );
			if ( soundNames == null || soundNames.Length == 0 )
				return new string[] { "#noSoundsFound" };

			return soundNames;
		}
	}

	class IntervalAnimSkelLister : DynamicEnumUITypeEditorLister
	{
		public string[] GetNames( object instance )
		{
			IntervalSound interval = instance.As<IntervalSound>();
			if ( interval == null )
				// returning an non-empty string is necessary to avaid LongEnumEditorCrash
				//
				return new string[] { "#objectIsNotIntervalSound" };

			// return joints based on where this interval is rooted at
			//

			// root is GroupAnimController
			//
			foreach ( DomNode pp in interval.DomNode.Lineage )
			{
				GroupAnimController group = pp.As<GroupAnimController>();
				if ( group != null )
				{
					SkelFileInfo sfi = AnimCache.GetSkelInfo( group.SkelFilename );
					if ( sfi != null )
						return sfi.JointNames;
				}
			}

			// root is GroupCharacterController
			//
			foreach ( DomNode pp in interval.DomNode.Lineage )
			{
				GroupCharacterController group = pp.As<GroupCharacterController>();
				if ( group != null )
				{
					SkelFileInfo sfi = AnimCache.GetSkelInfo( group.SkelFilename );
					if ( sfi != null )
						return sfi.JointNames;
				}
			}

			return new string[] { "#noJointsFound" };
		}
	}

	class IntervalAnimPositionalEnabledCallback : ICustomEnableAttributePropertyDescriptorCallback
	{
		public bool IsReadOnly( DomNode domNode, CustomEnableAttributePropertyDescriptor descriptor )
		{
			IntervalSound interval = domNode.As<IntervalSound>();
			if ( interval == null )
				return true;

			if ( interval.GroupAC != null )
				return false;

			if ( interval.GroupCC != null )
				return false;

			return true;
		}
	};

	class IntervalAnimPositionEnabledCallback : ICustomEnableAttributePropertyDescriptorCallback
	{
		public bool IsReadOnly( DomNode domNode, CustomEnableAttributePropertyDescriptor descriptor )
		{
			IntervalSound interval = domNode.As<IntervalSound>();
			if ( interval == null )
				return true;

			if ( !interval.Positional )
				return true;

			if ( interval.GroupAC != null )
				return false;

			if ( interval.GroupCC != null )
				return false;

			return true;
		}
	};

    /// <summary>
    /// Adapts DomNode to a IntervalSound</summary>
	public class IntervalSound : Interval, ITimelineValidationCallback, ITimelineObjectCreator
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
			get { return Color.FromArgb( (int) DomNode.GetAttribute( Schema.intervalSoundType.colorAttribute ) ); }
			set { DomNode.SetAttribute( Schema.intervalSoundType.colorAttribute, value.ToArgb() ); }
		}

		#endregion

		#region ITimelineObjectCreator Members
		ITimelineObject ITimelineObjectCreator.Create()
		{
			DomNodeType type = Schema.intervalSoundType.Type;
			DomNode dn = new DomNode( type );
			IntervalSound i = dn.As<IntervalSound>();
			
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
			if ( e.AttributeInfo.Equivalent( Schema.intervalSoundType.soundBankAttribute ) )
			{
				TimelineEditor.LastSoundBankFilename = SoundBank;
			}
		}

		/// <summary>
		/// Gets and sets the sound bank</summary>
		public string SoundBank
		{
			get { return (string) DomNode.GetAttribute( Schema.intervalSoundType.soundBankAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalSoundType.soundBankAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the sound name</summary>
		public string Sound
		{
			get { return (string) DomNode.GetAttribute( Schema.intervalSoundType.soundAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalSoundType.soundAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets whether sound is positional or not</summary>
		public bool Positional
		{
			get { return (bool) DomNode.GetAttribute( Schema.intervalSoundType.positionalAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalSoundType.positionalAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the local position on character</summary>
		public string Position
		{
			get { return (string) DomNode.GetAttribute( Schema.intervalSoundType.positionAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalSoundType.positionAttribute, value ); }
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

		public bool CanParentTo( DomNode parent )
		{
			return ValidateImpl( parent, 0 );
		}

		public bool Validate( DomNode parent )
		{
			return ValidateImpl( parent, 1 );
		}

		private bool ValidateImpl( DomNode parent, int validating )
		{
			foreach ( DomNode pp in parent.Lineage )
			{
				if ( pp.Type == Schema.groupCharacterControllerType.Type
					|| pp.Type == Schema.groupAnimControllerType.Type
					|| pp.Type == Schema.groupType.Type
					)
				{
					return true;
				}
			}

			return false;
		}
	}
}




