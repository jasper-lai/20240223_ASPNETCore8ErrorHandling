namespace ASPNETCore8ErrorHandling.Middlewares
{
    using ASPNETCore8ErrorHandling.Filters;
    using ASPNETCore8ErrorHandling.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using System.Net;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Unicode;

    /// <summary>
    /// 自定義的例外攔截 Middleware
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly static JsonSerializerOptions _jsonOptions = new()
        {
            // 預設第一字母轉小寫 (camel), 若要改成 C# 的字首大寫格式 (Pascal), 要設為 null 
            // 重要:
            // (1) 如果有設 options 的話, 就一定維持字首大寫, 因為沒設 PropertyNamingPolicy, 等同 null.
            // (2) 只有在完全沒設 options, 或設為 JsonNamingPolicy.CamelCase, 才會是小寫.
            // (3) 若設成字首小寫, 則自行在 ProblemDetails 擴增的欄位 (TraceId, ControllerName...), 並不會轉小寫 !!
            //
            //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,   
            //PropertyNamingPolicy = null,   
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs)
        };

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        //#region 方式一: 回傳自定義的錯誤類別 (ErrorViewModel)
        //private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        //{

        //    // STEP 1: 取得 controller name / action name / trace id
        //    var routeData = context.GetRouteData();
        //    var controllerName = routeData?.Values["controller"]?.ToString();
        //    var actionName = routeData?.Values["action"]?.ToString();
        //    var traceId = context.TraceIdentifier;

        //    // STEP 2: 建立回傳物件
        //    ErrorViewModel response = exception switch
        //    {
        //        MyParamNullException _ or
        //        MyOutRangeException _ or
        //        MyClientException _ => new ErrorViewModel()
        //        { TraceId = traceId, StatusCode = (int)HttpStatusCode.BadRequest, StatusCodeName = HttpStatusCode.BadRequest.ToString(), Message = exception.Message },

        //        MyDataNotExistException _ => new ErrorViewModel()
        //        { TraceId = traceId, StatusCode = (int)HttpStatusCode.NotFound, StatusCodeName = HttpStatusCode.NotFound.ToString(), Message = exception.Message },

        //        MyDataExistException _ => new ErrorViewModel()
        //        { TraceId = traceId, StatusCode = (int)HttpStatusCode.Conflict, StatusCodeName = HttpStatusCode.Conflict.ToString(), Message = exception.Message },

        //        MyUnauthorizedException _ => new ErrorViewModel()
        //        { TraceId = traceId, StatusCode = (int)HttpStatusCode.Unauthorized, StatusCodeName = HttpStatusCode.Unauthorized.ToString(), Message = exception.Message },

        //        MyForbiddenException _ => new ErrorViewModel()
        //        { TraceId = traceId, StatusCode = (int)HttpStatusCode.Forbidden, StatusCodeName = HttpStatusCode.Forbidden.ToString(), Message = exception.Message },

        //        _ => new()
        //        { TraceId = traceId, StatusCode = (int)HttpStatusCode.InternalServerError, StatusCodeName = HttpStatusCode.InternalServerError.ToString(), Message = "伺服器發生未預期的錯誤" },
        //    };
        //    response.ControllerName = controllerName ?? string.Empty;
        //    response.ActionName = actionName ?? string.Empty;

        //    // STEP 3: 設定回傳的 response header
        //    context.Response.ContentType = "application/json";
        //    context.Response.StatusCode = (int)response.StatusCode;

        //    // STEP 4: 寫入至 Log
        //    string jsonString = JsonSerializer.Serialize(response, _jsonOptions);

        //    if (response.StatusCode >= 400 && response.StatusCode < 500)
        //        _logger.LogWarning("Controller={controllerName} Action={actionName} => Message={message}", controllerName, actionName, exception.Message);
        //    if (response.StatusCode >= 500)
        //        _logger.LogError(exception, "Controller={controllerName} Action={actionName} => Message={message}", controllerName, actionName, exception.Message);

        //    _logger.LogInformation("{json}", jsonString);    //輸出完整的 json 字串

        //    // STEP 5: 回傳結果
        //    // await context.Response.WriteAsJsonAsync(response);
        //    await context.Response.WriteAsJsonAsync(response, _jsonOptions);
        //}

        //#endregion


        #region 方式二: 回傳 ASP.NET Core 內建的 ProblemDetails 類別

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {

            // STEP 1: 取得 trace id / controller name / action name
            var traceId = context.TraceIdentifier;
            string? controllerName = null;
            string? actionName = null;

            //// 方式 1: 利用 RouteData 只能取到 controller 的簡名 (不含 namespace)
            //var routeData = context.GetRouteData();
            //controllerName = routeData?.Values["controller"]?.ToString();
            //actionName = routeData?.Values["action"]?.ToString();

            // 方式 2: 利用 Endpoint 可取到 controller 的全名 (含 namespace)
            var endpoint = context.GetEndpoint();

            if (endpoint != null)
            {
                var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

                if (actionDescriptor != null)
                {
                    // controllerName = actionDescriptor.ControllerName;
                    controllerName = actionDescriptor.ControllerTypeInfo.FullName;
                    actionName = actionDescriptor.ActionName;
                }
            }

            // STEP 2: 建立回傳物件
            ProblemDetails response = exception switch
            {
                MyParamNullException _ or
                MyOutRangeException _ or
                MyClientException _ => new ProblemDetails()
                {
                    Title = HttpStatusCode.BadRequest.ToString(),
                    Status = StatusCodes.Status400BadRequest,
                },

                MyDataNotExistException _ => new ProblemDetails()
                {
                    Title = HttpStatusCode.NotFound.ToString(),
                    Status = StatusCodes.Status404NotFound,
                },

                MyDataExistException _ => new ProblemDetails()
                {
                    Title = HttpStatusCode.Conflict.ToString(),
                    Status = StatusCodes.Status409Conflict,
                },

                MyUnauthorizedException _ => new ProblemDetails()
                {
                    Title = HttpStatusCode.Unauthorized.ToString(),
                    Status = StatusCodes.Status401Unauthorized,
                },

                MyForbiddenException _ => new ProblemDetails()
                {
                    Title = HttpStatusCode.Forbidden.ToString(),
                    Status = StatusCodes.Status403Forbidden,
                },

                _ => new()
                {
                    Title = HttpStatusCode.InternalServerError.ToString(),
                    Status = StatusCodes.Status500InternalServerError,
                }
            };
            if (response.Status != StatusCodes.Status500InternalServerError)
                response.Detail = exception.Message;
            else
                response.Detail = "伺服器發生未預期的錯誤";

            response.Instance = context.Request.Path;
            response.Extensions.Add("TraceId", traceId);
            response.Extensions.Add("ControllerName", controllerName);
            response.Extensions.Add("ActionName", actionName);

            // STEP 3: 設定回傳的 response header
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = response.Status ?? StatusCodes.Status500InternalServerError;

            // STEP 4: 寫入至 Log
            //var options = new JsonSerializerOptions()
            //{
            //    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs)
            //};
            string jsonString = JsonSerializer.Serialize(response, _jsonOptions);

            if (response.Status >= 400 && response.Status < 500)
                _logger.LogWarning("Controller={controllerName} Action={actionName} => Message={message}", controllerName, actionName, exception.Message);
            if (response.Status >= 500)
                _logger.LogError(exception, "Controller={controllerName} Action={actionName} => Message={message}", controllerName, actionName, exception.Message);

            _logger.LogInformation("{json}", jsonString);    //輸出完整的 json 字串

            // STEP 5: 回傳結果
            //await context.Response.WriteAsJsonAsync(response);
            await context.Response.WriteAsJsonAsync(response, _jsonOptions);
        }

        #endregion
    }
}
