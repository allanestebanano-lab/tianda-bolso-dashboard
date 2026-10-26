using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace TiendaBolsos.Shared.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;

                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Detail = ex.Message
                });

                await context.Response.WriteAsync(result);
            }
        }
    }
}
