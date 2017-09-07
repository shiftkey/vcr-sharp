using System;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace VcrSharp.Tests
{
    public class HttpClientFactory
    {
        public static string GetFixturePath(string session)
        {
            return Path.Combine(AssemblyLoadDirectory, "fixtures", session + ".json");
        }

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
            var testCassettePath = GetFixturePath(session);
            var handler = new ReplayingHandler(testCassettePath);
            var httpClient = new HttpClient(handler);
            return httpClient;
        }
    }
}
