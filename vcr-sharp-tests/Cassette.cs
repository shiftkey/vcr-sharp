using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VcrSharp.Tests
{
    public class Cassette
    {
        private int currentIndex = 0;
        private readonly string cassettePath;
        private List<CachedRequestResponse> cachedEntries;
        private List<CachedRequestResponse> storedEntries;

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
                cachedEntries = new List<CachedRequestResponse>(contents.HttpInteractions ?? Array.Empty<CachedRequestResponse>());
            }
            else
            {
                cachedEntries = new List<CachedRequestResponse>();
            }

            storedEntries = new List<CachedRequestResponse>();
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
            if (cachedEntries == null)
            {
                await SetupCache();
            }

            if (currentIndex < 0 || currentIndex >= cachedEntries.Count)
            {
                return CacheResult.Missing();
            }

            var entry = cachedEntries[currentIndex];
            currentIndex++;
            if (MatchesRequest(entry, request))
            {
                // persist the existing cached entry to disk
                storedEntries.Add(entry);
                return CacheResult.Success(Serializer.Deserialize(entry.Response));
            }

            return CacheResult.Missing();
        }

        internal async Task StoreCachedResponseAsync(HttpRequestMessage request, HttpResponseMessage freshResponse)
        {
            if (cachedEntries == null)
            {
                await SetupCache();
            }

            var cachedResponse = new CachedRequestResponse
            {
                Request = await Serializer.Serialize(request),
                Response = await Serializer.Serialize(freshResponse)
            };

            storedEntries.Add(cachedResponse);
        }

        internal Task FlushToDisk()
        {
            var json = new CachedRequestResponseArray
            {
                HttpInteractions = storedEntries.ToArray()
            };

            var text = JsonConvert.SerializeObject(json, Formatting.Indented);
            var directory = Path.GetDirectoryName(cassettePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(cassettePath, text);
            return Task.CompletedTask;
        }
    }

    internal class Body
    {
        [JsonProperty("encoding")]
        public string Encoding { get; set; }
        [JsonProperty("Base64_string")]
        public string Base64String { get; set; }
    }

    internal class CachedRequest
    {
        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("uri")]
        public Uri Uri { get; set; }
        [JsonProperty("body")]
        public Body Body { get; set; }
        [JsonProperty("headers")]
        public Dictionary<string, string[]> Headers { get; set; }
    }

    internal class Status
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    internal class CachedResponse
    {
        [JsonProperty("status")]
        public Status Status { get; set; }
        [JsonProperty("headers")]
        public Dictionary<string, string[]> Headers { get; set; }
        [JsonProperty("body")]
        public Body Body { get; set; }
    }

    internal class CachedRequestResponse
    {
        [JsonProperty("request")]
        public CachedRequest Request { get; set; }
        [JsonProperty("response")]
        public CachedResponse Response { get; set; }
    }

    internal class CachedRequestResponseArray
    {
        [JsonProperty("http_interactions")]
        public CachedRequestResponse[] HttpInteractions { get; set; }
    }
}
