namespace ASPNETCore8ErrorHandling.Filters
{
    using ASPNETCore8ErrorHandling.Models;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System.Diagnostics;
    using System.Net;
    using System;
    using Microsoft.AspNetCore.Mvc;

    public class MyExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<MyExceptionFilter> _logger;

        public MyExceptionFilter(ILogger<MyExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            // STEP 1: 建立回傳物件
            var exception = context.Exception;
            var traceId = context.HttpContext.TraceIdentifier;   // 這個已經被 middleware 改成 GUID

            ErrorViewModel result = exception switch
            {
                MyParamNullException _  or
                MyOutRangeException  _  or
                MyClientException    _ => new ErrorViewModel()
                { TraceId = traceId, StatusCode = (int)HttpStatusCode.BadRequest, StatusCodeName = HttpStatusCode.BadRequest.ToString(), Message = exception.Message },

                MyDataNotExistException _ => new ErrorViewModel()
                { TraceId = traceId, StatusCode = (int)HttpStatusCode.NotFound, StatusCodeName = HttpStatusCode.NotFound.ToString(), Message = exception.Message },

                MyDataExistException _ => new ErrorViewModel()
                { TraceId = traceId, StatusCode = (int)HttpStatusCode.Conflict, StatusCodeName = HttpStatusCode.Conflict.ToString(), Message = exception.Message },

                MyUnauthorizedException _ => new ErrorViewModel()
                { TraceId = traceId, StatusCode = (int)HttpStatusCode.Unauthorized, StatusCodeName = HttpStatusCode.Unauthorized.ToString(), Message = exception.Message },

                MyForbiddenException _ => new ErrorViewModel()
                { TraceId = traceId, StatusCode = (int)HttpStatusCode.Forbidden, StatusCodeName = HttpStatusCode.Forbidden.ToString(), Message = exception.Message },

                _ => new()
                { TraceId = traceId, StatusCode = (int)HttpStatusCode.InternalServerError, StatusCodeName = HttpStatusCode.InternalServerError.ToString(), Message = "伺服器發生未預期的錯誤" },
            };


            // STEP 2: 設定已處理例外, 不再往外拋出
            context.ExceptionHandled = true;

            // STEP 3: 回傳結果 (JSON 格式)
            context.Result = new JsonResult(result)
            {
                StatusCode = result.StatusCode // Optionally set the status code of the HTTP response
            };

            // STEP 4: 寫入至 Log
            if (result.StatusCode >= 400 &&  result.StatusCode < 500 )
                _logger.LogWarning("{ExceptionMessage}", exception.Message);
            if (result.StatusCode >= 500)
                _logger.LogError(exception, "{ExceptionMessage}",  exception.Message);

        }
    }
}
