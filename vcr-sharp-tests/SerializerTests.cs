using System.Net.Http;
using System.Threading.Tasks;
using VcrSharp.Tests;
using Xunit;

public class SerializerTests
{
    public class Get
    {
        [Fact]
        public async Task RountTripRequest()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/meta");
            request.Headers.UserAgent.ParseAdd("vcr-test");
            var response = await client.SendAsync(request);

            var serializedRequest = await Serializer.Serialize(request);
            var deserializedRequest = Serializer.Deserialize(serializedRequest);

            Assert.Equal(request.Version, deserializedRequest.Version);
            Assert.Equal(request.Method, deserializedRequest.Method);
            Assert.Equal(request.Content, deserializedRequest.Content);
        }

        [Fact]
        public async Task RountTripResponse()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/meta");
            request.Headers.UserAgent.ParseAdd("vcr-test");
            var response = await client.SendAsync(request);

            var serializedResponse = await Serializer.Serialize(response);
            var deserializedResponse = Serializer.Deserialize(serializedResponse);

            Assert.Equal(deserializedResponse.Version, response.Version);
            Assert.Equal(deserializedResponse.StatusCode, response.StatusCode);
            Assert.Equal(deserializedResponse.Content.Headers.ContentType, response.Content.Headers.ContentType);

            var actualText = await deserializedResponse.Content.ReadAsStringAsync();
            var expectedText = await response.Content.ReadAsStringAsync();

            Assert.Equal(actualText, expectedText);
        }
    }

    public class Post
    {
        [Fact]
        public async Task RountTripRequest()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.github.com/user/repos");
            request.Headers.UserAgent.ParseAdd("vcr-test");
            var response = await client.SendAsync(request);

            var serializedRequest = await Serializer.Serialize(request);
            var deserializedRequest = Serializer.Deserialize(serializedRequest);

            Assert.Equal(request.Version, deserializedRequest.Version);
            Assert.Equal(request.Method, deserializedRequest.Method);
            Assert.Equal(request.Content, deserializedRequest.Content);
        }

        [Fact]
        public async Task RountTripResponse()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.github.com/user/repos");
            request.Headers.UserAgent.ParseAdd("vcr-test");
            var response = await client.SendAsync(request);

            var serializedResponse = await Serializer.Serialize(response);
            var deserializedResponse = Serializer.Deserialize(serializedResponse);

            Assert.Equal(response.Version, deserializedResponse.Version);
            Assert.Equal(response.StatusCode, deserializedResponse.StatusCode);
            Assert.Equal(response.Content.Headers.ContentType, deserializedResponse.Content.Headers.ContentType);

            var actualText = await deserializedResponse.Content.ReadAsStringAsync();
            var expectedText = await response.Content.ReadAsStringAsync();

            Assert.Equal(actualText, expectedText);
        }
    }
}

