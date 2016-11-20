using System.Drawing;

namespace misz
{
	public class ColorHelp
	{
		public static Color fromRGB( float r, float g, float b )
		{
			float rc = Sce.Atf.MathUtil.Clamp<float>( r, 0, 1 );
			float gc = Sce.Atf.MathUtil.Clamp<float>( g, 0, 1 );
			float bc = Sce.Atf.MathUtil.Clamp<float>( b, 0, 1 );

			int ri = (int)( rc * 255 );
			int gi = (int)( gc * 255 );
			int bi = (int)( bc * 255 );

			return Color.FromArgb( 255, ri, gi, bi );
		}

        public static void ConvertColor( Color color, float intensity, out float red, out float green, out float blue )
        {
            float m = intensity * (1.0f / 255.0f);
            red = color.R * m;
            green = color.G * m;
            blue = color.B * m;
        }
    }
}
