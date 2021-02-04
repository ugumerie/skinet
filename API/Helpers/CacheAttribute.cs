using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    public class CacheAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _timeToLiveSeconds;

        public CacheAttribute(int timeToLiveSeconds)
        {
            _timeToLiveSeconds = timeToLiveSeconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //b4 executing
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();

            // generate cache key
            var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
            var cachedResponse = await cacheService.GetCachedResponseAsync(cacheKey);

            // if we have the cached response 
            if(!string.IsNullOrEmpty(cachedResponse))
            {
                //retrieve from memory (redis) and send back to the client
                //instead of going to the controller, then to the database and query
                var contentResult = new ContentResult
                {
                    Content = cachedResponse,
                    ContentType = "application/json",
                    StatusCode = 200
                };

                context.Result = contentResult;

                return;
            }

            var executedContext = await next(); //This is were we move to our controller and excute the action
            if(executedContext.Result is OkObjectResult okObjectResult)
            {
                await cacheService.CacheResponseAsync(cacheKey, okObjectResult.Value,
                     TimeSpan.FromSeconds(_timeToLiveSeconds));
            }


        }

        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();

            keyBuilder.Append($"{request.Path}");

            foreach(var (key, value) in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"|{key}-{value}");
            }

            return keyBuilder.ToString();
        }
    }
}