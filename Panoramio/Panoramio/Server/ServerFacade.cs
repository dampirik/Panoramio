using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Panoramio.Server.Data;

namespace Panoramio.Server
{
    /// <summary>
    /// http://www.panoramio.com/api/data/api.html
    /// </summary>
    public static class ServerFacade
    {
        public const string BaseApiUrl = "http://www.panoramio.com/map/get_panoramas.php?";

        public static async Task<Photos> GetPhotos(int from, int to, double minx, double miny, double maxx, double maxy, CancellationToken cancellationToken)
        {
            //var query = $"set={"public"}&from={"0"}&to={"20"}&minx={"-180"}&miny={"-90"}&maxx={"180"}&maxy={"90"}&size={"medium"}&mapfilter={"true"}";

            var sminx = minx.ToString("G8", CultureInfo.InvariantCulture);
            var sminy = miny.ToString("G8", CultureInfo.InvariantCulture);
            var smaxx = maxx.ToString("G8", CultureInfo.InvariantCulture);
            var smaxy = maxy.ToString("G8", CultureInfo.InvariantCulture);
            var query = $"set={"public"}&from={from}&to={to}&minx={sminx}&miny={sminy}&maxx={smaxx}&maxy={smaxy}&size={"medium"}&mapfilter={"true"}";

            var data = await ConnectHelper.LoadData(BaseApiUrl + query, cancellationToken);
            
            if (data == null)
                throw new NullReferenceException("data");

            var json = Encoding.UTF8.GetString(data);
            var result = JsonConvert.DeserializeObject<Photos>(json);
            
            return result;
        }
    }
}
