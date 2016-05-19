using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;

namespace CookieSessionSample 
{
    public class RedisCacheTicketStore : ITicketStore 
    {
        private static readonly char[] _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private readonly IDistributedCache _cache;
        private readonly IDataSerializer<AuthenticationTicket> _ticketSerializer = TicketSerializer.Default;

        public RedisCacheTicketStore(RedisCacheOptions options) 
        {
            _cache = new RedisCache(options);
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket) 
        {
            var key = GetUniqueKey();
            await RenewAsync(key, ticket);
            return key;
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket) 
        {
            var options = new DistributedCacheEntryOptions();
            var expiresUtc = ticket.Properties.ExpiresUtc;
            if (expiresUtc.HasValue) {
                options.SetAbsoluteExpiration(expiresUtc.Value);
            }
            options.SetSlidingExpiration(TimeSpan.FromHours(1)); // TODO: configurable.

            var data = _ticketSerializer.Serialize(ticket);

            await _cache.SetAsync(key, data, options);
        }

        public async Task<AuthenticationTicket> RetrieveAsync(string key) 
        {
            var data = await _cache.GetAsync(key);

            var ticket = _ticketSerializer.Deserialize(data);

            return ticket;
        }

        public async Task RemoveAsync(string key) 
        {
            await _cache.RemoveAsync(key);
        }

        private static string GetUniqueKey() 
        {
            int maxSize = 8;
            int size = maxSize;
            var data = new byte[1];
            var crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            var sb = new StringBuilder(size);
            foreach (byte b in data) {
                sb.Append(_chars[b % (_chars.Length - 1)]);
            }

            return sb.ToString();
        }
    }
}
