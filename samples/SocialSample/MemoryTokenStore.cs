using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;

namespace SocialSample
{
    public class MemoryTokenStore : ITokenStore
    {
        private IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public string Get(string key)
        {
            return _cache.Get<string>(key);
        }

        public void Set(string key, string value)
        {
            _cache.Set(key, value);
        }
    }
}
