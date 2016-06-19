//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Sce.Atf;
using Sce.Atf.Direct2D;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Adaptation;
using picoAnimClipEditor.DomNodeAdapters;

namespace picoAnimClipEditor
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
			//TrackBrush2 = graphics.CreateSolidBrush( Color.Green );
		}

		/// <summary>
		/// Disposes unmanaged resources</summary>
		/// <param name="disposing">Whether or not Dispose invoked this method</param>
		protected override void Dispose( bool disposing )
		{
			if (!m_picoDisposed)
			{
				if (disposing)
				{
					m_picoTextBrush.Dispose();
					//TrackBrush2.Dispose();
				}

				// dispose any unmanaged resources.

				m_picoDisposed = true;
			}
			// always call base regardles.
			base.Dispose( disposing );
		}

		/// <summary>
		/// Finds the list of hits on timeline objects that intersect the given rectangle
		/// pico: this is overriden to block selection of IntervalAnim.
		/// By not permitting selection, we avoid various problems with editing this interval (like, changing length, deleting, etc)
		/// </summary>
		/// <param name="timeline">Timeline</param>
		/// <param name="pickRect">Picking rectangle</param>
		/// <param name="transform">Transform taking timeline objects to display coordinates</param>
		/// <param name="clientRectangle">Bounds of displayed area of timeline, in screen space</param>
		/// <returns>List of hits on timeline objects that intersect the given rectangle</returns>
		public override IList<HitRecord> Pick(
			ITimeline timeline,
			RectangleF pickRect,
			Matrix transform,
			Rectangle clientRectangle )
		{
			IList<HitRecord> result = base.Pick( timeline, pickRect, transform, clientRectangle );
			if ( result.Count == 0 )
				return result;

			foreach( HitRecord hr in result )
			{
				IntervalAnim ia = hr.HitObject.As<IntervalAnim>();
				if ( ia != null )
				{
					// return empty list
					//
					return new List<HitRecord>();
				}
			}

			return result;
		}

		///// <summary>
		///// Draws a group</summary>
		///// <param name="group">Group</param>
		///// <param name="bounds">Bounding rectangle, computed during layout phase</param>
		///// <param name="drawMode">Drawing mode</param>
		///// <param name="c">Drawing context</param>
		//protected override void Draw( IGroup group, RectangleF bounds, DrawMode drawMode, Context c )
		//{
		//	//switch (drawMode & DrawMode.States)
		//	//{
		//	//	case DrawMode.Normal:
		//	//	case DrawMode.Collapsed:
		//	//		GroupBrush.StartPoint = new PointF( 0, bounds.Top );
		//	//		GroupBrush.EndPoint = new PointF( 0, bounds.Bottom );
		//	//		c.Graphics.FillRectangle( bounds, GroupBrush );
		//	//		break;
		//	//	case DrawMode.Ghost:
		//	//		c.Graphics.FillRectangle( bounds, GhostGroupBrush );
		//	//		break;
		//	//}
		//}

		///// <summary>
		///// Gets the bounding rectangle for an interval, in timeline coordinates</summary>
		///// <param name="interval">Interval</param>
		///// <param name="trackTop">Top of track holding interval</param>
		///// <param name="c">Drawing context</param>
		///// <returns>Bounding rectangle for the interval, in timeline coordinates</returns>
		//protected RectangleF GetBounds( float Start, float Length, float trackTop, Context c )
		//{
		//	// Calculate the width, in timeline coordinates. If the group is expanded, then
		//	//  make sure that interval meets the minimum visible width requirement.
		//	float visibleWidth = Length;
		//	//if (interval.Track != null &&
		//	//	interval.Track.Group != null &&
		//	//	interval.Track.Group.Expanded)
		//	{
		//		float minimumTimelineUnits = c.PixelSize.Width * MinimumDrawnIntervalLength;
		//		visibleWidth = Math.Max( visibleWidth, minimumTimelineUnits );
		//	}

		//	return new RectangleF(
		//		Start,
		//		trackTop,
		//		visibleWidth,
		//		TrackHeight );
		//}


		///// <summary>
		///// Draws a track</summary>
		///// <param name="track">Track</param>
		///// <param name="bounds">Bounding rectangle, in screen space</param>
		///// <param name="drawMode">Drawing mode</param>
		///// <param name="c">Drawing context</param>
		//protected override void Draw( ITrack track, RectangleF boundsOrig, DrawMode drawMode, Context c )
		//{
		//	float eventTop = 0;// boundsOrig.Top;
		//	float trackBottom = eventTop;

		//	//RectangleF bounds = GetBounds( interval, eventTop, c );
		//	RectangleF bounds = GetBounds( 0, 6000, eventTop, c );
		//	trackBottom = Math.Max( trackBottom, bounds.Bottom );
		//	bounds = GdiUtil.Transform( c.Transform, bounds );

		//	RectangleF canvasBounds = c.ClientRectangle; //clipBounds minus the left-side header
		//	canvasBounds.X = HeaderWidth;
		//	canvasBounds.Width -= HeaderWidth;

		//	if (bounds.IntersectsWith( canvasBounds ))
		//	{
		//		switch (drawMode & DrawMode.States)
		//		{
		//			case DrawMode.Normal:
		//				//c.Graphics.DrawRectangle( bounds, TrackBrush2 );
		//				c.Graphics.FillRectangle( bounds, TrackBrush2 );
		//				break;
		//			case DrawMode.Collapsed:
		//				break;
		//			case DrawMode.Ghost:
		//				c.Graphics.FillRectangle( bounds, GhostTrackBrush );
		//				break;
		//		}
		//	}
		//}

		///// <summary>
		///// Draws an interval</summary>
		///// <param name="interval">Interval</param>
		///// <param name="bounds">Bounding rectangle, in screen space</param>
		///// <param name="drawMode">Drawing mode</param>
		///// <param name="c">Drawing context</param>
		//protected override void Draw( IInterval interval, RectangleF bounds, DrawMode drawMode, Context c )
		//{
		//	Color color = interval.Color;
		//	switch ( drawMode & DrawMode.States )
		//	{
		//		case DrawMode.Normal:
		//			RectangleF realPart = new RectangleF(
		//				bounds.X,
		//				bounds.Y,
		//				GdiUtil.TransformVector( c.Transform, interval.Length ),
		//				bounds.Height );
		//			bool hasTail = realPart.Width < MinimumDrawnIntervalLength;

		//			float h = color.GetHue();
		//			float s = color.GetSaturation();
		//			float b = color.GetBrightness();
		//			Color endColor = ColorUtil.FromAhsb( color.A, h, s * 0.3f, b );
		//			c.Graphics.FillRectangle(
		//				realPart,
		//				new PointF( 0, realPart.Top ), new PointF( 0, realPart.Bottom ),
		//				color, endColor );

		//			if ( hasTail )
		//			{
		//				endColor = ColorUtil.FromAhsb( 64, h, s * 0.3f, b );
		//				RectangleF tailPart = new RectangleF(
		//					realPart.Right,
		//					bounds.Y,
		//					bounds.Width - realPart.Width,
		//					bounds.Height );
		//				c.Graphics.FillRectangle( tailPart, endColor );
		//			}

		//			// pico
		//			// add line at the left border of interval
		//			// this helps to notice where intervals begin/end when they are tightly packed next to each other
		//			//
		//			Color lineColor = ColorUtil.FromAhsb( endColor.A, 360.0f - h, 1.0f, 0.5f );
		//			c.Graphics.DrawLine( new PointF( realPart.Left, realPart.Top ), new PointF( realPart.Left, realPart.Bottom ), lineColor, 2 );

		//			if ( color.R + color.G + color.B < 3 * 160 )
		//				TextBrush.Color = SystemColors.HighlightText;
		//			else
		//				TextBrush.Color = SystemColors.WindowText;

		//			c.Graphics.DrawText( interval.Name, c.TextFormat, bounds.Location, TextBrush );

		//			if ( ( drawMode & DrawMode.Selected ) != 0 )
		//			{
		//				c.Graphics.DrawRectangle(
		//					new RectangleF( bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2 ),
		//					SelectedBrush, 3.0f );
		//			}
		//			break;
		//		case DrawMode.Collapsed:
		//			c.Graphics.FillRectangle( bounds, CollapsedBrush );
		//			break;
		//		case DrawMode.Ghost:
		//			c.Graphics.FillRectangle( bounds, Color.FromArgb( 128, color ) );
		//			bool showRight = ( drawMode & DrawMode.ResizeRight ) != 0;
		//			float x = showRight ? bounds.Right : bounds.Left;
		//			c.Graphics.DrawText(
		//				GetXPositionString( x, c ),
		//				c.TextFormat,
		//				new PointF( x, bounds.Bottom - c.FontHeight ),
		//				TextBrush );
		//			break;
		//		case DrawMode.Invalid:
		//			c.Graphics.FillRectangle( bounds, InvalidBrush );
		//			break;
		//	}
		//}

		/// <summary>
		/// Draws a key</summary>
		/// <param name="key">Key</param>
		/// <param name="bounds">Bounding rectangle, computed during layout phase</param>
		/// <param name="drawMode">Drawing mode</param>
		/// <param name="c">Drawing context</param>
		protected override void Draw( IKey key, RectangleF bounds, DrawMode drawMode, Context c )
		{
			KeyTag keyTag = key.As<KeyTag>();
			if ( keyTag != null )
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

						if ( (drawMode & DrawMode.Selected) != 0 )
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

		/// <summary>
		/// The brush used for drawing text on intervals, keys, and markers</summary>
		protected D2dSolidColorBrush m_picoTextBrush;
		private bool m_picoDisposed;

		///// <summary>
		///// The brush used for drawing tracks</summary>
		//protected D2dBrush TrackBrush2;
	}
}
