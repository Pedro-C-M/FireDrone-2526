using System.Net;
using System.Text.Json;
using CentralBackend.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CentralBackend.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentType = "application/json";

                var json = JsonSerializer.Serialize(new
                {
                    error = "NotFound",
                    message = ex.Message
                });

                await context.Response.WriteAsync(json);
            }
            catch (DbUpdateException ex)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = "InvalidData",
                    message = "Los datos enviados violan las restricciones de la base de datos."
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine("INTERNAL ERROR");
                Console.WriteLine(ex.GetType().Name);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var json = JsonSerializer.Serialize(new
                {
                    error = ex.GetType().Name,     
                    message = ex.Message           
                });

                await context.Response.WriteAsync(json);
            }
        }
    }
}
