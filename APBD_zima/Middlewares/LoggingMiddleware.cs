using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace APBD_zima.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();

            if (httpContext.Request != null)
            {
                string path = httpContext.Request.Path;
                string querystring = httpContext.Request?.QueryString.ToString();
                string method = httpContext.Request.Method.ToString();
                string bodyStr = "";

                using (StreamReader reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                }
                using (StreamWriter file = File.AppendText(@"/Users/DamianGoraj/Desktop/C#_Logger/log.txt"))
                {
                    file.WriteLine($"path: {path} querystring: {querystring} method: {method} bodyStr: {bodyStr}");
                }
                await _next(httpContext);
            }
        }
    }
}
