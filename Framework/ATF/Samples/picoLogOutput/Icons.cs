using System.Drawing;
using System.Windows.Forms;

namespace pico.LogOutput
{
	public class Icons
	{
		public Icons()
		{
			m_errorImage = _ScaleIcon( SystemIcons.Error );
			m_warningImage = _ScaleIcon( SystemIcons.Warning );
			m_infoImage = _ScaleIcon( SystemIcons.Information );
			m_debugImage = _ScaleIcon( SystemIcons.Application );
		}

		public Image ErrorIcon
		{
			get { return m_errorImage; }
		}

		public Image WarningIcon
		{
			get { return m_warningImage; }
		}

		public Image InfoIcon
		{
			get { return m_infoImage; }
		}

		public Image DebugIcon
		{
			get { return m_debugImage; }
		}

		private Image _ScaleIcon( Icon sourceIcon )
		{
			Size iconSize = SystemInformation.SmallIconSize;
			Bitmap bitmap = new Bitmap( iconSize.Width, iconSize.Height );

			using (Graphics g = Graphics.FromImage( bitmap ))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				g.DrawImage( sourceIcon.ToBitmap(), new Rectangle( Point.Empty, iconSize ) );
			}

			return bitmap;
		}

		private Image m_errorImage;
		private Image m_warningImage;
		private Image m_infoImage;
		private Image m_debugImage;
	}
}
