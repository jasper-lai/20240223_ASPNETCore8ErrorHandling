namespace ASPNETCore8ErrorHandling.Middlewares
{
    using ASPNETCore8ErrorHandling.Filters;
    using ASPNETCore8ErrorHandling.Models;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        //#region 方式一: 回傳自定義的錯誤類別 (ErrorViewModel)

        //public async ValueTask<bool> TryHandleAsync(
        //    HttpContext context,
        //    Exception exception,
        //    CancellationToken cancellationToken)
        //{
        //    // STEP 1: 取得 controller name / action name
        //    // 重要: context.GetRouteData(); always return null
        //    var routeData = context.GetRouteData();
        //    var controllerName = routeData?.Values["controller"]?.ToString();
        //    var actionName = routeData?.Values["action"]?.ToString();

        //    //var endpoint = context.GetEndpoint();
        //    //var controllerName = string.Empty;
        //    //var actionName = string.Empty;
        //    //if (endpoint != null)
        //    //{
        //    //    var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        //    //    if (controllerActionDescriptor != null)
        //    //    {
        //    //        controllerName = controllerActionDescriptor.ControllerName;
        //    //        actionName = controllerActionDescriptor.ActionName;
        //    //    }
        //    //}

        //    // STEP 2: 建立回傳物件
        //    var traceId = context.TraceIdentifier;   // 這個已經被 middleware 改成 GUID

        //    ErrorViewModel result = exception switch
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

        //    // STEP 3: 設定回傳的 response header
        //    context.Response.StatusCode = result.StatusCode;

        //    // STEP 4: 寫入至 Log
        //    if (result.StatusCode >= 400 && result.StatusCode < 500)
        //        _logger.LogWarning("Controller={controllerName} Action={actionName} => Message={message}", controllerName, actionName, exception.Message);
        //    if (result.StatusCode >= 500)
        //        _logger.LogError(exception, "Controller={controllerName} Action={actionName} => Message={message}", controllerName, actionName, exception.Message);

        //    // STEP 5: 回傳結果
        //    await context.Response
        //        .WriteAsJsonAsync(result, cancellationToken);

        //    // STEP 6: 設定已處理例外, 不再往外拋出
        //    return true;
        //}

        //#endregion

        #region 方式二: 回傳 ASP.NET Core 內建的 ProblemDetails 類別

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {

            // STEP 1: 取得 controller name / action name
            // 重要: context.GetRouteData(); always return null
            var routeData = context.GetRouteData();
            var controllerName = routeData?.Values["controller"]?.ToString();
            var actionName = routeData?.Values["action"]?.ToString();

            // STEP 2: 建立回傳物件
            var traceId = context.TraceIdentifier;

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
            response.Extensions.Add("traceId", traceId);

            // STEP 3: 設定回傳的 response header
            context.Response.StatusCode = response.Status ?? StatusCodes.Status500InternalServerError;

            // STEP 4: 寫入至 Log
            if (response.Status >= 400 && response.Status < 500)
                _logger.LogWarning("Controller={controllerName} Action={actionName} => Message={message}", controllerName, actionName, exception.Message);
            if (response.Status >= 500)
                _logger.LogError(exception, "Controller={controllerName} Action={actionName} => Message={message}", controllerName, actionName, exception.Message);

            // STEP 5: 回傳結果
            await context.Response
                .WriteAsJsonAsync(response, cancellationToken);

            // STEP 6: 設定已處理例外, 不再向外拋出
            return true;
        }
        #endregion

    }
}
