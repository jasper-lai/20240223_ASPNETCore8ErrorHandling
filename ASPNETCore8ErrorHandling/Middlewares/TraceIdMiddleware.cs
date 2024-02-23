namespace ASPNETCore8ErrorHandling.Middlewares
{

    /// <summary>
    /// 用以產生貫穿 Request 軌跡 TraceId 的 Middleware
    /// </summary>
    public class TraceIdMiddleware
    {
        private readonly RequestDelegate _next;

        public TraceIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.TraceIdentifier = Guid.NewGuid().ToString();
            string traceId = context.TraceIdentifier;
            context.Response.Headers["X-Trace-Id"] = traceId;
            await _next(context);
        }
    }
}
