using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Data.Classes
{
    public class CacheService : ICacheService
    {
        private readonly IDatabase _database;
        public CacheService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }
        public async Task<string> GetCachedObject(string key)
        {
            var data = await _database.StringGetAsync(key);
            if(data.IsNullOrEmpty) return null;
            return data;
        }

        public async Task ObjectToCache(string key, object itemToCache, TimeSpan timetolive)
        {
            if(itemToCache is null) return;
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var serializedOject = JsonSerializer.Serialize(itemToCache,options);
            await _database.StringSetAsync(key,serializedOject,timetolive);
        }
    }
}