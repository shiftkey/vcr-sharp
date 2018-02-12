# vcr-sharp

A library for supporting recording and replaying HTTP requests when using [`HttpClient`](https://www.nuget.org/packages/System.Net.Http).

## Installation

Not just yet, gotta make it work better.

## Usage

Rather than creating a `HttpClient` manually, your test should use the `HttpClientFactory`
with a provided **cassette** to represent the context of the test.

This cassette file is a JSON file on disk, which is used to store cached HTTP requests.

You can then use the received `HttpClient` instance like you would normally:

```cs
using (var httpClient = HttpClientFactory.WithCassette("my-test-scenario"))
{
    var request = new HttpRequestMessage(HttpMethod.Get, "http://www.iana.org/domains/reserved");
    var response = await httpClient.SendAsync(request);
    var body = await response.Content.ReadAsStringAsync();
    body.ShouldContain("Example domains");
}
```

VCR supports a number of modes, which are specified by environment variables to
simplify maintenance of tests:

 - `playback` (default) - always use the cache, throw an error if not found in cache
 - `cache` - try the cache, fallback to the network if not found and add the new response
 - `record` - avoid the cache, always use the network and store the new response
 
Simply set the `VCR_MODE` environment variable to whatever mode you want when running the tests.
 
## Why not use `XYZ`?

I evaluated `scotch` before and wanted to take it further but ended up starting clean for a
couple of reasons:

 - the use of F# - I wasn't in the mood to re-learn F#, I just wanted to get this idea out there
 - putting the replay/recording state in the API isn't something I'm a fan of - I wanted to 
   instead use environment variables so the tests remained simpler.

## Credits

While it's early days (and I'm not sure if this will even reach production usage) there's already
a lot of prior art in here which I've used for research as part of starting this:

 - The contributors to the [`vcr`](https://github.com/vcr/vcr) gem
 - [**@philschatz**](https://github.com/philschatz) for his work on [`fetch-vcr`](https://github.com/philschatz/fetch-vcr)
 - [**@mleech**](https://github.com/mleech) for his work on [`scotch`](https://github.com/mleech/scotch)

