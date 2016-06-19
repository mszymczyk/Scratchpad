//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;
using Sce.Atf.Direct2D;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.VectorMath;

using picoTimelineEditor.DomNodeAdapters;

namespace picoTimelineEditor
{
    /// <summary>
    /// Default timeline renderer. This class is designed to be instantiated
    /// once per TimelineControl.</summary>
	public class picoD2dTimelineRenderer : Sce.Atf.Controls.Timelines.Direct2D.D2dDefaultTimelineRenderer
    {

		/// <summary>
		/// Initializes class with graphics object</summary>
		/// <param name="graphics">Graphics object for drawing</param>
		public override void Init( D2dGraphics graphics )
		{
			base.Init( graphics );

			HeaderWidth = 180;

			m_picoTextBrush = graphics.CreateSolidBrush( SystemColors.WindowText );

			D2dGradientStop[] gradstops = 
            { 
                new D2dGradientStop(Color.LightGray, 0),
                new D2dGradientStop(Color.FromArgb(0x40, 0x40, 0x40), 1.0f),
            };
			m_fillLinearGradientBrush = graphics.CreateLinearGradientBrush( gradstops );

			//var props = new D2dStrokeStyleProperties();
			//props.EndCap = D2dCapStyle.Flat;
			//props.StartCap = D2dCapStyle.Flat;
			//props.DashStyle = D2dDashStyle.Dash;
			//m_curveStrokeStyle = D2dFactory.CreateD2dStrokeStyle( props );
		}


		public IContextRegistry ContextRegistry
		{
			get;
			set;
		}

		/// <summary>
		/// Disposes unmanaged resources</summary>
		/// <param name="disposing">Whether or not Dispose invoked this method</param>
		protected override void Dispose( bool disposing )
		{
			if ( !m_picoDisposed )
			{
				if ( disposing )
				{
					m_picoTextBrush.Dispose();
					m_fillLinearGradientBrush.Dispose();
					//m_curveStrokeStyle.Dispose();
				}

				// dispose any unmanaged resources.

				m_picoDisposed = true;
			}
			// always call base regardles.
			base.Dispose( disposing );
		}

		/// <summary>
		/// Draws an interval</summary>
		/// <param name="interval">Interval</param>
		/// <param name="bounds">Bounding rectangle, in screen space</param>
		/// <param name="drawMode">Drawing mode</param>
		/// <param name="c">Drawing context</param>
		protected override void Draw( IInterval interval, RectangleF bounds, DrawMode drawMode, Context c )
		{
			Color color = interval.Color;
			switch ( drawMode & DrawMode.States )
			{
				case DrawMode.Normal:
					RectangleF realPart = new RectangleF(
						bounds.X,
						bounds.Y,
						GdiUtil.TransformVector( c.Transform, interval.Length ),
						bounds.Height );
					bool hasTail = realPart.Width < MinimumDrawnIntervalLength;

					float h = color.GetHue();
					float s = color.GetSaturation();
					float b = color.GetBrightness();
					Color endColor = ColorUtil.FromAhsb( color.A, h, s * 0.3f, b );
					c.Graphics.FillRectangle(
						realPart,
						new PointF( 0, realPart.Top ), new PointF( 0, realPart.Bottom ),
						color, endColor );

					if ( hasTail )
					{
						endColor = ColorUtil.FromAhsb( 64, h, s * 0.3f, b );
						RectangleF tailPart = new RectangleF(
							realPart.Right,
							bounds.Y,
							bounds.Width - realPart.Width,
							bounds.Height );
						c.Graphics.FillRectangle( tailPart, endColor );
					}

					if ( interval.Is<ICurveSet>() )
						DrawCurves( interval, interval.As<ICurveSet>(), realPart, c );
					//if ( interval.Is<IntervalFader>() )
					//	DrawCurves( interval.As<IntervalFader>(), bounds, c );

					// pico
					// add line at the left border of interval
					// this helps to notice where intervals begin/end when they are tightly packed next to each other
					//
					Color lineColor = ColorUtil.FromAhsb( endColor.A, 360.0f - h, 1.0f, 0.5f );
					c.Graphics.DrawLine( new PointF( realPart.Left, realPart.Top ), new PointF( realPart.Left, realPart.Bottom ), lineColor, 2 );

					if ( color.R + color.G + color.B < 3 * 160 )
						TextBrush.Color = SystemColors.HighlightText;
					else
						TextBrush.Color = SystemColors.WindowText;

					c.Graphics.DrawText( interval.Name, c.TextFormat, bounds.Location, TextBrush );

					if ( ( drawMode & DrawMode.Selected ) != 0 )
					{
						c.Graphics.DrawRectangle(
							new RectangleF( bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2 ),
							SelectedBrush, 3.0f );
					}

					// a tiny hack to draw rectangle around selected IntervalSettings even, when selection switched to TimelineSetting
					//
					ISelectionContext selCtx = ContextRegistry.GetActiveContext<ISelectionContext>();
					if ( selCtx != null )
					{
						TimelineSetting tiSett = selCtx.LastSelected.As<TimelineSetting>();
						if ( tiSett != null )
						{
							IInterval isett = tiSett.DomNode.Parent.As<IInterval>();
							if ( object.ReferenceEquals(isett, interval) )
							{
								c.Graphics.DrawRectangle(
									new RectangleF( bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2 ),
									SelectedBrush, 3.0f );
							}
						}
					}

					break;
				case DrawMode.Collapsed:
					c.Graphics.FillRectangle( bounds, CollapsedBrush );
					break;
				case DrawMode.Ghost:
					c.Graphics.FillRectangle( bounds, Color.FromArgb( 128, color ) );
					bool showRight = ( drawMode & DrawMode.ResizeRight ) != 0;
					float x = showRight ? bounds.Right : bounds.Left;
					c.Graphics.DrawText(
						GetXPositionString( x, c ),
						c.TextFormat,
						new PointF( x, bounds.Bottom - c.FontHeight ),
						TextBrush );
					break;
				case DrawMode.Invalid:
					c.Graphics.FillRectangle( bounds, InvalidBrush );
					break;
			}
		}

        /// <summary>
        /// Draws a key</summary>
        /// <param name="key">Key</param>
        /// <param name="bounds">Bounding rectangle, computed during layout phase</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected override void Draw(IKey key, RectangleF bounds, DrawMode drawMode, Context c)
        {
			LuaScript luaScript = key as LuaScript;
			if ( luaScript != null )
			{
				Color color = key.Color;
				bounds.Width = bounds.Height = KeySize; // key is always square, fixed size

				switch ( drawMode & DrawMode.States )
				{
					case DrawMode.Normal:
						//c.Graphics.FillEllipse( bounds, color );
						c.Graphics.FillRectangle( bounds, color );

						//m_picoTextBrush.Color = Color.Red;
						//c.Graphics.DrawText(
						//	luaScript.Description,
						//	c.TextFormat,
						//	new PointF( bounds.Right + 8, bounds.Y ),
						//	m_picoTextBrush );

						if ( ( drawMode & DrawMode.Selected ) != 0 )
						{
							//D2dAntialiasMode originalAntiAliasMode = c.Graphics.AntialiasMode;
							//c.Graphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
							//c.Graphics.DrawEllipse(
							//	new D2dEllipse(
							//		new PointF( bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.5f ),
							//		bounds.Width * 0.5f, bounds.Height * 0.5f ),
							//	SelectedBrush, 3.0f );
							c.Graphics.DrawRectangle( bounds, SelectedBrush, 3.0f );
							//c.Graphics.DrawEllipse(
							//	new D2dEllipse(
							//		new PointF( bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.5f ),
							//		bounds.Width * 0.5f, bounds.Height * 0.5f ),
							//	SelectedBrush, 3.0f ); 
							//c.Graphics.AntialiasMode = originalAntiAliasMode;
						}
						break;
					case DrawMode.Collapsed:
						//c.Graphics.FillEllipse( bounds, CollapsedBrush );
						c.Graphics.FillRectangle( bounds, CollapsedBrush );
						break;
					case DrawMode.Ghost:
						//c.Graphics.FillEllipse( bounds, Color.FromArgb( 128, color ) );
						c.Graphics.FillRectangle( bounds, Color.FromArgb( 128, color ) );
						c.Graphics.DrawText(
							GetXPositionString( bounds.Left + KeySize * 0.5f, c ),
							c.TextFormat,
							new PointF( bounds.Right + 16, bounds.Y ),
							m_picoTextBrush );
						break;
					case DrawMode.Invalid:
						//c.Graphics.FillEllipse( bounds, InvalidBrush );
						c.Graphics.FillRectangle( bounds, InvalidBrush );
						break;
				}
			}
			else
			{
				base.Draw( key, bounds, drawMode, c );
			}
        }

		protected void DrawCurves( IInterval interval, ICurveSet curveSet, RectangleF bounds, Context c )
		{
			IList<ICurve> curveList = curveSet.Curves;
			if (curveList.Count == 0)
				return;

			ICurve curve = curveList[0];
			DrawCurve( interval, curve, bounds, c );
		}

		/// <summary>
		/// Transforms x and y-coordinates from client space to graph space</summary>      
		/// <param name="px">X-coordinate to convert</param>
		/// <param name="py">Y-coordinate to convert</param>
		/// <returns>Vec2F representing transformed point in graph space</returns>
		public Vec2F ClientToGraph( float px, float py )
		{
			Vec2F result = new Vec2F();
			result.X = (float) ((px - m_trans.X) / m_scale.X);
			result.Y = (float) ((py - m_trans.Y) / m_scale.Y);
			result.Y = 1 - result.Y;
			return result;
		}

		/// <summary>
		/// Transforms x-coordinate from client space to graph space</summary>   
		/// <param name="px">X-coordinate to be transformed</param>
		/// <returns>Transformed x-coordinate in graph space</returns>
		public float ClientToGraph( float px )
		{
			return (float) ((px - m_trans.X) / m_scale.X);
		}

		/// <summary>
		/// Transforms x and y-coordinates from graph space to client space</summary>        
		/// <param name="x">X-coordinate to be transformed</param>
		/// <param name="y">Y-coordinate to be transformed</param>
		/// <returns>Vec2F representing transformed x and y-coordinates in client space</returns>
		public Vec2F GraphToClient( float x, float y )
		{
			Vec2F result = new Vec2F();
			y = 1.0f - y;
			result.X = (float) (m_trans.X + x * m_scale.X);
			result.Y = (float) (m_trans.Y + y * m_scale.Y);
			//result.Y = (float)Math.Floor( result.Y );
			return result;
		}

		/// <summary>
		/// Transforms x-coordinate from graph space to client space</summary>        
		/// <param name="x">X-coordinate to be transformed</param>
		/// <returns>Transformed x-coordinate in client space</returns>
		public float GraphToClient( float x )
		{
			return (float) (m_trans.X + x * m_scale.X);
		}


		/// <summary>
		/// Computes indices for pre-last and post-first points on the left and right of the
		/// viewing rectangle.
		/// Set lIndex to -1 and rIndex to -2 to indicate that curve is completely panned 
		/// either to left or right of the viewing rectangle.        
		/// This method is used by picking and rendering.</summary>
		/// <param name="curve">Curve</param>
		/// <param name="lIndex">Left index</param>
		/// <param name="rIndex">Right index</param>
		private void ComputeIndices( ICurve curve, RectangleF bounds, out int lIndex, out int rIndex )
		{
			lIndex = -1;
			rIndex = -2;

			ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
			if ( points.Count == 0 )
				return;

			float leftgx = ClientToGraph( bounds.X );
			float rightgx = ClientToGraph( bounds.Right );

			if ( points[0].X >= rightgx )
				return;

			if ( points[points.Count - 1].X <= leftgx )
				return;

			// find the index of the control point that 
			// comes before the first visible control-point from left.            
			for ( int i = (points.Count - 1); i >= 0; i-- )
			{
				IControlPoint cp = points[i];
				lIndex = i;
				if ( cp.X < leftgx )
					break;
			}

			// find the index of the control-point that comes after last visible control-point 
			// from right.            
			for ( int i = lIndex; i < points.Count; i++ )
			{
				IControlPoint cp = points[i];
				rIndex = i;
				if ( cp.X > rightgx )
					break;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="curve"></param>
		/// <param name="bounds"></param>
		/// <param name="c"></param>
		/// <remarks>Implementation based on Sce.Atf.Controls.CurveEditing.CurveRenderer</remarks>
		protected void DrawCurve( IInterval interval, ICurve curve, RectangleF bounds, Context c )
		{
			ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
			if (points.Count < 1)
				return;

			// not all curves are supported right now
			//
			if ( curve.MinX != 0 || curve.MinY != 0 || curve.MaxY > 1 )
				throw new InvalidOperationException( "Only curves in range <0,inf>x<0,1> are supported!" );

			D2dGraphics g = c.Graphics;
			RectangleF clipBounds = g.ClipBounds;
			clipBounds.X = HeaderWidth;
			clipBounds.Width -= HeaderWidth;

			m_trans = new PointF( bounds.X, bounds.Y );
			m_scale = new PointF( bounds.Width / curve.MaxX, bounds.Height / curve.MaxY );

			RectangleF boundsIntersected = RectangleF.Intersect( bounds, clipBounds );

			float client0 = GraphToClient( 0 );
			float client1 = GraphToClient( 1 );
			float clientPixelsPerUnit = client1 - client0;

			float x0 = ClientToGraph( boundsIntersected.X );
			float x1 = ClientToGraph( boundsIntersected.Right );
			IControlPoint fpt = points[0];
			IControlPoint lpt = points[points.Count - 1];
			// step is size of a single pixel in graph space
			// just imagine proportion:
			// Curve's length	-	bounds.Width
			// step				-	1
			//
			float step = curve.MaxX / bounds.Width;
			//float nSteps = curve.MaxX / step;
			float nSteps = bounds.Width;
			List<PointF> pointList = new List<PointF>( (int) nSteps );
			ICurveEvaluator cv = CurveUtils.CreateCurveEvaluator( curve );
			PointF scrPt = new PointF();

			m_fillLinearGradientBrush.StartPoint = bounds.Location;
			m_fillLinearGradientBrush.EndPoint = new PointF( bounds.X, bounds.Bottom );

			g.PushAxisAlignedClip( bounds );

			float strokeWidth = 2;

			// just try pick line color to be different from the background
			// it's just hacked, perceptually ok formula:)
			//
			Color color = interval.Color;
			float h = color.GetHue();
			float s = color.GetSaturation();
			float b = color.GetBrightness();
			Color lineColor = ColorUtil.FromAhsb( color.A, h + 180.0f, MathUtil.Min<float>(s + 0.5f, 1.0f), 1.0f - b * 0.5f );

			int nStepsMade = 0;

			// draw pre infinity
			if ( fpt.X > x0 )
			{
				float start = x0;
				float end = Math.Min( fpt.X, x1 );
				float rangeX = end - start;
				for (float x = 0; x < rangeX; x += step)
				{
					++nStepsMade;
					float xv = start + x;
					float y = cv.Evaluate( xv );
					scrPt = GraphToClient( xv, y );
					//scrPt.Y = MathUtil.Clamp( scrPt.Y, minY, maxY );
					pointList.Add( scrPt );
				}
				scrPt = GraphToClient( end, cv.Evaluate( end ) );
				//scrPt.Y = MathUtil.Clamp( scrPt.Y, minY, maxY );
				pointList.Add( scrPt );
				if (pointList.Count > 1)
				{
					PointF firstPoint = pointList[0];
					PointF lastPoint = pointList[pointList.Count-1];
					pointList.Add( new PointF( lastPoint.X, bounds.Bottom ) );
					pointList.Add( new PointF( firstPoint.X, bounds.Bottom ) );

					//g.FillPolygon( pointList, Color.Black );
					g.FillPolygon( pointList, m_fillLinearGradientBrush );

					g.DrawLines( pointList.GetRange( 0, pointList.Count - 2 ), lineColor, strokeWidth );
				}
			}

            // draw actual 
			if ( (fpt.X > x0 || lpt.X > x0) && (fpt.X < x1 || lpt.X < x1) )
			{
				//int leftIndex = 0;
				//int rightIndex = points.Count - 1;
				int leftIndex;
				int rightIndex;
				ComputeIndices( curve, boundsIntersected, out leftIndex, out rightIndex );
				if ( curve.CurveInterpolation == InterpolationTypes.Linear )
				{
					for ( int i = leftIndex; i < rightIndex; i++ )
					{
						IControlPoint p1 = points[i];
						IControlPoint p2 = points[i + 1];
						PointF cp1 = GraphToClient( p1.X, p1.Y );
						PointF cp2 = GraphToClient( p2.X, p2.Y );
						//g.DrawLine( cp1.X, cp1.Y, cp2.X, cp2.Y, Color.Black, strokeWidth );

						pointList.Clear();
						pointList.Add( new PointF( cp1.X, cp1.Y ) );
						pointList.Add( new PointF( cp2.X, cp2.Y ) );
						pointList.Add( new PointF( cp2.X, bounds.Bottom ) );
						pointList.Add( new PointF( cp1.X, bounds.Bottom ) );
						g.FillPolygon( pointList, m_fillLinearGradientBrush );

						g.DrawLines( pointList.GetRange( 0, pointList.Count - 2 ), lineColor, strokeWidth );
					}
				}
				else
				{
					for ( int i = leftIndex; i < rightIndex; i++ )
					{
						IControlPoint p1 = points[i];
						IControlPoint p2 = points[i + 1];
						if ( p1.TangentOutType == CurveTangentTypes.Stepped )
						{
							PointF cp1 = GraphToClient( p1.X, p1.Y );
							PointF cp2 = GraphToClient( p2.X, p2.Y );
							//g.DrawLine( cp1.X, cp1.Y, cp2.X, cp1.Y, Color.Black, strokeWidth );
							//g.DrawLine( cp2.X, cp1.Y, cp2.X, cp2.Y, Color.Black, strokeWidth );

							pointList.Clear();
							pointList.Add( new PointF( cp1.X, cp1.Y ) );
							pointList.Add( new PointF( cp2.X, cp1.Y ) );
							pointList.Add( new PointF( cp2.X, bounds.Bottom ) );
							pointList.Add( new PointF( cp1.X, bounds.Bottom ) );
							g.FillPolygon( pointList, m_fillLinearGradientBrush );

							g.DrawLines( pointList.GetRange( 0, pointList.Count - 2 ), lineColor, strokeWidth );
						}
						else if ( p1.TangentOutType != CurveTangentTypes.SteppedNext )
						{
							float start = Math.Max( p1.X, x0 );
							float end = Math.Min( p2.X, x1 );
							pointList.Clear();
							float rangeX = end - start;
							for ( float x = 0; x < rangeX; x += step )
							{
								++nStepsMade;
								float xv = start + x;
								float y = cv.Evaluate( xv );
								scrPt = GraphToClient( xv, y );
								//scrPt.Y = MathUtil.Clamp( scrPt.Y, minY, maxY );
								pointList.Add( scrPt );
							}
							scrPt = GraphToClient( end, cv.Evaluate( end ) );
							//scrPt.Y = MathUtil.Clamp( scrPt.Y, minY, maxY );
							pointList.Add( scrPt );
							if ( pointList.Count > 1 )
							{
								//g.DrawLines( pointList, lineColor, strokeWidth );

								PointF firstPoint = pointList[0];
								PointF lastPoint = pointList[pointList.Count-1];
								pointList.Add( new PointF( lastPoint.X, bounds.Bottom ) );
								pointList.Add( new PointF( firstPoint.X, bounds.Bottom ) );
								//pointList.Add( new PointF( firstPoint.X, firstPoint.Y) );
								//g.DrawPolygon( pointList, Color.Black, strokeWidth );
								//g.FillPolygon( pointList, Color.Black );
								g.FillPolygon( pointList, m_fillLinearGradientBrush );

								g.DrawLines( pointList.GetRange(0, pointList.Count - 2), lineColor, strokeWidth );
							}
						}
					}// for (int i = leftIndex; i < rightIndex; i++)
				}
			}

			//draw post-infinity.
			if (lpt.X < x1)
			{
				pointList.Clear();
				float start = Math.Max( x0, lpt.X );
				float end = x1;
				float rangeX = end - start;
				for (float x = 0; x < rangeX; x += step)
				{
					++nStepsMade;
					float xv = start + x;
					float y = cv.Evaluate( xv );
					scrPt = GraphToClient( xv, y );
					//scrPt.Y = MathUtil.Clamp( scrPt.Y, minY, maxY );
					pointList.Add( scrPt );
				}
				scrPt = GraphToClient( end, cv.Evaluate( end ) );
				//scrPt.Y = MathUtil.Clamp( scrPt.Y, minY, maxY );
				pointList.Add( scrPt );
				if (pointList.Count > 1)
				{
					PointF firstPoint = pointList[0];
					PointF lastPoint = pointList[pointList.Count-1];
					pointList.Add( new PointF( lastPoint.X, bounds.Bottom ) );
					pointList.Add( new PointF( firstPoint.X, bounds.Bottom ) );
					//g.FillPolygon( pointList, Color.Black );
					g.FillPolygon( pointList, m_fillLinearGradientBrush );

					g.DrawLines( pointList.GetRange( 0, pointList.Count - 2 ), lineColor, strokeWidth );
				}
			}

			g.PopAxisAlignedClip();
		}

		/// <summary>
		/// The brush used for drawing text on intervals, keys, and markers</summary>
		protected D2dSolidColorBrush m_picoTextBrush;
		private D2dLinearGradientBrush m_fillLinearGradientBrush;
		//private D2dStrokeStyle m_curveStrokeStyle;
		private bool m_picoDisposed;

		private PointF m_trans;
		private PointF m_scale;
	}
}
