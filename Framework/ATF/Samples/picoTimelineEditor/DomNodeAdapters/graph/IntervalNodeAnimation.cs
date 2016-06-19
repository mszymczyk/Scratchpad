//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

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

#pragma warning disable 0649 // suppress "field never set" warning

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to an Interval</summary>
	public class IntervalNodeAnimation : Interval, ICurveSet, ITimelineObjectCreator, ITimelineValidationCallback
    {

		///// <summary>
		///// Gets and sets the event's name</summary>
		//public override string Name
		//{
		//	get { return (string) DomNode.GetAttribute( Schema.intervalType.nameAttribute ); }
		//	set { DomNode.SetAttribute( Schema.intervalType.nameAttribute, value ); }
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

		///// <summary>
		///// Gets and sets the event's color</summary>
		//public override Color Color
		//{
		//	get { return Color.FromArgb( (int)DomNode.GetAttribute( Schema.intervalFaderType.colorAttribute ) ); }
		//	set { DomNode.SetAttribute( Schema.intervalFaderType.colorAttribute, value.ToArgb() ); }
		//}

		/// <summary>
		/// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
		/// Raises the DomNodeAdapter NodeSet event. Creates several curves, automatically added to the animation.</summary>
		protected override void OnNodeSet()
		{
			base.OnNodeSet();

			//DomNode.AttributeChanged += DomNode_AttributeChanged;

			m_curves = GetChildList<ICurve>( Schema.intervalNodeAnimationType.curveChild );
			if ( m_curves.Count > 0 )
				return;

			addCurve( "Translate X" );
			addCurve( "Translate Y" );
			addCurve( "Translate Z" );

			addCurve( "Rotate X" );
			addCurve( "Rotate Y" );
			addCurve( "Rotate Z" );
		}

		private Curve addCurve( string name )
		{
			Curve curve = (new DomNode( Schema.curveType.Type )).As<Curve>();
			curve.Name = Name;
			curve.DisplayName = name;
			curve.MinX = 0;
			curve.MaxX = (float) (Length * 0.001);
			//curve.MinY = 0.0f;
			//curve.MaxY = 1.0f;
			curve.MinY = float.MinValue;
			curve.MaxY = float.MaxValue;
			//curve.CurveColor = Color.Red;
			curve.CurveColor = Color;
			curve.PreInfinity = CurveLoopTypes.Constant;
			curve.PostInfinity = CurveLoopTypes.Constant;
			curve.XLabel = "Time";
			curve.YLabel = "Value";

			//curve.DomNode.InitializeExtensions();

			m_curves.Add( curve );

			return curve;
		}

		//private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		//{
		//	if ( e.AttributeInfo.Equivalent(Schema.intervalFaderType.startAttribute) )
		//	{
		//		foreach( Curve curve in m_curves )
		//		{
		//			//curve.MinX = Start;
		//			curve.MaxX = (float)( Length * 0.001 ); ;
		//			curve.Name = Name;
		//			curve.DisplayName = "FaderValue:" + Name;
		//			curve.CurveColor = Color;
		//		}
		//	}
		//}

        #region ICurveSet Members

        /// <summary>
        /// Gets list of the curves associated with an animation.</summary>
        public IList<ICurve> Curves
        {
            get { return m_curves; }
        }

        private IList<ICurve> m_curves;
        #endregion

		#region ITimelineObjectCreator Members
		ITimelineObject ITimelineObjectCreator.Create()
		{
			DomNodeType nodeType = Schema.intervalNodeAnimationType.Type;
			DomNode dn = new DomNode( nodeType );

			NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
			if (paletteItem != null)
				dn.SetAttribute( nodeType.IdAttribute, paletteItem.Name );
			else
				dn.SetAttribute( nodeType.IdAttribute, "NodeAnimation" );

			return dn.Cast<ITimelineObject>();
		}
		#endregion

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
			if ( parent.Type != Schema.trackType.Type )
				return false;

			return true;
		}

		/// <summary>
		/// Enum used for Channel types.
		/// </summary>
		[Flags]
		public enum ChannelType
		{
			TrX = 0x1,
			TrY = 0x2,
			TrZ = 0x4,
			RotX = 0x10,
			RotY = 0x20,
			RotZ = 0x40
		}
	}
}





