using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using ObserveTool.Models;
using ObserveTool.Models.Enums;
using ObserveTool.Services.Interfaces;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace ObserveTool.Middlewares
{
    /// <summary>
    /// Middleware responsible for logging incoming HTTP requests and outgoing responses.
    ///
    /// It captures key request information such as:
    /// - HTTP method
    /// - Request path
    /// - Client IP address
    /// - Request body
    /// - Response body
    /// - Execution time
    /// - Trace identifier
    ///
    /// Static resources (e.g., CSS, JS, images) are automatically ignored to avoid
    /// unnecessary logging overhead.
    ///
    /// The collected data is forwarded to <see cref="IRequestLogger"/> for buffering
    /// and later persistence.
    /// </summary>
    public sealed class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly string[] IgnoredExtensions = new[]
        {
            ".css", ".js", ".png", ".jpg", ".jpeg", ".gif",
            ".svg", ".ico", ".woff", ".woff2", ".ttf",
            ".map"
        };

        public RequestLoggingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx, ILogger<RequestLoggingMiddleware> logger, IRequestLogger requestLogger)
        {
            var path = ctx.Request.Path.Value;

            if (ShouldIgnore(path, ctx.Connection.RemoteIpAddress))
            {
                await _next(ctx);
                return;
            }

            ctx.Request.EnableBuffering();

            string? requestBody = null;
            if (ctx.Request.Body != null && ctx.Request.ContentLength > 0)
            {
                using (var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: -1, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
         
                    ctx.Request.Body.Position = 0;
                }
            }

            var originalResponseBodyStream = ctx.Response.Body;
            using (var responseBodyStream = new MemoryStream())
            {
                ctx.Response.Body = responseBodyStream;

                var sw = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    await _next(ctx);
                    sw.Stop();

                    ctx.Response.Body.Seek(0, SeekOrigin.Begin);
                    string responseBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();

                    ctx.Response.Body.Seek(0, SeekOrigin.Begin);

                    await responseBodyStream.CopyToAsync(originalResponseBodyStream);

                    requestLogger.Log(new RequestLog
                    {
                        Method = GetMethod(ctx.Request.Method),
                        ElapsedMilliseconds = sw.ElapsedMilliseconds,
                        IpAddress = ctx.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                        Path = ctx.Request.GetEncodedPathAndQuery(),
                        TraceId = Activity.Current?.TraceId.ToString(),
                        Request = requestBody,
                        Response = responseBody,
                        StatusCode = ctx.Response.StatusCode,
                    });
                }
                catch (Exception ex)
                {
                    sw.Stop();

                    logger.LogError(ex, "Unhandled exception : " + ex.Message);
                    throw;
                }
                finally
                {
                    ctx.Response.Body = originalResponseBodyStream;
                }
            }
        }

        private static bool ShouldIgnore(string? path, IPAddress? ipAddress)
        {
            if (string.IsNullOrEmpty(path))
                return true;

            if (IgnoredExtensions.Any(ext =>
                path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                return true;

            if (ipAddress is not null)
            {
                if (path.ToLower() == "/health" && IPAddress.IsLoopback(ipAddress))
                {
                    return true;
                }
            }

            return false;
        }
    
        private static RequestMethod GetMethod (string method)
        {
            if (string.IsNullOrWhiteSpace(method))
                throw new ArgumentException("Method cannot be null or empty.", nameof(method));

            // Trim and compare case-insensitively
            string trimmedMethod = method.Trim();
            switch (trimmedMethod.ToUpperInvariant())
            {
                case "GET":
                    return RequestMethod.GET;
                case "POST":
                    return RequestMethod.POST;
                case "PUT":
                    return RequestMethod.PUT;
                case "PATCH":
                    return RequestMethod.PATCH;
                case "DELETE":
                    return RequestMethod.DELETE;
                default:
                    throw new ArgumentException($"Unknown HTTP method: '{trimmedMethod}'.", nameof(method));
            }
        }
    }
}
