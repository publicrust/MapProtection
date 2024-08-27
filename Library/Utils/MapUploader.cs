using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Library.Utils
{
    public static class MapUploader
    {
        private static readonly HttpClient Http = new HttpClient();

        public static async Task<string> UploadMapAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл карты не найден.", filePath);

            using (var fileStream = File.OpenRead(filePath))
            {
                return await UploadMapImplAsync(fileStream, Path.GetFileName(filePath));
            }
        }

        public static async Task<string> UploadMapAsync(Stream stream, string mapFileName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrWhiteSpace(mapFileName))
                throw new ArgumentNullException(nameof(mapFileName));

            return await UploadMapImplAsync(stream, mapFileName);
        }

        private static async Task<string> UploadMapImplAsync(Stream stream, string mapFileName)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new ArgumentException("Stream must be readable and seekable.", nameof(stream));

            string requestUri = "https://api.facepunch.com/api/public/rust-map-upload/" + mapFileName;
            int retries = 0;

            while (retries < 10)
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, requestUri))
                {
                    request.Content = new StreamContent(stream);

                    try
                    {
                        HttpResponseMessage response = await Http.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            if (string.IsNullOrWhiteSpace(responseBody) || !responseBody.StartsWith("http"))
                            {
                                throw new Exception("Backend sent an invalid success response when uploading the map.");
                            }
                            return responseBody;
                        }
                        else if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                        {
                            string error = await response.Content.ReadAsStringAsync();
                            return null;
                        }
                        else
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }
                    catch (Exception)
                    {
                        await Task.Delay(1000 + retries * 5000);
                        retries++;
                    }
                }
            }

            return null;
        }

        public static string UploadMap(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл карты не найден.", filePath);

            using (var fileStream = File.OpenRead(filePath))
            {
                return UploadMapImpl(fileStream, Path.GetFileName(filePath));
            }
        }

        public static string UploadMap(Stream stream, string mapFileName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrWhiteSpace(mapFileName))
                throw new ArgumentNullException(nameof(mapFileName));

            return UploadMapImpl(stream, mapFileName);
        }

        private static string UploadMapImpl(Stream stream, string mapFileName)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new ArgumentException("Stream must be readable and seekable.", nameof(stream));

            string requestUri = "https://api.facepunch.com/api/public/rust-map-upload/" + mapFileName;
            int retries = 0;

            while (retries < 10)
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, requestUri))
                {
                    request.Content = new StreamContent(stream);

                    try
                    {
                        HttpResponseMessage response = Http.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = response.Content.ReadAsStringAsync().Result;
                            if (string.IsNullOrWhiteSpace(responseBody) || !responseBody.StartsWith("http"))
                            {
                                throw new Exception("Backend sent an invalid success response when uploading the map.");
                            }
                            return responseBody;
                        }
                        else if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                        {
                            string error = response.Content.ReadAsStringAsync().Result;
                            return null;
                        }
                        else
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }
                    catch (Exception)
                    {
                        Task.Delay(1000 + retries * 5000).Wait();
                        retries++;
                    }
                }
            }

            return null;
        }
    }
}
