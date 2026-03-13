using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ObserveTool.Options;
using System.Text;

namespace ObserveTool.Middlewares
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly MonitoringSecurityOptions _options;

        public BasicAuthMiddleware(RequestDelegate next, IOptions<MonitoringSecurityOptions> options)
        {
            _next = next;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!_options.EnableAuth)
            {
                await _next(context);
                return;
            }

            if (!context.Request.Path.ToString().StartsWith("/monitoring"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                Challenge(context);
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                Challenge(context);
                return;
            }

            var encoded = authHeader["Basic ".Length..].Trim();
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));

            var parts = decoded.Split(':', 2);
            if (parts.Length != 2 ||
                parts[0] != _options.Username ||
                parts[1] != _options.Password)
            {
                Challenge(context);
                return;
            }

            await _next(context);
        }

        private static void Challenge(HttpContext context)
        {
            context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Monitoring\"";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
    }
}
