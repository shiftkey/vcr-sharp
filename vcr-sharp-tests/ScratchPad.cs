using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace VcrSharp.Tests
{
    public class HttpClientFactory
    {
        static string AssemblyLoadDirectory
        {
            get
            {
                var codeBase = Assembly.GetCallingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static HttpClient WithCassette(string session)
        {
            var currentDirectory = Assembly.GetExecutingAssembly();
            var testCassettePath = Path.Combine(AssemblyLoadDirectory, "fixtures", session + ".json");
            var handler = new ReplayingHandler(testCassettePath);
            var httpClient = new HttpClient(handler);
            return httpClient;
        }
    }

    public class ScratchPad
    {
        [Fact]
        public async Task ScratchpadUsesGivenCassette()
        {
            using (var httpClient = HttpClientFactory.WithCassette("synopsis"))
            {
                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://www.iana.org/domains/reserved"));
                var body = await response.Content.ReadAsStringAsync();
                body.ShouldContain("Example domains");
            }
        }
    }
}
