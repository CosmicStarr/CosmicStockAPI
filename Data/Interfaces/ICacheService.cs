using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface ICacheService
    {
        Task ObjectToCache(string key,object itemToCache,TimeSpan timetolive);
        Task<string> GetCachedObject(string key);
    }
}