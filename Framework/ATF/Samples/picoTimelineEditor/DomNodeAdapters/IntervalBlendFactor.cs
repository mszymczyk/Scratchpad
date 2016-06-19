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
	/// Adapts DomNode to an IntervalBlendFactor</summary>
	public class IntervalBlendFactor : Interval, ICurveSet, ITimelineObjectCreator, ITimelineValidationCallback
    {
		/// <summary>
		/// Gets and sets the event's name</summary>
		public override string Name
		{
			get { return (string) DomNode.GetAttribute( Schema.intervalBlendFactorType.nameAttribute ); }
			set { DomNode.SetAttribute( Schema.intervalBlendFactorType.nameAttribute, value ); }
		}

		/// <summary>
		/// Gets and sets the event's length (duration)</summary>
		public override float Length
		{
			get { return (float) DomNode.GetAttribute( Schema.intervalBlendFactorType.lengthAttribute ); }
			set
			{
				float constrained = Math.Max( value, 1 );                 // >= 1
				constrained = (float)MathUtil.Snap( constrained, 1.0 );   // snapped to nearest integral frame number
				DomNode.SetAttribute( Schema.intervalBlendFactorType.lengthAttribute, constrained );
			}
		}

		/// <summary>
		/// Gets and sets the event's color</summary>
		public override Color Color
		{
			get { return Color.FromArgb( (int) DomNode.GetAttribute( Schema.intervalBlendFactorType.colorAttribute ) ); }
			set { DomNode.SetAttribute( Schema.intervalBlendFactorType.colorAttribute, value.ToArgb() ); }
		}

		/// <summary>
		/// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
		/// Raises the DomNodeAdapter NodeSet event. Creates several curves, automatically added to the animation.</summary>
		protected override void OnNodeSet()
		{
			base.OnNodeSet();

			DomNode.AttributeChanged += DomNode_AttributeChanged;

			m_curves = GetChildList<ICurve>( Schema.intervalBlendFactorType.curveChild );
			if ( m_curves.Count > 0 )
				return;

			// add sample curve

			Curve curve = (new DomNode( Schema.curveType.Type )).As<Curve>();
			curve.Name = Name;
			curve.DisplayName = "BlendValue:" + Name;
			curve.MinX = 0;
			curve.MaxX = (float)( Length * 0.001 );
			curve.MinY = 0.0f;
			curve.MaxY = 1.0f;
			curve.CurveColor = Color;
			curve.PreInfinity = CurveLoopTypes.Constant;
			curve.PostInfinity = CurveLoopTypes.Constant;
			curve.XLabel = "Time";
			curve.YLabel = "Value";

			Vec2F pStart = new Vec2F( 0, 1 );
			Vec2F pEnd = new Vec2F( curve.MaxX, 0 );

			Vec2F p1 = Vec2F.Lerp( pStart, pEnd, 0.33f );
			Vec2F p2 = Vec2F.Lerp( pStart, pEnd, 0.66f );

			IControlPoint cp = curve.CreateControlPoint();
			cp.X = pStart.X;
			cp.Y = pStart.Y;
			curve.AddControlPoint( cp );

			cp = curve.CreateControlPoint();
			cp.X = p1.X;
			cp.Y = p1.Y;
			curve.AddControlPoint( cp );

			cp = curve.CreateControlPoint();
			cp.X = p2.X;
			cp.Y = p2.Y;
			curve.AddControlPoint( cp );

			cp = curve.CreateControlPoint();
			cp.X = pEnd.X;
			cp.Y = pEnd.Y;
			curve.AddControlPoint( cp );

			CurveUtils.ComputeTangent( curve );

			m_curves.Add( curve );
		}

		private void DomNode_AttributeChanged( object sender, AttributeEventArgs e )
		{
			if ( e.AttributeInfo.Equivalent( Schema.intervalBlendFactorType.lengthAttribute ) )
			{
				foreach( Curve curve in m_curves )
				{
					float newLength = (float)( Length * 0.001 );
					float oldLength = curve.MaxX;
					oldLength = Math.Max( oldLength, 0.001f );
					float scale = newLength / oldLength;

					if ( scale > 1.0f )
						curve.MaxX = newLength;

					foreach ( IControlPoint cp in curve.ControlPoints )
					{
						cp.X = cp.X * scale;
					}

					if ( scale < 1.0f )
						curve.MaxX = newLength;

					CurveUtils.ComputeTangent( curve );

					curve.CurveColor = Color;
				}
			}
			else if ( e.AttributeInfo.Equivalent( Schema.intervalBlendFactorType.nameAttribute ) )
			{
				foreach ( Curve curve in m_curves )
				{
					curve.Name = Name;
					curve.DisplayName = "BlendValue: " + Name;
				}
			}
			else if ( e.AttributeInfo.Equivalent( Schema.intervalBlendFactorType.colorAttribute ) )
			{
				foreach ( Curve curve in m_curves )
				{
					curve.CurveColor = Color;
				}
			}
		}

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
			DomNodeType nodeType = Schema.intervalBlendFactorType.Type;
			DomNode dn = new DomNode( nodeType );

			NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
			if (paletteItem != null)
				dn.SetAttribute( nodeType.IdAttribute, paletteItem.Name );
			else
				dn.SetAttribute( nodeType.IdAttribute, "BlendFactor" );

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
			if ( parent.Type != Schema.trackBlendFactorType.Type )
				return false;

			return true;
		}
	}
}





