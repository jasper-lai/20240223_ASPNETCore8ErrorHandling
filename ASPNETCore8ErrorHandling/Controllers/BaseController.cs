namespace ASPNETCore8ErrorHandling.Controllers
{
    using ASPNETCore8ErrorHandling.Filters;
    using Microsoft.AspNetCore.Mvc;

    public abstract class BaseController : Controller
    {
        protected string ModelErrorToString()
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
               .Select(e => e.ErrorMessage)
               .ToList();
            var description = string.Join(Environment.NewLine, errors);
            description = "輸入的資料有誤: " + Environment.NewLine + description;
            return description;
            
        }
    }
}
