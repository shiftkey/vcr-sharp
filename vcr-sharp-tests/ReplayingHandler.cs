using System;
using System.Collections.Generic;
using System.Text;

namespace VcrSharp.Tests
{
public class ReplayingHandler : DelegatingHandler
{
    private readonly Cassette cassette;

    public ReplayingHandler(HttpMessageHandler innerHandler, string cassettePath) : base(innerHandler)
    {
        cassette = new Cassette(cassettePath);
    }

    public ReplayingHandler(string cassettePath) : this(new HttpClientHandler(), cassettePath)
    {

    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cachedResponse = await cassette.findCachedResponseAsync(request);

        if (cachedResponse.Item1)
        {
            return cachedResponse.Item2;
        }

        var freshResponse = await base.SendAsync(request, cancellationToken);

        await cassette.storeCachedResponseAsync(request, freshResponse);

        return freshResponse;
    }

    protected override async void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        await cassette.FlushToDisk();
    }
}
}
