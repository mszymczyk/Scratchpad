using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.VectorMath;

using pico.Timeline;

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to an Interval</summary>
	public class IntervalSetting : Interval, ITimelineObjectCreator, ITimelineValidationCallback
    {
		///// <summary>
		///// Gets and sets the event's name</summary>
		//public override string Name
		//{
		//	get { return (string) DomNode.GetAttribute( Schema.intervalFaderType.nameAttribute ); }
		//	set { DomNode.SetAttribute( Schema.intervalFaderType.nameAttribute, value ); }
		//}

		///// <summary>
		///// Gets and sets the event's length (duration)</summary>
		//public override float Length
		//{
		//	get { return (float)DomNode.GetAttribute( Schema.intervalFaderType.lengthAttribute ); }
		//	set
		//	{
		//		float constrained = Math.Max( value, 1 );                 // >= 1
		//		constrained = (float)MathUtil.Snap( constrained, 1.0 );   // snapped to nearest integral frame number
		//		DomNode.SetAttribute( Schema.intervalFaderType.lengthAttribute, constrained );
		//	}
		//}

		/// <summary>
		/// Gets and sets the event's color</summary>
		public override Color Color
		{
			get { return Color.FromArgb( (int) DomNode.GetAttribute( Schema.intervalSettingType.colorAttribute ) ); }
			set { DomNode.SetAttribute( Schema.intervalSettingType.colorAttribute, value.ToArgb() ); }
		}

		/// <summary>
		/// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
		/// Raises the DomNodeAdapter NodeSet event. Creates several curves, automatically added to the animation.</summary>
		protected override void OnNodeSet()
		{
			base.OnNodeSet();

			//DomNode.AttributeChanged += DomNode_AttributeChanged;

			m_settingList = GetChildList<TimelineSetting>( Schema.intervalSettingType.settingChild );
			if ( m_settingList.Count > 0 )
				return;

			//CResLodSetting cresLod = (new DomNode( Schema.cresLodSettingType.Type )).As<CResLodSetting>();
			//m_settingList.Add( cresLod );

			//CResLodSetting cresLod2 = (new DomNode( Schema.cresLodSettingType.Type )).As<CResLodSetting>();
			//m_settingList.Add( cresLod2 );
		}

		//private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		//{
		//	if ( e.AttributeInfo.Equivalent(Schema.intervalFaderType.lengthAttribute) )
		//	{
		//		foreach( Curve curve in m_curves )
		//		{
		//			float newLength = (float)( Length * 0.001 );
		//			float oldLength = curve.MaxX;
		//			oldLength = Math.Max( oldLength, 0.001f );
		//			float scale = newLength / oldLength;

		//			if ( scale > 1.0f )
		//			//{
		//				curve.MaxX = newLength;

		//			//	ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
		//			//	int count = controlPoints.Count;
		//			//	for ( int i = count - 1; i >= 0; --i )
		//			//	{
		//			//		IControlPoint cp = controlPoints[i];
		//			//		cp.X = cp.X * scale;
		//			//	}
		//			//}
		//			//else
		//			//{
		//				foreach ( IControlPoint cp in curve.ControlPoints )
		//				{
		//					cp.X = cp.X * scale;
		//				}

		//			if ( scale < 1.0f )
		//				curve.MaxX = newLength;
		//			//}

		//			CurveUtils.ComputeTangent( curve );

		//			//curve.MinX = Start;
		//			//curve.Name = Name;
		//			//curve.DisplayName = "FaderValue:" + Name;
		//			curve.CurveColor = Color;
		//		}
		//	}
		//	else if ( e.AttributeInfo.Equivalent(Schema.intervalFaderType.nameAttribute) )
		//	{
		//		foreach ( Curve curve in m_curves )
		//		{
		//			curve.Name = Name;
		//			curve.DisplayName = "FaderValue: " + Name;
		//		}
		//	}
		//	else if ( e.AttributeInfo.Equivalent( Schema.intervalFaderType.colorAttribute ) )
		//	{
		//		foreach ( Curve curve in m_curves )
		//		{
		//			curve.CurveColor = Color;
		//		}
		//	}
		//}

		public IList<TimelineSetting> Settings
		{
			get { return m_settingList; }
		}

		private IList<TimelineSetting> m_settingList;

		#region ITimelineObjectCreator Members
		ITimelineObject ITimelineObjectCreator.Create()
		{
			DomNodeType nodeType = Schema.intervalSettingType.Type;
			DomNode dn = new DomNode( nodeType );

			NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
			if (paletteItem != null)
				dn.SetAttribute( nodeType.IdAttribute, paletteItem.Name );
			else
				dn.SetAttribute( nodeType.IdAttribute, "Setting" );

			return dn.Cast<ITimelineObject>();
		}
		#endregion

		public TimelineSetting CreateSetting( DomNodeType type, string defaultName )
		{
			TimelineSetting sett = new DomNode( type ).As<TimelineSetting>();

			AttributeInfo idAttribute = type.IdAttribute;
			if ( idAttribute != null )
				sett.DomNode.SetAttribute( idAttribute, defaultName );

			Settings.Add( sett );

			return sett;
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
			//if ( parent.Type != Schema.trackFaderType.Type )
			//	return false;

			return true;
		}
	}
}





