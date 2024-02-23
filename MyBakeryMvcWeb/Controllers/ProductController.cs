namespace MyBakeryMvcWeb.Controllers
{
    using ASPNETCore8ErrorHandling.Controllers;
    using ASPNETCore8ErrorHandling.Filters;
    using Microsoft.AspNetCore.Mvc;
    using MyBakeryMvcWeb.Services;
    using MyBakeryMvcWeb.ViewModels;

    //[Route("[controller]/[action]")]
    public class ProductController : BaseController
    {
        private readonly IProductService _service;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService service, ILogger<ProductController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var products = new List<ProductViewModel>()
            {   new() {Id=1, Name="布丁", OrderQty=1, UnitPrice=50 },
                new() {Id=2, Name="蛋塔", OrderQty=1, UnitPrice=40 },
            };

            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var product = new ProductViewModel() { Id=1, Name="豆塔", OrderQty=1, UnitPrice=30 };
            return View(product);
        }

        [HttpPost]
        public IActionResult Create(ProductViewModel product)
        {
            var result = _service.Create(product);
            _logger.LogInformation("處理結果: {result}", result);
            return View(product);
        }

        [HttpPost]
        public IActionResult CreateAjaxForm(ProductViewModel product)
        {

            // 處理 validation attribute (model binding) 檢核未過的錯誤
            if (!ModelState.IsValid)
            {
                var description = this.ModelErrorToString();
                throw new MyClientException(description);
                //return BadRequest(ModelState);
            }

            var result = _service.Create(product);
            _logger.LogInformation("處理結果: {result}", result);
            return View("Create", product);
        }


        [HttpPost]
        public IActionResult CreateAjaxJson([FromBody] ProductViewModel product)
        {

            // 處理 validation attribute (model binding) 檢核未過的錯誤
            if (!ModelState.IsValid)
            {
                var description = this.ModelErrorToString();
                throw new MyClientException(description); 
                //return BadRequest(ModelState);
            }

            var result = _service.Create(product);
            _logger.LogInformation("處理結果: {result}", result);
            return View("Create",product);
        }

        [HttpPost]
        public IActionResult OccursParamNullException([FromBody] ProductViewModel product)
        {

            throw new MyParamNullException("產品名稱");
            //if (!ModelState.IsValid)
            //{
            //    // Inspect ModelState for errors
            //    return BadRequest(ModelState);
            //}
            //return View(product);
        }

        [HttpPost]
        public IActionResult OccursOutRangeException([FromBody] ProductViewModel product)
        {
            var result = _service.OccursOutRangeException(product);
            return Json(new { Result = result });
        }


        [HttpPost]
        public IActionResult OccursDataExistException([FromBody] ProductViewModel product)
        {
            throw new MyDataExistException("豆塔");
        }

        [HttpPost]
        public IActionResult OccursDataNotExistException([FromBody] ProductViewModel product)
        {
            throw new MyDataNotExistException("費南雪");
        }

        [HttpPost]
        public IActionResult OccursIOException([FromBody] ProductViewModel product)
        {
            throw new System.IO.IOException("費南雪密笈 不存在");
        }
    }
}
