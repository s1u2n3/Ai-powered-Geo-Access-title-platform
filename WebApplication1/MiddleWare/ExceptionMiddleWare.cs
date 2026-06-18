using System.Text.Json;
using WebApplication1.MiddleWare.Exceptions;

namespace WebApplication1.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _ilogger;

        public ExceptionMiddleware(
            RequestDelegate next,ILogger<ExceptionMiddleware> ilogger)
        {
            _next = next;
            _ilogger = ilogger;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.ContentType =
                    "application/json";

                var statusCode = ex switch
                {
                    NotFoundException => 404,

                    ValidationException => 400,

                    UnauthorizedException => 401,

                    _ => 500
                };

                context.Response.StatusCode =
                    statusCode;

                var userId =
                    context.User
                    .FindFirst(
                        System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?.Value;

                var userType =
                    context.User
                    .FindFirst("UserType")
                    ?.Value;

                _ilogger.LogError(
                    ex,
                    "Unhandled Exception. UserType:{UserType} UserId:{UserId} Path:{Path}",
                    userType,
                    userId,
                    context.Request.Path);

                var message =
                    statusCode == 500
                        ? "Something went wrong"
                        : ex.Message;

                var response = new
                {
                    Message = message,
                    StatusCode = statusCode
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(
                        response));
            }
        }
    }
}