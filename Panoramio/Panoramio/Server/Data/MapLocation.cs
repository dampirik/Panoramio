using Newtonsoft.Json;

namespace Panoramio.Server.Data
{
    public class MapLocation
    {
        [JsonProperty(PropertyName = "lon")]
        public double Longitude { get; set; }

        [JsonProperty(PropertyName = "lat")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "panoramio_zoom")]
        public int PanoramioZoom { get; set; }
    }
}
