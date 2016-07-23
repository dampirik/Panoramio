using System;
using Windows.Devices.Geolocation;

namespace Panoramio.Helpers
{
    public static class Helper
    {
        public static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public static double CalculationByDistance(Geopoint point1, Geopoint point2)
        {
            var longitude1 = point1.Position.Longitude;
            var latitude1 = point1.Position.Latitude;
            var longitude2 = point2.Position.Longitude;
            var latitude2 = point2.Position.Latitude;

            var Radius = 6371; //radius of earth in Km
            var lat1 = latitude1;
            var lat2 = latitude2;
            var lon1 = longitude1;
            var lon2 = longitude2;
            var dLat = ConvertToRadians(lat2 - lat1);
            var dLon = ConvertToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ConvertToRadians(lat1)) *
                    Math.Cos(ConvertToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Asin(Math.Sqrt(a));
            var valueResult = Radius * c;

            return valueResult;
        }
    }
}
