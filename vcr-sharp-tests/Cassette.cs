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

        internal async Task<CacheResult> FindCachedResponseAsync(HttpRequestMessage request)
        {
            if (cache == null)
            {
                await SetupCache();
            }

            var match = cache.FirstOrDefault(c => MatchesRequest(c, request));
            if (match != null)
            {
                return CacheResult.Success(Serializer.Deserialize(match.Response));
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
                Request = await Serializer.Serialize(request),
                Response = await Serializer.Serialize(freshResponse)
            };

            cache.Add(cachedResponse);
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
