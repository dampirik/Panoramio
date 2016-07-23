using Windows.Devices.Geolocation;

namespace Panoramio.PushpinControl
{
    public interface IPushpinModel
    {
        Geopoint Location { get; }
    }
}
