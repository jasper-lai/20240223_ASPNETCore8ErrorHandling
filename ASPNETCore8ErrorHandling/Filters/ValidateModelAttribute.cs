namespace ASPNETCore8ErrorHandling.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc;

    public class ValidateModelAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                // 方式1: 統一集中例外處理 if you want to throw a custom exception
                var description = ModelErrorToString(context.ModelState);
                throw new MyClientException(description);
                //// 方式2: 利用 context.Result 回傳, 但這就無法統一回傳格式了.
                //context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }

        /// <summary>
        /// 將未通過 Model Validation 資料檢核的清單, 轉為字串
        /// </summary>
        /// <returns></returns>
        private string ModelErrorToString(ModelStateDictionary modelState)
        {
            // Your implementation to convert model state errors to a single string
            var errors = modelState.Values.SelectMany(v => v.Errors)
               .Select(e => e.ErrorMessage)
               .ToList();
            var description = string.Join(Environment.NewLine, errors);
            description = "輸入的資料有誤: " + Environment.NewLine + description;
            return description;
        }
    }
}
