using System.Net.Http;

namespace VcrSharp.Tests
{
    public class CacheResult
    {
        public bool Found { get; set; }

        public HttpResponseMessage Response { get; set; }

        public static CacheResult Success(HttpResponseMessage response)
        {
            return new CacheResult
            {
                Found = true,
                Response = response
            };
        }

        public static CacheResult Missing()
        {
            return new CacheResult
            {
                Found = false
            };
        }
    }
}
