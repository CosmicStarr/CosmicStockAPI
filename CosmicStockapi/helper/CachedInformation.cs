using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CosmicStockapi.helper
{
    public class CachedInformation : Attribute, IAsyncActionFilter
    {
        private readonly double _momentsToBeAlive;

        public CachedInformation(double momentsToBeAlive)
        {
            _momentsToBeAlive = momentsToBeAlive;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
            var key = CreateKeyFromRequest(context.HttpContext.Request);
            var cachedInformation = await cacheService.GetCachedObject(key);

            if(!string.IsNullOrEmpty(cachedInformation))
            {
                var contentResult = new ContentResult
                {
                    Content = cachedInformation,
                    ContentType = "application/json",
                    StatusCode = 200
                };
                context.Result = contentResult;
                return;
            }
            var excutedInfo = await next();
            if (excutedInfo.Result is OkObjectResult ok)
            {
                await cacheService.ObjectToCache(key,ok.Value,TimeSpan.FromSeconds(_momentsToBeAlive));
            }
        }
        private string CreateKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");
            foreach (var (key, value) in request.Query.OrderBy(x=>x.Key))
            {
                keyBuilder.Append($"|{key}-{value}");
            }
            return keyBuilder.ToString();
        }
    }
}