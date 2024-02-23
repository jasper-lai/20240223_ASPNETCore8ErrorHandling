namespace ASPNETCore8ErrorHandling.Controllers
{
    using ASPNETCore8ErrorHandling.Filters;
    using ASPNETCore8ErrorHandling.Models;
    using Microsoft.AspNetCore.Mvc;
    using System.Diagnostics;
    using System.Net;


    /// <summary>
    /// 如果要作為其它 MVC 專案的基礎時, 才要設定這個 route attribute
    /// 若要單獨測試, 則要將此 route attribute 作 comment
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    /// <remarks>
    /// AmbiguousMatchException: The request matched multiple endpoints. Matches: 
    ///     MyBakeryMvcWeb.Controllers.HomeController.Index (MyBakeryMvcWeb) 
    ///     ASPNETCore8ErrorHandling.Controllers.HomeController.Index (ASPNETCore8ErrorHandling)
    /// </remarks>
    [Route("ASPNETCore8ErrorHandling/[controller]/[action]")]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("visit Privacy page");
            return View();
        }

        public IActionResult About(int id = 0)
        {
            if (id == 1)
            {
                throw new MyClientException("傳入的參數值有誤");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            //return View(new ErrorViewModel { TraceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            //var traceId = HttpContext.Response.Headers["X-Trace-Id"].ToString();
            var traceId = HttpContext.TraceIdentifier;
            if (string.IsNullOrEmpty(traceId))
            {
                traceId = Activity.Current?.Id;
            }
            return View(new ErrorViewModel
            {
                TraceId = traceId,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                StatusCodeName = HttpStatusCode.InternalServerError.ToString(),
                Message = "伺服器端發生錯誤!"
            });
        }
    }
}
