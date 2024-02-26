namespace ASPNETCore8ErrorHandling.Controllers
{
    using ASPNETCore8ErrorHandling.Filters;
    using Microsoft.AspNetCore.Mvc;

    public abstract class BaseController : Controller
    {
        ///// <summary>
        ///// 將未通過 Model Validation 資料檢核的清單, 轉為字串
        ///// </summary>
        ///// <returns></returns>
        ///// <remarks>
        ///// 這個函式目前用不到, 已經移到 Action Filter (ValidateModelAttribute)
        ///// </remarks>
        //protected string ModelErrorToString()
        //{
        //    var errors = ModelState.Values.SelectMany(v => v.Errors)
        //       .Select(e => e.ErrorMessage)
        //       .ToList();
        //    var description = string.Join(Environment.NewLine, errors);
        //    description = "輸入的資料有誤: " + Environment.NewLine + description;
        //    return description;
        //}
    }
}
