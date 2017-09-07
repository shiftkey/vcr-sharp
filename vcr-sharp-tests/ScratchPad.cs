using Shouldly;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace VcrSharp.Tests
{
    public class ScratchPad
    {
        [Fact]
        public async Task ScratchpadUsesGivenCassette()
        {
            using (var httpClient = HttpClientFactory.WithCassette("example-test"))
            {
                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://www.iana.org/domains/reserved"));
                var body = await response.Content.ReadAsStringAsync();
                body.ShouldContain("Example domains");
            }
        }
    }
}
