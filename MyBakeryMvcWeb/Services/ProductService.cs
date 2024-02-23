namespace MyBakeryMvcWeb.Services
{
    using ASPNETCore8ErrorHandling.Filters;
    using Microsoft.AspNetCore.Mvc;
    using MyBakeryMvcWeb.ViewModels;

    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _logger;

        public ProductService(ILogger<ProductService> logger) 
        {
            _logger = logger;
        }

        public int Create(ProductViewModel product)
        {
            _logger.LogInformation("-----");
            return 0;
        }

        public int OccursOutRangeException(ProductViewModel product)
        {
            throw new MyOutRangeException("產品單價");
        }

    }
}
