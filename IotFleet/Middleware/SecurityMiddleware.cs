using Application.Abstractions.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace IotFleet.Middleware
{
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ITokenProvider tokenProvider)
        {
            try
            {
                var token = context.Request.Headers["authorization"].FirstOrDefault();
                var userId = tokenProvider.ValidateToken(token);
                if (userId != null)
                {
                    // attach user to context on successful jwt validation
                    context.Items["User"] = userId;
                }
            }
            catch (SecurityTokenExpiredException)
            {
                context.Items["Error"] = "SecurityTokenExpiredException";
            }
            //await _next(context);
            await _next(context);
        }
    }

    public static class SecurityMiddlewareExtension
    {
        public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityMiddleware>();
        }
    }
}
