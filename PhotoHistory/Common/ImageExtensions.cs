using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace PhotoHistory
{
	public static class ImageExtensions
	{
		public static double? GPSLatitude(this Image image)
		{
			return GetImageGPSProperty( image, 0x02 );
		}

		public static double? GPSLongitude(this Image image)
		{
			return GetImageGPSProperty( image, 0x04 );
		}

		private static double? GetImageGPSProperty(Image image, int property)
		{
			int[] pils = image.PropertyIdList;
			int index = Array.IndexOf( pils, property );
			if ( index == -1 )
				return null;

			PropertyItem pi = image.PropertyItems[index];

			double deg = BitConverter.ToUInt32( pi.Value, 0 );
			uint deg_div = BitConverter.ToUInt32( pi.Value, 4 );

			double min = BitConverter.ToUInt32( pi.Value, 8 );
			uint min_div = BitConverter.ToUInt32( pi.Value, 12 );

			double mmm = BitConverter.ToUInt32( pi.Value, 16 );
			uint mmm_div = BitConverter.ToUInt32( pi.Value, 20 );

			double m = 0;
			if ( deg_div != 0 || deg != 0 )
			{
				m = (deg / deg_div);
			}

			if ( min_div != 0 || min != 0 )
			{
				m = m + (min / min_div) / 60;
			}

			if ( mmm_div != 0 || mmm != 0 )
			{
				m = m + (mmm / mmm_div / 3600);
			}

			return m;
		}
	}
}