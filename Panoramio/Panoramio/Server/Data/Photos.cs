using Newtonsoft.Json;

namespace Panoramio.Server.Data
{
    /// <summary>
    /// {
    ///   "count": 773840,"photos": []
    /// }
    /// </summary>
    public class Photos
    {
        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "photos")]
        public PhotoItem[] PhotoItems { get; set; }

        [JsonProperty(PropertyName = "has_more")]
        public bool HasMore { get; set; }

        [JsonProperty(PropertyName = "map_location")]
        public MapLocation MapLocation { get; set; }
    }
}
