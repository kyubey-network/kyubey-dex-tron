using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Framework.Logger;

namespace Andoromeda.Kyubey.Dex.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(
            HttpContext context, Exception exception)
        {
            var exceptionMessage = exception.ToString();
            var logger = (ILogger)context.RequestServices.GetService(typeof(ILogger));
            logger.LogError(exceptionMessage);

            var code = HttpStatusCode.InternalServerError;
            var result = JsonConvert.SerializeObject(new ApiResult
            {
                code = 500,
                msg = exceptionMessage
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
    // Extension method used to add the middleware to the HTTP request pipeline
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
