using System;

namespace GeoUtility.GeoSystem
{
    /// <summary>
    /// Координатный класс СК-42 (Пулково 1942).
    /// Система координат на эллипсоиде Красовского 1940 с проекцией Гаусса-Крюгера в 6-градусных зонах.
    /// </summary>
    public class SK42
    {
        // Krasovsky 1940 ellipsoid parameters
        private const double KRA_A = 6378245.0;                          // semi-major axis
        private const double KRA_F = 1.0 / 298.3;                       // flattening
        private const double KRA_B = KRA_A * (1.0 - KRA_F);             // semi-minor axis
        private const double KRA_E2 = 2.0 * KRA_F - KRA_F * KRA_F;     // eccentricity squared
        private const double KRA_EP2 = KRA_E2 / (1.0 - KRA_E2);        // second eccentricity squared

        // WGS-84 ellipsoid parameters
        private const double WGS_A = 6378137.0;
        private const double WGS_F = 1.0 / 298.257223563;
        private const double WGS_B = WGS_A * (1.0 - WGS_F);
        private const double WGS_E2 = 2.0 * WGS_F - WGS_F * WGS_F;

        // Molodensky datum shift: WGS-84 -> SK-42 (Krasovsky)
        private const double DX = 23.92;
        private const double DY = -141.27;
        private const double DZ = -80.9;
        private const double DA = KRA_A - WGS_A;   // difference in semi-major axis
        private const double DF = KRA_F - WGS_F;   // difference in flattening

        /// <summary>X (easting) в метрах.</summary>
        public double X { get; set; }

        /// <summary>Y (northing) в метрах.</summary>
        public double Y { get; set; }

        /// <summary>Номер зоны (6-градусной).</summary>
        public int Zone { get; set; }

        /// <summary>Конструктор по умолчанию.</summary>
        public SK42() { }

        /// <summary>Конструктор с параметрами.</summary>
        public SK42(double x, double y, int zone)
        {
            X = x;
            Y = y;
            Zone = zone;
        }

        /// <summary>
        /// Преобразование координат из WGS-84 (широта/долгота в градусах) в СК-42.
        /// </summary>
        /// <param name="lat">Широта WGS-84, градусы.</param>
        /// <param name="lon">Долгота WGS-84, градусы.</param>
        /// <returns>Объект SK42 с координатами X, Y и номером зоны.</returns>
        public static SK42 FromWGS84(double lat, double lon)
        {
            // Step 1: Molodensky transform WGS-84 -> Krasovsky
            double latRad = lat * Math.PI / 180.0;
            double lonRad = lon * Math.PI / 180.0;

            double sinLat = Math.Sin(latRad);
            double cosLat = Math.Cos(latRad);
            double sinLon = Math.Sin(lonRad);
            double cosLon = Math.Cos(lonRad);
            double sin2Lat = sinLat * sinLat;

            double Rn = WGS_A / Math.Sqrt(1.0 - WGS_E2 * sin2Lat);
            double Rm = WGS_A * (1.0 - WGS_E2) / Math.Pow(1.0 - WGS_E2 * sin2Lat, 1.5);

            double dLat = ((-DX * sinLat * cosLon - DY * sinLat * sinLon + DZ * cosLat)
                           + DA * (Rn * WGS_E2 * sinLat * cosLat) / WGS_A
                           + DF * (Rm / (1.0 - WGS_F) + Rn * (1.0 - WGS_F)) * sinLat * cosLat)
                          / (Rm + 0.0);  // dLat in radians (divide by Rm)

            // Correct: divide by (Rm + h), assume h=0
            dLat = ((-DX * sinLat * cosLon - DY * sinLat * sinLon + DZ * cosLat)
                    + DA * (Rn * WGS_E2 * sinLat * cosLat) / WGS_A
                    + DF * (Rm / (1.0 - WGS_F) + Rn * (1.0 - WGS_F)) * sinLat * cosLat)
                   / Rm;

            double dLon = (-DX * sinLon + DY * cosLon) / (Rn * cosLat);

            double kraLat = latRad + dLat;  // latitude on Krasovsky, radians
            double kraLon = lonRad + dLon;  // longitude on Krasovsky, radians

            // Step 2: Gauss-Krueger projection (6-degree zones) on Krasovsky ellipsoid
            double lonDeg = kraLon * 180.0 / Math.PI;
            int zone = (int)Math.Floor(lonDeg / 6.0) + 1;
            double centralMeridian = zone * 6.0 - 3.0;
            double l = kraLon - centralMeridian * Math.PI / 180.0; // delta longitude in radians

            double latK = kraLat;
            double sinLatK = Math.Sin(latK);
            double cosLatK = Math.Cos(latK);
            double tanLatK = Math.Tan(latK);
            double sin2LatK = sinLatK * sinLatK;

            double cos2 = cosLatK * cosLatK;
            double cos3 = cos2 * cosLatK;
            double cos4 = cos3 * cosLatK;
            double cos5 = cos4 * cosLatK;
            double tan2 = tanLatK * tanLatK;
            double tan4 = tan2 * tan2;

            double eta2 = KRA_EP2 * cos2;

            // Radius of curvature in prime vertical
            double N = KRA_A / Math.Sqrt(1.0 - KRA_E2 * sin2LatK);

            // Meridian arc length (series expansion)
            double e2 = KRA_E2;
            double e4 = e2 * e2;
            double e6 = e4 * e2;
            double e8 = e4 * e4;
            double ep2 = KRA_EP2;
            double ep4 = ep2 * ep2;
            double ep6 = ep4 * ep2;
            double ep8 = ep4 * ep4;

            double C = KRA_A / (1.0 - KRA_E2);  // = POL (polar radius of curvature)
            double A0 = C * (Math.PI / 180.0) * (1.0 - 3.0 * ep2 / 4.0 + 45.0 * ep4 / 64.0 - 175.0 * ep6 / 256.0 + 11025.0 * ep8 / 16384.0);
            double A2 = C * (-3.0 * ep2 / 8.0 + 15.0 * ep4 / 32.0 - 525.0 * ep6 / 1024.0 + 2205.0 * ep8 / 4096.0);
            double A4 = C * (15.0 * ep4 / 256.0 - 105.0 * ep6 / 1024.0 + 2205.0 * ep8 / 16384.0);
            double A6 = C * (-35.0 * ep6 / 3072.0 + 315.0 * ep8 / 12288.0);

            double latDeg = latK * 180.0 / Math.PI;
            double M = A0 * latDeg + A2 * Math.Sin(2.0 * latK) + A4 * Math.Sin(4.0 * latK) + A6 * Math.Sin(6.0 * latK);

            double l2 = l * l;
            double l3 = l2 * l;
            double l4 = l3 * l;
            double l5 = l4 * l;

            // Northing (Y in Russian convention, but we use Y for northing)
            double northing = M
                              + N * cosLatK * cosLatK * tanLatK * l2 / 2.0
                              + N * cos4 * tanLatK * (5.0 - tan2 + 9.0 * eta2) * l4 / 24.0;

            // Easting
            double easting = N * cosLatK * l
                             + N * cos3 * (1.0 - tan2 + eta2) * l3 / 6.0
                             + N * cos5 * (5.0 - 18.0 * tan2 + tan4) * l5 / 120.0
                             + zone * 1e6 + 500000.0;

            return new SK42(easting, northing, zone);
        }

        /// <summary>
        /// Форматированная строка координат СК-42.
        /// </summary>
        public override string ToString()
        {
            return string.Format("зона {0}: X={1:0}, Y={2:0}", Zone, X, Y);
        }
    }
}
