using Newtonsoft.Json;

namespace Panoramio.Server.Data
{
    /// <summary>
    ///  "photo_id": 532693,
    ///  "photo_title": "Wheatfield in afternoon light",
    ///  "photo_url": "http://www.panoramio.com/photo/532693",
    ///  "photo_file_url": "http://static2.bareka.com/photos/medium/532693.jpg",
    ///  "longitude": 11.280727,
    ///  "latitude": 59.643198,
    ///  "width": 500,
    ///  "height": 333,
    ///  "upload_date": "22 January 2007",
    ///  "owner_id": 39160,
    ///  "owner_name": "Snemann",
    ///  "owner_url": "http://www.panoramio.com/user/39160",
    /// </summary>
    public class PhotoItem
    {
        [JsonProperty(PropertyName = "photo_id")]
        public int PhotoId { get; set; }

        [JsonProperty(PropertyName = "photo_title")]
        public string PhotoTitle { get; set; }

        [JsonProperty(PropertyName = "photo_url")]
        public string PhotoUrl { get; set; }

        [JsonProperty(PropertyName = "photo_file_url")]
        public string PhotoFileUrl { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        [JsonProperty(PropertyName = "upload_date")]
        public string UploadDate { get; set; }

        [JsonProperty(PropertyName = "owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty(PropertyName = "owner_name")]
        public string OwnerName { get; set; }

        [JsonProperty(PropertyName = "owner_url")]
        public string OwnerUrl { get; set; }
    }
}
