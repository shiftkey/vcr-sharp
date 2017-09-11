using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace VcrSharp.Tests
{
    public class ScratchPad : IDisposable
    {
        [Fact]
        public async Task UseTheLocalFixtureToRetrieveTheResponse()
        {
            Environment.SetEnvironmentVariable("VCR_MODE", "playback");

            using (var httpClient = HttpClientFactory.WithCassette("example-test"))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://www.iana.org/domains/reserved");
                var response = await httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();
                body.ShouldContain("Example domains");
            }
        }

        [Fact]
        public async Task ErrorsWhenTheRequestIsNotInTheCache()
        {
            Environment.SetEnvironmentVariable("VCR_MODE", "playback");

            using (var httpClient = HttpClientFactory.WithCassette("no-cache-defined"))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://www.iana.org/domains/reserved");
                await Assert.ThrowsAsync<PlaybackException>(() => httpClient.SendAsync(request));
            }
        }

        [Fact]
        public async Task WritesAndFlushesAResponseToDisk()
        {
            Environment.SetEnvironmentVariable("VCR_MODE", "record");
            var session = "create-local-file";

            using (var httpClient = HttpClientFactory.WithCassette(session))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://www.iana.org/domains/reserved");
                var response = await httpClient.SendAsync(request);
            }

            var cassette = await ReadCassetteFile(session);
            cassette.HttpInteractions.Length.ShouldBe(1);
        }

        [Fact]
        public async Task ReplacesExistingRequest()
        {
            Environment.SetEnvironmentVariable("VCR_MODE", "cache");
            var session = "overwrite-request";

            var cassette = await ReadCassetteFile(session);
            cassette.HttpInteractions.Length.ShouldBe(1);

            using (var httpClient = HttpClientFactory.WithCassette(session))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://www.iana.org/performance/ietf-statistics");
                var response = await httpClient.SendAsync(request);
            }

            cassette = await ReadCassetteFile(session);
            cassette.HttpInteractions.Length.ShouldBe(1);
        }
        
        static async Task<CachedRequestResponseArray> ReadCassetteFile(string session)
        {
            var file = HttpClientFactory.GetFixturePath(session);
            File.Exists(file).ShouldBe(true);
            var text = await File.ReadAllTextAsync(file);
            return JsonConvert.DeserializeObject<CachedRequestResponseArray>(text);
        }
        
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Environment.SetEnvironmentVariable("VCR_MODE", "");
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
