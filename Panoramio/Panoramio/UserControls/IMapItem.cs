using Windows.Devices.Geolocation;

namespace Panoramio.UserControls
{
    public interface IMapItem
    {
        int Id { get; }

        Geopoint Location { get; }

        string PhotoUrl { get; }
    }
}
