namespace ASPNETCore8ErrorHandling.Middlewares
{
    using System.Net;

    /// <summary>
    /// 自定義的例外回傳記錄 (record)
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ASPNETCore8ErrorHandling.Middlewares.ExceptionResponseByMiddleware&gt;" />
    public record ExceptionResponseByMiddleware(HttpStatusCode StatusCode, string Description, string TraceId);
}
