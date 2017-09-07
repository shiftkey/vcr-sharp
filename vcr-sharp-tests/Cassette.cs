using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VcrSharp.Tests
{
    public class Cassette
    {
        private readonly string cassettePath;
        private List<CachedRequestResponse> cache;

        public Cassette(string cassettePath)
        {
            this.cassettePath = cassettePath;
        }

        async Task SetupCache()
        {
            if (File.Exists(cassettePath))
            {
                var task = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<CachedRequestResponseArray>(File.ReadAllText(cassettePath)));
                var contents = await task;
                cache = new List<CachedRequestResponse>(contents.http_interactions ?? Array.Empty<CachedRequestResponse>());
            }
            else
            {
                cache = new List<CachedRequestResponse>();
            }
        }

        static bool MatchesRequest(CachedRequestResponse cached, HttpRequestMessage request)
        {
            var method = request.Method.Method;
            var uri = request.RequestUri;

            return string.Equals(cached.Request.Method, method, StringComparison.OrdinalIgnoreCase)
                && cached.Request.Uri == uri;
        }

        static bool IsValidHeader(string header)
        {
            if (header == "Content-Length"
                || header == "Last-Modified"
                || header == "Expires"
                || header == "Content-Type")
            {
                return false;
            }

            return true;
        }

        static HttpResponseMessage FormatResponse(CachedResponse cached)
        {
            var statusCode = (System.Net.HttpStatusCode)cached.Status.Code;
            var response = new HttpResponseMessage(statusCode);
            foreach (var kvp in cached.Headers)
            {
                if (IsValidHeader(kvp.Key))
                {
                    response.Headers.Add(kvp.Key, kvp.Value);
                }
            }
            response.Content = SetContent(cached.Body);
            return response;
        }

        static HttpContent SetContent(Body body)
        {
            if (body.Encoding == "ASCII-8BIT")
            {
                var text = body.Base64_string;
                var textWithoutNewLines = text.Replace("\n", "");
                var decodedBytes = Convert.FromBase64String(textWithoutNewLines);
                var output = Encoding.UTF8.GetString(decodedBytes);
                return new StringContent(output);
            }

            if (body.Encoding == "UTF8-8BIT")
            {
                var text = body.Base64_string;
                var decodedBytes = Convert.FromBase64String(text);
                var output = Encoding.UTF8.GetString(decodedBytes);
                return new StringContent(output);
            }

            return null;
        }

        internal async Task<CacheResult> FindCachedResponseAsync(HttpRequestMessage request)
        {
            if (cache == null)
            {
                await SetupCache();
            }

            var match = cache.FirstOrDefault(c => MatchesRequest(c, request));
            if (match != null)
            {
                return CacheResult.Success(FormatResponse(match.Response));
            }

            return CacheResult.Missing();
        }

        internal async Task StoreCachedResponseAsync(HttpRequestMessage request, HttpResponseMessage freshResponse)
        {
            if (cache == null)
            {
                await SetupCache();
            }

            var cachedResponse = new CachedRequestResponse
            {
                Request = new CachedRequest
                {
                    Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray()),
                    Method = request.Method.Method,
                    Uri = request.RequestUri,
                    Body = await ParseContent(request.Content)
                },
                Response = new CachedResponse
                {
                    Headers = freshResponse.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray()),
                    Status = new Status
                    {
                        Code = (int)freshResponse.StatusCode,
                        Message = freshResponse.StatusCode.ToString()
                    },
                    Body = await ParseContent(freshResponse.Content)
                }
            };

            cache.Add(cachedResponse);
        }

        private async Task<Body> ParseContent(HttpContent content)
        {
            if (content == null)
            {
                return new Body
                {
                    Encoding = "",
                    Base64_string = ""
                };
            }

            var text = await content.ReadAsStringAsync();
            var bytes = Encoding.UTF8.GetBytes(text);
            return new Body
            {
                Base64_string = Convert.ToBase64String(bytes),
                Encoding = "UTF8-8BIT"
            };
        }

        internal Task FlushToDisk()
        {
            var json = new CachedRequestResponseArray
            {
                http_interactions = cache.ToArray()
            };

            var text = JsonConvert.SerializeObject(json);
            return File.WriteAllTextAsync(cassettePath, text);
        }
    }

    internal class Body
    {
        public string Encoding { get; set; }
        public string Base64_string { get; set; }
    }

    internal class CachedRequest
    {
        public string Method { get; set; }
        public Uri Uri { get; set; }
        public Body Body { get; set; }
        public Dictionary<string, string[]> Headers { get; set; }
    }

    internal class Status
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }

    internal class CachedResponse
    {
        public Status Status { get; set; }
        public Dictionary<string, string[]> Headers { get; set; }
        public Body Body { get; set; }
    }

    internal class CachedRequestResponse
    {
        public CachedRequest Request { get; set; }
        public CachedResponse Response { get; set; }
    }

    internal class CachedRequestResponseArray
    {
        public CachedRequestResponse[] http_interactions { get; set; }
    }
}
