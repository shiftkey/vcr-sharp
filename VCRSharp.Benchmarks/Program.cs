using BenchmarkDotNet.Attributes;
using System;
using BenchmarkDotNet.Running;
using VcrSharp.Tests;
using System.Net.Http;
using System.Threading.Tasks;

namespace VCRSharp.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<VCRSharpBenchmarks>();
        }
    }

    public class VCRSharpBenchmarks : IDisposable
    {
        public VCRSharpBenchmarks()
        {
            Environment.SetEnvironmentVariable("VCR_MODE", "playback");
        }

        [Benchmark]
        public async Task ReadFromCache()
        {

            using (var httpClient = HttpClientFactory.WithCassette("example-test"))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://www.iana.org/domains/reserved");
                var response = await httpClient.SendAsync(request);
            }
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
