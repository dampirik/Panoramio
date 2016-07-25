using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Panoramio.Server
{
    public static class ConnectHelper
    {
        public static TimeSpan ClientTimeout = new TimeSpan(0, 0, 0, 15, 0);

        public static Task<byte[]> LoadData(string url, CancellationToken cancellationToken)
        {
            var taskCompletion = new TaskCompletionSource<byte[]>();

            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            else
            {
                cancellationToken.Register(() =>
                {
                    taskCompletion.TrySetException(new TaskCanceledException("Execute canceled!"));
                });
            }
            
            Task.Run(() =>
            {
                try
                {
                    var request = WebRequest.CreateHttp(url);
                    request.ContinueTimeout = (int)ClientTimeout.TotalMilliseconds;
                    request.BeginGetResponse(callback =>
                    {
                        var requestAsync = callback.AsyncState as HttpWebRequest;
                        if (requestAsync != null)
                        {
                            try
                            {
                                var response = (HttpWebResponse)requestAsync.EndGetResponse(callback);
                                using (var streamResponse = response.GetResponseStream())
                                {
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        var buffer = new byte[2048];
                                        int bytesRead;
                                        while ((bytesRead = streamResponse.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            memoryStream.Write(buffer, 0, bytesRead);
                                        }
                                        taskCompletion.TrySetResult(memoryStream.ToArray());
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                taskCompletion.TrySetException(ex);
                            }
                        }
                        else
                        {
                            taskCompletion.TrySetResult(null);
                        }
                    }, request);
                }
                catch (Exception ex)
                {
                    taskCompletion.TrySetException(ex);
                }
            }, cancellationToken);

            return taskCompletion.Task;
        }
    }
}
