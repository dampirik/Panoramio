using Windows.Devices.Geolocation;
using Panoramio.UserControls;

namespace Panoramio.Models
{
    public class MapItemModel: IMapItem
    {
        public Geopoint Location { get; set; }

        public int Id { get; set; }
    }
}
