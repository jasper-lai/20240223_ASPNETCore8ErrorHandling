namespace ASPNETCore8ErrorHandling.Filters
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class CalcActionExecTimeAttribute : ActionFilterAttribute
    {
        private readonly ILogger<CalcActionExecTimeAttribute> _logger;

        public CalcActionExecTimeAttribute(ILogger<CalcActionExecTimeAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 計算開始時間
            context.HttpContext.Items["StartTime"] = DateTime.Now;
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // 計算結束時間
            var startTime = context.HttpContext.Items["StartTime"] as DateTime?;
            if (startTime != null)
            {
                var endTime = DateTime.Now;
                var duration = endTime - startTime.Value;

                // 傳送到 ViewBag
                var controller = context.Controller as Controller;
                controller!.ViewBag.Duration = $"{duration.TotalMilliseconds} ms";

                // 寫入 Log
                var controllerName = context.RouteData.Values["controller"];
                var actionName = context.RouteData.Values["action"];
                var message = $"Controller: {controllerName}, Action: {actionName}, Duration: {duration.TotalMilliseconds} ms";
                //Console.WriteLine(message);
                _logger.LogInformation("Controller: {controllerName}, Action: {actionName}, Duration: {duration_ms} ms", controllerName, actionName, duration.TotalMilliseconds);
            }

            base.OnActionExecuted(context);
        }
    }
}
