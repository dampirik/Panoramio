using System;
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
        public static async Task<Photos> GetPhotos(CancellationToken cancellationToken)
        {
            var query = $"set={"public"}&from={"0"}&to={"20"}&minx={"-180"}&miny={"-90"}&maxx={"180"}&maxy={"90"}&size={"medium"}&mapfilter={"true"}";
            byte[] data = null;
            try
            {
                data = await ConnectHelper.LoadData(query, cancellationToken);
            }
            catch (TaskCanceledException ex)
            {
                
            }
            catch (Exception ex)
            {
                var type = ex.GetType();

                //нет инета
                //{Name = "WebException" FullName = "System.Net.WebException"}
                //{"An error occurred while sending the request. The text associated with this error code could not be found.\r\n\r\nThe server name or address could not be resolved\r\n"}

                //не корректный запрос
                //{Name = "WebException" FullName = "System.Net.WebException"}
                //{"The remote server returned an error: (400) Bad Request."}
            }

            //"{\"count\":0,\"has_more\":false,\"photos\":[]}"

            var json = Encoding.UTF8.GetString(data);
            var result = JsonConvert.DeserializeObject<Photos>(json);



            return result;
        }
    }
}
